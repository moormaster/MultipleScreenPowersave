namespace MultipleScreenPowersave.Model;

using System.Text;
using MultipleScreenPowersave.Model.Handles;

public class PhysicalMonitorInformation
{
    public PhysicalMonitorInformation(PhysicalMonitorHandle handle, DisplayMonitorHandle displayMonitor, string description)
    {
        this.Handle = handle;
        this.Description = description;
        this.DisplayMonitor = displayMonitor;
    }

    public PhysicalMonitorHandle Handle { get; }

    public string Description { get; }

    public DisplayMonitorHandle DisplayMonitor { get; }

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
