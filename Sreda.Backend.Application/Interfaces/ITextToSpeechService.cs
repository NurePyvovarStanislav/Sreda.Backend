namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Синтез речи: текст -> WAV (байты).
/// Реализация (Mock / Piper) подменяется только в DI.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    /// Синтезирует речь. <paramref name="language"/> (например, "en", "ru") выбирает голос;
    /// если null/пусто — используется голос по умолчанию.
    /// </summary>
    Task<byte[]> SynthesizeAsync(string text, string? language = null, CancellationToken cancellationToken = default);
}
