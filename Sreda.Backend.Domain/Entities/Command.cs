using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Факт распознанной голосовой команды и результат её обработки.
/// </summary>
public class Command
{
    public int Id { get; set; }

    /// <summary>Пользователь, произнёсший команду (может быть неизвестен).</summary>
    public int? UserId { get; set; }

    /// <summary>Исходный распознанный текст.</summary>
    public string RawText { get; set; } = null!;

    /// <summary>Сопоставленная команда из справочника (если распознана).</summary>
    public int? AvailableCommandId { get; set; }

    /// <summary>Распарсенные аргументы, например {"brightness":80} для "включи лампу на 80 процентов".</summary>
    public string? ArgumentsJson { get; set; }

    public CommandStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }

    public AvailableCommand? AvailableCommand { get; set; }

    /// <summary>События, связанные с обработкой этой команды.</summary>
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
