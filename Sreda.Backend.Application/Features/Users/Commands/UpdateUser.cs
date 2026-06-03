using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Users.Commands;

public record UpdateUserCommand(int Id, string Name, AccessLevel AccessLevel, bool IsActive) : IRequest<Result>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.AccessLevel).IsInEnum();
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateUserCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure($"Пользователь с Id {request.Id} не найден.");
        }

        user.Name = request.Name;
        user.AccessLevel = request.AccessLevel;
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
