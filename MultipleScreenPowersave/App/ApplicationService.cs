namespace MultipleScreenPowersave.App;

using CommunityToolkit.Diagnostics;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.App.WindowsImpl;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query.WindowsImpl;
using Serilog;

/// <summary>
/// ApplicationService providing functions to turn off Monitors based on activity.
/// </summary>
public class ApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;

    private readonly IDisplayDataChannelService displayDataChannelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    public ApplicationService()
    {
        Log.Logger.Information(
            "Using configuration file: {configurationFileName}",
            ConfigurationQueryFactory.GetConfigurationFileName()
        );

        var screenInformation = GetScreenInformation();
        Log.Logger.Debug("{screenInformation}", screenInformation);

        this.displayDataChannelService = new DisplayDataChannelService();
    }

    /// <summary>
    /// Turns on physical monitors that currently
    ///     - show at least one window,
    ///     - show the mouse cursor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnOnlyUsedMonitors()
    {
        ScreenInformation screenInformation = GetScreenInformation();

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

        var currentCursorPosition = GetCurrentMouseCursorPosition();
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
                    isMonitorNeededByHandle[physicalMonitor.Handle] = true;
                }
            }
        }

        var windowsShown = GetWindows();

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
        foreach (var monitor in GetScreenInformation().PhysicalMonitors)
        {
            this.displayDataChannelService.TurnOnMonitor(monitor);
        }
    }

    /// <summary>
    /// Returns the coordinates of the mouse cursor.
    /// </summary>
    /// <returns>Coordinates of the mouse cursor.</returns>
    private static Point GetCurrentMouseCursorPosition()
    {
        return new MouseQuery().GetCurrentMouseCursorPosition();
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

    private static ScreenInformation GetScreenInformation()
    {
        return new ScreenQuery().GetScreenInformation();
    }

    private static IEnumerable<WindowProcessInformation> GetWindows()
    {
        return new WindowQuery().GetWindows();
    }
}
