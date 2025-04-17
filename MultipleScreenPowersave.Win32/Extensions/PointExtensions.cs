namespace MultipleScreenPowersave.Extensions;

/// <summary>
/// Extension methods for System.Drawing.<see cref="Point"/>.
/// </summary>
public static class PointExtensions
{
    /// <summary>
    /// Converts a System.Drawing.<see cref="Point"/> to a Microsoft.Maui.Graphics.<see cref="Microsoft.Maui.Graphics.Rect"/>.
    /// </summary>
    /// <param name="point">The System.Drawing.<see cref="Point"/> to convert.</param>
    /// <returns>The converted <see cref="Microsoft.Maui.Graphics.Point"/>.</returns>
    public static Microsoft.Maui.Graphics.Point ToPoint(this Point point)
    {
        return new Microsoft.Maui.Graphics.Point(point.X, point.Y);
    }
}
