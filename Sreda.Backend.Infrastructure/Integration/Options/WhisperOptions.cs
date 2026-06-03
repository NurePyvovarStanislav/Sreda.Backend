namespace Sreda.Backend.Infrastructure.Integration.Options;

/// <summary>
/// Настройки распознавания речи через whisper.cpp (whisper-cli).
/// </summary>
public class WhisperOptions
{
    public const string SectionName = "Whisper";

    /// <summary>Путь к исполняемому файлу whisper-cli.</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>Путь к ggml-модели (например, ggml-base.bin).</summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>Язык распознавания: "auto", "en", "ru" и т.д.</summary>
    public string Language { get; set; } = "auto";

    /// <summary>Дополнительные аргументы командной строки (необязательно).</summary>
    public string? AdditionalArguments { get; set; }

    /// <summary>Таймаут выполнения процесса в секундах.</summary>
    public int TimeoutSeconds { get; set; } = 120;
}
