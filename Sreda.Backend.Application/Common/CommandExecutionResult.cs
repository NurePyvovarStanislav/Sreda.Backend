using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Результат выполнения команды: успех, текст ответа и итоговый статус.
/// </summary>
public record CommandExecutionResult(bool Success, string Response)
{
    public CommandStatus Status => Success ? CommandStatus.Success : CommandStatus.Failed;
}
