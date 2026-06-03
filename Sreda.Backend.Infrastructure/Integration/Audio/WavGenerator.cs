namespace Sreda.Backend.Infrastructure.Integration.Audio;

/// <summary>
/// Генератор простого корректного WAV-файла (PCM 16-bit).
/// Используется Mock-синтезатором для возврата валидного аудио.
/// </summary>
internal static class WavGenerator
{
    public static byte[] GenerateSilence(
        TimeSpan duration,
        int sampleRate = 16000,
        short bitsPerSample = 16,
        short channels = 1)
    {
        var sampleCount = (int)(sampleRate * duration.TotalSeconds);
        var dataSize = sampleCount * channels * (bitsPerSample / 8);

        using var memory = new MemoryStream();
        using var writer = new BinaryWriter(memory);

        // RIFF header
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8.ToArray());

        // fmt chunk
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);                 // размер fmt-чанка для PCM
        writer.Write((short)1);           // формат: 1 = PCM
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * (bitsPerSample / 8)); // byteRate
        writer.Write((short)(channels * (bitsPerSample / 8)));     // blockAlign
        writer.Write(bitsPerSample);

        // data chunk (тишина)
        writer.Write("data"u8.ToArray());
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);

        writer.Flush();
        return memory.ToArray();
    }
}
