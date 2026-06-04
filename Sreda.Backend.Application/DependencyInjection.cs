using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Common.Behaviors;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Application.Services;

namespace Sreda.Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<CommandResolverOptions>(
            configuration.GetSection(CommandResolverOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IDeviceTargetSelector, DeviceTargetSelector>();
        services.AddScoped<IEffectiveUserResolver, EffectiveUserResolver>();
        services.AddScoped<ICommandExecutor, CommandExecutor>();

        services.AddScoped<ICommandResolver, CommandResolverService>();
        services.AddScoped<ICommandAccessPolicy, CommandAccessPolicy>();
        services.AddScoped<ICommandPipeline, CommandPipeline>();
        services.AddSingleton<ISemanticCommandMatcher, NullSemanticCommandMatcher>();

        services.AddScoped<IEventLogger, EventLogger>();

        return services;
    }
}
