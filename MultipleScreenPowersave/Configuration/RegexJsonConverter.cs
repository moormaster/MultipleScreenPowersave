namespace MultipleScreenPowersave.Configuration;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// JsonConverter for serializing a regular expression.
/// </summary>
public class RegexJsonConverter : JsonConverter<Regex>
{
    /// <inheritdoc/>
    public override Regex? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();
        if (value == null)
            return null;

        return new Regex(value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
