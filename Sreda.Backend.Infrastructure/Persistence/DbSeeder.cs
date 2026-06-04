using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Services;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;
using Sreda.Backend.Infrastructure.Persistence.Seeding;

namespace Sreda.Backend.Infrastructure.Persistence;

/// <summary>
/// Ідемпотентний сідер: додає команди та фрази з каталогу, якщо їх ще немає в БД.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(SredaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(dbContext, cancellationToken);
        await SeedCommandsAsync(dbContext, cancellationToken);
        await SeedPhrasesAsync(dbContext, cancellationToken);
        await SeedDevicesAsync(dbContext, cancellationToken);
        await SyncDeviceLocalizationsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedUsersAsync(SredaDbContext dbContext, CancellationToken cancellationToken)
    {
        var admin = UserCatalogSeed.DefaultAdmin;
        var exists = await dbContext.Users
            .AnyAsync(u => u.Name == admin.Name, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Name = admin.Name,
            AccessLevel = admin.AccessLevel,
            IsActive = admin.IsActive,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCommandsAsync(SredaDbContext dbContext, CancellationToken cancellationToken)
    {
        var catalog = CommandCatalogSeed.GetAll();
        var existingCodes = await dbContext.AvailableCommands
            .Select(c => c.Code)
            .ToListAsync(cancellationToken);

        var missing = catalog
            .Where(c => !existingCodes.Contains(c.Code))
            .Select(ToEntity)
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await dbContext.AvailableCommands.AddRangeAsync(missing, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPhrasesAsync(SredaDbContext dbContext, CancellationToken cancellationToken)
    {
        var catalog = CommandCatalogSeed.GetAll();
        var commands = await dbContext.AvailableCommands
            .Include(c => c.Phrases)
            .ToListAsync(cancellationToken);

        var added = false;

        foreach (var definition in catalog)
        {
            var command = commands.FirstOrDefault(c => c.Code == definition.Code);
            if (command is null)
            {
                continue;
            }

            foreach (var (language, phrase) in definition.Phrases)
            {
                var exists = command.Phrases.Any(p =>
                    p.Language == language && p.Phrase == phrase);

                if (exists)
                {
                    continue;
                }

                dbContext.CommandPhrases.Add(new CommandPhrase
                {
                    AvailableCommandId = command.Id,
                    Language = language,
                    Phrase = phrase,
                    IsActive = true
                });
                added = true;
            }
        }

        if (added)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedDevicesAsync(SredaDbContext dbContext, CancellationToken cancellationToken)
    {
        var catalog = DeviceCatalogSeed.GetAll();
        var existingCodes = await dbContext.Devices
            .Select(d => d.Code)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var definition in catalog)
        {
            if (existingCodes.Contains(definition.Code))
            {
                continue;
            }

            var device = ToDeviceEntity(definition, now);
            dbContext.Devices.Add(device);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.DeviceStates.Add(new DeviceState
            {
                DeviceId = device.Id,
                IsOnline = true,
                LastSeenAt = now,
                StateJson = definition.InitialStateJson,
                UpdatedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SyncDeviceLocalizationsAsync(
        SredaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var catalog = DeviceCatalogSeed.GetAll();
        var devices = await dbContext.Devices
            .Include(d => d.Localizations)
            .ToListAsync(cancellationToken);

        var changed = false;

        foreach (var definition in catalog)
        {
            var device = devices.FirstOrDefault(d => d.Code == definition.Code);
            if (device is null)
            {
                continue;
            }

            var deviceChanged = false;

            foreach (var seedLabel in definition.Localizations)
            {
                var lang = ResponseLanguageResolver.Normalize(seedLabel.Language);
                var existing = device.Localizations.FirstOrDefault(l =>
                    string.Equals(l.Language, lang, StringComparison.OrdinalIgnoreCase));

                if (existing is null)
                {
                    device.Localizations.Add(new DeviceLocalizedLabel
                    {
                        Language = lang,
                        Name = seedLabel.Name,
                        Location = seedLabel.Location
                    });
                    deviceChanged = true;
                    continue;
                }

                if (existing.Name == seedLabel.Name && existing.Location == seedLabel.Location)
                {
                    continue;
                }

                existing.Name = seedLabel.Name;
                existing.Location = seedLabel.Location;
                deviceChanged = true;
            }

            if (deviceChanged)
            {
                DeviceLocalization.SyncDefaultDisplayFields(device);
                changed = true;
            }
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static Device ToDeviceEntity(SeedDeviceDefinition definition, DateTime createdAt)
    {
        var uk = definition.Localizations.First(l =>
            ResponseLanguageResolver.Normalize(l.Language) == ResponseLanguageResolver.Ukrainian);

        var device = new Device
        {
            Code = definition.Code,
            Name = uk.Name,
            Location = uk.Location,
            Type = definition.Type,
            IsActive = definition.IsActive,
            CreatedAt = createdAt,
            Localizations = definition.Localizations
                .Select(l => new DeviceLocalizedLabel
                {
                    Language = ResponseLanguageResolver.Normalize(l.Language),
                    Name = l.Name,
                    Location = l.Location
                })
                .ToList()
        };

        return device;
    }

    private static AvailableCommand ToEntity(SeedCommandDefinition definition)
    {
        return new AvailableCommand
        {
            Code = definition.Code,
            Name = definition.Name,
            Description = definition.Description,
            Category = definition.Category,
            RequiredAccessLevel = definition.RequiredAccessLevel,
            RequiresDevice = definition.RequiresDevice,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            Phrases = definition.Phrases
                .Select(p => new CommandPhrase
                {
                    Language = p.Language,
                    Phrase = p.Phrase,
                    IsActive = true
                })
                .ToList()
        };
    }
}
