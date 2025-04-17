namespace MultipleScreenPowersave.Configuration;

using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

/// <summary>
/// Serializable class representing the configuration for blacklisted monitors or windows.
/// </summary>
public class BlacklistOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistOptions"/> class.
    /// </summary>
    public BlacklistOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistOptions"/> class.
    /// </summary>
    /// <param name="ignoreMouseAtRectangles">list of <see cref="Rect"/>'s to ignore mouse cursor when its located in it.</param>
    /// <param name="displayMonitors">list of monitors to be blacklisted.</param>
    /// <param name="windows">list of windows to be blacklisted.</param>
    [JsonConstructor]
    public BlacklistOptions(
        IList<Rect> ignoreMouseAtRectangles,
        IList<DisplayMonitorBlacklistEntry> displayMonitors,
        IList<ProcessBlacklistEntry> windows
    )
    {
        this.IgnoreMouseAtRectangles = ignoreMouseAtRectangles ?? [];

        foreach (var monitor in displayMonitors ?? [])
            this.DisplayMonitors.Add(monitor);

        foreach (var window in windows ?? [])
            this.Windows.Add(window);
    }

    /// <summary>
    /// Gets the list of <see cref="Rect"/>'s to ignore mouse cursor when its located in it.
    /// </summary>
    public IList<Rect> IgnoreMouseAtRectangles { get; } = [];

    /// <summary>
    /// Gets the list of monitors to be blacklisted.
    /// </summary>
    public IList<DisplayMonitorBlacklistEntry> DisplayMonitors { get; } = [];

    /// <summary>
    /// Gets the list of windows to be blacklisted.
    /// </summary>
    public IList<ProcessBlacklistEntry> Windows { get; } = [];
}
