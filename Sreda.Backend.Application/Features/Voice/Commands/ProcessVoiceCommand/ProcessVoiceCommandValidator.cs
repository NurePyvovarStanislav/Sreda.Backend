using FluentValidation;

namespace Sreda.Backend.Application.Features.Voice.Commands.ProcessVoiceCommand;

public class ProcessVoiceCommandValidator : AbstractValidator<ProcessVoiceCommand>
{
    public ProcessVoiceCommandValidator()
    {
        RuleFor(x => x.Audio)
            .NotNull().WithMessage("Аудіопотік обов'язковий.");
    }
}
