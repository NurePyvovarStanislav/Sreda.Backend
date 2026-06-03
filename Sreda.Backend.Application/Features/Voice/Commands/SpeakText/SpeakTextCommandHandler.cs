using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.Voice.Commands.SpeakText;

public class SpeakTextCommandHandler : IRequestHandler<SpeakTextCommand, Result<byte[]>>
{
    private readonly ITextToSpeechService _textToSpeech;

    public SpeakTextCommandHandler(ITextToSpeechService textToSpeech)
    {
        _textToSpeech = textToSpeech;
    }

    public async Task<Result<byte[]>> Handle(
        SpeakTextCommand request,
        CancellationToken cancellationToken)
    {
        var wav = await _textToSpeech.SynthesizeAsync(request.Text, request.Language, cancellationToken);
        return Result.Success(wav);
    }
}
