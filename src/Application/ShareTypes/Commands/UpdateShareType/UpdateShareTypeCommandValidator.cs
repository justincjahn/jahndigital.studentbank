using FluentValidation;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.UpdateShareType;

public class UpdateShareTypeCommandValidator : AbstractValidator<UpdateShareTypeCommand>
{
    public UpdateShareTypeCommandValidator()
    {
        RuleFor(x => x.ShareTypeId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .MinimumLength(3)
            .When(x => x.Name is not null);
    }
}
