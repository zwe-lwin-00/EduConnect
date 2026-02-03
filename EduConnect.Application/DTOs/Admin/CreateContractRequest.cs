namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin creates contract: assign teacher, package hours, validity — Master Doc B4.
/// </summary>
public class CreateContractRequest
{
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int PackageHours { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
