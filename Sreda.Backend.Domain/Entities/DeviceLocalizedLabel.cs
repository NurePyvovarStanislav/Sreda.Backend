namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Локализованное отображаемое имя и расположение устройства (ISO 639-1: uk, ru, en, …).
/// </summary>
public class DeviceLocalizedLabel
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string Language { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public Device Device { get; set; } = null!;
}
