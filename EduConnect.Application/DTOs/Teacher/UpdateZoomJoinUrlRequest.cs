namespace EduConnect.Application.DTOs.Teacher;

/// <summary>Request to set or clear the teacher's default Zoom meeting join URL for 1:1 teaching.</summary>
public class UpdateZoomJoinUrlRequest
{
    public string? ZoomJoinUrl { get; set; }
}
