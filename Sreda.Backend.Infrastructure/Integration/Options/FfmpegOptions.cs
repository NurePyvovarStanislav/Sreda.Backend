namespace Sreda.Backend.Infrastructure.Integration.Options;

/// <summary>
/// Настройки ffmpeg. Используется только когда аудио не в целевом формате
/// (WAV 16 kHz mono PCM) и не MP3/FLAC (их whisper читает сам).
/// </summary>
public class FfmpegOptions
{
    public const string SectionName = "Ffmpeg";

    /// <summary>Путь к ffmpeg.exe. Если пусто/не найден — конвертация недоступна.</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>Таймаут конвертации в секундах.</summary>
    public int TimeoutSeconds { get; set; } = 60;
}
