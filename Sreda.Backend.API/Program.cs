using Microsoft.EntityFrameworkCore;
using Sreda.Backend.Infrastructure;
using Sreda.Backend.Infrastructure.Persistence;

namespace Sreda.Backend.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            // Применяем миграции автоматически при старте.
            await ApplyMigrationsAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SredaDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
