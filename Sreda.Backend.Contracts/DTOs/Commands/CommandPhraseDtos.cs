namespace Sreda.Backend.Contracts.DTOs.Commands;

public record CommandPhraseDto(
    int Id,
    int AvailableCommandId,
    string Phrase,
    string Language,
    bool IsActive);

public record CreateCommandPhraseRequest(
    int AvailableCommandId,
    string Phrase,
    string Language,
    bool IsActive = true);

public record UpdateCommandPhraseRequest(
    string Phrase,
    string Language,
    bool IsActive);
