using FluentValidation;

namespace JahnDigital.StudentBank.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .GreaterThan(0)
            .When(x => x.RoleId.HasValue);

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
            .WithMessage("Password must contain at least one special character.");
    }
}
