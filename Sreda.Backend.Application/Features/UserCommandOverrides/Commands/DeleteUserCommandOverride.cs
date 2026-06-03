using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.UserCommandOverrides.Commands;

public record DeleteUserCommandOverrideCommand(int Id) : IRequest<Result>;

public class DeleteUserCommandOverrideHandler : IRequestHandler<DeleteUserCommandOverrideCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public DeleteUserCommandOverrideHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteUserCommandOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.UserCommandOverrides
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure($"Переопределение с Id {request.Id} не найдено.");
        }

        _dbContext.UserCommandOverrides.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
