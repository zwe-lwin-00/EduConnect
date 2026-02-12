namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Admin-only parent creation — Master Doc B3.
/// </summary>
public class CreateParentRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
