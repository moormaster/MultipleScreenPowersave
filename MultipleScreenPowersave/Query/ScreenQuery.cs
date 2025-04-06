namespace MultipleScreenPowersave.Query;

using System.Runtime.InteropServices;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Class used to query information about screens available in the system.
/// </summary>
public static class ScreenQuery
{
    /// <summary>
    /// Determines screens available.
    /// </summary>
    /// <returns>ScreenInformation.</returns>
    public static ScreenInformation GetScreenInformation()
    {
        List<DisplayMonitorInformation> displayMonitors = [];

        unsafe
        {
            Dictionary<HMONITOR, Microsoft.Maui.Graphics.Rect> displayMonitorRectangles = [];

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
                        ((System.Drawing.Rectangle)(*displayMonitor)).ToRect()
                    );
                    return (BOOL)true;
                },
                0
            );

            foreach (var hMonitor in displayMonitorRectangles.Keys)
            {
                MONITORINFO monitorInfo = default;
                monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));
                PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo);

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

                displayMonitors.Add(
                    new DisplayMonitorInformation(
                        new DisplayMonitorHandle((int)hMonitor),
                        isPrimary: (monitorInfo.dwFlags & PInvoke.MONITORINFOF_PRIMARY) != 0,
                        monitorInfo.rcMonitor.ToRect(),
                        physicalMonitors.Select(element => new PhysicalMonitorInformation(
                            new PhysicalMonitorHandle((int)element.hPhysicalMonitor),
                            new DisplayMonitorHandle((int)hMonitor),
                            element.szPhysicalMonitorDescription.ToString()
                        ))
                    )
                );
            }
        }

        return new ScreenInformation(displayMonitors);
    }
}
