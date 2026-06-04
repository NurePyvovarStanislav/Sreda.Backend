namespace Sreda.Backend.Contracts.DTOs;

/// <summary>
/// Уніфікована відповідь конвеєра обробки команди (голос або текст).
/// </summary>
public record CommandProcessResponse(
    int CommandId,
    int UserId,
    string RecognizedText,
    string Message,
    string Status,
    string? CommandCode,
    double Confidence,
    CommandProcessDeviceDto? Device);

/// <summary>
/// Повний голосовий цикл: STT → конвеєр → TTS (один запит, для low-latency клієнтів).
/// </summary>
public record CommandProcessWithSpeechResponse(
    int CommandId,
    int UserId,
    string RecognizedText,
    string Message,
    string Status,
    string? CommandCode,
    double Confidence,
    CommandProcessDeviceDto? Device,
    string AudioBase64,
    string AudioContentType);

/// <summary>Пристрій, стан якого змінено під час виконання команди.</summary>
public record CommandProcessDeviceDto(
    int Id,
    string Code,
    string Name,
    string? Location,
    bool IsOnline,
    string? StateJson);
