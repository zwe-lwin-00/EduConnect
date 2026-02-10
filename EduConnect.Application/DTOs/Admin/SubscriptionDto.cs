namespace EduConnect.Application.DTOs.Admin;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime SubscriptionPeriodEnd { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
