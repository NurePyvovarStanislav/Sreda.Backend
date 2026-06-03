using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

/// <summary>
/// Обработать голосовую команду: аудио -> распознанный текст -> разбор намерения -> ответ.
/// UserId опционален (идентификация пользователя появится позже).
/// </summary>
public record ProcessVoiceCommand(Stream Audio, int? UserId = null) : IRequest<Result<VoiceProcessResponse>>;
