using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.History;
using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Features.History.Queries;

/// <summary>История распознанных команд (новые сверху), с фильтрами и пагинацией.</summary>
public record GetCommandsQuery(int? UserId = null, CommandStatus? Status = null, int Skip = 0, int Take = 50)
    : IRequest<Result<IReadOnlyList<CommandHistoryDto>>>;

public class GetCommandsQueryHandler : IRequestHandler<GetCommandsQuery, Result<IReadOnlyList<CommandHistoryDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetCommandsQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<CommandHistoryDto>>> Handle(
        GetCommandsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Commands.AsNoTracking();

        if (request.UserId is { } userId)
        {
            query = query.Where(c => c.UserId == userId);
        }

        if (request.Status is { } status)
        {
            query = query.Where(c => c.Status == status);
        }

        var take = Math.Clamp(request.Take, 1, 200);
        var skip = Math.Max(request.Skip, 0);

        var commands = await query
            .OrderByDescending(c => c.Id)
            .Skip(skip)
            .Take(take)
            .Select(c => new CommandHistoryDto(
                c.Id, c.UserId, c.RawText, c.AvailableCommandId, c.ArgumentsJson, c.Status, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CommandHistoryDto>>(commands);
    }
}
