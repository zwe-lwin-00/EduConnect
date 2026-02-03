namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin dashboard daily command center — Master Doc B1.
/// </summary>
public class DashboardDto
{
    public List<DashboardAlertDto> Alerts { get; set; } = new();
    public List<TodaySessionDto> TodaySessions { get; set; } = new();
    public int PendingActionsCount { get; set; }
    public RevenueSnapshotDto RevenueSnapshot { get; set; } = new();
}

public class DashboardAlertDto
{
    public string Type { get; set; } = string.Empty; // LowHours | NoCheckIn | ContractExpiring
    public string Message { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
}

public class TodaySessionDto
{
    public int Id { get; set; }
    public string ContractId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class RevenueSnapshotDto
{
    public decimal RevenueThisMonth { get; set; }
    public int SessionsDeliveredThisMonth { get; set; }
    public decimal HoursConsumedThisMonth { get; set; }
}
