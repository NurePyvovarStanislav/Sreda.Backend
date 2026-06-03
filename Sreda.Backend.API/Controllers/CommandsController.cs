using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.History.Queries;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.API.Controllers;

[Route("api/commands")]
public class CommandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CommandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>История распознанных команд.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? userId,
        [FromQuery] CommandStatus? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => FromResult(await _mediator.Send(new GetCommandsQuery(userId, status, skip, take), cancellationToken));
}
