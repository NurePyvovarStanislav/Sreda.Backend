using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Integration.Audio;
using Sreda.Backend.Infrastructure.Integration.Options;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Распознавание речи через whisper.cpp (whisper-cli).
/// Сохраняет входной аудиопоток во временный файл, при необходимости конвертирует
/// его в WAV 16 kHz mono через ffmpeg, запускает CLI и читает результат из txt.
/// </summary>
public class WhisperSpeechToTextService : ISpeechToTextService
{
    private readonly WhisperOptions _options;
    private readonly FfmpegAudioConverter _audioConverter;
    private readonly ILogger<WhisperSpeechToTextService> _logger;

    public WhisperSpeechToTextService(
        IOptions<WhisperOptions> options,
        FfmpegAudioConverter audioConverter,
        ILogger<WhisperSpeechToTextService> logger)
    {
        _options = options.Value;
        _audioConverter = audioConverter;
        _logger = logger;
    }

    public async Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var workingDirectory = Path.Combine(Path.GetTempPath(), "sreda-whisper");
        Directory.CreateDirectory(workingDirectory);

        var id = Guid.NewGuid().ToString("N");
        var sourcePath = Path.Combine(workingDirectory, $"{id}.src");
        var convertedPath = Path.Combine(workingDirectory, $"{id}.16k.wav");
        var outputPrefix = Path.Combine(workingDirectory, $"{id}_out");
        var outputTxtPath = $"{outputPrefix}.txt";
        var usedConvertedFile = false;

        try
        {
            await using (var fileStream = File.Create(sourcePath))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Конвертируем только если аудио не в целевом формате (WAV 16 kHz mono PCM)
            // и не в формате, который whisper читает сам (MP3/FLAC).
            string audioPath;
            if (AudioFormatDetector.CanUseDirectly(sourcePath))
            {
                audioPath = sourcePath;
            }
            else
            {
                _logger.LogInformation(
                    "Аудио не в целевом формате — конвертирую через ffmpeg в WAV 16 kHz mono.");
                await _audioConverter.ConvertToWhisperWavAsync(sourcePath, convertedPath, cancellationToken);
                audioPath = convertedPath;
                usedConvertedFile = true;
            }

            var arguments = BuildArguments(audioPath, outputPrefix);

            var result = await ExternalProcessRunner.RunAsync(
                _options.ExecutablePath,
                arguments,
                standardInput: null,
                _options.TimeoutSeconds,
                cancellationToken);

            if (result.ExitCode != 0)
            {
                _logger.LogError(
                    "whisper-cli завершился с кодом {ExitCode}. stderr: {Error}",
                    result.ExitCode, result.StandardError);

                throw new InvalidOperationException(
                    $"Распознавание речи (whisper-cli) завершилось с ошибкой (код {result.ExitCode}). " +
                    $"Подробности: {result.StandardError.Trim()}");
            }

            if (!File.Exists(outputTxtPath))
            {
                throw new InvalidOperationException(
                    "whisper-cli не создал файл с результатом распознавания. " +
                    $"Ожидался файл: {outputTxtPath}. stderr: {result.StandardError.Trim()}");
            }

            var recognizedText = await File.ReadAllTextAsync(outputTxtPath, cancellationToken);
            return recognizedText.Trim();
        }
        finally
        {
            TryDelete(sourcePath);
            if (usedConvertedFile)
            {
                TryDelete(convertedPath);
            }
            TryDelete(outputTxtPath);
        }
    }

    private string BuildArguments(string inputPath, string outputPrefix)
    {
        // -m модель, -f вход, -l язык, -otxt вывод в txt, -of префикс файла, -nt без таймкодов
        var arguments =
            $"-m \"{_options.ModelPath}\" " +
            $"-f \"{inputPath}\" " +
            $"-l {_options.Language} " +
            $"-otxt -of \"{outputPrefix}\" -nt";

        if (!string.IsNullOrWhiteSpace(_options.AdditionalArguments))
        {
            arguments += " " + _options.AdditionalArguments.Trim();
        }

        return arguments;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ExecutablePath) || !File.Exists(_options.ExecutablePath))
        {
            throw new InvalidOperationException(
                $"Не найден исполняемый файл whisper-cli по пути '{_options.ExecutablePath}'. " +
                "Укажите корректный путь в секции 'Whisper:ExecutablePath' конфигурации.");
        }

        if (string.IsNullOrWhiteSpace(_options.ModelPath) || !File.Exists(_options.ModelPath))
        {
            throw new InvalidOperationException(
                $"Не найдена модель whisper по пути '{_options.ModelPath}'. " +
                "Укажите корректный путь в секции 'Whisper:ModelPath' конфигурации.");
        }
    }

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось удалить временный файл {Path}", path);
        }
    }
}
