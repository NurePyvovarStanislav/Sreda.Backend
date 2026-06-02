using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Infrastructure.Persistence;

/// <summary>
/// Идемпотентный сидер: добавляет базовый набор команд и их голосовых фраз,
/// если их ещё нет в базе. Безопасно вызывать при каждом старте.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(SredaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedAvailableCommandsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedAvailableCommandsAsync(SredaDbContext dbContext, CancellationToken cancellationToken)
    {
        var seedCommands = GetSeedCommands();

        var existingCodes = await dbContext.AvailableCommands
            .Select(c => c.Code)
            .ToListAsync(cancellationToken);

        var missing = seedCommands
            .Where(c => !existingCodes.Contains(c.Code))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await dbContext.AvailableCommands.AddRangeAsync(missing, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<AvailableCommand> GetSeedCommands()
    {
        return
        [
            Build("GetTime", "Узнать время", "Сообщает текущее время.",
                CommandCategory.Information, AccessLevel.Guest, requiresDevice: false,
                ("ru", "который час"), ("ru", "сколько времени"), ("en", "what time is it")),

            Build("GetDate", "Узнать дату", "Сообщает текущую дату.",
                CommandCategory.Information, AccessLevel.Guest, requiresDevice: false,
                ("ru", "какая сегодня дата"), ("ru", "какое сегодня число"), ("en", "what is the date")),

            Build("Hello", "Приветствие", "Отвечает приветствием.",
                CommandCategory.System, AccessLevel.Guest, requiresDevice: false,
                ("ru", "привет"), ("ru", "привет среда"), ("en", "hello")),

            Build("GetStatus", "Показать статус", "Сообщает текущее состояние системы.",
                CommandCategory.Information, AccessLevel.User, requiresDevice: false,
                ("ru", "покажи статус"), ("ru", "как дела"), ("en", "show status")),

            Build("OpenSite", "Открыть сайт", "Открывает указанный сайт в браузере.",
                CommandCategory.System, AccessLevel.User, requiresDevice: false,
                ("ru", "открой сайт"), ("en", "open site")),

            Build("Shutdown", "Выключение", "Завершает работу системы.",
                CommandCategory.System, AccessLevel.Admin, requiresDevice: false,
                ("ru", "выключись"), ("ru", "выключи систему"), ("en", "shutdown")),

            Build("TurnLightOn", "Включить свет", "Включает освещение на устройстве.",
                CommandCategory.Device, AccessLevel.User, requiresDevice: true,
                ("ru", "включи свет"), ("en", "turn on the light")),

            Build("TurnLightOff", "Выключить свет", "Выключает освещение на устройстве.",
                CommandCategory.Device, AccessLevel.User, requiresDevice: true,
                ("ru", "выключи свет"), ("en", "turn off the light"))
        ];
    }

    private static AvailableCommand Build(
        string code,
        string name,
        string description,
        CommandCategory category,
        AccessLevel requiredAccessLevel,
        bool requiresDevice,
        params (string Language, string Phrase)[] phrases)
    {
        return new AvailableCommand
        {
            Code = code,
            Name = name,
            Description = description,
            Category = category,
            RequiredAccessLevel = requiredAccessLevel,
            RequiresDevice = requiresDevice,
            IsEnabled = true,
            Phrases = phrases
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
