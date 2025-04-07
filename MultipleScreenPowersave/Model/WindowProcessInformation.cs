namespace MultipleScreenPowersave.Model;

using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Model.Handles;
using Windows.Win32;

/// <summary>
/// Dto containing process information belonging to a certain window.
/// </summary>
public class WindowProcessInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProcessInformation"/> class.
    /// </summary>
    /// <param name="handle">WindowHandle as returned by <see cref="PInvoke.EnumWindows(Windows.Win32.UI.WindowsAndMessaging.WNDENUMPROC, Windows.Win32.Foundation.LPARAM)"/>.</param>
    /// <param name="processName">Name of the process belonging to the window.</param>
    /// <param name="windowTitle">Title of the window.</param>
    /// <param name="dwStyle">Gets the Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">window styles</see>
    /// of the window. For a table of window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles">Window Styles</see>.</param>
    /// <param name="dwExStyle">Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">extended window styles</see>
    /// of the window. For a table of extended window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles">Extended Window Styles</see>.</param>
    /// <param name="rectangle">Rectangle describing the position and size of the window.</param>
    public WindowProcessInformation(
        WindowHandle handle,
        string processName,
        string windowTitle,
        uint? dwStyle,
        uint? dwExStyle,
        Rect rectangle
    )
    {
        Guard.IsFalse(handle == WindowHandle.Empty);
        Guard.IsNotNullOrWhiteSpace(processName);
        Guard.IsNotNull(windowTitle);
        // dwStyle may be null
        // dwExStyle may be null
        Guard.IsNotNull(rectangle);

        this.Handle = handle;
        this.ProcessName = processName;
        this.WindowTitle = windowTitle;
        this.DwStyle = dwStyle;
        this.DwExStyle = dwExStyle;
        this.Rectangle = rectangle;
    }

    /// <summary>
    /// Gets the WindowHandle as returned by <see cref="PInvoke.EnumWindows(Windows.Win32.UI.WindowsAndMessaging.WNDENUMPROC, Windows.Win32.Foundation.LPARAM)"/>.
    /// </summary>
    public WindowHandle Handle { get; }

    /// <summary>
    /// Gets the name of the process belonging to the window.
    /// </summary>
    public string ProcessName { get; }

    /// <summary>
    /// Gets the title of the window.
    /// </summary>
    public string WindowTitle { get; }

    /// <summary>
    /// Gets the Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">window styles</see>
    /// of the window. For a table of window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles">Window Styles</see>.
    /// </summary>
    public uint? DwStyle { get; }

    /// <summary>
    /// Gets the Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">extended window styles</see>
    /// of the window. For a table of extended window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles">Extended Window Styles</see>.
    /// </summary>
    public uint? DwExStyle { get; }

    /// <summary>
    /// Gets the rectangle describing the position and size of the window.
    /// </summary>
    public Rect Rectangle { get; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("WindowProcessInformation {");
        sb.AppendLine($"\tProcessName: \"{this.ProcessName}\",");
        sb.AppendLine($"\tWindowTitle: \"{this.WindowTitle}\"");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
