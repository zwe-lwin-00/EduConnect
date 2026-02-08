namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Returned when a parent is created. Includes the temporary password so the admin can share it with the parent.
/// The parent must change it on first login (MustChangePassword = true).
/// </summary>
public class CreateParentResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
