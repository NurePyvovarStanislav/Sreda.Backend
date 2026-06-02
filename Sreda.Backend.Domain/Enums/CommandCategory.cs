namespace Sreda.Backend.Domain.Enums;

/// <summary>
/// Категория команды — используется для группировки и политики безопасности.
/// </summary>
public enum CommandCategory
{
    System = 0,
    Information = 1,
    Device = 2,
    Media = 3,
    Security = 4,
    Automation = 5
}
