using MultipleScreenPowersave.Model.Handles;
using System.Text;

namespace MultipleScreenPowersave.Model;

public class PhysicalMonitorInformation
{
    public PhysicalMonitorHandle Handle { get; }

    public string Description { get; }

    public DisplayMonitorHandle DisplayMonitor { get; }

    public PhysicalMonitorInformation(PhysicalMonitorHandle handle, DisplayMonitorHandle displayMonitor, string description)
    {
        Handle = handle;
        Description = description;
        DisplayMonitor = displayMonitor;
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("PhysicalMonitorInformation {");

        sb.AppendLine($"\tHandle: {this.Handle},");
        sb.AppendLine($"\tDescription: {this.Description},");
        sb.AppendLine($"\tDisplayMonitor: {this.DisplayMonitor}");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
