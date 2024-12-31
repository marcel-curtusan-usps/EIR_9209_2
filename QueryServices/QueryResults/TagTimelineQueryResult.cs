using Newtonsoft.Json;

public class TagTimelineQueryResult
{
    public required string User { get; set; }
    public required string Area { get; set; }
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime Start { get; set; }
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime End { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan Duration { get; set; }
    public required string Type { get; set; }
}