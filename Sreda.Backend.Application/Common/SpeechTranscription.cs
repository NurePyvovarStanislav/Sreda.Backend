namespace Sreda.Backend.Application.Common;

/// <summary>
/// Результат STT: распознанный текст и язык (ISO 639-1, нормализованный).
/// </summary>
public record SpeechTranscription(string Text, string Language);
