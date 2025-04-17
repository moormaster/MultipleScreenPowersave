namespace MultipleScreenPowersave.Query.WindowsImpl;

using MultipleScreenPowersave.App;
using MultipleScreenPowersave.Extensions;
using Windows.Win32;

/// <summary>
/// Win32 implementation of <see cref="IMouseQuery"/>.
/// </summary>
public class MouseQuery : IMouseQuery
{
    /// <inheritdoc/>
    public Microsoft.Maui.Graphics.Point GetCurrentMouseCursorPosition()
    {
        if (!PInvoke.GetCursorPos(out var currentCursorPosition))
            ThrowHelper.ThrowLastError();

        return currentCursorPosition.ToPoint();
    }
}
