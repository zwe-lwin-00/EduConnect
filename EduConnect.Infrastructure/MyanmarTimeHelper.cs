using Microsoft.Extensions.Configuration;

namespace EduConnect.Infrastructure;

/// <summary>
/// Provides app timezone (default Myanmar/Asia/Yangon) for "today" and date-range logic.
/// Configure via appsettings TimeZone:Id and TimeZone:IdFallback. Call Initialize at startup.
/// </summary>
public static class MyanmarTimeHelper
{
    private static TimeZoneInfo? _myanmarTz;
    private static readonly object _lock = new();

    /// <summary>Call at startup so timezone is read from config (TimeZone:Id, TimeZone:IdFallback).</summary>
    public static void Initialize(IConfiguration configuration)
    {
        var section = configuration?.GetSection("TimeZone");
        var id = section?["Id"] ?? "Asia/Yangon";
        var idFallback = section?["IdFallback"] ?? "Myanmar Standard Time";
        lock (_lock)
        {
            try
            {
                _myanmarTz = TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
                _myanmarTz = TimeZoneInfo.FindSystemTimeZoneById(idFallback);
            }
        }
    }

    public static TimeZoneInfo MyanmarTimeZone
    {
        get
        {
            if (_myanmarTz != null) return _myanmarTz;
            lock (_lock)
            {
                if (_myanmarTz != null) return _myanmarTz;
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

    /// <summary>Monday of the week containing the given Myanmar date.</summary>
    public static DateTime GetWeekStartMonday(DateTime myanmarDate)
    {
        var d = myanmarDate.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }

    /// <summary>UTC range for a full week starting at Monday 00:00 Myanmar. weekStartMonday is date-only in Myanmar.</summary>
    public static (DateTime StartUtc, DateTime EndUtc) GetUtcRangeForWeek(DateTime weekStartMonday)
    {
        var start = weekStartMonday.Date;
        var end = start.AddDays(7);
        var startMyanmar = DateTime.SpecifyKind(start, DateTimeKind.Unspecified);
        var endMyanmar = DateTime.SpecifyKind(end, DateTimeKind.Unspecified);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startMyanmar, MyanmarTimeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endMyanmar, MyanmarTimeZone);
        return (startUtc, endUtc);
    }

    /// <summary>Convert UTC to Myanmar time and return time as "HH:mm".</summary>
    public static string FormatTimeUtcToMyanmar(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, MyanmarTimeZone);
        return local.ToString("HH:mm");
    }

    /// <summary>Convert UTC to Myanmar date (date only).</summary>
    public static DateTime UtcToMyanmarDate(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, MyanmarTimeZone).Date;
    }
}
