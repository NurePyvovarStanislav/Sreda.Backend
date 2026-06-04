using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Commands.ProcessText;
using Sreda.Backend.Application.Features.Commands.ResolveCommand;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.API.Controllers;

/// <summary>Обробка текстових команд і діагностика наміру.</summary>
[Route("api/commands")]
public class CommandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CommandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Текст → конвеєр команд → відповідь (без STT/TTS).</summary>
    [HttpPost("process-text")]
    [ProducesResponseType(typeof(CommandProcessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessText(
        [FromBody] ProcessTextRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(
            new ProcessTextCommand(request.Text, request.UserId), cancellationToken));

    /// <summary>Діагностика: намір + confidence без виконання.</summary>
    [HttpPost("resolve")]
    [ProducesResponseType(typeof(CommandResolveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Resolve(
        [FromBody] ResolveTextRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new ResolveCommandQuery(request.Text), cancellationToken));
}
