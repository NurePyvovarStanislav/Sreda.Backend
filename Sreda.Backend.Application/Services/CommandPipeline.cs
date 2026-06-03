using System.Text.Json;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Конвейер обработки команды: единый путь для голоса и текста.
/// Сохраняет запрос, разрешает намерение (с уверенностью), проверяет доступ,
/// выполняет команду и пишет наблюдаемость в журнал (Events).
/// </summary>
public class CommandPipeline : ICommandPipeline
{
    private readonly ICommandResolver _resolver;
    private readonly ICommandAccessPolicy _accessPolicy;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISredaDbContext _dbContext;
    private readonly IEventLogger _eventLogger;

    public CommandPipeline(
        ICommandResolver resolver,
        ICommandAccessPolicy accessPolicy,
        ICommandExecutor commandExecutor,
        ISredaDbContext dbContext,
        IEventLogger eventLogger)
    {
        _resolver = resolver;
        _accessPolicy = accessPolicy;
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _eventLogger = eventLogger;
    }

    public async Task<CommandPipelineResult> ProcessAsync(
        string rawText,
        int? userId,
        CommandSource source,
        CancellationToken cancellationToken = default)
    {
        var command = new Command
        {
            UserId = userId,
            RawText = rawText,
            Status = CommandStatus.Pending
        };
        _dbContext.Commands.Add(command);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (source == CommandSource.Voice)
        {
            await _eventLogger.LogAsync(EventType.VoiceRecognized,
                new { text = rawText }, command.Id, cancellationToken: cancellationToken);
        }

        await _eventLogger.LogAsync(EventType.CommandReceived,
            new { text = rawText, source = source.ToString() }, command.Id, cancellationToken: cancellationToken);

        var resolution = await _resolver.ResolveAsync(rawText, cancellationToken);

        string response;

        if (!resolution.Accepted || resolution.Command is null)
        {
            command.Status = CommandStatus.Rejected;
            response = resolution.Command is null
                ? "Команда не распознана."
                : "Команда распознана недостаточно уверенно.";

            await _eventLogger.LogAsync(EventType.CommandRejected, new
            {
                reason = "low_confidence",
                text = rawText,
                confidence = resolution.Confidence,
                matchKind = resolution.MatchKind.ToString(),
                candidate = resolution.Command?.Code,
                matchedPhrase = resolution.MatchedPhrase
            }, command.Id, cancellationToken: cancellationToken);
        }
        else
        {
            command.AvailableCommandId = resolution.Command.Id;

            var access = await _accessPolicy.EvaluateAsync(userId, resolution.Command, cancellationToken);
            if (!access.IsAllowed)
            {
                command.Status = CommandStatus.Rejected;
                response = "Недостаточно прав для выполнения команды.";

                await _eventLogger.LogAsync(EventType.CommandRejected, new
                {
                    reason = "access_denied",
                    detail = access.Reason,
                    code = resolution.Command.Code,
                    confidence = resolution.Confidence
                }, command.Id, cancellationToken: cancellationToken);
            }
            else
            {
                if (resolution.Arguments.Count > 0)
                {
                    command.ArgumentsJson = JsonSerializer.Serialize(resolution.Arguments);
                }

                var execution = await _commandExecutor.ExecuteAsync(
                    resolution.Command, resolution.Arguments, cancellationToken);
                command.Status = execution.Status;
                response = execution.Response;

                await _eventLogger.LogAsync(
                    execution.Success ? EventType.CommandExecuted : EventType.Error,
                    new
                    {
                        code = resolution.Command.Code,
                        success = execution.Success,
                        confidence = resolution.Confidence,
                        matchKind = resolution.MatchKind.ToString(),
                        matchedPhrase = resolution.MatchedPhrase
                    },
                    command.Id, cancellationToken: cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CommandPipelineResult(
            rawText,
            response,
            command.Status,
            resolution.Command?.Code,
            resolution.Confidence);
    }
}
