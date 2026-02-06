namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Returned when admin resets a teacher's password. Includes email and new temporary password to share with the teacher.
/// </summary>
public class ResetTeacherPasswordResponse
{
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
