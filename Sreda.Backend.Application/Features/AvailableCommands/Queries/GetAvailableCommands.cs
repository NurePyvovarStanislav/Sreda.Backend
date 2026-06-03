using MediatR;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs.Commands;

namespace Sreda.Backend.Application.Features.AvailableCommands.Queries;

public record GetAvailableCommandsQuery : IRequest<Result<IReadOnlyList<AvailableCommandDto>>>;

public class GetAvailableCommandsQueryHandler
    : IRequestHandler<GetAvailableCommandsQuery, Result<IReadOnlyList<AvailableCommandDto>>>
{
    private readonly ISredaDbContext _dbContext;

    public GetAvailableCommandsQueryHandler(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<AvailableCommandDto>>> Handle(
        GetAvailableCommandsQuery request, CancellationToken cancellationToken)
    {
        var commands = await _dbContext.AvailableCommands
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new AvailableCommandDto(
                c.Id, c.Code, c.Name, c.Description, c.Category,
                c.RequiresDevice, c.RequiredAccessLevel, c.IsEnabled, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AvailableCommandDto>>(commands);
    }
}
