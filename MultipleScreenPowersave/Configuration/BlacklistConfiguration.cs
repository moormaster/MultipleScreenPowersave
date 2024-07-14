using System.Text.Json.Serialization;

namespace MultipleScreenPowersave.Configuration;

public class BlacklistConfiguration
{
    public BlacklistConfiguration()
    {
    }

    [JsonConstructor]
    public BlacklistConfiguration(IList<DisplayMonitorBlacklistEntry> displayMonitors, IList<ProcessBlacklistEntry> windows)
    {
        foreach (var monitor in displayMonitors ?? [])
            this.DisplayMonitors.Add(monitor);

        foreach (var window in windows ?? [])
            this.Windows.Add(window);
    }

    public IList<DisplayMonitorBlacklistEntry> DisplayMonitors { get; } = [];

    public IList<ProcessBlacklistEntry> Windows { get; } = [];
}
