using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model.Handles;
using System.Collections.Immutable;
using System.Text;
namespace MultipleScreenPowersave.Model;

public class ScreenInformation
{
    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors {
        get => _displayMonitors.SelectMany(monitor => monitor.PhysicalMonitors);
    }

    public IEnumerable<DisplayMonitorInformation> DisplayMonitors { get => _displayMonitors; }

    public IReadOnlyDictionary<DisplayMonitorHandle, DisplayMonitorInformation> DisplayMonitorByHandle { get => _displayMonitorByHandle; }

    private List<DisplayMonitorInformation> _displayMonitors = new List<DisplayMonitorInformation>();
    private Dictionary<DisplayMonitorHandle, DisplayMonitorInformation> _displayMonitorByHandle = new ();

    public ScreenInformation(IEnumerable<DisplayMonitorInformation> displayMonitors)
    {
        Guard.IsNotNull(displayMonitors);

        _displayMonitors.AddRange(displayMonitors);

        foreach (var monitor in this._displayMonitors)
            _displayMonitorByHandle.Add(monitor.Handle, monitor);
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ScreenInformation [");
        foreach (var monitor in this._displayMonitors)
        {
            var monitorLines = monitor.ToString()?.Split('\n') ?? [];
            monitorLines.All(monitorLine =>
            {
                sb.Append('\t');
                sb.AppendLine(monitorLine);

                return true;
            });
        }

        sb.AppendLine("]");
        return sb.ToString();
    }
}
