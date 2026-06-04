using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Common;

public static class CommandProcessMapper
{
    public static CommandProcessResponse ToResponse(CommandPipelineResult result) =>
        new(
            result.CommandId,
            result.UserId,
            result.RawText,
            result.Response,
            result.Status.ToString(),
            result.CommandCode,
            result.Confidence,
            result.AffectedDevice is null
                ? null
                : new CommandProcessDeviceDto(
                    result.AffectedDevice.DeviceId,
                    result.AffectedDevice.Code,
                    result.AffectedDevice.Name,
                    result.AffectedDevice.Location,
                    result.AffectedDevice.IsOnline,
                    result.AffectedDevice.StateJson));
}
