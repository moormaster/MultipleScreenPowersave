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

    public BlacklistConfiguration GetBlacklist()
    {
        JsonDocument jsonConfiguration = JsonDocument.Parse(File.ReadAllText(this.file));

        var blacklist = jsonConfiguration.RootElement.GetProperty("blacklist");
        return JsonSerializer.Deserialize<BlacklistConfiguration>(
            blacklist,
            Serialization.GetJsonSerializerOptions())
            ?? new BlacklistConfiguration();
    }
}
