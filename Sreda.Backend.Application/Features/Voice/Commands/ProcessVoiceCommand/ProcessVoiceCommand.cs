using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

/// <summary>
/// Обработать голосовую команду: аудио -> распознанный текст -> ответ системы.
/// </summary>
public record ProcessVoiceCommand(Stream Audio) : IRequest<Result<VoiceProcessResponse>>;
