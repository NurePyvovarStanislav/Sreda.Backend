namespace Sreda.Backend.Contracts.DTOs;

/// <summary>
/// Застарілий alias — використовуйте <see cref="CommandProcessResponse"/>.
/// </summary>
[Obsolete("Використовуйте CommandProcessResponse.")]
public record VoiceProcessResponse(
    string RecognizedText,
    string Response,
    string Status,
    string? CommandCode,
    double Confidence);
