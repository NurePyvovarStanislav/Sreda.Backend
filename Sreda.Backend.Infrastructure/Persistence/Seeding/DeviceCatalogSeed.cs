using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Infrastructure.Persistence.Seeding;

/// <summary>
/// Демонстраційні віртуальні пристрої для курсового прототипу.
/// </summary>
public static class DeviceCatalogSeed
{
    public static IReadOnlyList<SeedDeviceDefinition> GetAll() =>
    [
        new(
            Code: "Light-1",
            Type: DeviceType.Light,
            IsActive: true,
            InitialStateJson: """{"power":false,"brightness":100}""",
            Localizations:
            [
                new("uk", "Основне світло", "Кімната"),
                new("ru", "Основной свет", "Комната"),
                new("en", "Main light", "Room")
            ])
    ];
}

public record SeedDeviceDefinition(
    string Code,
    DeviceType Type,
    bool IsActive,
    string InitialStateJson,
    IReadOnlyList<SeedDeviceLocalization> Localizations);

public record SeedDeviceLocalization(string Language, string Name, string? Location);
