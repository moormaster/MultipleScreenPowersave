namespace MultipleScreenPowersave.Configuration;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class RegexJsonConverter : JsonConverter<Regex>
{
    public override Regex? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value == null)
            return null;

        return new Regex(value);
    }

    public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
