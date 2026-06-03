namespace Sreda.Backend.Contracts.DTOs;

/// <summary>
/// Результат обработки голосовой команды: распознанный текст и ответ системы.
/// </summary>
public record VoiceProcessResponse(string RecognizedText, string Response);
