namespace MultipleScreenPowersave.App;

using MultipleScreenPowersave.Model;

/// <summary>
/// Technology-agnostic interface for turning a monitor on or off.
/// </summary>
public interface IDisplayControlService
{
    /// <summary>
    /// Turn off given physical monitor.
    /// </summary>
    /// <param name="physicalMonitor">Physical monitor to be turned off.</param>
    /// <param name="virtualMonitor">Virtual monitor currently displayed on the physicla monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn off monitor.</exception>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    );

    /// <summary>
    /// Turn on given physical monitor.
    /// </summary>
    /// <param name="monitor">Physical monitor to be turned on.</param>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor);
}
