using EduConnect.Application.Common.Exceptions;

namespace EduConnect.Application.Common;

/// <summary>
/// Validates class schedule fields (days of week, start/end time) to catch unexpected or invalid values.
/// </summary>
public static class ScheduleValidation
{
    /// <summary>
    /// Validates days of week (comma-separated ISO 1-7) and start/end time (end must be after start if both set).
    /// Throws <see cref="BusinessException"/> with code INVALID_SCHEDULE when invalid.
    /// </summary>
    public static void Validate(string? daysOfWeek, TimeOnly? startTime, TimeOnly? endTime)
    {
        if (!string.IsNullOrWhiteSpace(daysOfWeek))
        {
            var parts = daysOfWeek.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            var seen = new HashSet<int>();
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!int.TryParse(trimmed, out var day) || day < 1 || day > 7)
                    throw new BusinessException("Days of week must be comma-separated numbers 1–7 (1=Monday, 7=Sunday).", "INVALID_SCHEDULE");
                if (!seen.Add(day))
                    throw new BusinessException("Days of week must not contain duplicates.", "INVALID_SCHEDULE");
            }
        }

        if (startTime.HasValue && endTime.HasValue && endTime.Value <= startTime.Value)
            throw new BusinessException("End time must be after start time.", "INVALID_SCHEDULE");
    }
}
