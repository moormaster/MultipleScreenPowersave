using MultipleScreenPowersave.Model.Handles;
using System.Text;

namespace MultipleScreenPowersave.Model;

public class DisplayMonitorInformation
{
    public DisplayMonitorHandle Handle { get; }

    public bool IsPrimary { get; }

    public Rectangle MonitorRectangle { get; }

    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors { get => _physicalMonitors; }

    public Size Size
        => MonitorRectangle.Size;

    private List<PhysicalMonitorInformation> _physicalMonitors = new List<PhysicalMonitorInformation>();

    public DisplayMonitorInformation(DisplayMonitorHandle handle, bool isPrimary, Rectangle monitorRectangle, IEnumerable<PhysicalMonitorInformation> physicalMonitors)
    {
        Handle = handle;
        IsPrimary = isPrimary;
        MonitorRectangle = monitorRectangle;

        _physicalMonitors.AddRange(physicalMonitors);
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("DisplayMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tIsPrimary: {this.IsPrimary},");
        sb.AppendLine($"\tMonitorRectangle: {this.MonitorRectangle},");
        sb.AppendLine();
        sb.AppendLine("\tPhysicalMonitors: [");

        foreach (var monitor in this._physicalMonitors)
        {
            var monitorLines = monitor.ToString()?.Split('\n') ?? [];
            monitorLines.All(monitorLine =>
            {
                sb.Append("\t\t");
                sb.AppendLine(monitorLine);

                return true;
            });
        }

        sb.AppendLine("\t]");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
