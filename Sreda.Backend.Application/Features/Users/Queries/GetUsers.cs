using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Users;

namespace Sreda.Backend.Application.Features.Users.Queries;

public record GetUsersQuery : IRequest<Result<IReadOnlyList<UserDto>>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetUsersQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserDto(u.Id, u.Name, u.IsActive, u.AccessLevel, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<UserDto>>(users);
    }
}
