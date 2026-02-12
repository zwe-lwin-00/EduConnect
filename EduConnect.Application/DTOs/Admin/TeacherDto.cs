namespace EduConnect.Application.DTOs.Admin;

public class TeacherDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int VerificationStatus { get; set; }
    public string VerificationStatusName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsActive { get; set; }
}
