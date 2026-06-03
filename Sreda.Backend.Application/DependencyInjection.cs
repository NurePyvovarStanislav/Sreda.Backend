using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sreda.Backend.Application.Common.Behaviors;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Application.Services;

namespace Sreda.Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIntentParser, DbIntentParser>();
        services.AddSingleton<ICommandExecutor, BasicCommandExecutor>();

        return services;
    }
}
