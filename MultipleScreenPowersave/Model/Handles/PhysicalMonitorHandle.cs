namespace MultipleScreenPowersave.Model.Handles;

using StronglyTypedIds;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Strongly typed handle to a PhysicalMonitor as set by <see cref="PInvoke.GetPhysicalMonitorsFromHMONITOR(HMONITOR, Span{PHYSICAL_MONITOR})"/>.
/// </summary>
[StronglyTypedId(Template.Int)]
public readonly partial struct PhysicalMonitorHandle;
