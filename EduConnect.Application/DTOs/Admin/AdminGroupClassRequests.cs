namespace EduConnect.Application.DTOs.Admin;

/// <summary>Admin creates a group class and assigns a teacher.</summary>
public class AdminCreateGroupClassRequest
{
    public string Name { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string? ZoomJoinUrl { get; set; }
}

/// <summary>Admin can update name, assigned teacher, active, and Zoom URL.</summary>
public class AdminUpdateGroupClassRequest
{
    public string? Name { get; set; }
    public int TeacherId { get; set; }
    public bool IsActive { get; set; }
    public string? ZoomJoinUrl { get; set; }
}
