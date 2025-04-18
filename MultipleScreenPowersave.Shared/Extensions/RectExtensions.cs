namespace MultipleScreenPowersave.Extensions;

using Microsoft.Maui.Graphics;

/// <summary>
/// Extension methods for Microsoft.Maui.Graphics.<see cref="Rect"/>.
/// </summary>
public static class RectExtensions
{
    /// <summary>
    /// Calculates the area of a Microsoft.Maui.Graphics.<see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The Microsoft.Maui.Graphics.<see cref="Rect"/> to examine.</param>
    /// <returns>The area of the <see cref="Rect"/>.</returns>
    public static double GetArea(this Rect rect)
    {
        return rect.Width * rect.Height;
    }
}
