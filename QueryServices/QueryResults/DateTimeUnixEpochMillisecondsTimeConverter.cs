using Newtonsoft.Json;

public class DateTimeUnixEpochMillisecondsTimeConverter : JsonConverter<DateTime>
{
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var unixTimeSeconds = Convert.ToInt64(reader.Value);
            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeSeconds);
            return dateTimeOffset.DateTime;
        }
        catch (Exception e)
        {
            throw new JsonSerializationException($"Exception during unix time deserialization: {e}");
        }
    }

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(DateTimeToUnixEpochMilliseconds(value));
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    private static long DateTimeToUnixEpochMilliseconds(DateTime dateTime)
    {
        DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
        DateTimeOffset unixEpochStartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        TimeSpan timeSinceUnixEpoch = dateTimeOffset - unixEpochStartTime;
        return (long)timeSinceUnixEpoch.TotalMilliseconds;
    }
}