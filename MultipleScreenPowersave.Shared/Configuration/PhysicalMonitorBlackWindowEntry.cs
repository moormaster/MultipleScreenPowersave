namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.Model;

/// <summary>
/// Configuration entry for <see cref="BlackWindowServiceOptions"/> matching to physical monitors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhysicalMonitorBlacklistEntry"/> class.
/// </remarks>
/// <param name="invertBackgroundColor">Value indicating whether the background color of the black window should be inverted.</param>
/// <param name="description">Regular expression that should match to physical monitors description..</param>
/// <param name="deviceId">Regular expression that should match to physical monitors deviceId.</param>
/// <param name="index">0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.</param>
/// <param name="linuxBacklightDevice">Regular expression that should match the backlight control linux device.</param>
/// <param name="linuxI2cDevice">Regular expression that should match the I2cBus linux device.</param>
/// <param name="wmiInstanceName">Regular expression that should match to the physical monitors wmiInstanceName.</param>
public class PhysicalMonitorBlackWindowEntry(
    bool invertBackgroundColor = false,
    string? description = null,
    string? deviceId = null,
    uint? index = null,
    string? linuxBacklightDevice = null,
    string? linuxI2cDevice = null,
    string? wmiInstanceName = null
)
    : PhysicalMonitorEntryBase(
        description,
        deviceId,
        index,
        linuxBacklightDevice,
        linuxI2cDevice,
        wmiInstanceName
    )
{
    /// <summary>
    /// Gets a value indicating whether the background color of the black window should be inverted.
    /// This is useful for LCD monitors where black screens consume more power that white screens.
    /// </summary>
    public bool InvertBackgroundColor { get; } = invertBackgroundColor;
}
