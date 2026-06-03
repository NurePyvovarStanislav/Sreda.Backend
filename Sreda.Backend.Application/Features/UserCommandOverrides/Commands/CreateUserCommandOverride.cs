using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Overrides;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Features.UserCommandOverrides.Commands;

public record CreateUserCommandOverrideCommand(
    int UserId,
    int AvailableCommandId,
    bool IsAllowed) : IRequest<Result<UserCommandOverrideDto>>;

public class CreateUserCommandOverrideValidator : AbstractValidator<CreateUserCommandOverrideCommand>
{
    public CreateUserCommandOverrideValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AvailableCommandId).GreaterThan(0);
    }
}

public class CreateUserCommandOverrideHandler
    : IRequestHandler<CreateUserCommandOverrideCommand, Result<UserCommandOverrideDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateUserCommandOverrideHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserCommandOverrideDto>> Handle(
        CreateUserCommandOverrideCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<UserCommandOverrideDto>($"Пользователь с Id {request.UserId} не найден.");
        }

        var commandExists = await _dbContext.AvailableCommands
            .AnyAsync(c => c.Id == request.AvailableCommandId, cancellationToken);
        if (!commandExists)
        {
            return Result.Failure<UserCommandOverrideDto>(
                $"Команда с Id {request.AvailableCommandId} не найдена.");
        }

        var duplicate = await _dbContext.UserCommandOverrides.AnyAsync(
            o => o.UserId == request.UserId && o.AvailableCommandId == request.AvailableCommandId,
            cancellationToken);
        if (duplicate)
        {
            return Result.Failure<UserCommandOverrideDto>(
                "Переопределение для этой пары пользователь/команда уже существует.");
        }

        var entity = new UserCommandOverride
        {
            UserId = request.UserId,
            AvailableCommandId = request.AvailableCommandId,
            IsAllowed = request.IsAllowed,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserCommandOverrides.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserCommandOverrideDto(
            entity.Id, entity.UserId, entity.AvailableCommandId, entity.IsAllowed, entity.CreatedAt));
    }
}
