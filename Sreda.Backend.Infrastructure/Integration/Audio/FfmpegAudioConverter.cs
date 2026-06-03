using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sreda.Backend.Infrastructure.Integration.Options;

namespace Sreda.Backend.Infrastructure.Integration.Audio;

/// <summary>
/// Конвертирует произвольное аудио в WAV 16 kHz mono PCM через ffmpeg.
/// Вызывается только когда формат не поддерживается whisper.cpp напрямую.
/// </summary>
public class FfmpegAudioConverter
{
    private readonly FfmpegOptions _options;
    private readonly ILogger<FfmpegAudioConverter> _logger;

    public FfmpegAudioConverter(IOptions<FfmpegOptions> options, ILogger<FfmpegAudioConverter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsAvailable =>
        !string.IsNullOrWhiteSpace(_options.ExecutablePath) && File.Exists(_options.ExecutablePath);

    public async Task ConvertToWhisperWavAsync(string inputPath, string outputPath, CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Аудио пришло в формате, который не поддерживается напрямую, а ffmpeg не настроен " +
                "('Ffmpeg:ExecutablePath'). Установите ffmpeg или присылайте аудио в формате WAV/MP3/FLAC.");
        }

        var arguments = $"-y -i \"{inputPath}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{outputPath}\"";

        var result = await ExternalProcessRunner.RunAsync(
            _options.ExecutablePath,
            arguments,
            standardInput: null,
            _options.TimeoutSeconds,
            cancellationToken);

        if (result.ExitCode != 0 || !File.Exists(outputPath))
        {
            _logger.LogError("ffmpeg завершился с кодом {ExitCode}. stderr: {Error}",
                result.ExitCode, result.StandardError);

            throw new InvalidOperationException(
                $"Конвертация аудио (ffmpeg) завершилась с ошибкой (код {result.ExitCode}). " +
                $"Подробности: {result.StandardError.Trim()}");
        }
    }
}
