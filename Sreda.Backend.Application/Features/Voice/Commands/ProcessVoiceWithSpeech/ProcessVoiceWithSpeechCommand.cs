using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Application.Services;
using Sreda.Backend.Contracts.DTOs;
using System.Diagnostics; // замер времени обработки внутри backend до получения WAV
using Microsoft.Extensions.Logging; // too

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
    private readonly ILogger<ProcessVoiceWithSpeechCommandHandler> _logger; // Diagnostic logs
    private readonly ISpeechToTextService _speechToText;
    private readonly ITextToSpeechService _textToSpeech;
    private readonly ICommandPipeline _commandPipeline;
    private readonly IEffectiveUserResolver _effectiveUserResolver;

    public ProcessVoiceWithSpeechCommandHandler(
        ISpeechToTextService speechToText,
        ITextToSpeechService textToSpeech,
        ICommandPipeline commandPipeline,
        IEffectiveUserResolver effectiveUserResolver,
        ILogger<ProcessVoiceWithSpeechCommandHandler> logger)
    {
        _speechToText = speechToText;
        _textToSpeech = textToSpeech;
        _commandPipeline = commandPipeline;
        _effectiveUserResolver = effectiveUserResolver;
        _logger = logger;
    }

    public async Task<Result<CommandProcessWithSpeechResponse>> Handle(
        ProcessVoiceWithSpeechCommand request,
        CancellationToken cancellationToken)
    {
        var totalTimer = Stopwatch.StartNew();
        var stageTimer = Stopwatch.StartNew(); // старт времени

        var transcription = await _speechToText.TranscribeAsync(request.Audio, cancellationToken);

        var sttMs = stageTimer.Elapsed.TotalMilliseconds;

        if (string.IsNullOrWhiteSpace(transcription.Text))
        {
            return Result.Failure<CommandProcessWithSpeechResponse>("Не вдалося розпізнати мовлення.");
        }

        stageTimer.Restart();

        var userId = await _effectiveUserResolver.ResolveUserIdAsync(request.UserId, cancellationToken);

        var pipelineResult = await _commandPipeline.ProcessAsync(
            transcription.Text,
            userId,
            CommandSource.Voice,
            transcription.Language,
            cancellationToken);

        var commandMs = stageTimer.Elapsed.TotalMilliseconds;

        var ttsLanguage = ResponseLanguageResolver.ResolveForTts(
            transcription.Language, request.Language);

        stageTimer.Restart();

        var wav = await _textToSpeech.SynthesizeAsync(
            pipelineResult.Response, ttsLanguage, cancellationToken);

        var ttsMs = stageTimer.Elapsed.TotalMilliseconds;
        totalTimer.Stop();

        _logger.LogInformation(
            "Voice timing: STT={SttMs:F0} ms; Command={CommandMs:F0} ms; " +
            "TTS={TtsMs:F0} ms; Total={TotalMs:F0} ms; Language={Language}; AudioBytes={AudioBytes}",
            sttMs,
            commandMs,
            ttsMs,
            totalTimer.Elapsed.TotalMilliseconds,
            transcription.Language,
            wav.Length);

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
