using Newtonsoft.Json;

public class TagTimelineQueryResult
{
    public string User { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime Start { get; set; }
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime End { get; set; }
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan Duration { get; set; }
    public string Type { get; set; } = string.Empty;
}