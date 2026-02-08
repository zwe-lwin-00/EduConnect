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
}

public class UpdateGroupClassRequest
{
    public string? Name { get; set; }
    public bool IsActive { get; set; }
}

public class EnrollInGroupClassRequest
{
    public int StudentId { get; set; }
    public int ContractId { get; set; }
}
