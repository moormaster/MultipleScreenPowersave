namespace MultipleScreenPowersave.App;

using System.Diagnostics;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using MultipleScreenPowersave.VCP;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

public class ApplicationService
{
    private const int MainWindowHandleCacheLifetimeMs = 60000;

    private readonly ScreenInformation screenInformation;
    private readonly Dictionary<int, MainWindowHandleOfProcess> mainWindowHandleOfProcess = [];

    public ApplicationService()
    {
        this.screenInformation = ScreenQuery.GetScreenInformation();
        Console.WriteLine(this.screenInformation);
    }

    public void TurnOnOnlyUsedMonitors()
    {
        IDictionary<PhysicalMonitorHandle, bool> isMonitorNeeded = this.screenInformation.PhysicalMonitors.ToDictionary(v => v.Handle, v => false);
        BlacklistConfiguration blacklist = ConfigurationQueryFactory.GetConfigurationQuery().GetBlacklist();

        foreach (var displayMonitor in this.screenInformation.DisplayMonitors)
        {
            if (IsBlacklisted(blacklist, displayMonitor))
            {
                // ensure that physical screens of blacklisted monitors never gets powered off
                foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
                    isMonitorNeeded[physicalMonitor.Handle] = true;
            }
        }

        PInvoke.GetCursorPos(out Point currentCursorPosition);
        foreach (var displayMonitor in this.screenInformation.DisplayMonitors)
        {
            if (displayMonitor.MonitorRectangle.Contains(currentCursorPosition))
            {
                // enable physical monitors currently visited by the mouse cursor
                foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
                {
                    Console.WriteLine($"PhysicalMonitor #{physicalMonitor.Handle}: Mouse cursor position ({currentCursorPosition.X}x{currentCursorPosition.Y})");
                    isMonitorNeeded[physicalMonitor.Handle] = true;
                }
            }
        }

        foreach (var process in Process.GetProcesses())
        {
            // use cached MainWindowHandle to reduce expensive kernel calls
            if (!this.mainWindowHandleOfProcess.TryGetValue(process.Id, out var v) || (DateTime.Now.Ticks - v.LastSeenTicks) / TimeSpan.TicksPerMillisecond >= MainWindowHandleCacheLifetimeMs)
                this.mainWindowHandleOfProcess[process.Id] = new(process.Id, process.MainWindowHandle, DateTime.Now.Ticks);
            var mainWindowHandle = this.mainWindowHandleOfProcess[process.Id].MainWindowHandle;

            if (mainWindowHandle == default)
                continue;
            var windowInfo = default(WINDOWINFO);
            PInvoke.GetWindowInfo(new HWND(mainWindowHandle), ref windowInfo);

            if ((windowInfo.dwStyle & WINDOW_STYLE.WS_MINIMIZE) > 0)
                continue;

            if (windowInfo.rcClient.Width == 0 && windowInfo.rcClient.Height == 0)
                continue;

            var screenOfApp = Screen.FromHandle(mainWindowHandle);

            // hacky way to get the display monitor handle
            var displayMonitorHandle = new DisplayMonitorHandle(screenOfApp.GetHashCode());
            var displayMonitor = this.screenInformation.DisplayMonitorByHandle[displayMonitorHandle];

            if (IsBlacklisted(blacklist, process))
                continue;

            foreach (var physicalMonitor in displayMonitor.PhysicalMonitors)
            {
                Console.WriteLine($"PhysicalMonitor #{physicalMonitor.Handle}: {process.ProcessName} - \"{process.MainWindowTitle}\" (#{mainWindowHandle})");
                isMonitorNeeded[physicalMonitor.Handle] = true;
            }
        }

        foreach (var kv in isMonitorNeeded)
        {
            if (kv.Value)
            {
                Console.WriteLine($"Turning on physical monitor #{kv.Key}");
                TurnOnMonitor(kv.Key);
            }
            else
            {
                Console.WriteLine($"Turning off physical monitor #{kv.Key}");
                TurnOffMonitor(kv.Key);
            }
        }
    }

    public void TurnOnAllMonitors()
    {
        foreach (var monitor in this.screenInformation.PhysicalMonitors)
        {
            TurnOnMonitor(monitor.Handle);
        }
    }

    private static bool IsBlacklisted(BlacklistConfiguration blacklist, Process process)
    {
        return blacklist.Windows.Any(windowEntry => windowEntry.IsMatch(process));
    }

    private static bool IsBlacklisted(BlacklistConfiguration blacklist, DisplayMonitorInformation displayMonitor)
    {
        return blacklist.DisplayMonitors.Any(monitorEntry => monitorEntry.IsMatch(displayMonitor));
    }

    private static void TurnOffMonitor(PhysicalMonitorHandle monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Value,

            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,

            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmsOff);

        if (hresult != 1)
            throw new Exception($"Failed to turn off monitor #{monitor.Value}: HRESULT={hresult.ToHexString()}");
    }

    private static void TurnOnMonitor(PhysicalMonitorHandle monitor)
    {
        var hresult = PInvoke.SetVCPFeature(
            (HANDLE)monitor.Value,

            // see https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L4099
            FeatureConstants.PowerMode,

            // https://github.com/rockowitz/ddcutil/blob/b4039d15d87c2ec6e20b4bb79607cc7c979e74a1/src/vcp/vcp_feature_codes.c#L2635
            PowerModeValueConstants.DpmOn);

        if (hresult != 1)
            throw new Exception($"Failed to turn on monitor #{monitor.Value}: HRESULT={hresult.ToHexString()}");
    }
}