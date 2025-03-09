namespace MultipleScreenPowersave.Model;

using System.Text;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;

/// <summary>
/// Dto returned by <see cref="ScreenQuery.GetScreenInformation"/>.
/// </summary>
public class ScreenInformation
{
    private readonly List<DisplayMonitorInformation> displayMonitors = [];
    private readonly Dictionary<
        DisplayMonitorHandle,
        DisplayMonitorInformation
    > displayMonitorByHandle = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenInformation"/> class.
    /// </summary>
    /// <param name="displayMonitors">Enumerable of all DisplayMonitors.</param>
    public ScreenInformation(IEnumerable<DisplayMonitorInformation> displayMonitors)
    {
        Guard.IsNotNull(displayMonitors);

        this.displayMonitors.AddRange(displayMonitors);

        foreach (var monitor in this.displayMonitors)
            this.displayMonitorByHandle.Add(monitor.Handle, monitor);
    }

    /// <summary>
    /// Gets the enumerable of all PhysicalMonitors.
    /// </summary>
    public IEnumerable<PhysicalMonitorInformation> PhysicalMonitors
    {
        get => this.displayMonitors.SelectMany(monitor => monitor.PhysicalMonitors);
    }

    /// <summary>
    /// Gets the enumerable of all DisplayMonitors.
    /// </summary>
    public IEnumerable<DisplayMonitorInformation> DisplayMonitors
    {
        get => this.displayMonitors;
    }

    /// <summary>
    /// Gets the dictionary mapping each DisplayMonitorHandle to a DisplayMonitorInformation.
    /// </summary>
    public IReadOnlyDictionary<
        DisplayMonitorHandle,
        DisplayMonitorInformation
    > DisplayMonitorByHandle
    {
        get => this.displayMonitorByHandle;
    }

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
