using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Результат виконання команди: успіх, текст відповіді, статус і опційно змінений пристрій.
/// </summary>
public record CommandExecutionResult(
    bool Success,
    string Response,
    AffectedDeviceSnapshot? AffectedDevice = null)
{
    public CommandStatus Status => Success ? CommandStatus.Success : CommandStatus.Failed;
}
