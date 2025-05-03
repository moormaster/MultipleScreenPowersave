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
    private readonly Dictionary<
        PhysicalMonitorInformation,
        DisplayControlServiceType
    > displayControlServiceTypeMonitorWasTurnedOffWith = new Dictionary<
        PhysicalMonitorInformation,
        DisplayControlServiceType
    >(
        EqualityComparer<PhysicalMonitorInformation>.Create(
            ComparePhysicalMonitorInformationIdentity,
            getHashCode: physicalMonitorInformation => 1
        )
    );

    private Exception? lastException;

    /// <inheritdoc/>
    public void TurnOffMonitor(
        PhysicalMonitorInformation physicalMonitor,
        DisplayMonitorInformation virtualMonitor
    )
    {
        if (
            this.displayControlServiceTypeMonitorWasTurnedOffWith.TryGetValue(
                physicalMonitor,
                out _
            )
        )
        {
            // prevent turning off monitor twice
            return;
        }

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
        bool success = false;

        if (
            this.displayControlServiceTypeMonitorWasTurnedOffWith.TryGetValue(
                monitor,
                out var displayControlServiceType
            )
        )
        {
            // try to turn on monitor using the same service it was successfully turned off with before.
            success = displayControlServiceType switch
            {
                DisplayControlServiceType.Ddc => this.TryTurnOnMonitorUsingDdc(monitor),
                DisplayControlServiceType.Backlight => this.TryTurnOnMonitorUsingBacklight(monitor),
                DisplayControlServiceType.BlackWindow => this.TryTurnOnMonitorUsingBlackWindow(
                    monitor
                ),
                _ => false,
            };
        }

        // if service is unknown try every service until success
        if (
            !(
                success
                || this.TryTurnOnMonitorUsingDdc(monitor)
                || this.TryTurnOnMonitorUsingBacklight(monitor)
                || this.TryTurnOnMonitorUsingBlackWindow(monitor)
            )
        )
            throw this.lastException!;
    }

    private static bool ComparePhysicalMonitorInformationIdentity(
        PhysicalMonitorInformation? physicalMonitorInformation1,
        PhysicalMonitorInformation? physicalMonitorInformation2
    )
    {
        if ((physicalMonitorInformation1 == null) ^ (physicalMonitorInformation2 == null))
            return false;

        if (physicalMonitorInformation1 == null && physicalMonitorInformation2 == null)
            return true;

        if (physicalMonitorInformation1?.Handle == physicalMonitorInformation2?.Handle)
            return true;

        if (!string.IsNullOrEmpty(physicalMonitorInformation1?.EdidHex))
        {
            if (physicalMonitorInformation1.EdidHex == physicalMonitorInformation2?.EdidHex)
                return true;
        }

        if (!string.IsNullOrEmpty(physicalMonitorInformation1?.LinuxBacklightDevice))
        {
            if (
                physicalMonitorInformation1.LinuxBacklightDevice
                == physicalMonitorInformation2?.LinuxBacklightDevice
            )
                return true;
        }

        if (!string.IsNullOrEmpty(physicalMonitorInformation1?.LinuxI2cDevice))
        {
            if (
                physicalMonitorInformation1.LinuxI2cDevice
                == physicalMonitorInformation2?.LinuxI2cDevice
            )
                return true;
        }

        if (!string.IsNullOrEmpty(physicalMonitorInformation1?.WmiInstanceName))
        {
            if (
                physicalMonitorInformation1.WmiInstanceName
                == physicalMonitorInformation2?.WmiInstanceName
            )
                return true;
        }

        if (physicalMonitorInformation1?.Index == physicalMonitorInformation2?.Index)
            return true;

        return false;
    }

    private bool TryTurnOnMonitorUsingDdc(PhysicalMonitorInformation monitor)
    {
        try
        {
            displayDataChannelService.TurnOnMonitor(monitor);

            this.displayControlServiceTypeMonitorWasTurnedOffWith.Remove(monitor);
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

            this.displayControlServiceTypeMonitorWasTurnedOffWith.Remove(monitor);
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

            this.displayControlServiceTypeMonitorWasTurnedOffWith.Remove(monitor);
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

            this.displayControlServiceTypeMonitorWasTurnedOffWith[physicalMonitor] =
                DisplayControlServiceType.Ddc;
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

            this.displayControlServiceTypeMonitorWasTurnedOffWith[physicalMonitor] =
                DisplayControlServiceType.Backlight;
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

            this.displayControlServiceTypeMonitorWasTurnedOffWith[physicalMonitor] =
                DisplayControlServiceType.BlackWindow;
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
