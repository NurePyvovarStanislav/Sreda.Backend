using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

public class ProcessVoiceCommandHandler
    : IRequestHandler<ProcessVoiceCommand, Result<CommandProcessResponse>>
{
    private readonly ISpeechToTextService _speechToText;
    private readonly ICommandPipeline _commandPipeline;
    private readonly IEffectiveUserResolver _effectiveUserResolver;

    public ProcessVoiceCommandHandler(
        ISpeechToTextService speechToText,
        ICommandPipeline commandPipeline,
        IEffectiveUserResolver effectiveUserResolver)
    {
        _speechToText = speechToText;
        _commandPipeline = commandPipeline;
        _effectiveUserResolver = effectiveUserResolver;
    }

    public async Task<Result<CommandProcessResponse>> Handle(
        ProcessVoiceCommand request,
        CancellationToken cancellationToken)
    {
        var transcription = await _speechToText.TranscribeAsync(request.Audio, cancellationToken);

        if (string.IsNullOrWhiteSpace(transcription.Text))
        {
            return Result.Failure<CommandProcessResponse>("Не вдалося розпізнати мовлення.");
        }

        var userId = await _effectiveUserResolver.ResolveUserIdAsync(request.UserId, cancellationToken);

        var result = await _commandPipeline.ProcessAsync(
            transcription.Text,
            userId,
            CommandSource.Voice,
            transcription.Language,
            cancellationToken);

        return Result.Success(CommandProcessMapper.ToResponse(result));
    }
}
