namespace MultipleScreenPowersave.App;

using Microsoft.Extensions.Logging;
using MultipleScreenPowersave.Model;

/// <summary>
/// Implementation of <see cref="IDisplayControlServiceFacade"/>.
/// </summary>
public class DisplayControlServiceFacade : IDisplayControlServiceFacade
{
    private readonly IDisplayBacklightService displayBacklightService;
    private readonly IDisplayDataChannelService displayDataChannelService;
    private readonly ILogger<DisplayControlServiceFacade> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayControlServiceFacade"/> class.
    /// </summary>
    /// <param name="displayBacklightService">DisplayBacklightService instance.</param>
    /// <param name="displayDataChannelService">DisplayDataChannelService instance.</param>
    /// <param name="logger">Logger instance.</param>
    public DisplayControlServiceFacade(
        IDisplayBacklightService displayBacklightService,
        IDisplayDataChannelService displayDataChannelService,
        ILogger<DisplayControlServiceFacade> logger
    )
    {
        this.displayBacklightService = displayBacklightService;
        this.displayDataChannelService = displayDataChannelService;
        this.logger = logger;
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
            this.logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            this.logger.LogDebug("{exception}", e);
        }

        try
        {
            this.logger.LogWarning(
                "Turning off monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            this.displayBacklightService.TurnOffMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            this.logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            this.logger.LogDebug("{exception}", e);
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
            this.logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            this.logger.LogDebug("{exception}", e);
        }

        try
        {
            this.logger.LogWarning(
                "Turning on monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            this.displayBacklightService.TurnOnMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            this.logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            this.logger.LogDebug("{exception}", e);
            throw;
        }
    }
}
