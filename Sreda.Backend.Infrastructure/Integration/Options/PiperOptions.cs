namespace Sreda.Backend.Infrastructure.Integration.Options;

/// <summary>
/// Настройки синтеза речи через Piper.
/// </summary>
public class PiperOptions
{
    public const string SectionName = "Piper";

    /// <summary>Путь к исполняемому файлу piper.</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>Язык по умолчанию, если в запросе он не указан.</summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Голоса по языку: код языка ("en", "ru") -> путь к модели (.onnx).
    /// Рядом с каждым .onnx ожидается .onnx.json.
    /// </summary>
    public Dictionary<string, string> Voices { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Дополнительные аргументы командной строки (необязательно).</summary>
    public string? AdditionalArguments { get; set; }

    /// <summary>Таймаут выполнения процесса в секундах.</summary>
    public int TimeoutSeconds { get; set; } = 60;
}
