using System.Text.Json;
using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

public class ProcessVoiceCommandHandler
    : IRequestHandler<ProcessVoiceCommand, Result<VoiceProcessResponse>>
{
    private readonly ISpeechToTextService _speechToText;
    private readonly IIntentParser _intentParser;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISredaDbContext _dbContext;
    private readonly IEventLogger _eventLogger;

    public ProcessVoiceCommandHandler(
        ISpeechToTextService speechToText,
        IIntentParser intentParser,
        ICommandExecutor commandExecutor,
        ISredaDbContext dbContext,
        IEventLogger eventLogger)
    {
        _speechToText = speechToText;
        _intentParser = intentParser;
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _eventLogger = eventLogger;
    }

    public async Task<Result<VoiceProcessResponse>> Handle(
        ProcessVoiceCommand request,
        CancellationToken cancellationToken)
    {
        var recognizedText = await _speechToText.TranscribeAsync(request.Audio, cancellationToken);

        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            return Result.Failure<VoiceProcessResponse>("Не удалось распознать речь.");
        }

        var command = new Command
        {
            RawText = recognizedText,
            Status = CommandStatus.Pending
        };
        _dbContext.Commands.Add(command);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventLogger.LogAsync(EventType.VoiceRecognized, new { text = recognizedText },
            command.Id, cancellationToken: cancellationToken);
        await _eventLogger.LogAsync(EventType.CommandReceived, new { text = recognizedText },
            command.Id, cancellationToken: cancellationToken);

        string response;
        var match = await _intentParser.ParseAsync(recognizedText, cancellationToken);

        if (match is null)
        {
            command.Status = CommandStatus.Failed;
            response = "Команда не распознана.";
            await _eventLogger.LogAsync(EventType.Error,
                new { reason = "unrecognized", text = recognizedText },
                command.Id, cancellationToken: cancellationToken);
        }
        else
        {
            command.AvailableCommandId = match.Command.Id;
            if (match.Arguments.Count > 0)
            {
                command.ArgumentsJson = JsonSerializer.Serialize(match.Arguments);
            }

            var execution = await _commandExecutor.ExecuteAsync(match.Command, match.Arguments, cancellationToken);
            command.Status = execution.Status;
            response = execution.Response;

            await _eventLogger.LogAsync(
                execution.Success ? EventType.CommandExecuted : EventType.Error,
                new { code = match.Command.Code, success = execution.Success },
                command.Id, cancellationToken: cancellationToken);
        }

        // Фиксируем итоговый статус/аргументы команды (логгер использует тот же контекст).
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new VoiceProcessResponse(recognizedText, response));
    }
}
