using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Commands.ProcessText;
using Sreda.Backend.Application.Features.Commands.ResolveCommand;
using Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;
using Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceWithSpeech;
using Sreda.Backend.Application.Features.Voice.Commands.SpeakText;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.API.Controllers;

/// <summary>Голосовий інтерфейс: STT і TTS на краях, логіка команд у конвеєрі.</summary>
[ApiController]
[Route("api/voice")]
[Produces("application/json")]
public class VoiceController : ControllerBase
{
    private readonly IMediator _mediator;

    public VoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Аудіо → STT → конвеєр → JSON (без TTS).</summary>
    [HttpPost("process")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CommandProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process(
        IFormFile audio,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        if (audio is null || audio.Length == 0)
        {
            return BadRequest("Файл 'audio' обов'язковий.");
        }

        await using var stream = audio.OpenReadStream();
        var result = await _mediator.Send(new ProcessVoiceCommand(stream, userId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Аудіо → STT (Whisper + auto language) → конвеєр → TTS → JSON з audioBase64.
    /// Мова відповіді та озвучки — з Whisper; query <c>language</c> лише перевизначає голос Piper.
    /// </summary>
    [HttpPost("process-with-speech")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CommandProcessWithSpeechResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessWithSpeech(
        IFormFile audio,
        [FromQuery] int? userId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        if (audio is null || audio.Length == 0)
        {
            return BadRequest("Файл 'audio' обов'язковий.");
        }

        await using var stream = audio.OpenReadStream();
        var result = await _mediator.Send(
            new ProcessVoiceWithSpeechCommand(stream, userId, language), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>Текст → TTS → audio/wav.</summary>
    [HttpPost("speak")]
    [Produces("audio/wav")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Speak(
        [FromBody] SpeakRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SpeakTextCommand(request.Text, request.Language), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return File(result.Value!, "audio/wav");
    }

    /// <summary>Застарілий шлях (залишено для сумісності). Краще: /api/commands/process-text.</summary>
    [HttpPost("process-text")]
    [ProducesResponseType(typeof(CommandProcessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessTextLegacy(
        [FromBody] ProcessTextRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProcessTextCommand(request.Text, request.UserId), cancellationToken);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }

    /// <summary>Застарілий шлях (залишено для сумісності). Краще: /api/commands/resolve.</summary>
    [HttpPost("resolve")]
    [ProducesResponseType(typeof(CommandResolveResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveLegacy(
        [FromBody] ResolveTextRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResolveCommandQuery(request.Text), cancellationToken);
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
}
