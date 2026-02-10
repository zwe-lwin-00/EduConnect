namespace EduConnect.Domain.Entities;

/// <summary>
/// A holiday date (no classes / school closed). Admin-managed for scheduling and reporting.
/// </summary>
public class Holiday
{
    public int Id { get; set; }
    /// <summary>Date of the holiday (date only).</summary>
    public DateTime Date { get; set; }
    /// <summary>Display name (e.g. "National Day", "Thingyan").</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Optional description or notes.</summary>
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
