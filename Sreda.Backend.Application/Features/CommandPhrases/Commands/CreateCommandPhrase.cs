using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Commands;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Features.CommandPhrases.Commands;

public record CreateCommandPhraseCommand(
    int AvailableCommandId,
    string Phrase,
    string Language,
    bool IsActive) : IRequest<Result<CommandPhraseDto>>;

public class CreateCommandPhraseValidator : AbstractValidator<CreateCommandPhraseCommand>
{
    public CreateCommandPhraseValidator()
    {
        RuleFor(x => x.AvailableCommandId).GreaterThan(0);
        RuleFor(x => x.Phrase).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(8);
    }
}

public class CreateCommandPhraseHandler : IRequestHandler<CreateCommandPhraseCommand, Result<CommandPhraseDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateCommandPhraseHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CommandPhraseDto>> Handle(
        CreateCommandPhraseCommand request, CancellationToken cancellationToken)
    {
        var commandExists = await _dbContext.AvailableCommands
            .AnyAsync(c => c.Id == request.AvailableCommandId, cancellationToken);
        if (!commandExists)
        {
            return Result.Failure<CommandPhraseDto>(
                $"Команда с Id {request.AvailableCommandId} не найдена.");
        }

        var phrase = new CommandPhrase
        {
            AvailableCommandId = request.AvailableCommandId,
            Phrase = request.Phrase,
            Language = request.Language,
            IsActive = request.IsActive
        };

        _dbContext.CommandPhrases.Add(phrase);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CommandPhraseDto(
            phrase.Id, phrase.AvailableCommandId, phrase.Phrase, phrase.Language, phrase.IsActive));
    }
}
