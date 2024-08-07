using Newtonsoft.Json;

public class TagTimelineQueryResult
{
    public string User { get; set; }
    public string Area { get; set; }
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime Start { get; set; }
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime End { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan Duration { get; set; }
    public string Type { get; set; }
}