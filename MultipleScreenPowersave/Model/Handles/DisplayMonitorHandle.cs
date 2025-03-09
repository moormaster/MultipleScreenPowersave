namespace MultipleScreenPowersave.Model.Handles;

using StronglyTypedIds;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

/// <summary>
/// Strongly typed handle to a DisplayMonitor as set by the lpfnEnum callback of <see cref="PInvoke.EnumDisplayMonitors(HDC, RECT?, MONITORENUMPROC, LPARAM)"/>.
/// </summary>
[StronglyTypedId(Template.Int)]
public readonly partial struct DisplayMonitorHandle;
