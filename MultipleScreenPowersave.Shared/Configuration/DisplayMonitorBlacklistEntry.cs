namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a DisplayMonitor to be blacklisted.
/// </summary>
public record class DisplayMonitorBlacklistEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayMonitorBlacklistEntry"/> class.
    /// </summary>
    /// <param name="isPrimary">Flag indicating wether the primary monitor should be blacklisted.</param>
    /// <param name="physicalMonitors">List of physical monitors to be blacklisted.</param>
    /// <exception cref="ArgumentNullException">PhysicalMonitors argument was null.</exception>
    public DisplayMonitorBlacklistEntry(
        bool? isPrimary,
        IList<PhysicalMonitorBlacklistEntry>? physicalMonitors
    )
    {
        if (isPrimary == null && physicalMonitors == null)
        {
            throw new ArgumentNullException(
                nameof(isPrimary),
                "At least one property must be != null"
            );
        }

        this.IsPrimary = isPrimary;

        foreach (var monitor in physicalMonitors ?? [])
            this.PhysicalMonitors.Add(monitor);
    }

    /// <summary>
    /// Gets the flag indicating whether the primary monitor should be blacklisted.
    /// </summary>
    public bool? IsPrimary { get; }

    /// <summary>
    /// Gets the list of physical monitors to be blacklisted.
    /// </summary>
    public IList<PhysicalMonitorBlacklistEntry> PhysicalMonitors { get; } = [];

    /// <summary>
    /// Determines whether the current entry matches to a DisplayMonitor.
    /// </summary>
    /// <param name="displayMonitor">DisplayMonitor to check against.</param>
    /// <returns>True iff the displayMonitor should be blacklisted according to this entry.</returns>
    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.IsPrimary != null)
        {
            if (this.IsPrimary != displayMonitor.IsPrimary)
                return false;
        }

        if (
            this.PhysicalMonitors.Any()
            && !this.PhysicalMonitors.Any(physicalMonitorEntry =>
                physicalMonitorEntry.IsMatch(displayMonitor)
            )
        )
            return false;

        return true;
    }
}
