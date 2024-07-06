using CommunityToolkit.Diagnostics;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace MultipleScreenPowersave.Query;

public static class ScreenQuery
{
    public static ScreenInformation GetScreenInformation()
    {
        List<DisplayMonitorInformation> displayMonitors = new();

        unsafe
        {
            Dictionary<HMONITOR, Rectangle> displayMonitorRectangles = new Dictionary<HMONITOR, Rectangle>();

            // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
            PInvoke.EnumDisplayMonitors(
                new HDC(),
                (RECT?)null,
                (displayMonitorHandle, deviceContextHandle, displayMonitor, applicationDefinedData) =>
                {
                    displayMonitorRectangles.Add(displayMonitorHandle, (Rectangle)(*displayMonitor));
                    return (BOOL)true;
                },
                0);

            foreach (var hMonitor in displayMonitorRectangles.Keys)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo);

                // see https://learn.microsoft.com/en-us/windows/win32/api/physicalmonitorenumerationapi/nf-physicalmonitorenumerationapi-getnumberofphysicalmonitorsfromhmonitor
                PInvoke.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint numberOfPhysicalMonitors);

                PHYSICAL_MONITOR[] physicalMonitors = new PHYSICAL_MONITOR[numberOfPhysicalMonitors];

                // see https://learn.microsoft.com/en-us/windows/win32/api/physicalmonitorenumerationapi/nf-physicalmonitorenumerationapi-getphysicalmonitorsfromhmonitor
                PInvoke.GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitors);

                displayMonitors.Add(new DisplayMonitorInformation(
                    new DisplayMonitorHandle((int)hMonitor),
                    isPrimary: (monitorInfo.dwFlags & PInvoke.MONITORINFOF_PRIMARY) != 0,
                    monitorInfo.rcMonitor,
                    physicalMonitors.Select(
                        element => new PhysicalMonitorInformation(
                            new PhysicalMonitorHandle((int)element.hPhysicalMonitor),
                            new DisplayMonitorHandle((int)hMonitor),
                            element.szPhysicalMonitorDescription.ToString()))));
            }
        }


        return new ScreenInformation(displayMonitors);
    }
}
