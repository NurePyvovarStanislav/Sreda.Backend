using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Application.Services;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceWithSpeech;

/// <summary>
/// Аудіо → STT → конвеєр команд → TTS (повний голосовий цикл в одному запиті).
/// </summary>
public record ProcessVoiceWithSpeechCommand(
    Stream Audio,
    int? UserId = null,
    string? Language = null) : IRequest<Result<CommandProcessWithSpeechResponse>>;

public class ProcessVoiceWithSpeechCommandHandler
    : IRequestHandler<ProcessVoiceWithSpeechCommand, Result<CommandProcessWithSpeechResponse>>
{
    private readonly ISpeechToTextService _speechToText;
    private readonly ITextToSpeechService _textToSpeech;
    private readonly ICommandPipeline _commandPipeline;
    private readonly IEffectiveUserResolver _effectiveUserResolver;

    public ProcessVoiceWithSpeechCommandHandler(
        ISpeechToTextService speechToText,
        ITextToSpeechService textToSpeech,
        ICommandPipeline commandPipeline,
        IEffectiveUserResolver effectiveUserResolver)
    {
        _speechToText = speechToText;
        _textToSpeech = textToSpeech;
        _commandPipeline = commandPipeline;
        _effectiveUserResolver = effectiveUserResolver;
    }

    public async Task<Result<CommandProcessWithSpeechResponse>> Handle(
        ProcessVoiceWithSpeechCommand request,
        CancellationToken cancellationToken)
    {
        var transcription = await _speechToText.TranscribeAsync(request.Audio, cancellationToken);
        if (string.IsNullOrWhiteSpace(transcription.Text))
        {
            return Result.Failure<CommandProcessWithSpeechResponse>("Не вдалося розпізнати мовлення.");
        }

        var userId = await _effectiveUserResolver.ResolveUserIdAsync(request.UserId, cancellationToken);

        var pipelineResult = await _commandPipeline.ProcessAsync(
            transcription.Text,
            userId,
            CommandSource.Voice,
            transcription.Language,
            cancellationToken);

        var ttsLanguage = ResponseLanguageResolver.ResolveForTts(
            transcription.Language, request.Language);

        var wav = await _textToSpeech.SynthesizeAsync(
            pipelineResult.Response, ttsLanguage, cancellationToken);

        var baseResponse = CommandProcessMapper.ToResponse(pipelineResult);

        return Result.Success(new CommandProcessWithSpeechResponse(
            baseResponse.CommandId,
            baseResponse.UserId,
            baseResponse.RecognizedText,
            baseResponse.Message,
            baseResponse.Status,
            baseResponse.CommandCode,
            baseResponse.Confidence,
            baseResponse.Device,
            Convert.ToBase64String(wav),
            "audio/wav"));
    }
}
