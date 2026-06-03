namespace Sreda.Backend.Application.Common;

/// <summary>
/// Настройки резолвера команд (пороги уверенности сопоставления).
/// </summary>
public class CommandResolverOptions
{
    public const string SectionName = "CommandResolver";

    /// <summary>
    /// Минимальная уверенность, при которой команда считается распознанной и
    /// допускается к выполнению. Ниже порога — команда отклоняется (CommandRejected).
    /// </summary>
    public double MinConfidence { get; set; } = 0.6;
}
