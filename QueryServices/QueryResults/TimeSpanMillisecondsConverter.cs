using Newtonsoft.Json;
using System;

public class TimeSpanMillisecondsConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var milliseconds = Convert.ToInt64(reader.Value);
            return TimeSpan.FromMilliseconds(milliseconds);
        }
        catch (Exception e)
        {
            throw new JsonSerializationException($"Exception during milliseconds to timespan deserialization: {e}");
        }
    }

    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue((long)value.TotalMilliseconds);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}