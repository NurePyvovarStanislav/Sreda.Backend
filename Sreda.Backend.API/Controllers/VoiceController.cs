using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;
using Sreda.Backend.Application.Features.Voice.Commands.SpeakText;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.API.Controllers;

[ApiController]
[Route("api/voice")]
public class VoiceController : ControllerBase
{
    private readonly IMediator _mediator;

    public VoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Принимает аудиофайл, распознаёт речь и возвращает текст + ответ системы.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process(IFormFile audio, CancellationToken cancellationToken)
    {
        if (audio is null || audio.Length == 0)
        {
            return BadRequest("Файл 'audio' обязателен.");
        }

        await using var stream = audio.OpenReadStream();
        var result = await _mediator.Send(new ProcessVoiceCommand(stream), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Принимает текст и возвращает озвученный ответ в формате WAV.
    /// </summary>
    [HttpPost("speak")]
    public async Task<IActionResult> Speak([FromBody] SpeakRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SpeakTextCommand(request.Text, request.Language), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return File(result.Value!, "audio/wav");
    }
}
