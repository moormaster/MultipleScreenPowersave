namespace MultipleScreenPowersave;

using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Configuration;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using MultipleScreenPowersave.Query;
using MultipleScreenPowersave.VCP;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

public static partial class Program
{
    public static void Main()
    {
        PInvoke.GetCursorPos(out Point currentCursorPosition);
        Console.WriteLine($"Current mouse position: {currentCursorPosition.X}, {currentCursorPosition.Y}");

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            Console.WriteLine(screen);
        }

        var screenInformation = ScreenQuery.GetScreenInformation();
        Console.WriteLine(screenInformation);

        for (var i = 0; i < 120; i++)
        {
            TurnOnOnlyUsedMonitors(screenInformation);

            Thread.Sleep(1000);
        }

        foreach (var monitor in screenInformation.PhysicalMonitors)
        {
            TurnOnMonitor(monitor.Handle);
        }
    }

    private static void TurnOnOnlyUsedMonitors(ScreenInformation screenInformation)
    {
        IDictionary<PhysicalMonitorHandle, bool> isMonitorNeeded = screenInformation.PhysicalMonitors.ToDictionary(v => v.Handle, v => false);
        IReadOnlyList<ProcessBlacklistEntry> blacklist = ConfigurationQueryFactory.GetConfigurationQuery().GetProcessBlacklist();

        foreach (var process in Process.GetProcesses())
        {
            if (process.MainWindowHandle == default)
                continue;

            var windowInfo = default(WINDOWINFO);
            PInvoke.GetWindowInfo(new HWND(process.MainWindowHandle), ref windowInfo);

            if ((windowInfo.dwStyle & WINDOW_STYLE.WS_MINIMIZE) > 0)
                continue;

            if (windowInfo.rcClient.Width == 0 && windowInfo.rcClient.Height == 0)
                continue;

            if (IsBlacklisted(blacklist, process))
                continue;

            var screenOfApp = Screen.FromHandle(process.MainWindowHandle);

            // hacky way to get the display monitor handle
            var screenHandle = new DisplayMonitorHandle(screenOfApp.GetHashCode());
            var screen = screenInformation.DisplayMonitorByHandle[screenHandle];

            foreach (var monitor in screen.PhysicalMonitors)
            {
                Console.WriteLine($"PhysicalMonitor #{monitor.Handle}: {process.ProcessName} - {process.MainWindowTitle} (#{process.MainWindowHandle})");
                isMonitorNeeded[monitor.Handle] = true;
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

    private static bool IsBlacklisted(IReadOnlyList<ProcessBlacklistEntry> blacklist, Process process)
    {
        return blacklist.Any(
            entry =>
            {
                if (entry.ProcessName != null)
                {
                    if (!entry.ProcessName.IsMatch(process.ProcessName))
                        return false;
                }

                if (entry.WindowTitle != null)
                {
                    if (!entry.WindowTitle.IsMatch(process.MainWindowTitle))
                        return false;
                }

                return true;
            });
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