using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Результат разбора намерения: сопоставленная команда и извлечённые аргументы.
/// </summary>
public record IntentMatch(AvailableCommand Command, IReadOnlyDictionary<string, object?> Arguments);
