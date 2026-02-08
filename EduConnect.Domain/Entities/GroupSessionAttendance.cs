namespace EduConnect.Domain.Entities;

/// <summary>
/// Per-student attendance and hours deducted for one group session.
/// </summary>
public class GroupSessionAttendance
{
    public int Id { get; set; }
    public int GroupSessionId { get; set; }
    public int StudentId { get; set; }
    public int ContractId { get; set; }
    public decimal HoursUsed { get; set; }

    public GroupSession GroupSession { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ContractSession ContractSession { get; set; } = null!;
}
