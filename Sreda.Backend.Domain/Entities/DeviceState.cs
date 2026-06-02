namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Текущее (актуальное) состояние устройства.
/// Связь с устройством — один-к-одному (уникальный индекс по DeviceId).
/// </summary>
public class DeviceState
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public bool IsOnline { get; set; }

    /// <summary>Когда устройство в последний раз выходило на связь.</summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>Переменные свойства устройства в формате JSON (температура, яркость и т.д.).</summary>
    public string? StateJson { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Device Device { get; set; } = null!;
}
