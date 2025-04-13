namespace MultipleScreenPowersave.App;

using CommunityToolkit.Diagnostics;
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
public class ApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;

    private readonly IDisplayDataChannelService displayDataChannelService;
    private readonly IMouseQuery mouseQuery;
    private readonly IScreenQuery screenQuery;
    private readonly IWindowQuery windowQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="displayDataChannelService">DisplayDataChannelService instance.</param>
    /// <param name="mouseQuery">MouseQuery instance.</param>
    /// <param name="screenQuery">ScreenQuery instance.</param>
    /// <param name="windowQuery">WindowQuery instance.</param>
    public ApplicationService(
        IDisplayDataChannelService displayDataChannelService,
        IMouseQuery mouseQuery,
        IScreenQuery screenQuery,
        IWindowQuery windowQuery
    )
    {
        Guard.IsNotNull(displayDataChannelService);
        Guard.IsNotNull(mouseQuery);
        Guard.IsNotNull(screenQuery);
        Guard.IsNotNull(windowQuery);

        this.displayDataChannelService = displayDataChannelService;
        this.mouseQuery = mouseQuery;
        this.screenQuery = screenQuery;
        this.windowQuery = windowQuery;

        Log.Logger.Information(
            "Using configuration file: {configurationFileName}",
            ConfigurationQueryFactory.GetConfigurationFileName()
        );

        var screenInformation = this.screenQuery.GetScreenInformation();
        Log.Logger.Debug("{screenInformation}", screenInformation);
    }

    /// <summary>
    /// Turns on physical monitors that currently
    ///     - show at least one window,
    ///     - show the mouse cursor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnOnlyUsedMonitors()
    {
        ScreenInformation screenInformation = this.screenQuery.GetScreenInformation();

        Dictionary<PhysicalMonitorHandle, bool> isMonitorNeededByHandle =
            screenInformation.PhysicalMonitors.ToDictionary(v => v.Handle, v => false);
        BlacklistConfiguration blacklist = ConfigurationQueryFactory
            .GetConfigurationQuery()
            .GetBlacklist();

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
            var screenOfApp = System.Windows.Forms.Screen.FromHandle(
                windowProcessInformation.Handle.Value
            );

            // hacky way to get the display monitor handle
            var displayMonitorHandle = new DisplayMonitorHandle(screenOfApp.GetHashCode());
            screenInformation.DisplayMonitorByHandle.TryGetValue(
                displayMonitorHandle,
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
                    windowProcessInformation.DwStyle,
                    windowProcessInformation.DwExStyle?.ToHexString(),
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
                    windowProcessInformation.DwStyle,
                    windowProcessInformation.DwExStyle?.ToHexString(),
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
                    windowProcessInformation.DwStyle,
                    windowProcessInformation.DwExStyle?.ToHexString(),
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

                    this.displayDataChannelService.TurnOnMonitor(physicalMonitor);
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
                    this.displayDataChannelService.TurnOffMonitor(physicalMonitor);
                }
                catch (Exception e)
                {
                    Log.Logger.Error("{exception}", e);
                }
            }
        }
    }

    /// <summary>
    /// Turn on all physical monitors.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnAllMonitors()
    {
        foreach (var monitor in this.screenQuery.GetScreenInformation().PhysicalMonitors)
        {
            this.displayDataChannelService.TurnOnMonitor(monitor);
        }
    }

    private static bool IsBlacklisted(
        BlacklistConfiguration blacklist,
        WindowProcessInformation windowProcessInformation
    )
    {
        return blacklist.Windows.Any(windowEntry => windowEntry.IsMatch(windowProcessInformation));
    }

    private static bool IsBlacklisted(
        BlacklistConfiguration blacklist,
        DisplayMonitorInformation displayMonitor
    )
    {
        return blacklist.DisplayMonitors.Any(monitorEntry => monitorEntry.IsMatch(displayMonitor));
    }

    private static bool IsIgnoreMouse(BlacklistConfiguration blacklist, Point position)
    {
        return blacklist.IgnoreMouseAtRectangles.Any(rect => rect.Contains(position));
    }
}
