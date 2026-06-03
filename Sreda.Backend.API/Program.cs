using Microsoft.EntityFrameworkCore;
using Sreda.Backend.API.Common;
using Sreda.Backend.API.Hubs;
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
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // Realtime: SignalR для журнала событий (дашборд).
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IEventNotifier, SignalREventNotifier>();

            // Безопасность строится на распознавании голоса и правах доступа,
            // а не на CORS, поэтому к API может обращаться любой origin.
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

            // Применяем миграции и сидим базовые данные при старте.
            await InitializeDatabaseAsync(app);

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors(CorsPolicyName);

            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<EventsHub>("/hubs/events");

            await app.RunAsync();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SredaDbContext>();
            await dbContext.Database.MigrateAsync();
            await DbSeeder.SeedAsync(dbContext);
        }
    }
}
