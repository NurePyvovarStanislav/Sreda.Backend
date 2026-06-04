using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Единый конвейер обработки команды независимо от источника (голос/текст):
/// сохранение запроса, разрешение намерения, проверка доступа, выполнение и логирование.
/// </summary>
public interface ICommandPipeline
{
    Task<CommandPipelineResult> ProcessAsync(
        string rawText,
        int userId,
        CommandSource source,
        string? responseLanguage = null,
        CancellationToken cancellationToken = default);
}
