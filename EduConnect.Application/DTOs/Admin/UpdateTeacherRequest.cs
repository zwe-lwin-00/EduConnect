namespace EduConnect.Application.DTOs.Admin;

public class UpdateTeacherRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
}
