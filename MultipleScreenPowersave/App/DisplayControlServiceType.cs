namespace MultipleScreenPowersave.App;

/// <summary>
/// Enum describing the type of <see cref="IDisplayControlService"/>.
/// </summary>
public enum DisplayControlServiceType
{
    /// <summary>
    /// DisplayControl using DDC.
    /// </summary>
    Ddc,

    /// <summary>
    /// DisplayControl using backlight.
    /// </summary>
    Backlight,

    /// <summary>
    /// DisplayControl using black window.
    /// </summary>
    BlackWindow,
}
