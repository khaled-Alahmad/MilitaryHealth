namespace Application.Abstractions;

/// <summary>
/// Provides the current date/time in the application's configured local timezone
/// (e.g. Asia/Damascus) for display and business logic.
/// </summary>
public interface IAppClock
{
    /// <summary>
    /// Current date and time in the configured local timezone.
    /// Use this for CreatedAt, ModifiedAt, and supervisor/reception timestamps.
    /// </summary>
    DateTime Now { get; }
}
