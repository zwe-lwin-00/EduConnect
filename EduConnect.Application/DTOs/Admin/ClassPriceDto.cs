namespace EduConnect.Application.DTOs.Admin;

public class ClassPriceDto
{
    public int Id { get; set; }
    public int GradeLevel { get; set; }
    public string GradeLevelName { get; set; } = string.Empty;
    public int ClassType { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public decimal PricePerMonth { get; set; }
    public string Currency { get; set; } = "MMK";
    public DateTime UpdatedAt { get; set; }
}
