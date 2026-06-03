namespace Sreda.Backend.Contracts.DTOs;

/// <summary>
/// Запрос на озвучивание текста. Language (например, "en", "ru") выбирает голос;
/// если не указан — используется голос по умолчанию.
/// </summary>
public record SpeakRequest(string Text, string? Language = null);
