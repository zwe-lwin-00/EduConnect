namespace EduConnect.Application.DTOs.Admin;

public class OnboardTeacherRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string NrcNumber { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
}
