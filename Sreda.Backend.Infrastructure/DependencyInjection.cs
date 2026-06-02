using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Persistence;

namespace Sreda.Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Sreda")
            ?? throw new InvalidOperationException(
                "Connection string 'Sreda' is not configured in appsettings.json.");

        services.AddDbContext<SredaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISredaDbContext>(sp => sp.GetRequiredService<SredaDbContext>());

        return services;
    }
}
