using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

public class ProcessVoiceCommandHandler
    : IRequestHandler<ProcessVoiceCommand, Result<VoiceProcessResponse>>
{
    private readonly ISpeechToTextService _speechToText;
    private readonly ICommandPipeline _commandPipeline;

    public ProcessVoiceCommandHandler(
        ISpeechToTextService speechToText,
        ICommandPipeline commandPipeline)
    {
        _speechToText = speechToText;
        _commandPipeline = commandPipeline;
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

        var result = await _commandPipeline.ProcessAsync(
            recognizedText, request.UserId, CommandSource.Voice, cancellationToken);

        return Result.Success(new VoiceProcessResponse(
            result.RawText,
            result.Response,
            result.Status.ToString(),
            result.CommandCode,
            result.Confidence));
    }
}
