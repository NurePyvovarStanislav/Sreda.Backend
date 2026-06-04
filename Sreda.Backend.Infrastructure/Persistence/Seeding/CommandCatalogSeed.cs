using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Infrastructure.Persistence.Seeding;

/// <summary>
/// Каталог команд і голосових фраз для демонстрації мовного інтерфейсу (Етап 11).
/// Фрази — російською (ru) та англійською (en); нормалізатор зводить їх до одного формату при зіставленні.
/// </summary>
public static class CommandCatalogSeed
{
    public static IReadOnlyList<SeedCommandDefinition> GetAll() =>
    [
        // --- Інформаційні ---
        Build("GetTime", "Дізнатися час", "Повідомляє поточний час.",
            CommandCategory.Information, AccessLevel.Guest, false,
            ("ru", "который час"), ("ru", "сколько времени"), ("ru", "скажи время"), ("ru", "текущее время"),
            ("ru", "котра година"), ("ru", "сколько сейчас времени"),
            ("en", "what time is it"), ("en", "tell me the time"), ("en", "current time"), ("en", "what is the time")),

        Build("GetDate", "Дізнатися дату", "Повідомляє поточну дату.",
            CommandCategory.Information, AccessLevel.Guest, false,
            ("ru", "какая сегодня дата"), ("ru", "какое сегодня число"), ("ru", "скажи дату"), ("ru", "какое число"),
            ("en", "what is the date"), ("en", "what date is it"), ("en", "tell me the date"), ("en", "today's date")),

        Build("GetStatus", "Статус системи", "Повідомляє стан системи Sreda.",
            CommandCategory.Information, AccessLevel.User, false,
            ("ru", "покажи статус"), ("ru", "как дела"), ("ru", "статус системы"), ("ru", "как работает система"),
            ("en", "show status"), ("en", "system status"), ("en", "how are you"), ("en", "are you working")),

        // --- Системні / демо ---
        Build("Hello", "Привітання", "Вітає користувача.",
            CommandCategory.System, AccessLevel.Guest, false,
            ("ru", "привет"), ("ru", "привет среда"), ("ru", "привет сер"), ("ru", "здравствуй среда"), ("ru", "здравствуй"),
            ("en", "hello"), ("en", "hello sreda"), ("en", "hello ser"), ("en", "hi sreda"), ("en", "hi")),

        Build("Help", "Довідка", "Перелік доступних можливостей і команд.",
            CommandCategory.System, AccessLevel.Guest, false,
            ("ru", "помощь"), ("ru", "справка"), ("ru", "что ты умеешь"), ("ru", "какие команды"), ("ru", "список команд"),
            ("en", "help"), ("en", "what can you do"), ("en", "list commands"), ("en", "available commands")),

        Build("Cancel", "Скасування", "Скасовує поточну дію або очікування.",
            CommandCategory.System, AccessLevel.Guest, false,
            ("ru", "отмена"), ("ru", "отмени"), ("ru", "стоп"), ("ru", "хватит"), ("ru", "отменить"),
            ("en", "cancel"), ("en", "stop"), ("en", "never mind"), ("en", "abort")),

        Build("OpenBrowser", "Відкрити браузер", "Відкриває веб-браузер (прототип).",
            CommandCategory.System, AccessLevel.User, false,
            ("ru", "открой браузер"), ("ru", "открыть браузер"), ("ru", "запусти браузер"), ("ru", "открой веб"),
            ("en", "open browser"), ("en", "open the browser"), ("en", "launch browser"), ("en", "start browser")),

        Build("CloseBrowser", "Закрити браузер", "Закриває веб-браузер (прототип).",
            CommandCategory.System, AccessLevel.User, false,
            ("ru", "закрой браузер"), ("ru", "закрыть браузер"), ("ru", "закрой веб"),
            ("en", "close browser"), ("en", "close the browser"), ("en", "shut down browser")),

        Build("OpenSite", "Відкрити сайт", "Відкриває вказаний сайт у браузері.",
            CommandCategory.System, AccessLevel.User, false,
            ("ru", "открой сайт"), ("ru", "открыть сайт"), ("ru", "зайди на сайт"),
            ("en", "open site"), ("en", "open website"), ("en", "go to website")),

        Build("Shutdown", "Завершення роботи", "Завершує роботу системи (обмежено в прототипі).",
            CommandCategory.System, AccessLevel.Admin, false,
            ("ru", "выключись"), ("ru", "выключи систему"), ("ru", "заверши работу"), ("ru", "выключение"),
            ("en", "shutdown"), ("en", "shut down"), ("en", "power off system")),

        // --- Перегляд даних (ядро API для демонстрації) ---
        Build("ShowDevices", "Список пристроїв", "Показує зареєстровані пристрої.",
            CommandCategory.Information, AccessLevel.User, false,
            ("ru", "покажи устройства"), ("ru", "список устройств"), ("ru", "какие устройства"), ("ru", "все устройства"),
            ("en", "show devices"), ("en", "list devices"), ("en", "what devices"), ("en", "all devices")),

        Build("ShowDeviceStates", "Стан пристроїв", "Показує онлайн-стан і телеметрію пристроїв.",
            CommandCategory.Information, AccessLevel.User, false,
            ("ru", "покажи состояние устройств"), ("ru", "состояние устройств"), ("ru", "статус устройств"),
            ("en", "show device states"), ("en", "device states"), ("en", "devices status")),

        Build("ShowEvents", "Журнал подій", "Показує останні події системи.",
            CommandCategory.Information, AccessLevel.User, false,
            ("ru", "покажи события"), ("ru", "журнал событий"), ("ru", "последние события"), ("ru", "что происходило"),
            ("en", "show events"), ("en", "event log"), ("en", "recent events"), ("en", "what happened")),

        Build("ShowCommandHistory", "Історія команд", "Показує історію розпізнаних команд.",
            CommandCategory.Information, AccessLevel.User, false,
            ("ru", "история команд"), ("ru", "покажи команды"), ("ru", "последние команды"), ("ru", "что я говорил"),
            ("en", "command history"), ("en", "show commands"), ("en", "recent commands"), ("en", "my commands")),

        // --- Пристрої (опційно, для розширення теми) ---
        Build("TurnLightOn", "Увімкнути світло", "Вмикає освітлення на пристрої.",
            CommandCategory.Device, AccessLevel.User, true,
            ("ru", "включи свет"), ("ru", "включи освещение"), ("ru", "зажги свет"),
            ("en", "turn on the light"), ("en", "turn on light"), ("en", "lights on")),

        Build("TurnLightOff", "Вимкнути світло", "Вимикає освітлення на пристрої.",
            CommandCategory.Device, AccessLevel.User, true,
            ("ru", "выключи свет"), ("ru", "выключи освещение"), ("ru", "погаси свет"),
            ("en", "turn off the light"), ("en", "turn off light"), ("en", "lights off"))
    ];

    private static SeedCommandDefinition Build(
        string code,
        string name,
        string description,
        CommandCategory category,
        AccessLevel requiredAccessLevel,
        bool requiresDevice,
        params (string Language, string Phrase)[] phrases)
        => new(code, name, description, category, requiredAccessLevel, requiresDevice, phrases);
}

/// <summary>Опис однієї команди каталогу для сідера.</summary>
public record SeedCommandDefinition(
    string Code,
    string Name,
    string Description,
    CommandCategory Category,
    AccessLevel RequiredAccessLevel,
    bool RequiresDevice,
    (string Language, string Phrase)[] Phrases);
