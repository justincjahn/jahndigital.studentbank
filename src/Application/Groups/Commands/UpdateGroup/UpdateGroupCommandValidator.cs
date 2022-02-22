using FluentValidation;

namespace JahnDigital.StudentBank.Application.Groups.Commands.UpdateGroup;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("A Group's ID number must exist before it can be updated.");

        RuleFor(x => x.InstanceId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("A Group's Instance must exist.")
            .When(x => x.InstanceId is not null);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name cannot be empty.")
            .MinimumLength(3)
            .WithMessage("Name must be at least three characters.")
            .MaximumLength(32)
            .WithMessage("Name must be 32 characters or less.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
    }
}
