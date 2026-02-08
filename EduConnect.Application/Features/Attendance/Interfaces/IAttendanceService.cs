using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.GroupClass;

namespace EduConnect.Application.Features.Attendance.Interfaces;

public interface IAttendanceService : IService
{
    Task<int> CheckInAsync(int teacherId, int contractId);
    Task<bool> CheckOutAsync(int teacherId, int attendanceLogId, string lessonNotes);
    Task<int> CheckInGroupAsync(int teacherId, int groupClassId);
    Task<bool> CheckOutGroupAsync(int teacherId, int groupSessionId, string lessonNotes);
    Task<AttendanceSessionDto> GetSessionByIdAsync(int sessionId);
    Task<List<AttendanceSessionDto>> GetSessionsByContractAsync(int contractId);
    Task<List<GroupSessionDto>> GetGroupSessionsByTeacherAsync(int teacherId, DateTime? from = null, DateTime? to = null);
}

public class AttendanceSessionDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int ContractId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal HoursUsed { get; set; }
    public string? LessonNotes { get; set; }
    public string? ProgressReport { get; set; }
    public int Status { get; set; }
}
