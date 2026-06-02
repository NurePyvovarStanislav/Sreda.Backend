namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Персональное переопределение доступа пользователя к конкретной команде.
/// На пару (UserId, AvailableCommandId) допускается только одна запись.
/// </summary>
public class UserCommandOverride
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AvailableCommandId { get; set; }

    /// <summary>Разрешено (true) или явно запрещено (false) пользователю.</summary>
    public bool IsAllowed { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public AvailableCommand AvailableCommand { get; set; } = null!;
}
