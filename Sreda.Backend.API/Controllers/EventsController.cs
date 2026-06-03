using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.History.Queries;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.API.Controllers;

[Route("api/events")]
public class EventsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Журнал системных событий (Event Log).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] EventType? type,
        [FromQuery] int? commandId,
        [FromQuery] int? deviceId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => FromResult(await _mediator.Send(new GetEventsQuery(type, commandId, deviceId, skip, take), cancellationToken));
}
