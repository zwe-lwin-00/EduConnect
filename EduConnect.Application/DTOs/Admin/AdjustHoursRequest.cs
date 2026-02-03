namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin override: adjust session hours with reason — Master Doc B6. Every override logged.
/// </summary>
public class AdjustHoursRequest
{
    public decimal Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
}
