using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Application.Services;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Заглушка распознавания речи. Возвращает детерминированный текст,
/// чтобы голосовой пайплайн можно было проверить без whisper.cpp.
/// </summary>
public class MockSpeechToTextService : ISpeechToTextService
{
    public async Task<SpeechTranscription> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await audioStream.CopyToAsync(buffer, cancellationToken);

        const string text = "привет среда";
        return new SpeechTranscription(text, ResponseLanguageResolver.InferFromText(text));
    }
}
