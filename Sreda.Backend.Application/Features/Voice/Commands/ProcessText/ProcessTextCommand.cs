using FluentValidation;
using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessText;

/// <summary>
/// Обработать текстовую команду тем же конвейером, что и голос, но без STT.
/// Пригодится для будущих веб/мобильных клиентов и для тестирования.
/// </summary>
public record ProcessTextCommand(string Text, int? UserId) : IRequest<Result<VoiceProcessResponse>>;

public class ProcessTextCommandValidator : AbstractValidator<ProcessTextCommand>
{
    public ProcessTextCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

public class ProcessTextCommandHandler : IRequestHandler<ProcessTextCommand, Result<VoiceProcessResponse>>
{
    private readonly ICommandPipeline _commandPipeline;

    public ProcessTextCommandHandler(ICommandPipeline commandPipeline)
    {
        _commandPipeline = commandPipeline;
    }

    public async Task<Result<VoiceProcessResponse>> Handle(ProcessTextCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandPipeline.ProcessAsync(
            request.Text, request.UserId, CommandSource.Text, cancellationToken);

        return Result.Success(new VoiceProcessResponse(
            result.RawText,
            result.Response,
            result.Status.ToString(),
            result.CommandCode,
            result.Confidence));
    }
}
