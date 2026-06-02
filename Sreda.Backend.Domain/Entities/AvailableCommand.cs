using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Справочник команд, которые система в принципе умеет выполнять.
/// </summary>
public class AvailableCommand
{
    public int Id { get; set; }

    /// <summary>Технический код команды, например "GetTime", "TurnLightOn" (уникальный).</summary>
    public string Code { get; set; } = null!;

    /// <summary>Человекочитаемое имя, например "Включить свет".</summary>
    public string Name { get; set; } = null!;

    /// <summary>Описание того, что делает команда.</summary>
    public string? Description { get; set; }

    public CommandCategory Category { get; set; }

    /// <summary>Требуется ли команде целевое устройство.</summary>
    public bool RequiresDevice { get; set; }

    /// <summary>Базовый уровень доступа, необходимый для использования.</summary>
    public AccessLevel RequiredAccessLevel { get; set; }

    /// <summary>Активна ли команда сейчас.</summary>
    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Голосовые фразы, которые соответствуют этой команде.</summary>
    public ICollection<CommandPhrase> Phrases { get; set; } = new List<CommandPhrase>();

    /// <summary>Персональные переопределения доступа для пользователей.</summary>
    public ICollection<UserCommandOverride> UserOverrides { get; set; } = new List<UserCommandOverride>();

    /// <summary>Записи фактически выполненных команд этого типа.</summary>
    public ICollection<Command> Commands { get; set; } = new List<Command>();
}
