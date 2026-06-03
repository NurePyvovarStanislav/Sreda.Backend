using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Итог прохождения текста через конвейер обработки команды:
/// ответ пользователю, статус, сопоставленная команда и уверенность.
/// </summary>
public record CommandPipelineResult(
    string RawText,
    string Response,
    CommandStatus Status,
    string? CommandCode,
    double Confidence);
