namespace Sreda.Backend.Domain.Enums;

/// <summary>
/// Уровень доступа пользователя. Определяет, какие команды он может выполнять
/// и нужна ли дополнительная проверка (голос / второй фактор).
/// </summary>
public enum AccessLevel
{
    Guest = 0,
    User = 1,
    Trusted = 2,
    Admin = 3
}
