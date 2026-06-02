namespace Sreda.Backend.Domain.Entities;

/// <summary>
/// Голосовая фраза, по которой распознаётся конкретная команда
/// (одна команда может иметь много фраз и языков).
/// </summary>
public class CommandPhrase
{
    public int Id { get; set; }

    public int AvailableCommandId { get; set; }

    /// <summary>Текст фразы, например "включи свет".</summary>
    public string Phrase { get; set; } = null!;

    /// <summary>Код языка фразы, например "ru", "en".</summary>
    public string Language { get; set; } = null!;

    public bool IsActive { get; set; }

    public AvailableCommand AvailableCommand { get; set; } = null!;
}
