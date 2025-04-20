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
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn off monitor.</exception>
    public void TurnOffMonitor(PhysicalMonitorInformation monitor);

    /// <summary>
    /// Turn on given physical monitor.
    /// </summary>
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor);
}
