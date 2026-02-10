namespace EduConnect.Application.DTOs.Teacher;

public class GroupCheckInRequest
{
    public int GroupClassId { get; set; }
}

public class GroupCheckOutRequest
{
    public int GroupSessionId { get; set; }
    public string LessonNotes { get; set; } = string.Empty;
}

public class CreateGroupClassRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ZoomJoinUrl { get; set; }
}

public class UpdateGroupClassRequest
{
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public string? ZoomJoinUrl { get; set; }
}

public class EnrollInGroupClassRequest
{
    public int StudentId { get; set; }
    /// <summary>Legacy / teacher: 1:1 contract for this student with this teacher.</summary>
    public int? ContractId { get; set; }
    /// <summary>Admin only: parent-paid Group subscription for this student. When set, enrollment uses subscription.</summary>
    public int? SubscriptionId { get; set; }
}
