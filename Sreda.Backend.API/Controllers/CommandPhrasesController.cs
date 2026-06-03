using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sreda.Backend.Application.Features.CommandPhrases.Commands;
using Sreda.Backend.Application.Features.CommandPhrases.Queries;
using Sreda.Backend.Contracts.DTOs.Commands;

namespace Sreda.Backend.API.Controllers;

[Route("api/command-phrases")]
public class CommandPhrasesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CommandPhrasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? availableCommandId, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new GetCommandPhrasesQuery(availableCommandId), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCommandPhraseRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new CreateCommandPhraseCommand(
            request.AvailableCommandId, request.Phrase, request.Language, request.IsActive), cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCommandPhraseRequest request, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new UpdateCommandPhraseCommand(
            id, request.Phrase, request.Language, request.IsActive), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _mediator.Send(new DeleteCommandPhraseCommand(id), cancellationToken));
}
