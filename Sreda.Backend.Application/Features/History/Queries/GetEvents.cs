using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.History;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.History.Queries;

/// <summary>Журнал событий (новые сверху), с фильтрами и пагинацией.</summary>
public record GetEventsQuery(
    EventType? Type = null,
    int? CommandId = null,
    int? DeviceId = null,
    int Skip = 0,
    int Take = 50) : IRequest<Result<IReadOnlyList<EventDto>>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<IReadOnlyList<EventDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetEventsQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<EventDto>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Events.AsNoTracking();

        if (request.Type is { } type)
        {
            query = query.Where(e => e.Type == type);
        }

        if (request.CommandId is { } commandId)
        {
            query = query.Where(e => e.CommandId == commandId);
        }

        if (request.DeviceId is { } deviceId)
        {
            query = query.Where(e => e.DeviceId == deviceId);
        }

        var take = Math.Clamp(request.Take, 1, 200);
        var skip = Math.Max(request.Skip, 0);

        var events = await query
            .OrderByDescending(e => e.Id)
            .Skip(skip)
            .Take(take)
            .Select(e => new EventDto(e.Id, e.CommandId, e.DeviceId, e.Type, e.PayloadJson, e.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<EventDto>>(events);
    }
}
