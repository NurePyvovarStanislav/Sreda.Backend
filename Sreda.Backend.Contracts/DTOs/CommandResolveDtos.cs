namespace Sreda.Backend.Contracts.DTOs;

/// <summary>Запрос на обработку текстовой команды (без аудио).</summary>
public record ProcessTextRequest(string Text, int? UserId);

/// <summary>Запрос на диагностический разбор текста без выполнения команды.</summary>
public record ResolveTextRequest(string Text);

/// <summary>
/// Результат разбора текста резолвером (диагностика/отладка сопоставления):
/// что нашли, как нашли и с какой уверенностью.
/// </summary>
public record CommandResolveResponse(
    string RawText,
    bool Accepted,
    string? CommandCode,
    double Confidence,
    string MatchKind,
    string? MatchedPhrase,
    string? MatchedLanguage);
