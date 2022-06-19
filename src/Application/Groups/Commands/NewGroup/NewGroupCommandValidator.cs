using FluentValidation;

namespace JahnDigital.StudentBank.Application.Groups.Commands.NewGroup;

public class NewGroupCommandValidator : AbstractValidator<NewGroupCommand>
{
    public NewGroupCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty()
            .WithMessage("InstanceId must be specified")
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty.")
            .MinimumLength(3)
            .WithMessage("Name must be at least three characters.")
            .MaximumLength(32)
            .WithMessage("Name must be 32 characters or less.");
    }
}
