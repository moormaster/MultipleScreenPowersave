namespace MultipleScreenPowersave.Extensions;

using Microsoft.Maui.Graphics;
using Windows.Win32.Foundation;

/// <summary>
/// Extension methods for Windows.Win32.Foundation.<see cref="RECT"/>.
/// </summary>
public static class RectExtensions
{
    /// <summary>
    /// Converts a Windows.Win32.Foundation.<see cref="RECT"/> to a Microsoft.Maui.Graphics.<see cref="Rect"/>.
    /// </summary>
    /// <param name="rectangle">The Windows.Win32.Foundation.<see cref="RECT"/> to convert.</param>
    /// <returns>The converted <see cref="Rect"/>.</returns>
    internal static Rect ToRect(this RECT rectangle)
    {
        return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
}
