using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Вибір основного активного пристрою за типом, що відповідає коду команди.
/// </summary>
public class DeviceTargetSelector : IDeviceTargetSelector
{
    private readonly ISredaDbContext _dbContext;

    public DeviceTargetSelector(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Device?> SelectAsync(AvailableCommand command, CancellationToken cancellationToken = default)
    {
        var deviceType = MapCommandToDeviceType(command.Code);
        if (deviceType is null)
        {
            return null;
        }

        return await _dbContext.Devices
            .Include(d => d.Localizations)
            .Where(d => d.IsActive && d.Type == deviceType)
            .OrderBy(d => d.Code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static DeviceType? MapCommandToDeviceType(string commandCode) =>
        commandCode switch
        {
            "TurnLightOn" or "TurnLightOff" => DeviceType.Light,
            _ => null
        };
}
