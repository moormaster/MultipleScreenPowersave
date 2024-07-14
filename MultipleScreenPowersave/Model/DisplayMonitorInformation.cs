namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;

public class DisplayMonitorInformation
{
    private readonly List<PhysicalMonitorInformation> physicalMonitors = [];

    public DisplayMonitorInformation(DisplayMonitorHandle handle, bool isPrimary, Rectangle monitorRectangle, IEnumerable<PhysicalMonitorInformation> physicalMonitors)
    {
        this.Handle = handle;
        this.IsPrimary = isPrimary;
        this.MonitorRectangle = monitorRectangle;

        this.physicalMonitors.AddRange(physicalMonitors);
    }

    public DisplayMonitorHandle Handle { get; }

    public bool IsPrimary { get; }

    public Rectangle MonitorRectangle { get; }

    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors { get => this.physicalMonitors; }

    public Size Size
        => this.MonitorRectangle.Size;

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
