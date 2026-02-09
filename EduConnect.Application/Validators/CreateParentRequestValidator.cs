using EduConnect.Application.DTOs.Admin;
using FluentValidation;

namespace EduConnect.Application.Validators;

public class CreateParentRequestValidator : AbstractValidator<CreateParentRequest>
{
    public CreateParentRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
    }
}
