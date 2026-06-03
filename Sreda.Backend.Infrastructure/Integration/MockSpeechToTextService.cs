using Sreda.Backend.Application.Interfaces;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Заглушка распознавания речи. Возвращает детерминированный текст,
/// чтобы голосовой пайплайн можно было проверить без whisper.cpp.
/// </summary>
public class MockSpeechToTextService : ISpeechToTextService
{
    public async Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        // Полностью вычитываем поток, имитируя обработку аудио.
        using var buffer = new MemoryStream();
        await audioStream.CopyToAsync(buffer, cancellationToken);

        return "привет среда";
    }
}
