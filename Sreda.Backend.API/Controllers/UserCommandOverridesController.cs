using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.UserCommandOverrides.Commands;
using Sreda.Backend.Application.Features.UserCommandOverrides.Queries;
using Sreda.Backend.Contracts.DTOs.Overrides;

namespace Sreda.Backend.API.Controllers;

[Route("api/user-command-overrides")]
public class UserCommandOverridesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UserCommandOverridesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetUserCommandOverridesQuery(userId), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommandOverrideRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new CreateUserCommandOverrideCommand(
            request.UserId, request.AvailableCommandId, request.IsAllowed), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateUserCommandOverrideRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new UpdateUserCommandOverrideCommand(id, request.IsAllowed), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new DeleteUserCommandOverrideCommand(id), cancellationToken));
}
