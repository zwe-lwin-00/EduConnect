namespace EduConnect.Application.DTOs.Parent;

/// <summary>
/// Parent's view of their child (student) — Master Doc 11C Phase 1. No student login; all via Parent.
/// </summary>
public class ParentStudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    /// <summary>Latest subscription end (for display).</summary>
    public DateTime? SubscriptionValidUntil { get; set; }
    public string? AssignedTeacherName { get; set; }
    public int ActiveContractsCount { get; set; }
}
