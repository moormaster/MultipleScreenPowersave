namespace MultipleScreenPowersave.App;

/// <summary>
/// Application service for MultipleScreenPowersave application.
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Turns on physical monitors that currently
    ///     - show at least one window,
    ///     - show the mouse cursor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnAllMonitors();

    /// <summary>
    /// Turn on all physical monitors.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failure to turn on monitor.</exception>
    public void TurnOnOnlyUsedMonitors();
}
