using Newtonsoft.Json;

public class TagTimelineQueryResult
{
    public string User { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;

    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan Duration { get; set; }
    public string Type { get; set; } = string.Empty;
}