namespace MultipleScreenPowersave.App;

using MultipleScreenPowersave.Model;

/// <summary>
/// Service to turn on/off a monitor via DDC/CI and MCCS.
/// See <see href="https://de.wikipedia.org/wiki/Display_Data_Channel"/>
/// and <see href="https://en.wikipedia.org/wiki/Monitor_Control_Command_Set"/>.
/// </summary>
public interface IDisplayDataChannelService
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
