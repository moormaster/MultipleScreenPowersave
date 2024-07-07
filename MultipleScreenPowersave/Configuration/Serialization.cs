namespace MultipleScreenPowersave.Configuration;

using System.Text.Json;

public static class Serialization
{
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
