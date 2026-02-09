namespace EduConnect.Application.DTOs.GroupClass;

public class GroupClassDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    /// <summary>Set when returned for admin (list/detail).</summary>
    public string? TeacherName { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    /// <summary>Zoom meeting join URL for this group class.</summary>
    public string? ZoomJoinUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EnrolledCount { get; set; }
}

public class GroupClassEnrollmentDto
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public string ContractIdDisplay { get; set; } = string.Empty;
}

public class GroupSessionDto
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public string GroupClassName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal TotalDurationHours { get; set; }
    public string? LessonNotes { get; set; }
    /// <summary>Zoom meeting join URL for this session (group class default or session override).</summary>
    public string? ZoomJoinUrl { get; set; }
    public int Status { get; set; }
    public int AttendeeCount { get; set; }
}
