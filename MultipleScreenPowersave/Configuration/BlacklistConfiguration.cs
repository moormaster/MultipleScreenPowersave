namespace MultipleScreenPowersave.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable class representing the configuration for blacklisted monitors or windows.
/// </summary>
public class BlacklistConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistConfiguration"/> class.
    /// </summary>
    public BlacklistConfiguration() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistConfiguration"/> class.
    /// </summary>
    /// <param name="displayMonitors">list of monitors to be blacklisted.</param>
    /// <param name="windows">list of windows to be blacklisted.</param>
    [JsonConstructor]
    public BlacklistConfiguration(
        IList<DisplayMonitorBlacklistEntry> displayMonitors,
        IList<ProcessBlacklistEntry> windows
    )
    {
        foreach (var monitor in displayMonitors ?? [])
            this.DisplayMonitors.Add(monitor);

        foreach (var window in windows ?? [])
            this.Windows.Add(window);
    }

    /// <summary>
    /// Gets the list of monitors to be blacklisted.
    /// </summary>
    public IList<DisplayMonitorBlacklistEntry> DisplayMonitors { get; } = [];

    /// <summary>
    /// Gets the list of windows to be blacklisted.
    /// </summary>
    public IList<ProcessBlacklistEntry> Windows { get; } = [];
}
