using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Заглушка семантического матчера: всегда возвращает null.
/// Используется по умолчанию, пока не подключён реальный AI/embeddings-провайдер.
/// </summary>
public class NullSemanticCommandMatcher : ISemanticCommandMatcher
{
    public Task<SemanticMatch?> MatchAsync(
        string normalizedText,
        IReadOnlyCollection<AvailableCommand> candidates,
        CancellationToken cancellationToken = default)
        => Task.FromResult<SemanticMatch?>(null);
}
