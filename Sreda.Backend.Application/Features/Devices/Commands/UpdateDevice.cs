using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Features.Devices;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Devices;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.Devices.Commands;

public record UpdateDeviceCommand(
    int Id,
    IReadOnlyList<DeviceLocalizationDto> Localizations,
    DeviceType Type,
    bool IsActive) : IRequest<Result>;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
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

public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateDeviceHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .Include(d => d.Localizations)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (device is null)
        {
            return Result.Failure($"Пристрій з Id {request.Id} не знайдено.");
        }

        var requestedLanguages = request.Localizations
            .Select(l => l.Language.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toRemove = device.Localizations
            .Where(l => !requestedLanguages.Contains(l.Language))
            .ToList();

        foreach (var label in toRemove)
        {
            device.Localizations.Remove(label);
        }

        DeviceMapping.ApplyLocalizations(device, request.Localizations);
        device.Type = request.Type;
        device.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
