namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Dto returned by <see cref="IScreenQuery.GetScreenInformation"/>.
/// </summary>
/// <param name="handle">PhysicalMonitorHandle as set by <see cref="PInvoke.GetPhysicalMonitorsFromHMONITOR(HMONITOR, Span{PHYSICAL_MONITOR})"/>.</param>
/// <param name="displayMonitor">DisplayMonitorHandle as set by the lpfnEnum callback of <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT?, MONITORENUMPROC, LPARAM)"/>.</param>
/// <param name="description">Text description of the physical monitor as set by <see cref="PInvoke.GetPhysicalMonitorsFromHMONITOR(HMONITOR, Span{PHYSICAL_MONITOR})"/>.</param>
public class PhysicalMonitorInformation(
    PhysicalMonitorHandle handle,
    DisplayMonitorHandle displayMonitor,
    string description
)
{
    /// <summary>
    /// Gets the PhysicalMonitorHandle as set by <see cref="PInvoke.GetPhysicalMonitorsFromHMONITOR(HMONITOR, Span{PHYSICAL_MONITOR})"/>.
    /// </summary>
    public PhysicalMonitorHandle Handle { get; } = handle;

    /// <summary>
    /// Gets the text description of the physical monitor as set by <see cref="PInvoke.GetPhysicalMonitorsFromHMONITOR(HMONITOR, Span{PHYSICAL_MONITOR})"/>.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets the DisplayMonitorHandle as set by the lpfnEnum callback of <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT?, MONITORENUMPROC, LPARAM)"/>.
    /// </summary>
    public DisplayMonitorHandle DisplayMonitor { get; } = displayMonitor;

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("PhysicalMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tDescription: {this.Description},");
        sb.AppendLine($"\tDisplayMonitor: {this.DisplayMonitor}");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
