using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.Users.Commands;
using Sreda.Backend.Application.Features.Users.Queries;
using Sreda.Backend.Contracts.DTOs.Users;

namespace Sreda.Backend.API.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetUsersQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetUserByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(
            new CreateUserCommand(request.Name, request.AccessLevel, request.IsActive), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(
            new UpdateUserCommand(id, request.Name, request.AccessLevel, request.IsActive), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new DeleteUserCommand(id), cancellationToken));
}
