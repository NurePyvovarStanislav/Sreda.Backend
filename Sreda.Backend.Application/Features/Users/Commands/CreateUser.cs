using FluentValidation;
using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Users;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Users.Commands;

public record CreateUserCommand(string Name, AccessLevel AccessLevel, bool IsActive) : IRequest<Result<UserDto>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.AccessLevel).IsInEnum();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateUserCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.Name,
            AccessLevel = request.AccessLevel,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserDto(user.Id, user.Name, user.IsActive, user.AccessLevel, user.CreatedAt));
    }
}
