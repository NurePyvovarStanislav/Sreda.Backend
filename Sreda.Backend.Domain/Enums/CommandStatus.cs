namespace Sreda.Backend.Domain.Enums;

/// <summary>
/// Итоговый статус обработки распознанной команды.
/// </summary>
public enum CommandStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Rejected = 3
}
