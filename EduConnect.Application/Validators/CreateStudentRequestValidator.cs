using EduConnect.Application.DTOs.Admin;
using FluentValidation;

namespace EduConnect.Application.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.ParentId).NotEmpty().WithMessage("Parent is required.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(x => x.GradeLevel).InclusiveBetween(1, 4).WithMessage("Grade level must be between 1 and 4 (P1–P4).");
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of birth is required.");
    }
}
