using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Виконавець команд: інформаційні відповіді та керування віртуальними пристроями
/// (оновлення DeviceState у БД).
/// </summary>
public class CommandExecutor : ICommandExecutor
{
    private readonly TimeProvider _timeProvider;
    private readonly ISredaDbContext _dbContext;
    private readonly IDeviceTargetSelector _deviceTargetSelector;

    public CommandExecutor(
        TimeProvider timeProvider,
        ISredaDbContext dbContext,
        IDeviceTargetSelector deviceTargetSelector)
    {
        _timeProvider = timeProvider;
        _dbContext = dbContext;
        _deviceTargetSelector = deviceTargetSelector;
    }

    public async Task<CommandExecutionResult> ExecuteAsync(
        AvailableCommand command,
        IReadOnlyDictionary<string, object?> arguments,
        CommandExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var lang = ResponseLanguageResolver.Normalize(context.ResponseLanguage);

        if (command.RequiresDevice)
        {
            return await ExecuteDeviceCommandAsync(command, lang, cancellationToken);
        }

        var now = _timeProvider.GetLocalNow();
        var message = CommandResponseLocalizer.ForCommandCode(command.Code, now, lang);
        return Ok(message);
    }

    private async Task<CommandExecutionResult> ExecuteDeviceCommandAsync(
        AvailableCommand command,
        string lang,
        CancellationToken cancellationToken)
    {
        var device = await _deviceTargetSelector.SelectAsync(command, cancellationToken);
        if (device is null)
        {
            return Fail(CommandResponseLocalizer.NoMatchingDevice(lang));
        }

        var powerOn = command.Code == "TurnLightOn";
        var state = await _dbContext.DeviceStates
            .FirstOrDefaultAsync(s => s.DeviceId == device.Id, cancellationToken);

        var now = DateTime.UtcNow;
        var newStateJson = BuildPowerStateJson(state?.StateJson, powerOn);

        if (state is null)
        {
            state = new DeviceState
            {
                DeviceId = device.Id,
                IsOnline = true,
                LastSeenAt = now,
                StateJson = newStateJson,
                UpdatedAt = now
            };
            _dbContext.DeviceStates.Add(state);
        }
        else
        {
            state.IsOnline = true;
            state.LastSeenAt = now;
            state.StateJson = newStateJson;
            state.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var deviceName = DeviceLocalization.GetName(device, lang);
        var deviceLocation = DeviceLocalization.GetLocation(device, lang);
        var message = CommandResponseLocalizer.TurnLight(powerOn, deviceName, deviceLocation, lang);

        return Ok(message, ToSnapshot(device, state, lang));
    }

    private static string BuildPowerStateJson(string? currentJson, bool powerOn)
    {
        JsonObject root;
        try
        {
            root = string.IsNullOrWhiteSpace(currentJson)
                ? new JsonObject()
                : JsonNode.Parse(currentJson)?.AsObject() ?? new JsonObject();
        }
        catch (JsonException)
        {
            root = new JsonObject();
        }

        root["power"] = powerOn;
        if (!root.ContainsKey("brightness"))
        {
            root["brightness"] = 100;
        }

        return root.ToJsonString();
    }

    private static AffectedDeviceSnapshot ToSnapshot(Device device, DeviceState state, string lang) =>
        new(
            device.Id,
            device.Code,
            DeviceLocalization.GetName(device, lang),
            DeviceLocalization.GetLocation(device, lang),
            state.IsOnline,
            state.StateJson);

    private static CommandExecutionResult Ok(string response, AffectedDeviceSnapshot? device = null) =>
        new(true, response, device);

    private static CommandExecutionResult Fail(string response) => new(false, response);
}
