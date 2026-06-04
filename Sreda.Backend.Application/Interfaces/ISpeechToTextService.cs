using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Interfaces;

/// <summary>
/// Распознавание речи: аудиопоток -> текст и язык.
/// Реализация (Mock / whisper.cpp) подменяется только в DI.
/// </summary>
public interface ISpeechToTextService
{
    Task<SpeechTranscription> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
