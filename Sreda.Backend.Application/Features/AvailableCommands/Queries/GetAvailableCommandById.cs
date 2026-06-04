using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Commands;

namespace Sreda.Backend.Application.Features.AvailableCommands.Queries;

public record GetAvailableCommandByIdQuery(int Id) : IRequest<Result<AvailableCommandDto>>;

public class GetAvailableCommandByIdQueryHandler
    : IRequestHandler<GetAvailableCommandByIdQuery, Result<AvailableCommandDto>>
{
    private readonly ISredaDbContext _dbContext;

    public GetAvailableCommandByIdQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AvailableCommandDto>> Handle(
        GetAvailableCommandByIdQuery request, CancellationToken cancellationToken)
    {
        var command = await _dbContext.AvailableCommands
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new AvailableCommandDto(
                c.Id, c.Code, c.Name, c.Description, c.Category,
                c.RequiresDevice, c.RequiredAccessLevel, c.IsEnabled, c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return command is null
            ? Result.Failure<AvailableCommandDto>($"Команду з Id {request.Id} не знайдено.")
            : Result.Success(command);
    }
}
