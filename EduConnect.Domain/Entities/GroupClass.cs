namespace EduConnect.Domain.Entities;

/// <summary>
/// A group class: one teacher, multiple students. Students must have an active 1:1 contract with the teacher to be enrolled.
/// </summary>
public class GroupClass
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TeacherProfile Teacher { get; set; } = null!;
    public ICollection<GroupClassEnrollment> Enrollments { get; set; } = new List<GroupClassEnrollment>();
    public ICollection<GroupSession> Sessions { get; set; } = new List<GroupSession>();
}
