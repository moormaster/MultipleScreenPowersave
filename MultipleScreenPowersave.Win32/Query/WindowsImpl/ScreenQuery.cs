namespace MultipleScreenPowersave.Query.WindowsImpl;

using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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

                displayMonitor.PhysicalMonitors.AddRange(
                    physicalMonitors.Select(element => new PhysicalMonitorInformation(
                        new PhysicalMonitorHandle((int)element.hPhysicalMonitor)
                    )
                    {
                        Description = element.szPhysicalMonitorDescription.ToString(),
                        DeviceId = deviceId,
                    })
                );

                displayMonitors.Add(displayMonitor);
            }
        }

        return new ScreenInformation(displayMonitors);
    }
}
