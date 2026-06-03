namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Распознавание речи: аудиопоток -> текст.
/// Реализация (Mock / whisper.cpp) подменяется только в DI.
/// </summary>
public interface ISpeechToTextService
{
    Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
