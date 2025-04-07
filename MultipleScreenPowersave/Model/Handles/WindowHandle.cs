namespace MultipleScreenPowersave.Model.Handles;

using StronglyTypedIds;
using Windows.Win32;

/// <summary>
/// Strongly typed handle to a window as set by <see cref="PInvoke.EnumWindows(Windows.Win32.UI.WindowsAndMessaging.WNDENUMPROC, Windows.Win32.Foundation.LPARAM)"/>.
/// </summary>
[StronglyTypedId(Template.Int)]
public readonly partial struct WindowHandle;
