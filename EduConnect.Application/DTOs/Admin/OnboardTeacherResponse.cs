namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Returned when a teacher is onboarded. Include the temporary password so the admin can share it with the teacher.
/// The teacher must change it on first login (MustChangePassword = true).
/// </summary>
public class OnboardTeacherResponse
{
    public string UserId { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
