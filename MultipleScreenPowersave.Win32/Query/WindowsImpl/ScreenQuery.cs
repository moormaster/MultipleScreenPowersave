namespace MultipleScreenPowersave.Query.WindowsImpl;

using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Win32 implementation of <see cref="IScreenQuery"/>.
/// </summary>
public class ScreenQuery : IScreenQuery
{
    /// <summary>
    /// Determines screens available.
    /// </summary>
    /// <returns>ScreenInformation.</returns>
    public ScreenInformation GetScreenInformation()
    {
        List<DisplayMonitorInformation> displayMonitors = [];

        unsafe
        {
            Dictionary<HMONITOR, Microsoft.Maui.Graphics.Rect> displayMonitorRectangles = [];
            Dictionary<string, DISPLAY_DEVICEW> displayDeviceByDeviceName = [];

            // see https://learn.microsoft.com/de-de/windows/win32/api/winuser/nf-winuser-enumdisplaydevicesw
            var iDisplayAdapter = 0u;
            DISPLAY_DEVICEW displayAdapter = default;
            displayAdapter.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));
            while (
                PInvoke.EnumDisplayDevices(null, iDisplayAdapter++, ref displayAdapter, dwFlags: 0)
            )
            {
                var iDisplayDevice = 0u;
                DISPLAY_DEVICEW displayDevice = default;
                displayDevice.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));
                while (
                    PInvoke.EnumDisplayDevices(
                        displayAdapter.DeviceName.ToString(),
                        iDisplayDevice++,
                        ref displayDevice,
                        dwFlags: 0
                    )
                )
                {
                    displayDeviceByDeviceName.Add(
                        displayDevice.DeviceName.ToString(),
                        displayDevice
                    );
                }
            }

            // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
            PInvoke.EnumDisplayMonitors(
                default,
                (RECT?)null,
                (
                    displayMonitorHandle,
                    deviceContextHandle,
                    displayMonitor,
                    applicationDefinedData
                ) =>
                {
                    displayMonitorRectangles.Add(
                        displayMonitorHandle,
                        ((Rectangle)(*displayMonitor)).ToRect()
                    );
                    return (BOOL)true;
                },
                0
            );

            // enumerate WMI instances
            var wmiInstanceNameByEdidhex = GetWmiInstanceNameByEdidHex();

            foreach (var hMonitor in displayMonitorRectangles.Keys)
            {
                MONITORINFOEXW monitorInfo = default;
                monitorInfo.monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEXW));
                unsafe
                {
                    MONITORINFO* pointer = (MONITORINFO*)&monitorInfo;
                    PInvoke.GetMonitorInfo(hMonitor, pointer);
                }

                // see https://learn.microsoft.com/en-us/windows/win32/api/physicalmonitorenumerationapi/nf-physicalmonitorenumerationapi-getnumberofphysicalmonitorsfromhmonitor
                PInvoke.GetNumberOfPhysicalMonitorsFromHMONITOR(
                    hMonitor,
                    out uint numberOfPhysicalMonitors
                );

                PHYSICAL_MONITOR[] physicalMonitors = new PHYSICAL_MONITOR[
                    numberOfPhysicalMonitors
                ];

                // see https://learn.microsoft.com/en-us/windows/win32/api/physicalmonitorenumerationapi/nf-physicalmonitorenumerationapi-getphysicalmonitorsfromhmonitor
                PInvoke.GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitors);

                var displayMonitor = new DisplayMonitorInformation(
                    new DisplayMonitorHandle((int)hMonitor),
                    isPrimary: (monitorInfo.monitorInfo.dwFlags & PInvoke.MONITORINFOF_PRIMARY)
                        != 0,
                    monitorInfo.monitorInfo.rcMonitor.ToRect()
                );

                string deviceName = monitorInfo.szDevice.ToString();
                string? deviceId = null;
                if (displayDeviceByDeviceName.Any(kv => kv.Key.StartsWith(deviceName)))
                {
                    deviceId = displayDeviceByDeviceName
                        .First(kv => kv.Key.StartsWith(deviceName))
                        .Value.DeviceID.ToString();
                }

                string? edidHex = null;
                if (deviceId != null)
                {
                    var (monitorId, driverId) = ParseMonitorDriverId(deviceId);
                    edidHex = GetMonitorEdidHexFromRegistry(monitorId, driverId)
                        .Substring(0, 128 * 2);
                }

                string? wmiInstanceName = null;
                if (edidHex != null)
                {
                    if (wmiInstanceNameByEdidhex.TryGetValue(edidHex, out var tuple))
                        wmiInstanceName = tuple.InstanceName;
                }

                uint physicalMonitorIndex = 0;
                displayMonitor.PhysicalMonitors.AddRange(
                    physicalMonitors.Select(element => new PhysicalMonitorInformation(
                        new PhysicalMonitorHandle((int)element.hPhysicalMonitor),
                        physicalMonitorIndex++
                    )
                    {
                        Description = element.szPhysicalMonitorDescription.ToString(),
                        DeviceId = deviceId,
                        EdidHex = edidHex,
                        WmiInstanceName = wmiInstanceName,
                    })
                );

                displayMonitors.Add(displayMonitor);
            }
        }

        return new ScreenInformation(displayMonitors);
    }

    private static Dictionary<
        string,
        (string EdidHex, string InstanceName)
    > GetWmiInstanceNameByEdidHex()
    {
        Dictionary<string, (string EdidHex, string InstanceName)> result = [];

        // see https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitordescriptormethods
        ManagementScope scope = new(@"root\WMI");
        ObjectQuery query = new("SELECT * FROM WmiMonitorDescriptorMethods");

        ManagementObjectSearcher searcher = new(scope, query);
        foreach (var wmiObject in searcher.Get().OfType<ManagementObject>())
        {
            try
            {
                // see https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmigetmonitorraweedidv1block-wmimonitordescriptormethods
                var inParameters = wmiObject.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                inParameters["BlockId"] = 0;
                var outParameters = wmiObject.InvokeMethod(
                    "WmiGetMonitorRawEEdidV1Block",
                    inParameters,
                    new()
                );

                var edidBytes = (byte[])outParameters["BlockContent"];
                var edidHex = Convert.ToHexString(edidBytes);
                result.Add(edidHex, (edidHex, wmiObject["InstanceName"]?.ToString()!));
            }
            catch (ManagementException)
            {
                // ignore exceptions from monitors not supporting DDC.
            }
        }

        return result;
    }

    private static string GetMonitorEdidHexFromRegistry(string monitorId, string driverId)
    {
        var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        key = key.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\Display\{monitorId}");

        var instanceIds = key!.GetSubKeyNames();
        foreach (var instanceIdCandidate in instanceIds)
        {
            var instanceKey = key.OpenSubKey(instanceIdCandidate);
            if ((string?)instanceKey!.GetValue("Driver") != driverId)
                continue;

            var deviceParametersKey = instanceKey.OpenSubKey("Device Parameters");
            var edidBytes = (byte[])deviceParametersKey!.GetValue("EDID")!;

            return Convert.ToHexString(edidBytes);
        }

        throw new KeyNotFoundException(
            $"Could not find registry key for monitor instance with monitorId=\"{monitorId}\" and driverId=\"{driverId}\"."
        );
    }

    private static (string MonitorId, string DriverId) ParseMonitorDriverId(string deviceId)
    {
        var match = Regex.Match(deviceId, @"MONITOR\\(?<monitorId>\w+)\\(?<driverId>.*)");
        return (match.Groups["monitorId"].Value, match.Groups["driverId"].Value);
    }
}
