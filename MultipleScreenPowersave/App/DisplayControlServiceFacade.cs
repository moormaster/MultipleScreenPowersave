namespace MultipleScreenPowersave.App;

using Microsoft.Extensions.Logging;
using MultipleScreenPowersave.Model;

/// <summary>
/// Implementation of <see cref="IDisplayControlServiceFacade"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DisplayControlServiceFacade"/> class.
/// </remarks>
/// <param name="displayBacklightService">DisplayBacklightService instance.</param>
/// <param name="displayDataChannelService">DisplayDataChannelService instance.</param>
/// <param name="logger">Logger instance.</param>
public class DisplayControlServiceFacade(
    IDisplayBacklightService displayBacklightService,
    IDisplayDataChannelService displayDataChannelService,
    ILogger<DisplayControlServiceFacade> logger
) : IDisplayControlServiceFacade
{
    /// <inheritdoc/>
    public void TurnOffMonitor(PhysicalMonitorInformation monitor)
    {
        try
        {
            displayDataChannelService.TurnOffMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);
        }

        try
        {
            logger.LogWarning(
                "Turning off monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            displayBacklightService.TurnOffMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);
            throw;
        }
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        try
        {
            displayDataChannelService.TurnOnMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);
        }

        try
        {
            logger.LogWarning(
                "Turning on monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            displayBacklightService.TurnOnMonitor(monitor);
            return;
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);
            throw;
        }
    }
}
