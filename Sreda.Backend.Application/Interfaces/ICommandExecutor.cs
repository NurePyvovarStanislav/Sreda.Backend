using Sreda.Backend.Application.Common;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Виконує розпізнану команду і формує текстову відповідь (та side-effects у БД для пристроїв).
/// </summary>
public interface ICommandExecutor
{
    Task<CommandExecutionResult> ExecuteAsync(
        AvailableCommand command,
        IReadOnlyDictionary<string, object?> arguments,
        CommandExecutionContext context,
        CancellationToken cancellationToken = default);
}
