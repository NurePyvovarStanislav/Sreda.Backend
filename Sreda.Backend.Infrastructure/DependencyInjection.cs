using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Integration;
using Sreda.Backend.Infrastructure.Integration.Audio;
using Sreda.Backend.Infrastructure.Integration.Options;
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

        AddVoiceServices(services, configuration);

        return services;
    }

    /// <summary>
    /// Регистрирует STT/TTS-сервисы. Выбор Mock или реальных CLI (whisper.cpp + Piper)
    /// зависит только от настройки 'Voice:Provider' — бизнес-логика и endpoint'ы не меняются.
    /// </summary>
    private static void AddVoiceServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhisperOptions>(configuration.GetSection(WhisperOptions.SectionName));
        services.Configure<PiperOptions>(configuration.GetSection(PiperOptions.SectionName));
        services.Configure<FfmpegOptions>(configuration.GetSection(FfmpegOptions.SectionName));

        services.AddSingleton<FfmpegAudioConverter>();

        var provider = configuration
            .GetSection(VoiceOptions.SectionName)
            .GetValue<string>(nameof(VoiceOptions.Provider)) ?? "Mock";

        var useReal = string.Equals(provider, "Real", StringComparison.OrdinalIgnoreCase);

        if (useReal)
        {
            services.AddScoped<ISpeechToTextService, WhisperSpeechToTextService>();
            services.AddScoped<ITextToSpeechService, PiperTextToSpeechService>();
        }
        else
        {
            services.AddScoped<ISpeechToTextService, MockSpeechToTextService>();
            services.AddScoped<ITextToSpeechService, MockTextToSpeechService>();
        }
    }
}
