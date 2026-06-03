using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Базовый исполнитель команд: формирует текстовый ответ по коду команды.
/// Реальное управление устройствами (свет и т.п.) появится на Этапе 9.
/// </summary>
public class BasicCommandExecutor : ICommandExecutor
{
    private readonly TimeProvider _timeProvider;

    public BasicCommandExecutor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<CommandExecutionResult> ExecuteAsync(
        AvailableCommand command,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetLocalNow();

        var result = command.Code switch
        {
            "GetTime" => Ok($"Текущее время: {now:HH:mm}."),
            "GetDate" => Ok($"Сегодня {now:dd.MM.yyyy}."),
            "Hello" => Ok("Привет! Я Среда."),
            "GetStatus" => Ok("Система работает нормально."),
            "OpenSite" => Ok("Открываю сайт."),
            "Shutdown" => Ok("Завершение работы пока недоступно в прототипе."),
            "TurnLightOn" => Ok("Свет включён."),
            "TurnLightOff" => Ok("Свет выключен."),
            _ => Fail("Команда распознана, но её выполнение пока не реализовано.")
        };

        return Task.FromResult(result);
    }

    private static CommandExecutionResult Ok(string response) => new(true, response);

    private static CommandExecutionResult Fail(string response) => new(false, response);
}
