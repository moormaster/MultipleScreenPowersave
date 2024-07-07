namespace MultipleScreenPowersave.Query;

using System.Text.Json;
using MultipleScreenPowersave.Configuration;

public class ConfigurationQuery
{
    private readonly string file;

    public ConfigurationQuery(string file)
    {
        this.file = file;
    }

    public IReadOnlyList<ProcessBlacklistEntry> GetProcessBlacklist()
    {
        JsonDocument jsonConfiguration = JsonDocument.Parse(File.ReadAllText(this.file));

        var windowBlacklist = jsonConfiguration.RootElement.GetProperty("windowBlacklist");
        return JsonSerializer.Deserialize<IList<ProcessBlacklistEntry>>(
            windowBlacklist,
            Serialization.GetJsonSerializerOptions())?.ToList()
            ?? [];
    }
}
