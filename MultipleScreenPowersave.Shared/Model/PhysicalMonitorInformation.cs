namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;

/// <summary>
/// Dto returned by <see cref="IScreenQuery.GetScreenInformation"/>.
/// </summary>
/// <param name="handle">PhysicalMonitorHandle.</param>
public class PhysicalMonitorInformation(PhysicalMonitorHandle handle)
{
    /// <summary>
    /// Gets the PhysicalMonitorHandle.
    /// </summary>
    public PhysicalMonitorHandle Handle { get; } = handle;

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
        sb.AppendLine($"\tWmiInstanceName: {this.WmiInstanceName}");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
