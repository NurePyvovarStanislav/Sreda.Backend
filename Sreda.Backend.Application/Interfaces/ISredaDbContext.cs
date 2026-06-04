using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Абстракция доступа к данным для слоя Application.
/// Скрывает конкретную реализацию EF Core / провайдер БД от бизнес-логики.
/// </summary>
public interface ISredaDbContext
{
    DbSet<User> Users { get; }
    DbSet<VoiceProfile> VoiceProfiles { get; }
    DbSet<AvailableCommand> AvailableCommands { get; }
    DbSet<CommandPhrase> CommandPhrases { get; }
    DbSet<UserCommandOverride> UserCommandOverrides { get; }
    DbSet<Command> Commands { get; }
    DbSet<Device> Devices { get; }
    DbSet<DeviceLocalizedLabel> DeviceLocalizedLabels { get; }
    DbSet<Event> Events { get; }
    DbSet<DeviceState> DeviceStates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
