namespace EduConnect.Domain.Entities;

/// <summary>
/// Key-value setting for admin-configurable data (e.g. school name, notices, custom flags).
/// </summary>
public class SystemSetting
{
    public int Id { get; set; }
    /// <summary>Unique key (e.g. "SchoolName", "AcademicYearStart").</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Value stored as string; interpret per key (e.g. date, number, text).</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Optional description for admin UI.</summary>
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
