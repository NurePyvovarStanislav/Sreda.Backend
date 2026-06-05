using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Commands.ProcessText;
using Sreda.Backend.Application.Features.Commands.ResolveCommand;
using Sreda.Backend.Application.Features.History.Queries;
using Sreda.Backend.Contracts.DTOs;
using Sreda.Backend.Contracts.DTOs.History;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.API.Controllers;

/// <summary>Журнал команд, обробка тексту та діагностика наміру.</summary>
[Route("api/commands")]
public class CommandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CommandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Історія розпізнаних команд (новіші спочатку).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CommandHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? userId,
        [FromQuery] CommandStatus? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => FromResult(await _mediator.Send(
            new GetCommandsQuery(userId, status, skip, take), cancellationToken));

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
