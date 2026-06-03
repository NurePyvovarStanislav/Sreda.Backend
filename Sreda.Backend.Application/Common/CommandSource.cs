namespace Sreda.Backend.Application.Common;

/// <summary>
/// Источник команды: голос (после STT) или прямой текстовый ввод
/// (например, будущий веб/мобильный клиент).
/// </summary>
public enum CommandSource
{
    Voice = 0,
    Text = 1
}
