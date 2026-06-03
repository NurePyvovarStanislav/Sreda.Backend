using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Сопоставляет распознанный текст с командой системы и оценивает уверенность.
/// Заменяет простой парсер намерений: учитывает точное/частичное/нечёткое совпадение
/// и опциональный семантический фоллбэк.
/// </summary>
public interface ICommandResolver
{
    Task<CommandResolution> ResolveAsync(string rawText, CancellationToken cancellationToken = default);
}
