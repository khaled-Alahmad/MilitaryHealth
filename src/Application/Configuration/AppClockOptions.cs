namespace Application.Configuration;

/// <summary>
/// Configuration for application local time (e.g. timezone for Syria/Jordan).
/// Bind from "AppClock" in appsettings.
/// </summary>
public class AppClockOptions
{
    public const string SectionName = "AppClock";

    /// <summary>
    /// IANA timezone id (e.g. "Asia/Damascus", "Asia/Amman") or Windows id (e.g. "Syria Standard Time").
    /// Default: "Asia/Damascus" (UTC+3).
    /// </summary>
    public string TimeZoneId { get; set; } = "Asia/Damascus";
}
