using Microsoft.EntityFrameworkCore;
using Serilog;
using Sreda.Backend.API.Common;
using Sreda.Backend.API.Hubs;
using Sreda.Backend.API.OpenApi;
using Sreda.Backend.API.Realtime;
using Sreda.Backend.Application;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure;
using Sreda.Backend.Infrastructure.Persistence;

namespace Sreda.Backend.API
{
    public class Program
    {
        private const string CorsPolicyName = "SredaCorsPolicy";

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "Sreda.Backend"));

                builder.Services.AddControllers();
                builder.Services.AddSredaSwagger();

                builder.Services.AddApplication(builder.Configuration);
                builder.Services.AddInfrastructure(builder.Configuration);

                builder.Services.AddProblemDetails();
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

                builder.Services.AddSignalR();
                builder.Services.AddScoped<IEventNotifier, SignalREventNotifier>();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(CorsPolicyName, policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });

                var app = builder.Build();

                await InitializeDatabaseAsync(app);

                app.UseSerilogRequestLogging();

                app.UseExceptionHandler();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sreda API v1");
                        options.DocumentTitle = "Sreda — мовний інтерфейс";
                    });
                    app.MapOpenApi();
                }

                app.UseHttpsRedirection();
                app.UseCors(CorsPolicyName);
                app.UseAuthorization();

                app.MapControllers();
                app.MapHub<EventsHub>("/hubs/events");

                Log.Information("Застосунок Sreda.Backend запущено");

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Застосунок завершився з критичною помилкою");
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SredaDbContext>();
            await dbContext.Database.MigrateAsync();
            await DbSeeder.SeedAsync(dbContext);
            Log.Information("Міграції БД застосовано, каталог команд оновлено");
        }
    }
}
