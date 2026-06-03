using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Commands;

namespace Sreda.Backend.Application.Features.CommandPhrases.Queries;

/// <summary>Список фраз, опционально отфильтрованный по команде.</summary>
public record GetCommandPhrasesQuery(int? AvailableCommandId = null)
    : IRequest<Result<IReadOnlyList<CommandPhraseDto>>>;

public class GetCommandPhrasesQueryHandler
    : IRequestHandler<GetCommandPhrasesQuery, Result<IReadOnlyList<CommandPhraseDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetCommandPhrasesQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<CommandPhraseDto>>> Handle(
        GetCommandPhrasesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CommandPhrases.AsNoTracking();

        if (request.AvailableCommandId is { } commandId)
        {
            query = query.Where(p => p.AvailableCommandId == commandId);
        }

        var phrases = await query
            .OrderBy(p => p.Id)
            .Select(p => new CommandPhraseDto(p.Id, p.AvailableCommandId, p.Phrase, p.Language, p.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CommandPhraseDto>>(phrases);
    }
}
