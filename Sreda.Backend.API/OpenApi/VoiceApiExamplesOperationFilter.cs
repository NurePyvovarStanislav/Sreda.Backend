using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sreda.Backend.API.OpenApi;

/// <summary>
/// Додає приклади запитів/відповідей для мовних endpoint'ів у Swagger UI (OpenAPI 2.x / JsonNode).
/// </summary>
public sealed class VoiceApiExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;

        if (relativePath.Contains("api/commands/process-text") || relativePath.Contains("api/voice/process-text"))
        {
            operation.Summary ??= "Обробити текстову команду";
            operation.Description ??= "Той самий конвейер, що й для голосу, але без STT.";
            SetJsonExample(operation, JsonNode.Parse("""
                {
                  "text": "скільки часу",
                  "userId": null
                }
                """));
            SetResponseExample(operation, "200", JsonNode.Parse("""
                {
                  "commandId": 1,
                  "userId": 1,
                  "recognizedText": "скільки часу",
                  "message": "Поточний час: 14:30.",
                  "status": "Success",
                  "commandCode": "GetTime",
                  "confidence": 0.92,
                  "device": null
                }
                """));
        }
        else if (relativePath.Contains("api/commands/resolve") || relativePath.Contains("api/voice/resolve"))
        {
            operation.Summary ??= "Діагностика розпізнавання наміру";
            operation.Description ??= "Лише зіставлення тексту з командою, без виконання.";
            SetJsonExample(operation, JsonNode.Parse("""
                {
                  "text": "привіт среда"
                }
                """));
            SetResponseExample(operation, "200", JsonNode.Parse("""
                {
                  "rawText": "привіт среда",
                  "accepted": true,
                  "commandCode": "Hello",
                  "confidence": 1,
                  "matchKind": "Exact",
                  "matchedPhrase": "привет среда",
                  "matchedLanguage": "ru"
                }
                """));
        }
        else if (relativePath.Contains("api/voice/speak"))
        {
            operation.Summary ??= "Синтез мовлення (TTS)";
            SetJsonExample(operation, JsonNode.Parse("""
                {
                  "text": "Привіт! Я Sreda.",
                  "language": "uk"
                }
                """));
        }
        else if (relativePath.Contains("process-with-speech"))
        {
            operation.Summary ??= "Голос → STT → конвеєр → TTS (один запит)";
            operation.Description ??= "Повертає JSON з audioBase64 (WAV). userId опційний — за замовчуванням Admin.";
        }
        else if (relativePath.Contains("api/voice/process") && !relativePath.Contains("process-text") && !relativePath.Contains("process-with-speech"))
        {
            operation.Summary ??= "Обробити голосову команду (аудіо)";
            operation.Description ??= "multipart/form-data: поле audio (WAV/WebM тощо). Опційно userId у query.";
        }
    }

    private static void SetJsonExample(OpenApiOperation operation, JsonNode? example)
    {
        if (example is null || operation.RequestBody?.Content is null)
        {
            return;
        }

        foreach (var media in operation.RequestBody.Content.Values)
        {
            if (media is OpenApiMediaType openApiMedia)
            {
                openApiMedia.Example = example;
            }
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, JsonNode? example)
    {
        if (example is null
            || !operation.Responses.TryGetValue(statusCode, out var response)
            || response.Content is null
            || !response.Content.TryGetValue("application/json", out var media))
        {
            return;
        }

        if (media is OpenApiMediaType openApiMedia)
        {
            openApiMedia.Example = example;
        }
    }
}
