namespace MultipleScreenPowersave.Model;

using System.Text;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;

/// <summary>
/// Dto returned by <see cref="IScreenQuery.GetScreenInformation"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DisplayMonitorInformation"/> class.
/// </remarks>
/// <param name="handle">DisplayMonitorHandle.</param>
/// <param name="isPrimary">Value indicating whether the current DisplayMonitor is the primary one.</param>
/// <param name="monitorRectangle">MonitorRectangle (position and dimension) of the DisplayMonitor.</param>
public class DisplayMonitorInformation(
    DisplayMonitorHandle handle,
    bool isPrimary,
    Rect monitorRectangle
)
{
    /// <summary>
    /// Gets the DisplayMonitor.
    /// </summary>
    public DisplayMonitorHandle Handle { get; } = handle;

    /// <summary>
    /// Gets a value indicating whether the current DisplayMonitor is the primary one.
    /// </summary>
    public bool IsPrimary { get; } = isPrimary;

    /// <summary>
    /// Gets the MonitorRectangle (position and dimension) of the DisplayMonitor.
    /// </summary>
    public Rect MonitorRectangle { get; } = monitorRectangle;

    /// <summary>
    /// Gets the List of all PhysicalMonitors belonging to this DisplayMonitor.
    /// </summary>
    public List<PhysicalMonitorInformation> PhysicalMonitors { get; } = [];

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

        foreach (var monitor in this.PhysicalMonitors)
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
