namespace MultipleScreenPowersave.Model;

using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.Maui.Graphics;
using MultipleScreenPowersave.Model.Handles;

/// <summary>
/// Dto containing process information belonging to a certain window.
/// </summary>
public class WindowProcessInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProcessInformation"/> class.
    /// </summary>
    /// <param name="handle">WindowHandle.</param>
    /// <param name="processName">Name of the process belonging to the window.</param>
    /// <param name="windowTitle">Title of the window.</param>
    /// <param name="rectangle">Rectangle describing the position and size of the window.</param>
    public WindowProcessInformation(
        WindowHandle handle,
        string processName,
        string windowTitle,
        Rect rectangle
    )
    {
        Guard.IsFalse(handle == WindowHandle.Empty);
        Guard.IsNotNullOrWhiteSpace(processName);
        Guard.IsNotNull(windowTitle);
        Guard.IsNotNull(rectangle);

        this.Handle = handle;
        this.ProcessName = processName;
        this.WindowTitle = windowTitle;
        this.Rectangle = rectangle;
    }

    /// <summary>
    /// Gets the WindowHandle.
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
    /// Gets or sets the Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">window styles</see>
    /// of the window. For a table of window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles">Window Styles</see>.
    /// </summary>
    public uint? DwStyle { get; set; }

    /// <summary>
    /// Gets or sets the Windows-only <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo#members">extended window styles</see>
    /// of the window. For a table of extended window styles, see <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles">Extended Window Styles</see>.
    /// </summary>
    public uint? DwExStyle { get; set; }

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
