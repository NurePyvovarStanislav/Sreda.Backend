using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.CommandPhrases.Commands;

public record DeleteCommandPhraseCommand(int Id) : IRequest<Result>;

public class DeleteCommandPhraseHandler : IRequestHandler<DeleteCommandPhraseCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public DeleteCommandPhraseHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteCommandPhraseCommand request, CancellationToken cancellationToken)
    {
        var phrase = await _dbContext.CommandPhrases
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (phrase is null)
        {
            return Result.Failure($"Фразу з Id {request.Id} не знайдено.");
        }

        _dbContext.CommandPhrases.Remove(phrase);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
