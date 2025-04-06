namespace MultipleScreenPowersave.Model;

using System.Text;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Dto returned by <see cref="ScreenQuery.GetScreenInformation"/>.
/// </summary>
public class DisplayMonitorInformation
{
    private readonly List<PhysicalMonitorInformation> physicalMonitors = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayMonitorInformation"/> class.
    /// </summary>
    /// <param name="handle">DisplayMonitorHandle as set by the lpfnEnum callback of <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT?, MONITORENUMPROC, LPARAM)"/>.</param>
    /// <param name="isPrimary">Value indicating whether the current DisplayMonitor is the primary one.</param>
    /// <param name="monitorRectangle">MonitorRectangle (position and dimension) of the DisplayMonitor.</param>
    /// <param name="physicalMonitors">Enumerable of all PhysicalMonitors belonging to this DisplayMonitor.</param>
    public DisplayMonitorInformation(
        DisplayMonitorHandle handle,
        bool isPrimary,
        Rect monitorRectangle,
        IEnumerable<PhysicalMonitorInformation> physicalMonitors
    )
    {
        this.Handle = handle;
        this.IsPrimary = isPrimary;
        this.MonitorRectangle = monitorRectangle;

        this.physicalMonitors.AddRange(physicalMonitors);
    }

    /// <summary>
    /// Gets the DisplayMonitor as set by the lpfnEnum callback of <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT?, MONITORENUMPROC, LPARAM)"/>.
    /// </summary>
    public DisplayMonitorHandle Handle { get; }

    /// <summary>
    /// Gets a value indicating whether the current DisplayMonitor is the primary one.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets the MonitorRectangle (position and dimension) of the DisplayMonitor.
    /// </summary>
    public Rect MonitorRectangle { get; }

    /// <summary>
    /// Gets the enumerable of all PhysicalMonitors belonging to this DisplayMonitor.
    /// </summary>
    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors
    {
        get => this.physicalMonitors;
    }

    /// <summary>
    /// Gets the size (dimensions) of the DisplayMonitor.
    /// </summary>
    public Size Size => this.MonitorRectangle.Size;

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("DisplayMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tIsPrimary: {this.IsPrimary},");
        sb.AppendLine($"\tMonitorRectangle: {this.MonitorRectangle},");
        sb.AppendLine();
        sb.AppendLine("\tPhysicalMonitors: [");

        foreach (var monitor in this.physicalMonitors)
        {
            var monitorLines = monitor.ToString()?.Split('\n') ?? [];
            foreach (var line in monitorLines)
            {
                sb.Append("\t\t");
                sb.AppendLine(line);
            }
        }

        sb.AppendLine("\t]");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
