namespace MultipleScreenPowersave.Configuration;

using System;
using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

/// <summary>
/// Entry describing a PhysicalMonitor to be blacklisted.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
/// </remarks>
/// <param name="description">Regular expression that should match to the physical monitors description to be blacklisted.</param>
/// <param name="deviceId">Regular expression that should match to the physical monitors deviceId to be blacklisted.</param>
/// <param name="wmiInstanceName">Regular expression that should match to the physical monitors wmiInstanceName to be blacklisted.</param>
/// <param name="index">0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.</param>
/// <exception cref="ArgumentNullException">All arguments were null.</exception>
public class PhysicalMonitorBlacklistEntry(
    string? description,
    string? deviceId,
    string? wmiInstanceName,
    uint? index
)
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
    /// Gets the pattern for the regular expression that should match to the physical monitors deviceId to be blacklisted.
    /// </summary>
    public string? DeviceId { get; } = deviceId;

    /// <summary>
    /// Gets the regular expression that should match to the physical monitors deviceId to be blacklisted.
    /// </summary>
    public Regex? DeviceIdRegex => this.DeviceId != null ? new Regex(this.DeviceId) : null;

    /// <summary>
    /// Gets the 0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.
    /// </summary>
    public uint? Index { get; } = index;

    /// <summary>
    /// Gets the pattern for the regular expression that should match to the physical monitors wmiInstanceName to be blacklisted.
    /// </summary>
    public string? WmiInstanceName { get; } = wmiInstanceName;

    /// <summary>
    /// Gets the regular expression that should match to the physical monitors wmiInstanceName to be blacklisted.
    /// </summary>
    public Regex? WmiInstanceNameRegex =>
        this.WmiInstanceName != null ? new Regex(this.WmiInstanceName) : null;

    /// <summary>
    /// Determines whether the current entry matches to a DisplayMonitor.
    /// </summary>
    /// <param name="displayMonitor">DisplayMonitor to check against.</param>
    /// <returns>True iff the displayMonitor contains at least one physical monitor that should be blacklisted.</returns>
    public bool IsMatch(DisplayMonitorInformation displayMonitor)
    {
        if (this.Index.HasValue)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                physicalMonitor.Index == this.Index
            );
        }

        if (this.DescriptionRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.DescriptionRegex.IsMatch(physicalMonitor.Description)
            );
        }

        if (this.DeviceIdRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.DeviceIdRegex.IsMatch(physicalMonitor.DeviceId ?? string.Empty)
            );
        }

        if (this.WmiInstanceNameRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.WmiInstanceNameRegex.IsMatch(physicalMonitor.WmiInstanceName ?? string.Empty)
            );
        }

        return true;
    }
}
