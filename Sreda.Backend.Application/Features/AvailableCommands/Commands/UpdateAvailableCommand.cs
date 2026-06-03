using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.AvailableCommands.Commands;

public record UpdateAvailableCommandCommand(
    int Id,
    string Name,
    string? Description,
    CommandCategory Category,
    bool RequiresDevice,
    AccessLevel RequiredAccessLevel,
    bool IsEnabled) : IRequest<Result>;

public class UpdateAvailableCommandValidator : AbstractValidator<UpdateAvailableCommandCommand>
{
    public UpdateAvailableCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.RequiredAccessLevel).IsInEnum();
    }
}

public class UpdateAvailableCommandHandler : IRequestHandler<UpdateAvailableCommandCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateAvailableCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateAvailableCommandCommand request, CancellationToken cancellationToken)
    {
        var command = await _dbContext.AvailableCommands
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (command is null)
        {
            return Result.Failure($"Команда с Id {request.Id} не найдена.");
        }

        command.Name = request.Name;
        command.Description = request.Description;
        command.Category = request.Category;
        command.RequiresDevice = request.RequiresDevice;
        command.RequiredAccessLevel = request.RequiredAccessLevel;
        command.IsEnabled = request.IsEnabled;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
