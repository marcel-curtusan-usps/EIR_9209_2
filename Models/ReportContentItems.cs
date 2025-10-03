using System;
using Newtonsoft.Json;
namespace EIR_9209_2.Models;

/// <summary>
/// Represents the content items for a report, including asset, event, and location details.
/// </summary>
public class ReportContentItems
{
    /// <summary>Unique identifier for the wrapper object.</summary>
    public string WrapperId { get; set; } = string.Empty;
    /// <summary>Unique identifier for the report item.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Name of the asset.</summary>
    public string AssetName { get; set; } = string.Empty;
    /// <summary>Identifier for the asset.</summary>
    public string AssetId { get; set; } = string.Empty;
    /// <summary>Identifier for the tag.</summary>
    public string TagId { get; set; } = string.Empty;
    /// <summary>Integration key for external systems.</summary>
    public string IntegrationKey { get; set; } = string.Empty;
    /// <summary>Event information associated with the report item.</summary>
    public EventInfo Event { get; set; } = new EventInfo();
    /// <summary>Subtype of the event.</summary>
    public string SubEvent { get; set; } = string.Empty;
    /// <summary>Type of the employee (clerk|mail handler etc..)</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Reference for the event.</summary>
    public string EventReference { get; set; } = string.Empty;
    /// <summary>Code representing the event.</summary>
    public string EventCode { get; set; } = string.Empty;
    /// <summary>Label describing the event.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label EventLabel { get; set; } = new Label();
    /// <summary>Label for the tag location.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label TagLocationLabel { get; set; } = new Label();
    /// <summary>Label for the tag status.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label TagStatusLabel { get; set; } = new Label();
    /// <summary>Label for the clock ring.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label ClockRingLabel { get; set; } = new Label();
    /// <summary>Label for the area.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label AreaLabel { get; set; } = new Label();
    /// <summary>Label for the premises.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label Premises { get; set; } = new Label();
    /// <summary>Label for the floor.</summary>
    [JsonConverter(typeof(LabelJsonConverter))]
    public Label Floor { get; set; } = new Label();
    /// <summary>Indicates if this is a snapshot event.</summary>
    public bool Snapshot { get; set; } = false;
    /// <summary>Duration of the event in milliseconds.</summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    /// <summary>Start time of the event (epoch milliseconds).</summary>
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime Start { get; set; } = DateTime.MinValue;
    /// <summary>End time of the event (epoch milliseconds).</summary>
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter))] public DateTime End { get; set; } = DateTime.MinValue;
}

/// <summary>
/// Represents event information with a text description.
/// </summary>
public class EventInfo
{
    /// <summary>Text description of the event.</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Represents a label with a text value.
/// </summary>
/// <summary>
/// Represents a label with a text value.
/// </summary>
public class Label
{
    /// <summary>Text value of the label.</summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Custom JsonConverter for Label to handle both string and object values.
/// </summary>
public class LabelJsonConverter : JsonConverter<Label>
{
    public override Label ReadJson(JsonReader reader, Type objectType, Label existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return new Label { Text = (string)reader.Value ?? string.Empty };
        }
        if (reader.TokenType == JsonToken.Null)
        {
            return new Label();
        }
        if (reader.TokenType == JsonToken.StartObject)
        {
            var obj = serializer.Deserialize<Label>(reader);
            return obj ?? new Label();
        }
        // If it's an empty string or unexpected, return default
        return new Label();
    }

    public override void WriteJson(JsonWriter writer, Label value, JsonSerializer serializer)
    {
        if (value != null && !string.IsNullOrEmpty(value.Text))
        {
            writer.WriteValue(value.Text);
        }
        else
        {
            writer.WriteNull();
        }
    }
}