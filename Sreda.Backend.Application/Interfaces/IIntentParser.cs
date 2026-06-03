using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Разбор намерения: сопоставляет распознанный текст с доступной командой.
/// </summary>
public interface IIntentParser
{
    Task<IntentMatch?> ParseAsync(string text, CancellationToken cancellationToken = default);
}
