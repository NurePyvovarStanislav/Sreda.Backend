using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.Users.Commands;

public record DeleteUserCommand(int Id) : IRequest<Result>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public DeleteUserCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure($"Пользователь с Id {request.Id} не найден.");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
