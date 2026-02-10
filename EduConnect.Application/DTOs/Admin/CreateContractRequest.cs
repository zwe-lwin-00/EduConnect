namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin creates contract: assign teacher, monthly subscription from 1st to last day of StartDate's month — Master Doc B4.
/// </summary>
public class CreateContractRequest
{
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    /// <summary>Subscription period is the calendar month containing this date (1st–last day of month).</summary>
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
