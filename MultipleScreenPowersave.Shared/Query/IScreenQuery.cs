namespace MultipleScreenPowersave.Query;

using MultipleScreenPowersave.Model;

/// <summary>
/// Provides methods to query information about screens available in the system.
/// </summary>
public interface IScreenQuery
{
    /// <summary>
    /// Determines screens available.
    /// </summary>
    /// <returns>ScreenInformation.</returns>
    public ScreenInformation GetScreenInformation();
}
