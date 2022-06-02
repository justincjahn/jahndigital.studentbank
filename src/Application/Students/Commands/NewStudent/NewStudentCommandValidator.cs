using FluentValidation;

namespace JahnDigital.StudentBank.Application.Students.Commands.NewStudent;

public class NewStudentCommandValidator : AbstractValidator<NewStudentCommand>
{
    public NewStudentCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email address is invalid.")
            .When(x => !String.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.");
    }
}
