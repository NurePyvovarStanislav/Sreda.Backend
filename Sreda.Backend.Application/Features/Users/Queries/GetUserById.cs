using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Users;

namespace Sreda.Backend.Application.Features.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly ISredaDbContext _dbContext;

    public GetUserByIdQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(u => new UserDto(u.Id, u.Name, u.IsActive, u.AccessLevel, u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return user is null
            ? Result.Failure<UserDto>($"Пользователь с Id {request.Id} не найден.")
            : Result.Success(user);
    }
}
