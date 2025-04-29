namespace MultipleScreenPowersave.App;

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using MultipleScreenPowersave.Ui;

/// <summary>
/// ApplicationService providing functions to turn off Monitors based on activity.
/// </summary>
public class ApplicationService : IApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;
    private readonly IDisplayControlServiceFacade displayControlServiceFacade;
    private readonly ILogger<ApplicationService> logger;
    private readonly IMouseQuery mouseQuery;
    private readonly IScreenQuery screenQuery;
    private readonly IWindowQuery windowQuery;
    private readonly IOptions<BlacklistOptions> blacklistOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="displayControlServiceFacade">DisplayControlServiceFacade instance.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="mouseQuery">MouseQuery instance.</param>
    /// <param name="screenQuery">ScreenQuery instance.</param>
    /// <param name="windowQuery">WindowQuery instance.</param>
    /// <param name="blacklistOptions">Blacklist options.</param>
    public ApplicationService(
        IDisplayControlServiceFacade displayControlServiceFacade,
        ILogger<ApplicationService> logger,
        IMouseQuery mouseQuery,
        IScreenQuery screenQuery,
        IWindowQuery windowQuery,
        IOptions<BlacklistOptions> blacklistOptions
    )
    {
        Guard.IsNotNull(displayControlServiceFacade);
        Guard.IsNotNull(logger);
        Guard.IsNotNull(mouseQuery);
        Guard.IsNotNull(screenQuery);
        Guard.IsNotNull(windowQuery);
        Guard.IsNotNull(blacklistOptions);

        this.displayControlServiceFacade = displayControlServiceFacade;
        this.logger = logger;
        this.mouseQuery = mouseQuery;
        this.screenQuery = screenQuery;
        this.windowQuery = windowQuery;
        this.blacklistOptions = blacklistOptions;

        var screenInformation = this.screenQuery.GetScreenInformation();
        this.logger.LogDebug("{screenInformation}", screenInformation);
    }

    /// <inheritdoc/>
    public void TurnOnOnlyUsedMonitors()
    {
        ScreenInformation screenInformation = this.screenQuery.GetScreenInformation();

        Dictionary<PhysicalMonitorHandle, bool> isMonitorNeededByHandle =
            screenInformation.PhysicalMonitors.ToDictionary(v => v.Handle, v => false);
        BlacklistOptions blacklist = this.blacklistOptions.Value;

        foreach (var displayMonitor in screenInformation.DisplayMonitors)
        {
            if (IsBlacklisted(blacklist, displayMonitor))
            {
                // ensure that physical screens of blacklisted monitors never gets powered off
                foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
                    isMonitorNeededByHandle[physicalMonitor.Handle] = true;
            }
        }

        var currentCursorPosition = this.mouseQuery.GetCurrentMouseCursorPosition();
        foreach (var displayMonitor in screenInformation.DisplayMonitors)
        {
            if (displayMonitor.MonitorRectangle.Contains(currentCursorPosition))
            {
                // enable physical monitors currently visited by the mouse cursor
                foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
                {
                    this.logger.LogDebug(
                        "PhysicalMonitor #{physicalMonitorHandle}: Mouse cursor position ({x}x{y})",
                        physicalMonitor.Handle,
                        currentCursorPosition.X,
                        currentCursorPosition.Y
                    );

                    if (IsIgnoreMouse(blacklist, currentCursorPosition))
                    {
                        this.logger.LogDebug(
                            "Blacklisted mouse cursor position ({x}x{y})",
                            currentCursorPosition.X,
                            currentCursorPosition.Y
                        );

                        continue;
                    }

                    isMonitorNeededByHandle[physicalMonitor.Handle] = true;
                }
            }
        }

        var windowsShown = this.windowQuery.GetWindows();

        foreach (var windowProcessInformation in windowsShown)
        {
            TryGetDisplayMonitorByRect(
                screenInformation,
                windowProcessInformation.Rectangle,
                out var displayMonitor
            );

            if (IsBlacklisted(blacklist, windowProcessInformation))
            {
                this.logger.LogDebug(
                    "Blacklisted ProcessName: \"{processName}\" - WindowTitle: \"{windowTitle}\" (#{windowHandle})",
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                this.logger.LogDebug(
                    "\tdwStyle: {dwStyle}, dwExStyle: {dwExStyle}, Pos: ({x}, {y}), Size: {width}x{height}",
#if WINDOWS
                    windowProcessInformation.DwStyle?.WindowStyleToString() ?? null,
                    windowProcessInformation.DwExStyle?.ExtendedWindowStyleToString() ?? null,
#else
                    null,
                    null,
#endif
                    windowProcessInformation.Rectangle.X,
                    windowProcessInformation.Rectangle.Y,
                    windowProcessInformation.Rectangle.Width,
                    windowProcessInformation.Rectangle.Height
                );
                continue;
            }

            if (IsSystemWindow(windowProcessInformation))
                continue;

            if (displayMonitor is null)
            {
                this.logger.LogError(
                    "Failed to determine DisplayMonitor for windowProcessInformation:"
                );
                this.logger.LogError(
                    "\tProcessName: \"{processName}\" - WindowTitle: \"{windowTitle}\" (#{windowHandle})",
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                this.logger.LogError(
                    "\tdwStyle: {dwStyle}, dwExStyle: {dwExStyle}, Pos: ({x}, {y}), Size: {width}x{height}",
#if WINDOWS
                    windowProcessInformation.DwStyle?.WindowStyleToString() ?? null,
                    windowProcessInformation.DwExStyle?.ExtendedWindowStyleToString() ?? null,
#else
                    null,
                    null,
#endif
                    windowProcessInformation.Rectangle.X,
                    windowProcessInformation.Rectangle.Y,
                    windowProcessInformation.Rectangle.Width,
                    windowProcessInformation.Rectangle.Height
                );
                continue;
            }

            foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
            {
                this.logger.LogDebug(
                    "PhysicalMonitor #{physicalMonitorHandle}: ProcessName: \"{processName}\" - WindowTitle: \"{windowText}\" (#{windowHandle})",
                    physicalMonitor.Handle,
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                this.logger.LogDebug(
                    "\tdwStyle: {dwStyle}, dwExStyle: {dwExStyle}, Pos: ({x}, {y}), Size: {width}x{height}",
#if WINDOWS
                    windowProcessInformation.DwStyle?.WindowStyleToString() ?? null,
                    windowProcessInformation.DwExStyle?.ExtendedWindowStyleToString() ?? null,
#else
                    null,
                    null,
#endif
                    windowProcessInformation.Rectangle.X,
                    windowProcessInformation.Rectangle.Y,
                    windowProcessInformation.Rectangle.Width,
                    windowProcessInformation.Rectangle.Height
                );

                isMonitorNeededByHandle[physicalMonitor.Handle] = true;
            }
        }

        var physicalMonitorByHandle = screenInformation.PhysicalMonitorByHandle;
        var isMonitorNeededWithPhysicalMonitorInformation = isMonitorNeededByHandle.Join(
            screenInformation.DisplayMonitors.SelectMany(
                displayMonitor => displayMonitor.PhysicalMonitors,
                (displayMonitor, physicalMonitor) =>
                    (DisplayMonitor: displayMonitor, PhysicalMonitor: physicalMonitor)
            ),
            outerKeySelector: isMonitorNeeded => isMonitorNeeded.Key,
            innerKeySelector: tuple => tuple.PhysicalMonitor.Handle,
            resultSelector: (isMonitorNeeded, tuple) =>
                (
                    IsMonitorNeeded: isMonitorNeeded.Value,
                    tuple.PhysicalMonitor,
                    tuple.DisplayMonitor
                )
        );

        foreach (
            var (
                isMonitorNeeded,
                physicalMonitor,
                displayMonitor
            ) in isMonitorNeededWithPhysicalMonitorInformation
        )
        {
            if (isMonitorNeeded)
            {
                try
                {
                    this.logger.LogInformation(
                        "Turning on physical monitor #{physicalMonitorHandle}",
                        physicalMonitor.Handle
                    );

                    this.displayControlServiceFacade.TurnOnMonitor(physicalMonitor);
                }
                catch (Exception e)
                {
                    this.logger.LogError("{exception}", e);
                }
            }
            else
            {
                this.logger.LogInformation(
                    "Turning off physical monitor #{physicalMonitorHandle}",
                    physicalMonitor.Handle
                );

                try
                {
                    this.displayControlServiceFacade.TurnOffMonitor(
                        physicalMonitor,
                        displayMonitor
                    );
                }
                catch (Exception e)
                {
                    this.logger.LogError("{exception}", e);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void TurnOnAllMonitors()
    {
        (PhysicalMonitorInformation PhysicalMonitor, Exception? Exception)? lastError = null;

        foreach (var monitor in this.screenQuery.GetScreenInformation().PhysicalMonitors)
        {
            try
            {
                this.displayControlServiceFacade.TurnOnMonitor(monitor);
            }
            catch (Exception e)
            {
                lastError = (monitor, e);
            }
        }

        if (lastError.HasValue)
        {
            throw new InvalidOperationException(
                $"Failed to turn off monitor {lastError.Value.PhysicalMonitor.Handle}",
                lastError.Value.Exception
            );
        }
    }

    private static bool TryGetDisplayMonitorByRect(
        ScreenInformation screenInformation,
        Rect rectangle,
        out DisplayMonitorInformation displayMonitor
    )
    {
        displayMonitor = screenInformation
            .DisplayMonitors.Select(monitor =>
                (
                    IntersectionArea: monitor.MonitorRectangle.Intersect(rectangle).GetArea(),
                    DisplayMonitor: monitor
                )
            )
            .OrderByDescending(tuple => tuple.IntersectionArea)
            .FirstOrDefault()
            .DisplayMonitor;

        return displayMonitor != null;
    }

    private static bool IsBlacklisted(
        BlacklistOptions blacklist,
        WindowProcessInformation windowProcessInformation
    )
    {
        return blacklist.Windows.Any(windowEntry => windowEntry.IsMatch(windowProcessInformation));
    }

    private static bool IsBlacklisted(
        BlacklistOptions blacklist,
        DisplayMonitorInformation displayMonitor
    )
    {
        return blacklist.DisplayMonitors.Any(monitorEntry => monitorEntry.IsMatch(displayMonitor));
    }

    private static bool IsSystemWindow(WindowProcessInformation windowProcessInformation)
    {
        if (windowProcessInformation.WindowTitle == BlackWindow.BlackWindowTitle)
            return true;

        return false;
    }

    private static bool IsIgnoreMouse(BlacklistOptions blacklist, Point position)
    {
        return blacklist.IgnoreMouseAtRectangles.Any(rect => rect.Contains(position));
    }
}
