using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

/// <summary>
/// Admin-configurable price per grade and class type (One-to-one or Group).
/// Used for display and invoicing; one row per (GradeLevel, ClassType).
/// </summary>
public class ClassPrice
{
    public int Id { get; set; }
    /// <summary>Grade level (e.g. P1, P2).</summary>
    public GradeLevel GradeLevel { get; set; }
    /// <summary>1 = One-to-one, 2 = Group (matches SubscriptionType).</summary>
    public int ClassType { get; set; }
    /// <summary>Price per month (e.g. per subscription month).</summary>
    public decimal PricePerMonth { get; set; }
    /// <summary>Currency code (e.g. MMK, USD).</summary>
    public string Currency { get; set; } = "MMK";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
