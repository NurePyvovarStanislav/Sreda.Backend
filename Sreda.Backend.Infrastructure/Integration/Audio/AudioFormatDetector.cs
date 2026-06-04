namespace Sreda.Backend.Infrastructure.Integration.Audio;

/// <summary>
/// Определяет, нужна ли конвертация через ffmpeg перед whisper.cpp.
/// Конвертация — если формат не читается whisper (MP3/FLAC) и не PCM WAV mono 16-bit
/// (частота дискретизации whisper приводит к 16 kHz сам; ffmpeg нужен для стерео/32-bit и т.д.).
/// </summary>
internal static class AudioFormatDetector
{
    private const int PreferredSampleRate = 16000;
    private const short TargetChannels = 1;
    private const short TargetBitsPerSample = 16;
    private const short PcmFormatTag = 1;

    /// <summary>
    /// true — whisper-cli может взять файл как есть, ffmpeg не нужен.
    /// </summary>
    public static bool CanUseDirectly(string path) =>
        TryDescribe(path, out var info) && info.CanUseDirectly;

    /// <summary>Описание входного файла для логов и диагностики.</summary>
    public static bool TryDescribe(string path, out AudioFormatInfo info)
    {
        info = AudioFormatInfo.Unknown;

        if (!File.Exists(path))
        {
            return false;
        }

        if (IsMp3(path))
        {
            info = new AudioFormatInfo("MP3", null, null, null, true);
            return true;
        }

        if (IsFlac(path))
        {
            info = new AudioFormatInfo("FLAC", null, null, null, true);
            return true;
        }

        if (TryReadWavPcmFormat(path, out var sampleRate, out var channels, out var bitsPerSample, out var formatTag))
        {
            var isPcm = formatTag == PcmFormatTag;
            var isMono16 = isPcm && channels == TargetChannels && bitsPerSample == TargetBitsPerSample;
            var canUseDirectly = isMono16 && sampleRate > 0;

            info = new AudioFormatInfo(
                "WAV",
                sampleRate,
                channels,
                bitsPerSample,
                canUseDirectly);

            return true;
        }

        var ext = Path.GetExtension(path);
        info = new AudioFormatInfo(string.IsNullOrWhiteSpace(ext) ? "unknown" : ext.TrimStart('.'), null, null, null, false);
        return true;
    }

    internal readonly record struct AudioFormatInfo(
        string Kind,
        int? SampleRateHz,
        short? Channels,
        short? BitsPerSample,
        bool CanUseDirectly)
    {
        public static AudioFormatInfo Unknown => new("unknown", null, null, null, false);

        public string DescribeConversionReason()
        {
            if (Kind is "MP3" or "FLAC")
            {
                return $"{Kind} (whisper читает напрямую)";
            }

            if (Kind == "WAV" && SampleRateHz is not null && Channels is not null && BitsPerSample is not null)
            {
                if (Channels != TargetChannels)
                {
                    return $"WAV {SampleRateHz / 1000.0:0.#} kHz, {Channels} кан.(а), {BitsPerSample} bit — нужен mono";
                }

                if (BitsPerSample != TargetBitsPerSample)
                {
                    return $"WAV {SampleRateHz / 1000.0:0.#} kHz, PCM {BitsPerSample} bit — нужен 16 bit PCM";
                }

                if (SampleRateHz != PreferredSampleRate)
                {
                    return $"WAV {SampleRateHz / 1000.0:0.#} kHz mono 16 bit (оптимально {PreferredSampleRate / 1000} kHz)";
                }
            }

            return Kind;
        }
    }

    private static bool IsFlac(string path)
    {
        Span<byte> header = stackalloc byte[4];
        using var stream = File.OpenRead(path);
        if (stream.Read(header) < 4)
        {
            return false;
        }

        return header[0] == 'f' && header[1] == 'L' && header[2] == 'a' && header[3] == 'C';
    }

    private static bool IsMp3(string path)
    {
        Span<byte> header = stackalloc byte[3];
        using var stream = File.OpenRead(path);
        if (stream.Read(header) < 3)
        {
            return false;
        }

        // ID3-тег
        if (header[0] == 'I' && header[1] == 'D' && header[2] == '3')
        {
            return true;
        }

        stream.Position = 0;
        if (stream.Read(header) < 2)
        {
            return false;
        }

        // MPEG frame sync
        return header[0] == 0xFF && (header[1] & 0xE0) == 0xE0;
    }

    /// <summary>
    /// Читает параметры PCM из WAV (ищет чанк "fmt ").
    /// </summary>
    private static bool TryReadWavPcmFormat(
        string path,
        out int sampleRate,
        out short channels,
        out short bitsPerSample,
        out short formatTag)
    {
        sampleRate = 0;
        channels = 0;
        bitsPerSample = 0;
        formatTag = 0;

        using var stream = File.OpenRead(path);
        Span<byte> riff = stackalloc byte[12];
        if (stream.Read(riff) < 12)
        {
            return false;
        }

        if (riff[0] != 'R' || riff[1] != 'I' || riff[2] != 'F' || riff[3] != 'F'
            || riff[8] != 'W' || riff[9] != 'A' || riff[10] != 'V' || riff[11] != 'E')
        {
            return false;
        }

        Span<byte> chunkHeader = stackalloc byte[8];
        while (stream.Read(chunkHeader) == 8)
        {
            var chunkId = $"{(char)chunkHeader[0]}{(char)chunkHeader[1]}{(char)chunkHeader[2]}{(char)chunkHeader[3]}";
            var chunkSize = BitConverter.ToInt32(chunkHeader[4..8]);

            if (chunkId == "fmt ")
            {
                Span<byte> fmt = stackalloc byte[16];
                var read = stream.Read(fmt);
                if (read < 16)
                {
                    return false;
                }

                formatTag = BitConverter.ToInt16(fmt[0..2]);
                channels = BitConverter.ToInt16(fmt[2..4]);
                sampleRate = BitConverter.ToInt32(fmt[4..8]);
                bitsPerSample = BitConverter.ToInt16(fmt[14..16]);

                // Пропускаем остаток чанка fmt, если он длиннее 16 байт.
                var remaining = chunkSize - read;
                if (remaining > 0)
                {
                    stream.Seek(remaining, SeekOrigin.Current);
                }

                return true;
            }

            // Пропускаем неизвестный чанк (размер выровнен по 2 байта).
            var skip = chunkSize + (chunkSize % 2);
            stream.Seek(skip, SeekOrigin.Current);
        }

        return false;
    }
}
