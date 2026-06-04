using FluentValidation;
using MediatR;
using Sreda.Backend.Application.Common;
using Sreda.Backend.Application.Interfaces;
using Sreda.Backend.Contracts.DTOs;

namespace Sreda.Backend.Application.Features.Commands.ResolveCommand;

public record ResolveCommandQuery(string Text) : IRequest<Result<CommandResolveResponse>>;

public class ResolveCommandQueryValidator : AbstractValidator<ResolveCommandQuery>
{
    public ResolveCommandQueryValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

public class ResolveCommandQueryHandler : IRequestHandler<ResolveCommandQuery, Result<CommandResolveResponse>>
{
    private readonly ICommandResolver _resolver;

    public ResolveCommandQueryHandler(ICommandResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<Result<CommandResolveResponse>> Handle(
        ResolveCommandQuery request,
        CancellationToken cancellationToken)
    {
        var resolution = await _resolver.ResolveAsync(request.Text, cancellationToken);

        return Result.Success(new CommandResolveResponse(
            request.Text,
            resolution.Accepted,
            resolution.Command?.Code,
            resolution.Confidence,
            resolution.MatchKind.ToString(),
            resolution.MatchedPhrase,
            resolution.MatchedLanguage));
    }
}
