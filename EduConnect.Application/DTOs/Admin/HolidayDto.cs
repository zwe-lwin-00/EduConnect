namespace EduConnect.Application.DTOs.Admin;

public class HolidayDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
