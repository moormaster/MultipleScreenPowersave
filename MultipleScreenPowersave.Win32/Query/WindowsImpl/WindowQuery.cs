namespace MultipleScreenPowersave.Query.WindowsImpl;

using System.Diagnostics;
using MultipleScreenPowersave.App;
using MultipleScreenPowersave.Extensions;
using MultipleScreenPowersave.Model;
using MultipleScreenPowersave.Model.Handles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
/// Win32 implementation of <see cref="IWindowQuery"/>.
/// </summary>
public class WindowQuery : IWindowQuery
{
    /// <inheritdoc/>
    public IEnumerable<WindowProcessInformation> GetWindows()
    {
        List<WindowProcessInformation> windowProcessInformations = [];
        List<nint> windowHandles = [];

        PInvoke.EnumWindows(
            (windowHandle, param1) =>
            {
                windowHandles.Add(windowHandle);
                return true;
            },
            new LPARAM(0)
        );

        foreach (var windowHandle in windowHandles)
        {
            uint processId;
            unsafe
            {
                var result = PInvoke.GetWindowThreadProcessId(new HWND(windowHandle), &processId);
                ThrowHelper.ThrowLastErrorIfResultIsZero(result);
            }

            Process process;
            try
            {
                process = Process.GetProcessById((int)processId);
            }
            catch (ArgumentException)
            {
                // process is not active (anymore) - continue
                continue;
            }

            WindowProcessInformation windowProcessInformation;
            {
                var windowInfo = default(WINDOWINFO);
                if (!PInvoke.GetWindowInfo(new HWND(windowHandle), ref windowInfo))
                {
                    // window is not existing (anymore) - continue
                    continue;
                }

                var windowTextLength = PInvoke.GetWindowTextLength(new HWND(windowHandle));
                string windowText;
                unsafe
                {
                    fixed (char* windowTextBuffer = new char[windowTextLength + 1])
                    {
                        var result = PInvoke.GetWindowText(
                            new HWND(windowHandle),
                            windowTextBuffer,
                            windowTextLength + 1
                        );
                        ThrowHelper.ThrowLastErrorIfResultIsZero(result);

                        windowText = new string(windowTextBuffer);
                    }
                }

                windowProcessInformation = new WindowProcessInformation(
                    new WindowHandle((int)windowHandle),
                    process.ProcessName,
                    windowText,
                    windowInfo.rcWindow.ToRect()
                )
                {
                    DwStyle = (uint)windowInfo.dwStyle,
                    DwExStyle = (uint)windowInfo.dwExStyle,
                };
            }

            if (!IsWindowVisible(windowProcessInformation))
                continue;

            if (IsWindowMinimized(windowProcessInformation))
                continue;

            if (IsWindowAreaZero(windowProcessInformation))
                continue;

            windowProcessInformations.Add(windowProcessInformation);
        }

        return windowProcessInformations;
    }

    private static bool IsWindowAreaZero(WindowProcessInformation windowProcessInformation) =>
        windowProcessInformation.Rectangle.Width == 0
        || windowProcessInformation.Rectangle.Height == 0;

    private static bool IsWindowMinimized(WindowProcessInformation windowProcessInformation) =>
        (windowProcessInformation.DwStyle!.Value & (uint)WINDOW_STYLE.WS_MINIMIZE) > 0;

    private static bool IsWindowVisible(WindowProcessInformation windowProcessInformation) =>
        (windowProcessInformation.DwStyle!.Value & (uint)WINDOW_STYLE.WS_VISIBLE) != 0;
}
