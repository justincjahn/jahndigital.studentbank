using FluentValidation;

namespace JahnDigital.StudentBank.Application.Users.Commands.NewUser;

public class NewUserCommandValidator : AbstractValidator<NewUserCommand>
{
    public NewUserCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email address is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.");
    }
}
