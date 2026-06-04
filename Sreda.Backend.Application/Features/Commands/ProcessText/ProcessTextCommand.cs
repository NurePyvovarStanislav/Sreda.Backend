using FluentValidation;
using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Commands.ProcessText;

public record ProcessTextCommand(string Text, int? UserId) : IRequest<Result<CommandProcessResponse>>;

public class ProcessTextCommandValidator : AbstractValidator<ProcessTextCommand>
{
    public ProcessTextCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

public class ProcessTextCommandHandler : IRequestHandler<ProcessTextCommand, Result<CommandProcessResponse>>
{
    private readonly ICommandPipeline _commandPipeline;
    private readonly IEffectiveUserResolver _effectiveUserResolver;

    public ProcessTextCommandHandler(
        ICommandPipeline commandPipeline,
        IEffectiveUserResolver effectiveUserResolver)
    {
        _commandPipeline = commandPipeline;
        _effectiveUserResolver = effectiveUserResolver;
    }

    public async Task<Result<CommandProcessResponse>> Handle(
        ProcessTextCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await _effectiveUserResolver.ResolveUserIdAsync(request.UserId, cancellationToken);

        var result = await _commandPipeline.ProcessAsync(
            request.Text,
            userId,
            CommandSource.Text,
            responseLanguage: null,
            cancellationToken);

        return Result.Success(CommandProcessMapper.ToResponse(result));
    }
}
