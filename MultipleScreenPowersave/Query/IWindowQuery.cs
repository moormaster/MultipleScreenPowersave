namespace MultipleScreenPowersave.Query;

using MultipleScreenPowersave.Model;

/// <summary>
/// Provides methods to query information about windows shown in the system.
/// </summary>
public interface IWindowQuery
{
    /// <summary>
    /// Returns information about windows shown in the system.
    /// </summary>
    /// <returns>Enumerable of <see cref="WindowProcessInformation"/>'s.</returns>
    IEnumerable<WindowProcessInformation> GetWindows();
}
