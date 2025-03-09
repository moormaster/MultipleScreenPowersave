namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a PhysicalMonitor to be blacklisted.
/// </summary>
public class PhysicalMonitorBlacklistEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
    /// </summary>
    /// <param name="description">Regular expression that should match to the physical monitors description to be blacklisted.</param>
    /// <exception cref="ArgumentNullException">All arguments were null.</exception>
    public PhysicalMonitorBlacklistEntry(Regex? description)
    {
        if (description == null)
        {
            throw new ArgumentNullException(
                nameof(description),
                "At least one property must be != null"
            );
        }

        this.Description = description;
    }

    /// <summary>
    /// Gets the regular expression that should match to the physical monitors description to be blacklisted.
    /// </summary>
    public Regex? Description { get; }

    /// <summary>
    /// Determines whether the current entry matches to a DisplayMonitor.
    /// </summary>
    /// <param name="displayMonitor">DisplayMonitor to check against.</param>
    /// <returns>True iff the displayMonitor contains at least one physical monitor that should be blacklisted.</returns>
    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.Description != null)
        {
            if (
                !displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                {
                    if (!this.Description.IsMatch(physicalMonitor.Description))
                        return false;

                    return true;
                })
            )
                return false;
        }

        return true;
    }
}
