namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Daily report — Master Doc B8.
/// </summary>
public class DailyReportDto
{
    public DateTime Date { get; set; }
    public int SessionsDelivered { get; set; }
    public decimal HoursConsumed { get; set; }
}

/// <summary>
/// Monthly report — Master Doc B8.
/// </summary>
public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int SessionsDelivered { get; set; }
    public decimal HoursConsumed { get; set; }
    public List<TeacherUtilizationDto> TeacherUtilization { get; set; } = new();
}

public class TeacherUtilizationDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public decimal HoursDelivered { get; set; }
    public int SessionsCount { get; set; }
}
