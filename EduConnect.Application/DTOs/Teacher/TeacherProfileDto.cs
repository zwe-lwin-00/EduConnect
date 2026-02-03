namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// Teacher profile read-only — Master Doc 11B D. No prices, no margins. Teachers cannot edit core data.
/// </summary>
public class TeacherProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
}
