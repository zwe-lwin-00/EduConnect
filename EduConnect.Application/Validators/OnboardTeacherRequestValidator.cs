using EduConnect.Application.DTOs.Admin;
using FluentValidation;

namespace EduConnect.Application.Validators;

public class OnboardTeacherRequestValidator : AbstractValidator<OnboardTeacherRequest>
{
    public OnboardTeacherRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required.");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
        RuleFor(x => x.NrcNumber).NotEmpty().WithMessage("NRC number is required.");
        RuleFor(x => x.EducationLevel).NotEmpty().WithMessage("Education level is required.");
        RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0).WithMessage("Hourly rate must be non-negative.");
    }
}
