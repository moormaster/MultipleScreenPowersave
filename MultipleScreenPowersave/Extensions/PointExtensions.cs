namespace MultipleScreenPowersave.Extensions;

using Avalonia;

/// <summary>
/// Extension methods for <see cref="Microsoft.Maui.Graphics.Point"/>.
/// </summary>
public static class PointExtensions
{
    /// <summary>
    /// Creates a <see cref="Avalonia.PixelPoint"/> from a <see cref="Microsoft.Maui.Graphics.Point"/>.
    /// </summary>
    /// <param name="point">The <see cref="Microsoft.Maui.Graphics.Point"/> to examine.</param>
    /// <returns>The <see cref="Avalonia.PixelPoint"/>.</returns>
    public static PixelPoint ToPixelPoint(this Microsoft.Maui.Graphics.Point point) =>
        new((int)point.X, (int)point.Y);
}
