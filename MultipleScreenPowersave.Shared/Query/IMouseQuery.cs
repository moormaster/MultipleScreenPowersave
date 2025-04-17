namespace MultipleScreenPowersave.Query;

using Microsoft.Maui.Graphics;

/// <summary>
/// Provides methods to query information about the mouse cursor.
/// </summary>
public interface IMouseQuery
{
    /// <summary>
    /// Returns the coordinates of the mouse cursor.
    /// </summary>
    /// <returns>Coordinates of the mouse cursor.</returns>
    public Point GetCurrentMouseCursorPosition();
}
