namespace EduConnect.Application.DTOs.Admin;

public class CreateHolidayRequest
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateHolidayRequest
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
