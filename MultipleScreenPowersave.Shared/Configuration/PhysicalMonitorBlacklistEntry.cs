namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a PhysicalMonitor to be blacklisted.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
/// </remarks>
/// <param name="description">Regular expression that should match to the physical monitors description to be blacklisted.</param>
/// <exception cref="ArgumentNullException">All arguments were null.</exception>
public class PhysicalMonitorBlacklistEntry(string? description)
{

    /// <summary>
    /// Gets the the pattern for the regular expression that should match to the physical monitors description to be blacklisted.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets the regular expression that should match to the physical monitors description to be blacklisted.
    /// </summary>
    public Regex? DescriptionRegex => this.Description != null ? new Regex(this.Description) : null;

    /// <summary>
    /// Determines whether the current entry matches to a DisplayMonitor.
    /// </summary>
    /// <param name="displayMonitor">DisplayMonitor to check against.</param>
    /// <returns>True iff the displayMonitor contains at least one physical monitor that should be blacklisted.</returns>
    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.DescriptionRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.DescriptionRegex.IsMatch(physicalMonitor.Description)
            );
        }

        return true;
    }
}
