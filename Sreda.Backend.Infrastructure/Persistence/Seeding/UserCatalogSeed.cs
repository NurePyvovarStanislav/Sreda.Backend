using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Infrastructure.Persistence.Seeding;

/// <summary>
/// Системний користувач за замовчуванням (адміністратор прототипу).
/// </summary>
public static class UserCatalogSeed
{
    public static SeedUserDefinition DefaultAdmin => new(
        Name: "Admin",
        AccessLevel.Admin,
        IsActive: true);
}

public record SeedUserDefinition(string Name, AccessLevel AccessLevel, bool IsActive);
