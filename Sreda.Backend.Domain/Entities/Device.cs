using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Физическое устройство / узел экосистемы (ESP32, датчик, свет и т.д.).
/// </summary>
public class Device
{
    public int Id { get; set; }

    /// <summary>Уникальный технический код устройства.</summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DeviceType Type { get; set; }

    /// <summary>Физическое расположение, например "кухня".</summary>
    public string? Location { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Текущее состояние устройства (связь один-к-одному).</summary>
    public DeviceState? State { get; set; }

    /// <summary>События, связанные с устройством.</summary>
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
