using Sreda.Backend.Domain.Enums;

namespace Sreda.Backend.Application.Common;

/// <summary>
/// Ітог проходження тексту через конвеєр обробки команди.
/// </summary>
public record CommandPipelineResult(
    int CommandId,
    int UserId,
    string RawText,
    string Response,
    CommandStatus Status,
    string? CommandCode,
    double Confidence,
    AffectedDeviceSnapshot? AffectedDevice);
