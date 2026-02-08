namespace EduConnect.Domain.Entities;

/// <summary>
/// Links a student to a group class. ContractId is the 1:1 contract used to deduct hours when the student attends a group session.
/// </summary>
public class GroupClassEnrollment
{
    public int Id { get; set; }
    public int GroupClassId { get; set; }
    public int StudentId { get; set; }
    public int ContractId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public GroupClass GroupClass { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ContractSession ContractSession { get; set; } = null!;
}
