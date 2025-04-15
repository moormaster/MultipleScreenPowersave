using Microsoft.Maui.Graphics;

public record WindowProcessInformation
{
    /// <summary>
    /// Gets or sets the WindowHandle.
    /// </summary>
    public int Handle { get; set; }

    /// <summary>
    /// Gets or sets the name of the process belonging to the window.
    /// </summary>
    public string ProcessName { get; set; }

    /// <summary>
    /// Gets or sets the title of the window.
    /// </summary>
    public string WindowTitle { get; set; }

    public string X11MapState { get; set; }
    public IList<string> X11WindowStates { get; set; } = [];
    public string? X11WindowType { get; set; }

    /// <summary>
    /// Gets or sets the rectangle describing the position and size of the window.
    /// </summary>
    public Rect Rectangle { get; set; }
}
