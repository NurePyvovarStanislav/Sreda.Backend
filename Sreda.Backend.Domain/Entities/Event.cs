using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Системное событие журнала (Event Log): команды, обновления устройств, ошибки и т.д.
/// </summary>
public class Event
{
    public int Id { get; set; }

    /// <summary>Связанная команда (если событие порождено командой).</summary>
    public int? CommandId { get; set; }

    /// <summary>Связанное устройство (если событие относится к устройству).</summary>
    public int? DeviceId { get; set; }

    public EventType Type { get; set; }

    /// <summary>Детали события в формате JSON.</summary>
    public string? PayloadJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public Command? Command { get; set; }

    public Device? Device { get; set; }
}
