using FluentValidation;

namespace Sreda.Backend.Application.Features.Voice.Commands.SpeakText;

public class SpeakTextCommandValidator : AbstractValidator<SpeakTextCommand>
{
    public SpeakTextCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст для озвучивания обязателен.")
            .MaximumLength(2000).WithMessage("Текст слишком длинный (максимум 2000 символов).");
    }
}
