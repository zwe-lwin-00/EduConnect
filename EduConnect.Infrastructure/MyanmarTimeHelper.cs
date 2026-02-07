namespace EduConnect.Infrastructure;

/// <summary>
/// Provides Myanmar timezone (Asia/Yangon, UTC+6:30) for "today" and date-range logic.
/// All app times are interpreted in Myanmar time.
/// </summary>
public static class MyanmarTimeHelper
{
    private static TimeZoneInfo? _myanmarTz;

    public static TimeZoneInfo MyanmarTimeZone
    {
        get
        {
            if (_myanmarTz != null) return _myanmarTz;
            // IANA ID works on Linux/macOS and .NET 6+ on Windows; fallback for older Windows
            try
            {
                _myanmarTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yangon");
            }
            catch
            {
                _myanmarTz = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
            }
            return _myanmarTz;
        }
    }

    /// <summary>Current date and time in Myanmar.</summary>
    public static DateTime GetMyanmarNow()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MyanmarTimeZone);
    }

    /// <summary>Date part of "today" in Myanmar (no time).</summary>
    public static DateTime GetTodayInMyanmar()
    {
        return GetMyanmarNow().Date;
    }

    /// <summary>UTC range for "today" in Myanmar: [startUtc, endUtc). Use for filtering stored UTC times.</summary>
    public static (DateTime StartUtc, DateTime EndUtc) GetTodayUtcRange()
    {
        var today = GetTodayInMyanmar();
        return GetUtcRangeForMyanmarDate(today);
    }

    /// <summary>UTC range for a given calendar date in Myanmar. date is treated as date-only in Myanmar.</summary>
    public static (DateTime StartUtc, DateTime EndUtc) GetUtcRangeForMyanmarDate(DateTime myanmarDate)
    {
        var dateOnly = myanmarDate.Date;
        var startMyanmar = DateTime.SpecifyKind(dateOnly, DateTimeKind.Unspecified);
        var endMyanmar = startMyanmar.AddDays(1);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startMyanmar, MyanmarTimeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endMyanmar, MyanmarTimeZone);
        return (startUtc, endUtc);
    }
}
