using System.Text.Json;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.History;
using Sreda.Backend.Domain.Entities;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Services;

/// <summary>
/// Централизованный логгер событий: пишет Event в БД, затем публикует его
/// через IEventNotifier (SignalR). Использует общий с запросом ISredaDbContext,
/// поэтому SaveChanges заодно фиксирует и другие незакоммиченные изменения.
/// </summary>
public class EventLogger : IEventLogger
{
    private readonly ISredaDbContext _dbContext;
    private readonly IEventNotifier _eventNotifier;

    public EventLogger(ISredaDbContext dbContext, IEventNotifier eventNotifier)
    {
        _dbContext = dbContext;
        _eventNotifier = eventNotifier;
    }

    public async Task<EventDto> LogAsync(
        EventType type,
        object? payload = null,
        int? commandId = null,
        int? deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var entity = new Event
        {
            Type = type,
            CommandId = commandId,
            DeviceId = deviceId,
            PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Events.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new EventDto(entity.Id, entity.CommandId, entity.DeviceId, entity.Type, entity.PayloadJson, entity.CreatedAt);
        await _eventNotifier.PublishAsync(dto, cancellationToken);

        return dto;
    }
}
