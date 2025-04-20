namespace MultipleScreenPowersave.App;

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using Serilog;

/// <summary>
/// ApplicationService providing functions to turn off Monitors based on activity.
/// </summary>
public class ApplicationService : IApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;
    private readonly IDisplayControlServiceFacade displayControlServiceFacade;
    private readonly IMouseQuery mouseQuery;
    private readonly IScreenQuery screenQuery;
    private readonly IWindowQuery windowQuery;
    private readonly IOptions<BlacklistOptions> blacklistOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="displayControlServiceFacade">DisplayControlServiceFacade instance.</param>
    /// <param name="mouseQuery">MouseQuery instance.</param>
    /// <param name="screenQuery">ScreenQuery instance.</param>
    /// <param name="windowQuery">WindowQuery instance.</param>
    /// <param name="blacklistOptions">Blacklist options.</param>
    public ApplicationService(
        IDisplayControlServiceFacade displayControlServiceFacade,
        IMouseQuery mouseQuery,
        IScreenQuery screenQuery,
        IWindowQuery windowQuery,
        IOptions<BlacklistOptions> blacklistOptions
    )
    {
        Guard.IsNotNull(displayControlServiceFacade);
        Guard.IsNotNull(mouseQuery);
        Guard.IsNotNull(screenQuery);
        Guard.IsNotNull(windowQuery);
        Guard.IsNotNull(blacklistOptions);

        this.displayControlServiceFacade = displayControlServiceFacade;
        this.mouseQuery = mouseQuery;
        this.screenQuery = screenQuery;
        this.windowQuery = windowQuery;
        this.blacklistOptions = blacklistOptions;

        var screenInformation = this.screenQuery.GetScreenInformation();
        Log.Logger.Debug("{screenInformation}", screenInformation);
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
                    Log.Logger.Debug(
                        "PhysicalMonitor #{physicalMonitorHandle}: Mouse cursor position ({x}x{y})",
                        physicalMonitor.Handle,
                        currentCursorPosition.X,
                        currentCursorPosition.Y
                    );

                    if (IsIgnoreMouse(blacklist, currentCursorPosition))
                    {
                        Log.Logger.Debug(
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
                Log.Logger.Debug(
                    "Blacklisted ProcessName: \"{processName}\" - WindowTitle: \"{windowTitle}\" (#{windowHandle})",
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                Log.Logger.Debug(
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

            if (displayMonitor is null)
            {
                Log.Logger.Error(
                    "Failed to determine DisplayMonitor for windowProcessInformation:"
                );
                Log.Logger.Error(
                    "\tProcessName: \"{processName}\" - WindowTitle: \"{windowTitle}\" (#{windowHandle})",
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                Log.Logger.Error(
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
                Log.Logger.Debug(
                    "PhysicalMonitor #{physicalMonitorHandle}: ProcessName: \"{processName}\" - WindowTitle: \"{windowText}\" (#{windowHandle})",
                    physicalMonitor.Handle,
                    windowProcessInformation.ProcessName,
                    windowProcessInformation.WindowTitle,
                    windowProcessInformation.Handle
                );
                Log.Logger.Debug(
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
            screenInformation.PhysicalMonitorByHandle,
            outerKeySelector: physicalMonitorByHandleItem => physicalMonitorByHandleItem.Key,
            innerKeySelector: isMonitorNeeded => isMonitorNeeded.Key,
            resultSelector: (isMonitorNeeded, physicalMonitor) =>
                (IsMonitorNeeded: isMonitorNeeded.Value, PhysicalMonitor: physicalMonitor.Value)
        );

        foreach (
            var (isMonitorNeeded, physicalMonitor) in isMonitorNeededWithPhysicalMonitorInformation
        )
        {
            if (isMonitorNeeded)
            {
                try
                {
                    Log.Logger.Information(
                        "Turning on physical monitor #{physicalMonitorHandle}",
                        physicalMonitor.Handle
                    );

                    this.displayControlServiceFacade.TurnOnMonitor(physicalMonitor);
                }
                catch (Exception e)
                {
                    Log.Logger.Error("{exception}", e);
                }
            }
            else
            {
                Log.Logger.Information(
                    "Turning off physical monitor #{physicalMonitorHandle}",
                    physicalMonitor.Handle
                );

                try
                {
                    this.displayControlServiceFacade.TurnOffMonitor(physicalMonitor);
                }
                catch (Exception e)
                {
                    Log.Logger.Error("{exception}", e);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void TurnOnAllMonitors()
    {
        foreach (var monitor in this.screenQuery.GetScreenInformation().PhysicalMonitors)
            this.displayControlServiceFacade.TurnOnMonitor(monitor);
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

    private static bool IsIgnoreMouse(BlacklistOptions blacklist, Point position)
    {
        return blacklist.IgnoreMouseAtRectangles.Any(rect => rect.Contains(position));
    }
}
