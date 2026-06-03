using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Overrides;

namespace Sreda.Backend.Application.Features.UserCommandOverrides.Queries;

/// <summary>Переопределения доступа, опционально по пользователю.</summary>
public record GetUserCommandOverridesQuery(int? UserId = null)
    : IRequest<Result<IReadOnlyList<UserCommandOverrideDto>>>;

public class GetUserCommandOverridesQueryHandler
    : IRequestHandler<GetUserCommandOverridesQuery, Result<IReadOnlyList<UserCommandOverrideDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetUserCommandOverridesQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<UserCommandOverrideDto>>> Handle(
        GetUserCommandOverridesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.UserCommandOverrides.AsNoTracking();

        if (request.UserId is { } userId)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var overrides = await query
            .OrderBy(o => o.Id)
            .Select(o => new UserCommandOverrideDto(o.Id, o.UserId, o.AvailableCommandId, o.IsAllowed, o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<UserCommandOverrideDto>>(overrides);
    }
}
