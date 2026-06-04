using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Application.Services;

public class EffectiveUserResolver : IEffectiveUserResolver
{
    private readonly ISredaDbContext _dbContext;

    public EffectiveUserResolver(ISredaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> ResolveUserIdAsync(int? requestedUserId, CancellationToken cancellationToken = default)
    {
        if (requestedUserId is not null)
        {
            return requestedUserId.Value;
        }

        var admin = await _dbContext.Users
            .Where(u => u.Name == SystemUsers.DefaultAdminName && u.IsActive)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (admin is null)
        {
            throw new InvalidOperationException(
                $"Системного користувача '{SystemUsers.DefaultAdminName}' не знайдено. Перезапустіть застосунок для сіду БД.");
        }

        return admin.Id;
    }
}
