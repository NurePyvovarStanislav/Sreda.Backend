using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Пользователь системы. Голос пользователя — часть модели безопасности.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public AccessLevel AccessLevel { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Голосовой профиль (связь один-к-одному).</summary>
    public VoiceProfile? VoiceProfile { get; set; }

    /// <summary>Персональные разрешения/запреты на команды.</summary>
    public ICollection<UserCommandOverride> CommandOverrides { get; set; } = new List<UserCommandOverride>();

    /// <summary>История распознанных команд пользователя.</summary>
    public ICollection<Command> Commands { get; set; } = new List<Command>();
}
