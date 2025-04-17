namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;

/// <summary>
/// Dto returned by <see cref="IScreenQuery.GetScreenInformation"/>.
/// </summary>
/// <param name="handle">PhysicalMonitorHandle.</param>
/// <param name="index">0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.</param>
public class PhysicalMonitorInformation(PhysicalMonitorHandle handle, uint index)
{
    /// <summary>
    /// Gets the PhysicalMonitorHandle.
    /// </summary>
    public PhysicalMonitorHandle Handle { get; } = handle;

    /// <summary>
    /// Gets the 0-based index within <see cref="ScreenInformation.PhysicalMonitors"/> collection.
    /// </summary>
    public uint Index { get; } = index;

    /// <summary>
    /// Gets or sets the text description of the physical monitor.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the windows-only deviceId used to query windows-registry with.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the hexadecimal string representing first 128 byte of
    /// <see href="https://de.wikipedia.org/wiki/Extended_Display_Identification_Data">EDID</see> data.
    /// </summary>
    public string? EdidHex { get; set; }

    /// <summary>
    /// Gets or sets the I2cBus linux device.
    /// </summary>
    public string? LinuxI2cDevice { get; set; }

    /// <summary>
    /// Gets or sets the backlight control linux device.
    /// </summary>
    public string? LinuxBacklightDevice { get; set; }

    /// <summary>
    /// Gets or sets the windows-only WMI instance name.
    /// </summary>
    public string? WmiInstanceName { get; set; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("PhysicalMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tDescription: {this.Description},");
        sb.AppendLine($"\tDeviceId: {this.DeviceId},");
        sb.AppendLine($"\tEdidHex: {this.EdidHex}");
        sb.AppendLine($"\tLinuxI2cDevice: {this.LinuxI2cDevice}");
        sb.AppendLine($"\tLinuxBacklightDevice: {this.LinuxBacklightDevice}");
        sb.AppendLine($"\tWmiInstanceName: {this.WmiInstanceName}");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
