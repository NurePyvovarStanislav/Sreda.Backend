namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Голосовой профиль пользователя (биометрический эмбеддинг для speaker verification).
/// Связь с пользователем — один-к-одному (уникальный индекс по UserId).
/// </summary>
public class VoiceProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>Путь к файлу с голосовым эмбеддингом.</summary>
    public string EmbeddingPath { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
