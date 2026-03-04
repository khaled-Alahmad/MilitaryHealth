using Application.Abstractions;
using Application.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Returns current time in the configured local timezone so that
/// CreatedAt, ModifiedAt, and supervisor timestamps match user expectations.
/// </summary>
public class AppClock : IAppClock
{
    private readonly TimeZoneInfo _timeZone;

    public AppClock(IOptions<AppClockOptions> options)
    {
        var id = options?.Value?.TimeZoneId ?? "Asia/Damascus";
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            _timeZone = TimeZoneInfo.Utc;
        }
    }

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
}
