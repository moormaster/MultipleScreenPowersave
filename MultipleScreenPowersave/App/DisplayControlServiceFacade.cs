namespace MultipleScreenPowersave.App;

using MultipleScreenPowersave.Model;
using Serilog;

/// <summary>
/// Implementation of <see cref="IDisplayControlServiceFacade"/>.
/// </summary>
public class DisplayControlServiceFacade : IDisplayControlServiceFacade
{
    private readonly IDisplayBacklightService displayBacklightService;
    private readonly IDisplayDataChannelService displayDataChannelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayControlServiceFacade"/> class.
    /// </summary>
    /// <param name="displayBacklightService">DisplayBacklightService instance.</param>
    /// <param name="displayDataChannelService">DisplayDataChannelService instance.</param>
    public DisplayControlServiceFacade(
        IDisplayBacklightService displayBacklightService,
        IDisplayDataChannelService displayDataChannelService
    )
    {
        this.displayBacklightService = displayBacklightService;
        this.displayDataChannelService = displayDataChannelService;
    }

    /// <inheritdoc/>
    public void TurnOffMonitor(PhysicalMonitorInformation monitor)
    {
        try
        {
            this.displayDataChannelService.TurnOffMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            Log.Logger.Warning(
                "Failed to turn off monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            Log.Logger.Debug("{exception}", e);
        }

        try
        {
            Log.Logger.Warning(
                "Turning off monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            this.displayBacklightService.TurnOffMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            Log.Logger.Warning(
                "Failed to turn off monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            Log.Logger.Debug("{exception}", e);
            throw;
        }
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        try
        {
            this.displayDataChannelService.TurnOnMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            Log.Logger.Warning(
                "Failed to turn on monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            Log.Logger.Debug("{exception}", e);
        }

        try
        {
            Log.Logger.Warning(
                "Turning on monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            this.displayBacklightService.TurnOnMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            Log.Logger.Warning(
                "Failed to turn on monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            Log.Logger.Debug("{exception}", e);
            throw;
        }
    }
}
