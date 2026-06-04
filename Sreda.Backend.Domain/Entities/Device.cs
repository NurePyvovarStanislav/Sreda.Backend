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

    /// <summary>Отображаемое имя по умолчанию (украинская локаль из <see cref="Localizations"/>).</summary>
    public string Name { get; set; } = null!;

    public DeviceType Type { get; set; }

    /// <summary>Расположение по умолчанию (украинская локаль).</summary>
    public string? Location { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DeviceState? State { get; set; }

    public ICollection<DeviceLocalizedLabel> Localizations { get; set; } = new List<DeviceLocalizedLabel>();

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
