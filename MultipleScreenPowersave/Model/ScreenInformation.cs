namespace MultipleScreenPowersave.Model;

using System.Text;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model.Handles;

public class ScreenInformation
{
    private readonly List<DisplayMonitorInformation> displayMonitors = [];
    private readonly Dictionary<DisplayMonitorHandle, DisplayMonitorInformation> displayMonitorByHandle = [];

    public ScreenInformation(IEnumerable<DisplayMonitorInformation> displayMonitors)
    {
        Guard.IsNotNull(displayMonitors);

        this.displayMonitors.AddRange(displayMonitors);

        foreach (var monitor in this.displayMonitors)
            this.displayMonitorByHandle.Add(monitor.Handle, monitor);
    }

    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors
    {
        get => this.displayMonitors.SelectMany(monitor => monitor.PhysicalMonitors);
    }

    public IEnumerable<DisplayMonitorInformation> DisplayMonitors { get => this.displayMonitors; }

    public IReadOnlyDictionary<DisplayMonitorHandle, DisplayMonitorInformation> DisplayMonitorByHandle { get => this.displayMonitorByHandle; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("ScreenInformation [");
        foreach (var monitor in this.displayMonitors)
        {
            var monitorLines = monitor.ToString()?.Split('\n') ?? [];
            foreach (var line in monitorLines)
            {
                sb.Append('\t');
                sb.AppendLine(line);
            }
        }

        sb.AppendLine("]");
        return sb.ToString();
    }
}
