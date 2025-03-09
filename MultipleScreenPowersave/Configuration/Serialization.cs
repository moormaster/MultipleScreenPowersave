namespace MultipleScreenPowersave.Configuration;

using System.Text.Json;

/// <summary>
/// Helper class providing JsonSerializerOptions to be used within this project.
/// </summary>
public static class Serialization
{
    /// <summary>
    /// Determines JsonSerializerOptions to be used within this project.
    /// </summary>
    /// <returns>The JsonSerializerOptions instance.</returns>
    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        options.Converters.Add(new RegexJsonConverter());

        return options;
    }
}
