namespace Sreda.Backend.Application.Services;

/// <summary>
/// Текстові відповіді конвеєра та виконавця команд за мовою (en / ru / uk).
/// </summary>
public static class CommandResponseLocalizer
{
    public static string NotRecognized(string lang) => lang switch
    {
        Russian => "Команду не распознано.",
        Ukrainian => "Команду не розпізнано.",
        _ => "Command not recognized."
    };

    public static string LowConfidence(string lang) => lang switch
    {
        Russian => "Команду распознано недостаточно уверенно.",
        Ukrainian => "Команду розпізнано недостатньо впевнено.",
        _ => "Command recognized with insufficient confidence."
    };

    public static string AccessDenied(string lang) => lang switch
    {
        Russian => "Недостаточно прав для выполнения команды.",
        Ukrainian => "Недостатньо прав для виконання команди.",
        _ => "Insufficient permissions to run this command."
    };

    public static string NoMatchingDevice(string lang) => lang switch
    {
        Russian => "Нет активного устройства подходящего типа для этой команды.",
        Ukrainian => "Немає активного пристрою відповідного типу для цієї команди.",
        _ => "No active device of the required type for this command."
    };

    public static string NotImplemented(string lang) => lang switch
    {
        Russian => "Команду распознано, но её выполнение ещё не реализовано.",
        Ukrainian => "Команду розпізнано, але її виконання ще не реалізовано.",
        _ => "Command recognized but execution is not implemented yet."
    };

    public static string TurnLight(bool powerOn, string deviceName, string? location, string lang)
    {
        var locationPart = string.IsNullOrWhiteSpace(location) ? "" : $" ({location})";
        return (lang, powerOn) switch
        {
            (Russian, true) => $"Свет «{deviceName}»{locationPart} включён.",
            (Russian, false) => $"Свет «{deviceName}»{locationPart} выключен.",
            (Ukrainian, true) => $"Світло «{deviceName}»{locationPart} увімкнено.",
            (Ukrainian, false) => $"Світло «{deviceName}»{locationPart} вимкнено.",
            (_, true) => $"Light «{deviceName}»{locationPart} turned on.",
            (_, false) => $"Light «{deviceName}»{locationPart} turned off."
        };
    }

    public static string ForCommandCode(string code, DateTimeOffset now, string lang) => code switch
    {
        "GetTime" => lang switch
        {
            Russian => $"Текущее время: {now:HH:mm}.",
            Ukrainian => $"Поточний час: {now:HH:mm}.",
            _ => $"The time is {now:HH:mm}."
        },
        "GetDate" => lang switch
        {
            Russian => $"Сегодня {now:dd.MM.yyyy}.",
            Ukrainian => $"Сьогодні {now:dd.MM.yyyy}.",
            _ => $"Today is {now:dd.MM.yyyy}."
        },
        "Hello" => lang switch
        {
            Russian => "Привет! Я Sreda — локальная система голосового интерфейса.",
            Ukrainian => "Привіт! Я Sreda — локальна система мовного інтерфейсу.",
            _ => "Hello! I am Sreda — a local voice interface system."
        },
        "GetStatus" => lang switch
        {
            Russian => "Система работает нормально. Голосовой конвейер и журнал событий активны.",
            Ukrainian => "Система працює нормально. Голосовий конвейер і журнал подій активні.",
            _ => "System is running normally. Voice pipeline and event log are active."
        },
        "Help" => lang switch
        {
            Russian =>
                "Доступные команды: время, дата, приветствие, статус, браузер, устройства, события, история. " +
                "Скажите фразу из каталога CommandPhrases или используйте POST /api/commands/resolve.",
            Ukrainian =>
                "Доступні команди: час, дата, привітання, статус, браузер, пристрої, події, історія команд. " +
                "Скажіть фразу з каталогу CommandPhrases або використайте POST /api/commands/resolve.",
            _ =>
                "Available commands: time, date, greeting, status, browser, devices, events, history. " +
                "Say a phrase from CommandPhrases or use POST /api/commands/resolve."
        },
        "Cancel" => lang switch
        {
            Russian => "Отменено. Жду следующую команду.",
            Ukrainian => "Скасовано. Очікую наступну команду.",
            _ => "Cancelled. Waiting for the next command."
        },
        "OpenBrowser" => lang switch
        {
            Russian => "Открываю браузер (в прототипе интеграция с ОС будет на следующем этапе).",
            Ukrainian => "Відкриваю браузер (у прототипі — інтеграція з ОС буде на наступному етапі).",
            _ => "Opening browser (OS integration is planned for a later stage)."
        },
        "CloseBrowser" => lang switch
        {
            Russian => "Закрываю браузер (в прототипе интеграция с ОС будет на следующем этапе).",
            Ukrainian => "Закриваю браузер (у прототипі — інтеграція з ОС буде на наступному етапі).",
            _ => "Closing browser (OS integration is planned for a later stage)."
        },
        "OpenSite" => lang switch
        {
            Russian => "Открываю сайт (в прототипе интеграция с ОС будет на следующем этапе).",
            Ukrainian => "Відкриваю сайт (у прототипі — інтеграція з ОС буде на наступному етапі).",
            _ => "Opening website (OS integration is planned for a later stage)."
        },
        "Shutdown" => lang switch
        {
            Russian => "Завершение работы в прототипе недоступно из соображений безопасности.",
            Ukrainian => "Завершення роботи в прототипі недоступне з міркувань безпеки.",
            _ => "Shutdown is disabled in the prototype for security reasons."
        },
        "ShowDevices" => lang switch
        {
            Russian => "Список устройств: GET /api/devices",
            Ukrainian => "Список пристроїв: GET /api/devices",
            _ => "Device list: GET /api/devices"
        },
        "ShowDeviceStates" => lang switch
        {
            Russian => "Состояние устройств: GET /api/devices/{id}/state",
            Ukrainian => "Стан пристроїв: GET /api/devices/{id}/state",
            _ => "Device states: GET /api/devices/{id}/state"
        },
        "ShowEvents" => lang switch
        {
            Russian => "Журнал событий: GET /api/events (realtime: SignalR /hubs/events)",
            Ukrainian => "Журнал подій: GET /api/events (реалтайм: SignalR /hubs/events)",
            _ => "Event log: GET /api/events (realtime: SignalR /hubs/events)"
        },
        "ShowCommandHistory" => lang switch
        {
            Russian => "История команд: GET /api/commands",
            Ukrainian => "Історія команд: GET /api/commands",
            _ => "Command history: GET /api/commands"
        },
        _ => NotImplemented(lang)
    };

    private const string Russian = ResponseLanguageResolver.Russian;
    private const string Ukrainian = ResponseLanguageResolver.Ukrainian;
}
