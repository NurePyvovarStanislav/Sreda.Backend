using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Infrastructure.Integration.Options;

namespace Sreda.Backend.Infrastructure.Integration;

/// <summary>
/// Синтез речи через Piper. Текст передаётся через временный UTF-8 файл (-i),
/// результат сохраняется в WAV и возвращается как byte[].
/// Файл вместо stdin используется потому, что Python-piper на Windows читает
/// stdin в кодировке локали и портит кириллицу.
/// </summary>
public class PiperTextToSpeechService : ITextToSpeechService
{
    private readonly PiperOptions _options;
    private readonly ILogger<PiperTextToSpeechService> _logger;

    public PiperTextToSpeechService(
        IOptions<PiperOptions> options,
        ILogger<PiperTextToSpeechService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<byte[]> SynthesizeAsync(string text, string? language = null, CancellationToken cancellationToken = default)
    {
        var modelPath = ResolveModelPath(language);

        var workingDirectory = Path.Combine(Path.GetTempPath(), "sreda-piper");
        Directory.CreateDirectory(workingDirectory);

        var id = Guid.NewGuid().ToString("N");
        var inputPath = Path.Combine(workingDirectory, $"{id}.txt");
        var outputPath = Path.Combine(workingDirectory, $"{id}.wav");

        try
        {
            // Текст пишем в UTF-8 файл и подаём через -i (а не stdin),
            // чтобы Python-piper корректно прочитал кириллицу на Windows.
            await File.WriteAllTextAsync(inputPath, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);

            var arguments = BuildArguments(modelPath, inputPath, outputPath);

            var result = await ExternalProcessRunner.RunAsync(
                _options.ExecutablePath,
                arguments,
                standardInput: null,
                _options.TimeoutSeconds,
                cancellationToken);

            if (result.ExitCode != 0)
            {
                _logger.LogError(
                    "piper завершился с кодом {ExitCode}. stderr: {Error}",
                    result.ExitCode, result.StandardError);

                throw new InvalidOperationException(
                    $"Синтез речи (piper) завершился с ошибкой (код {result.ExitCode}). " +
                    $"Подробности: {result.StandardError.Trim()}");
            }

            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException(
                    "piper не создал выходной WAV-файл. " +
                    $"Ожидался файл: {outputPath}. stderr: {result.StandardError.Trim()}");
            }

            return await File.ReadAllBytesAsync(outputPath, cancellationToken);
        }
        finally
        {
            TryDelete(inputPath);
            TryDelete(outputPath);
        }
    }

    private string BuildArguments(string modelPath, string inputPath, string outputPath)
    {
        // -m модель, -i входной текстовый файл (UTF-8), -f выходной WAV.
        var arguments =
            $"-m \"{modelPath}\" " +
            $"-i \"{inputPath}\" " +
            $"-f \"{outputPath}\"";

        if (!string.IsNullOrWhiteSpace(_options.AdditionalArguments))
        {
            arguments += " " + _options.AdditionalArguments.Trim();
        }

        return arguments;
    }

    /// <summary>
    /// Определяет путь к модели голоса по языку: сначала запрошенный язык,
    /// затем язык по умолчанию. Проверяет наличие исполняемого файла и модели.
    /// </summary>
    private string ResolveModelPath(string? language)
    {
        if (string.IsNullOrWhiteSpace(_options.ExecutablePath) || !File.Exists(_options.ExecutablePath))
        {
            throw new InvalidOperationException(
                $"Не найден исполняемый файл piper по пути '{_options.ExecutablePath}'. " +
                "Укажите корректный путь в секции 'Piper:ExecutablePath' конфигурации.");
        }

        if (_options.Voices.Count == 0)
        {
            throw new InvalidOperationException(
                "Не настроен ни один голос Piper. Заполните секцию 'Piper:Voices' (язык -> путь к .onnx).");
        }

        var requestedLanguage = string.IsNullOrWhiteSpace(language) ? _options.DefaultLanguage : language;

        if (!_options.Voices.TryGetValue(requestedLanguage, out var modelPath)
            && !_options.Voices.TryGetValue(_options.DefaultLanguage, out modelPath))
        {
            throw new InvalidOperationException(
                $"Не настроен голос Piper для языка '{requestedLanguage}' и языка по умолчанию " +
                $"'{_options.DefaultLanguage}'. Проверьте секцию 'Piper:Voices'.");
        }

        if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
        {
            throw new InvalidOperationException(
                $"Не найдена модель голоса piper по пути '{modelPath}' (язык '{requestedLanguage}'). " +
                "Проверьте пути в секции 'Piper:Voices'.");
        }

        return modelPath;
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
