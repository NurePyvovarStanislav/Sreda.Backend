using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Integration.Audio;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Заглушка синтеза речи. Возвращает корректный WAV (тишину),
/// чтобы endpoint /api/voice/speak отдавал валидный audio/wav без Piper.
/// </summary>
public class MockTextToSpeechService : ITextToSpeechService
{
    public Task<byte[]> SynthesizeAsync(string text, string? language = null, CancellationToken cancellationToken = default)
    {
        // Длительность пропорциональна длине текста (имитация озвучивания).
        var duration = TimeSpan.FromMilliseconds(Math.Clamp(text.Length * 60, 300, 5000));
        var wav = WavGenerator.GenerateSilence(duration);
        return Task.FromResult(wav);
    }
}
