using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm:ss"; // Format penyimpanan dalam JSON

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeOnly.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }

}

public class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    private const string Format = "HH:mm:ss"; // Format penyimpanan dalam JSON

    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null; // Return null if the token is null
        }

        string timeString = reader.GetString();
        if (TimeOnly.TryParseExact(timeString, Format, out TimeOnly time))
        {
            return time;
        }

        return null; // Return null if parsing fails
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(Format));
        }
        else
        {
            writer.WriteNullValue(); // Write null if the value is null
        }
    }
}
