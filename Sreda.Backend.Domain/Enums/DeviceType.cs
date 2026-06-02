namespace Sreda.Backend.Domain.Enums;

/// <summary>
/// Тип физического устройства / узла в локальной экосистеме.
/// </summary>
public enum DeviceType
{
    Unknown = 0,
    Light = 1,
    Sensor = 2,
    Switch = 3,
    Lock = 4,
    Camera = 5,
    Thermostat = 6,
    Speaker = 7
}
