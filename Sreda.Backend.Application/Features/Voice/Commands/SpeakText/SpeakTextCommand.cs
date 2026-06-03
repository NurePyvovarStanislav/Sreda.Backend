using MediatR;
using Sreda.Backend.Application.Common;

namespace Sreda.Backend.Application.Features.Voice.Commands.SpeakText;

/// <summary>
/// Озвучить текст: текст -> WAV (байты). Language выбирает голос (необязательно).
/// </summary>
public record SpeakTextCommand(string Text, string? Language = null) : IRequest<Result<byte[]>>;
