namespace MultipleScreenPowersave.Extensions;

using Microsoft.Maui.Graphics;

/// <summary>
/// Extension methods for System.Drawing.<see cref="Rectangle"/>.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Converts a System.Drawing.<see cref="Rectangle"/> to a Microsoft.Maui.Graphics.<see cref="Rect"/>.
    /// </summary>
    /// <param name="rectangle">The System.Drawing.<see cref="Rectangle"/> to convert.</param>
    /// <returns>The converted <see cref="Rect"/>.</returns>
    public static Rect ToRect(this Rectangle rectangle)
    {
        return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
}
