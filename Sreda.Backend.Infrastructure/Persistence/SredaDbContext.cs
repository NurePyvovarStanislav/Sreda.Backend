using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Domain.Entities;

namespace Sreda.Backend.Infrastructure.Persistence;

public class SredaDbContext : DbContext
{
    public SredaDbContext(DbContextOptions<SredaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VoiceProfile> VoiceProfiles => Set<VoiceProfile>();
    public DbSet<AvailableCommand> AvailableCommands => Set<AvailableCommand>();
    public DbSet<CommandPhrase> CommandPhrases => Set<CommandPhrase>();
    public DbSet<UserCommandOverride> UserCommandOverrides => Set<UserCommandOverride>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<DeviceState> DeviceStates => Set<DeviceState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
