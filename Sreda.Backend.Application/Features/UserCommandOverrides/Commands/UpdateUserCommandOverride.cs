using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.UserCommandOverrides.Commands;

public record UpdateUserCommandOverrideCommand(int Id, bool IsAllowed) : IRequest<Result>;

public class UpdateUserCommandOverrideHandler : IRequestHandler<UpdateUserCommandOverrideCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public UpdateUserCommandOverrideHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateUserCommandOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.UserCommandOverrides
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure($"Переопределение с Id {request.Id} не найдено.");
        }

        entity.IsAllowed = request.IsAllowed;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
