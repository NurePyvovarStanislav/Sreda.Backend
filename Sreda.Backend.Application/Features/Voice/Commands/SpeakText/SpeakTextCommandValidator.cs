using FluentValidation;

namespace Sreda.Backend.Application.Features.Voice.Commands.SpeakText;

public class SpeakTextCommandValidator : AbstractValidator<SpeakTextCommand>
{
    public SpeakTextCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст для озвучування обов'язковий.")
            .MaximumLength(2000).WithMessage("Текст занадто довгий (максимум 2000 символів).");
    }
}
