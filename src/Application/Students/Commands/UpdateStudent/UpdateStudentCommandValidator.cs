using FluentValidation;

namespace JahnDigital.StudentBank.Application.Students.Commands.UpdateStudent;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.FirstName)
            .MinimumLength(1)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(1)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !String.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.")
            .MinimumLength(8)
            .WithMessage("Password must be greater than 8 characters.")
            .Matches(@"[A-Z]+")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]+")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]+")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[^A-Za-z0-9]")
            .WithMessage("Password must contain at least one special character.")
            .When(x => !String.IsNullOrWhiteSpace(x.Password));
    }
}
