using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

public class ProcessVoiceCommandHandler
    : IRequestHandler<ProcessVoiceCommand, Result<VoiceProcessResponse>>
{
    private readonly ISpeechToTextService _speechToText;

    public ProcessVoiceCommandHandler(ISpeechToTextService speechToText)
    {
        _speechToText = speechToText;
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

        // На Этапе 6 здесь появится разбор намерения и выполнение команды.
        var response = $"Вы сказали: {recognizedText}";

        return Result.Success(new VoiceProcessResponse(recognizedText, response));
    }
}
