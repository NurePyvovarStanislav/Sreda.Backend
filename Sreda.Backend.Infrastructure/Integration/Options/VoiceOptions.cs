namespace Sreda.Backend.Infrastructure.Integration.Options;

/// <summary>
/// Выбор провайдера голосовых сервисов. Управляет тем, какие реализации
/// STT/TTS регистрируются в DI: заглушки или реальные CLI-инструменты.
/// </summary>
public class VoiceOptions
{
    public const string SectionName = "Voice";

    /// <summary>"Mock" — заглушки, "Real" — whisper.cpp + Piper.</summary>
    public string Provider { get; set; } = "Mock";
}
