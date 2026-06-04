using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Вибирає цільовий пристрій для команди, що потребує пристрою (RequiresDevice).
/// У прототипі — перший активний пристрій відповідного типу.
/// </summary>
public interface IDeviceTargetSelector
{
    Task<Device?> SelectAsync(AvailableCommand command, CancellationToken cancellationToken = default);
}
