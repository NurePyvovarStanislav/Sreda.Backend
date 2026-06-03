using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.AvailableCommands.Commands;
using Sreda.Backend.Application.Features.AvailableCommands.Queries;
using Sreda.Backend.Contracts.DTOs.Commands;

namespace Sreda.Backend.API.Controllers;

[Route("api/available-commands")]
public class AvailableCommandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AvailableCommandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetAvailableCommandsQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetAvailableCommandByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAvailableCommandRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new CreateAvailableCommandCommand(
            request.Code, request.Name, request.Description, request.Category,
            request.RequiresDevice, request.RequiredAccessLevel, request.IsEnabled), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateAvailableCommandRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new UpdateAvailableCommandCommand(
            id, request.Name, request.Description, request.Category,
            request.RequiresDevice, request.RequiredAccessLevel, request.IsEnabled), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new DeleteAvailableCommandCommand(id), cancellationToken));
}
