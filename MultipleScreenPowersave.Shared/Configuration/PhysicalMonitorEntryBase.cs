namespace MultipleScreenPowersave.Configuration;

using System.Text.RegularExpressions;
using MultipleScreenPowersave.Model;

/// <summary>
/// Base class for a configuration entry matching to a PhysicalMonitor.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
/// </remarks>
/// <param name="description">Regular expression that should match to physical monitors description..</param>
/// <param name="deviceId">Regular expression that should match to physical monitors deviceId.</param>
/// <param name="index">0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.</param>
/// <param name="linuxBacklightDevice">Regular expression that should match the backlight control linux device.</param>
/// <param name="linuxI2cDevice">Regular expression that should match the I2cBus linux device.</param>
/// <param name="wmiInstanceName">Regular expression that should match to the physical monitors wmiInstanceName.</param>
public abstract class PhysicalMonitorEntryBase(
    string? description,
    string? deviceId,
    uint? index,
    string? linuxBacklightDevice,
    string? linuxI2cDevice,
    string? wmiInstanceName
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
    /// Gets the pattern for the regular expression that should match the backlight control linux device.
    /// </summary>
    public string? LinuxBacklightDevice { get; } = linuxBacklightDevice;

    /// <summary>
    /// Gets the regular expression that should match the backlight control linux device.
    /// </summary>
    public Regex? LinuxBacklightDeviceRegex =>
        this.LinuxBacklightDevice != null ? new Regex(this.LinuxBacklightDevice) : null;

    /// <summary>
    /// Gets the pattern for the regular expression that should match the I2cBus linux device.
    /// </summary>
    public string? LinuxI2cDevice { get; } = linuxI2cDevice;

    /// <summary>
    /// Gets the regular expression that should match the I2cBus linux device.
    /// </summary>
    public Regex? LinuxI2cDeviceRegex =>
        this.LinuxI2cDevice != null ? new Regex(this.LinuxI2cDevice) : null;

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

        if (this.LinuxBacklightDeviceRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.LinuxBacklightDeviceRegex.IsMatch(
                    physicalMonitor.LinuxBacklightDevice ?? string.Empty
                )
            );
        }

        if (this.LinuxI2cDeviceRegex != null)
        {
            return displayMonitor.PhysicalMonitors.Any(physicalMonitor =>
                this.LinuxI2cDeviceRegex.IsMatch(physicalMonitor.LinuxI2cDevice ?? string.Empty)
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
