using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Features.AvailableCommands.Commands;

public record DeleteAvailableCommandCommand(int Id) : IRequest<Result>;

public class DeleteAvailableCommandHandler : IRequestHandler<DeleteAvailableCommandCommand, Result>
{
    private readonly ISredaDbContext _dbContext;

    public DeleteAvailableCommandHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteAvailableCommandCommand request, CancellationToken cancellationToken)
    {
        var command = await _dbContext.AvailableCommands
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (command is null)
        {
            return Result.Failure($"Команда с Id {request.Id} не найдена.");
        }

        _dbContext.AvailableCommands.Remove(command);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
