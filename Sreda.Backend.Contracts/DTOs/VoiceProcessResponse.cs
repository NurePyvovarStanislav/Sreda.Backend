namespace Sreda.Backend.Contracts.DTOs;

/// <summary>
/// Результат обработки команды: исходный текст, ответ системы, итоговый статус,
/// сопоставленная команда и уверенность распознавания намерения.
/// </summary>
public record VoiceProcessResponse(
    string RecognizedText,
    string Response,
    string Status,
    string? CommandCode,
    double Confidence);
