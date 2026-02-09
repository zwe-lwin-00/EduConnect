namespace EduConnect.API.Models;

/// <summary>
/// Standard API error response for consistent frontend handling and support.
/// </summary>
public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string? RequestId { get; set; }
}
