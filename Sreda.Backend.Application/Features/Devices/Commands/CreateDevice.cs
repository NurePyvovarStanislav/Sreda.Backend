using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Features.Devices;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Devices.Commands;

public record CreateDeviceCommand(
    string Code,
    IReadOnlyList<DeviceLocalizationDto> Localizations,
    DeviceType Type,
    bool IsActive) : IRequest<Result<DeviceDto>>;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Localizations).NotEmpty();
        RuleForEach(x => x.Localizations).ChildRules(l =>
        {
            l.RuleFor(x => x.Language).NotEmpty().MaximumLength(8);
            l.RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            l.RuleFor(x => x.Location).MaximumLength(256);
        });
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, Result<DeviceDto>>
{
    private readonly ISredaDbContext _dbContext;

    public CreateDeviceHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeviceDto>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _dbContext.Devices.AnyAsync(d => d.Code == request.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<DeviceDto>($"Пристрій з кодом '{request.Code}' вже існує.");
        }

        var device = new Device
        {
            Code = request.Code,
            Type = request.Type,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            Name = request.Localizations[0].Name
        };
        DeviceMapping.ApplyLocalizations(device, request.Localizations);

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(DeviceMapping.ToDto(device));
    }
}
