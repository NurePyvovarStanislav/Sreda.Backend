using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.CommandPhrases.Commands;

public record UpdateCommandPhraseCommand(
    int Id,
    string Phrase,
    string Language,
    bool IsActive) : IRequest<Result>;

public class UpdateCommandPhraseValidator : AbstractValidator<UpdateCommandPhraseCommand>
{
    public UpdateCommandPhraseValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Phrase).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(8);
    }
}

public class UpdateCommandPhraseHandler : IRequestHandler<UpdateCommandPhraseCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateCommandPhraseHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateCommandPhraseCommand request, CancellationToken cancellationToken)
    {
        var phrase = await _dbContext.CommandPhrases
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (phrase is null)
        {
            return Result.Failure($"Фраза с Id {request.Id} не найдена.");
        }

        phrase.Phrase = request.Phrase;
        phrase.Language = request.Language;
        phrase.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
