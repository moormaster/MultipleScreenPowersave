namespace MultipleScreenPowersave.Configuration;

using MultipleScreenPowersave.App;

/// <summary>
/// Serializable class representing the configuration for <see cref="IBlackWindowService"/>.
/// </summary>
public class BlackWindowServiceOptions
{
    /// <summary>
    /// Gets or sets the option entries matching to physical monitors.
    /// </summary>
    public IList<PhysicalMonitorBlackWindowEntry> PhysicalMonitors { get; set; } = [];
}
