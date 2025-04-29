namespace MultipleScreenPowersave.App;

using Microsoft.Extensions.Logging;
using MultipleScreenPowersave.Model;

/// <summary>
/// Implementation of <see cref="IDisplayControlServiceFacade"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DisplayControlServiceFacade"/> class.
/// </remarks>
/// <param name="blackWindowService">BlackWindowService instance.</param>
/// <param name="displayBacklightService">DisplayBacklightService instance.</param>
/// <param name="displayDataChannelService">DisplayDataChannelService instance.</param>
/// <param name="logger">Logger instance.</param>
public class DisplayControlServiceFacade(
    IBlackWindowService blackWindowService,
    IDisplayBacklightService displayBacklightService,
    IDisplayDataChannelService displayDataChannelService,
    ILogger<DisplayControlServiceFacade> logger
) : IDisplayControlServiceFacade
{
    private Exception? lastException;

    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        if (
            !(
                this.TryTurnOffMonitorUsingDdc(physicalMonitor, virtualMonitor)
                || this.TryTurnOffMonitorUsingBacklight(physicalMonitor, virtualMonitor)
                || this.TryTurnOffMonitorUsingBlackWindow(physicalMonitor, virtualMonitor)
            )
        )
            throw this.lastException!;
    }

    /// <inheritdoc/>
    public void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        if (
            !(
                this.TryTurnOnMonitorUsingDdc(monitor)
                || this.TryTurnOnMonitorUsingBacklight(monitor)
                || this.TryTurnOnMonitorUsingBlackWindow(monitor)
            )
        )
            throw this.lastException!;
    }

    private bool TryTurnOnMonitorUsingDdc(PhysicalMonitorInformation monitor)
    {
        try
        {
            displayDataChannelService.TurnOnMonitor(monitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using DDC",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }

    private bool TryTurnOnMonitorUsingBacklight(PhysicalMonitorInformation monitor)
    {
        try
        {
            logger.LogWarning(
                "Turning on monitor {monitorHandle} using backlight control instead",
                monitor.Handle
            );
            displayBacklightService.TurnOnMonitor(monitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn on monitor {monitorHandle} using backlight control",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }

    private bool TryTurnOnMonitorUsingBlackWindow(PhysicalMonitorInformation monitor)
    {
        try
        {
            logger.LogWarning(
                "Turning on monitor {monitorHandle} by closing the black window instead",
                monitor.Handle
            );
            blackWindowService.TurnOnMonitor(monitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} by closing the black window",
                monitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }

    private bool TryTurnOffMonitorUsingDdc(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        try
        {
            displayDataChannelService.TurnOffMonitor(physicalMonitor, virtualMonitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using DDC",
                physicalMonitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }

    private bool TryTurnOffMonitorUsingBacklight(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        try
        {
            logger.LogWarning(
                "Turning off monitor {monitorHandle} using backlight control instead",
                physicalMonitor.Handle
            );
            displayBacklightService.TurnOffMonitor(physicalMonitor, virtualMonitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} using backlight control",
                physicalMonitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }

    private bool TryTurnOffMonitorUsingBlackWindow(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        try
        {
            logger.LogWarning(
                "Turning off monitor {monitorHandle} by showing a black window instead",
                physicalMonitor.Handle
            );
            blackWindowService.TurnOffMonitor(physicalMonitor, virtualMonitor);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Failed to turn off monitor {monitorHandle} by showing a black window",
                physicalMonitor.Handle
            );
            logger.LogDebug("{exception}", e);

            this.lastException = e;
        }

        return false;
    }
}
