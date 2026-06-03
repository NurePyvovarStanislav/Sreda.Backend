using Sreda.Backend.Application.Common;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Выполняет распознанную команду и формирует текстовый ответ системы.
/// </summary>
public interface ICommandExecutor
{
    Task<CommandExecutionResult> ExecuteAsync(
        AvailableCommand command,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken = default);
}
