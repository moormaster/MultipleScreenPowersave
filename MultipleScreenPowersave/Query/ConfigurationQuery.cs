namespace MultipleScreenPowersave.Query;

using System.Text.Json;
using MultipleScreenPowersave.Configuration;

/// <summary>
/// Class used to query information from the configuration file.
/// </summary>
/// <param name="file">Path to the configuration file.</param>
public class ConfigurationQuery(string file)
{
    /// <summary>
    /// Returns the BlacklistConfiguration read from the configuration file.
    /// </summary>
    /// <returns>The BlacklistConfiguration.</returns>
    public BlacklistConfiguration GetBlacklist()
    {
        JsonDocument jsonConfiguration = JsonDocument.Parse(File.ReadAllText(file));

        var blacklist = jsonConfiguration.RootElement.GetProperty("blacklist");
        return JsonSerializer.Deserialize<BlacklistConfiguration>(
                blacklist,
                Serialization.GetJsonSerializerOptions()
            ) ?? new BlacklistConfiguration();
    }
}
