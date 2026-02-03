namespace EduConnect.Application.DTOs.Teacher;

/// <summary>
/// Check-out requires lesson notes — Master Doc 11B B4. Check-out blocked until notes entered.
/// </summary>
public class CheckOutRequest
{
    public int SessionId { get; set; }
    public string LessonNotes { get; set; } = string.Empty;
}
