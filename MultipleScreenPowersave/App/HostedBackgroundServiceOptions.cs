namespace MultipleScreenPowersave.App;

/// <summary>
/// Options for <see cref="HostedBackgroundService"/>.
/// </summary>
public class HostedBackgroundServiceOptions
{
    /// <summary>
    /// Gets or sets the number of milliseconds between each attempt to determine actively used monitors.
    /// </summary>
    public int SleepTimeMs { get; set; } = 1000;
}
