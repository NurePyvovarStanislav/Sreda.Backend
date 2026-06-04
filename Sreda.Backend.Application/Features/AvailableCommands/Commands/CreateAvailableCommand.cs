using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Commands;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.AvailableCommands.Commands;

public record CreateAvailableCommandCommand(
    string Code,
    string Name,
    string? Description,
    CommandCategory Category,
    bool RequiresDevice,
    AccessLevel RequiredAccessLevel,
    bool IsEnabled) : IRequest<Result<AvailableCommandDto>>;

public class CreateAvailableCommandValidator : AbstractValidator<CreateAvailableCommandCommand>
{
    public CreateAvailableCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.RequiredAccessLevel).IsInEnum();
    }
}

public class CreateAvailableCommandHandler
    : IRequestHandler<CreateAvailableCommandCommand, Result<AvailableCommandDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateAvailableCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AvailableCommandDto>> Handle(
        CreateAvailableCommandCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _dbContext.AvailableCommands
            .AnyAsync(c => c.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<AvailableCommandDto>($"Команда з кодом '{request.Code}' вже існує.");
        }

        var command = new AvailableCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            RequiresDevice = request.RequiresDevice,
            RequiredAccessLevel = request.RequiredAccessLevel,
            IsEnabled = request.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AvailableCommands.Add(command);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AvailableCommandDto(
            command.Id, command.Code, command.Name, command.Description, command.Category,
            command.RequiresDevice, command.RequiredAccessLevel, command.IsEnabled, command.CreatedAt));
    }
}
