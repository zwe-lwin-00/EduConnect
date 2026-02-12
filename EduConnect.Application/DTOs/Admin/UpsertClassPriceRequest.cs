namespace EduConnect.Application.DTOs.Admin;

/// <summary>Create or update a class price for a grade and class type. Unique on (GradeLevel, ClassType).</summary>
public class UpsertClassPriceRequest
{
    /// <summary>Grade level (e.g. 1 = P1, 2 = P2).</summary>
    public int GradeLevel { get; set; }
    /// <summary>1 = One-to-one, 2 = Group.</summary>
    public int ClassType { get; set; }
    public decimal PricePerMonth { get; set; }
    public string? Currency { get; set; }
}
