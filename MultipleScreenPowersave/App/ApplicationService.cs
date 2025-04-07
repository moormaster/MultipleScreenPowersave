namespace MultipleScreenPowersave.App;

using System.Diagnostics;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using MultipleScreenPowersave.VCP;
using Serilog;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
/// ApplicationService providing functions to turn off Monitors based on activity.
/// </summary>
public class ApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    public ApplicationService()
    {
        Log.Logger.Information(
            "Using configuration file: {configurationFileName}",
            ConfigurationQueryFactory.GetConfigurationFileName()
        );

        var screenInformation = this.GetScreenInformation();
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
        ScreenInformation screenInformation = this.GetScreenInformation();

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

        PInvoke.GetCursorPos(out System.Drawing.Point currentCursorPosition);
        foreach (var displayMonitor in screenInformation.DisplayMonitors)
        {
            if (displayMonitor.MonitorRectangle.Contains(currentCursorPosition.ToPoint()))
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

        List<nint> windowHandles = [];

        PInvoke.EnumWindows(
            (windowHandle, param1) =>
            {
                windowHandles.Add(windowHandle);
                return true;
            },
            new LPARAM(0)
        );

        foreach (var windowHandle in windowHandles)
        {
            uint processId;
            unsafe
            {
                var result = PInvoke.GetWindowThreadProcessId(new HWND(windowHandle), &processId);
                ThrowHelper.ThrowLastErrorIfResultIsZero(result);
            }

            Process process;
            try
            {
                process = Process.GetProcessById((int)processId);
            }
            catch (ArgumentException)
            {
                // process is not active (anymore) - continue
                continue;
            }

            WindowProcessInformation windowProcessInformation;
            {
                var windowInfo = default(WINDOWINFO);
                PInvoke.GetWindowInfo(new HWND(windowHandle), ref windowInfo);

                var windowTextLength = PInvoke.GetWindowTextLength(new HWND(windowHandle));
                string windowText;
                unsafe
                {
                    fixed (char* windowTextBuffer = new char[windowTextLength + 1])
                    {
                        var result = PInvoke.GetWindowText(
                            new HWND(windowHandle),
                            windowTextBuffer,
                            windowTextLength + 1
                        );
                        ThrowHelper.ThrowLastErrorIfResultIsZero(result);

                        windowText = new string(windowTextBuffer);
                    }
                }

                windowProcessInformation = new WindowProcessInformation(
                    new WindowHandle((int)windowHandle),
                    process.ProcessName,
                    windowText,
                    (uint)windowInfo.dwStyle,
                    (uint)windowInfo.dwExStyle,
                    windowInfo.rcWindow.ToRect()
                );
            }

            if ((windowProcessInformation.DwStyle!.Value & (uint)WINDOW_STYLE.WS_VISIBLE) == 0)
                continue;

            if ((windowProcessInformation.DwStyle!.Value & (uint)WINDOW_STYLE.WS_MINIMIZE) > 0)
                continue;

            if (
                windowProcessInformation.Rectangle.Width == 0
                || windowProcessInformation.Rectangle.Height == 0
            )
                continue;

            var screenOfApp = System.Windows.Forms.Screen.FromHandle(windowHandle);

            // hacky way to get the display monitor handle
            var displayMonitorHandle = new DisplayMonitorHandle(screenOfApp.GetHashCode());
            var displayMonitor = screenInformation.DisplayMonitorByHandle[displayMonitorHandle];

            if (IsBlacklisted(blacklist, windowProcessInformation))
            {
                Log.Logger.Debug(
                    "Blacklisted ProcessName: \"{processName}\" - WindowTitle: \"{windowTitle}\" (#{windowHandle})",
                    process.ProcessName,
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

            foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
            {
                Log.Logger.Debug(
                    "PhysicalMonitor #{physicalMonitorHandle}: ProcessName: \"{processName}\" - WindowTitle: \"{windowText}\" (#{windowHandle})",
                    physicalMonitor.Handle,
                    process.ProcessName,
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

                    TurnOnMonitor(physicalMonitor);
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
                    TurnOffMonitor(physicalMonitor);
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
        foreach (var monitor in this.GetScreenInformation().PhysicalMonitors)
        {
            TurnOnMonitor(monitor);
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

    /// <summary>
    /// Turn off given physical monitor.
    /// </summary>
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn off monitor.</exception>
    private static void TurnOffMonitor(PhysicalMonitorInformation monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Handle.Value,
            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,
            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmsOff
        );

        if (hresult != 1)
        {
            throw new InvalidOperationException(
                $"Failed to turn off monitor #{monitor.Handle.Value}: HRESULT={hresult.ToHexString()}"
            );
        }
    }

    /// <summary>
    /// Turn on given physical monitor.
    /// </summary>
    /// <param name="monitor">Handle to the physical monitor.</param>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    private static void TurnOnMonitor(PhysicalMonitorInformation monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Handle.Value,
            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,
            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmOn
        );

        if (hresult != 1)
        {
            throw new InvalidOperationException(
                $"Failed to turn on monitor #{monitor.Handle.Value}: HRESULT={hresult.ToHexString()}"
            );
        }
    }

    private ScreenInformation GetScreenInformation()
    {
        return ScreenQuery.GetScreenInformation();
    }
}
