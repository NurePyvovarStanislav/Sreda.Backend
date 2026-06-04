using System.Reflection;
using Microsoft.OpenApi;

namespace Sreda.Backend.API.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSredaSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sreda API",
                Version = "v1",
                Description =
                    "Програмна система мовного інтерфейсу на основі методів штучного інтелекту. " +
                    "Голос/текст → розпізнавання наміру → перевірка доступу → виконання → журнал подій."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.OperationFilter<VoiceApiExamplesOperationFilter>();
        });

        return services;
    }
}
