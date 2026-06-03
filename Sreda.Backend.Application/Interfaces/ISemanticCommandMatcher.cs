using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Результат семантического сопоставления: код команды и уверенность [0..1].
/// </summary>
public record SemanticMatch(string CommandCode, double Confidence);

/// <summary>
/// Семантический (AI/embeddings) матчер как фоллбэк к лексическому сопоставлению.
/// По умолчанию — заглушка (null). Реальную реализацию (например, обращение к
/// локальному Python-сервису с эмбеддингами) можно подключить через DI позже.
/// </summary>
public interface ISemanticCommandMatcher
{
    /// <summary>
    /// Пытается семантически сопоставить текст с одной из доступных команд.
    /// Возвращает null, если матчер недоступен или совпадений нет.
    /// </summary>
    Task<SemanticMatch?> MatchAsync(
        string normalizedText,
        IReadOnlyCollection<AvailableCommand> candidates,
        CancellationToken cancellationToken = default);
}
