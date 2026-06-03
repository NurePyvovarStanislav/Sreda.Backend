using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Integration.Options;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Распознавание речи через whisper.cpp (whisper-cli).
/// Сохраняет входной аудиопоток во временный файл, запускает CLI,
/// читает результирующий txt и возвращает распознанный текст.
/// </summary>
public class WhisperSpeechToTextService : ISpeechToTextService
{
    private readonly WhisperOptions _options;
    private readonly ILogger<WhisperSpeechToTextService> _logger;

    public WhisperSpeechToTextService(
        IOptions<WhisperOptions> options,
        ILogger<WhisperSpeechToTextService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> TranscribeAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var workingDirectory = Path.Combine(Path.GetTempPath(), "sreda-whisper");
        Directory.CreateDirectory(workingDirectory);

        var id = Guid.NewGuid().ToString("N");
        var inputPath = Path.Combine(workingDirectory, $"{id}.wav");
        var outputPrefix = Path.Combine(workingDirectory, $"{id}_out");
        var outputTxtPath = $"{outputPrefix}.txt";

        try
        {
            await using (var fileStream = File.Create(inputPath))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
            }

            var arguments = BuildArguments(inputPath, outputPrefix);

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
            TryDelete(inputPath);
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
