namespace Sreda.Backend.Application.Common;

/// <summary>
/// Контекст виконання команди: ідентифікатор запису в історії Commands та мова відповіді.
/// </summary>
public record CommandExecutionContext(int CommandRecordId, string ResponseLanguage);
