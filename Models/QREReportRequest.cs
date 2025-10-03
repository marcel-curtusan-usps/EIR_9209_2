using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EIR_9209_2.Models;
/// <summary>
/// Enums for event types, subevents, views, etc.
/// </summary>
public enum ReportEvent
{
    /// <summary>
    /// Represents an event related to a specific area.
    /// </summary>
    AREA,
    /// <summary>
    /// Represents an event related to the status of a tag.
    /// </summary>
    TAG_STATUS,
    /// <summary>
    /// Represents an event related to clock or timing information.
    /// </summary>
    CLOCK
}
/// <summary>
/// Represents the sub-events that can occur within a report event.
/// </summary>
public enum ReportSubEvent
{
    /// <summary>
    /// Represents the "IN" sub-event.
    /// </summary>
    IN,
    /// <summary>
    /// Represents the "OUT" sub-event.
    /// </summary>
    OUT,
    /// <summary>
    /// Represents the "BEGIN_TOUR" sub-event.
    /// </summary>
    BEGIN_TOUR,
    /// <summary>
    /// Represents the "OUT_TO_LUNCH" sub-event.
    /// </summary>
    OUT_TO_LUNCH,
    /// <summary>
    /// Represents the "IN_FROM_LUNCH" sub-event.
    /// </summary>
    IN_FROM_LUNCH,
    /// <summary>
    /// Represents the "END_TOUR" sub-event.
    /// </summary>
    END_TOUR,
    /// <summary>
    /// Represents the "TAG_MOVING" sub-event.
    /// </summary>
    TAG_MOVING,
    /// <summary>
    /// Represents the "TAG_STATIC" sub-event.
    /// </summary>
    TAG_STATIC,
    /// <summary>
    /// Represents the "TAG_INVISIBLE" sub-event.
    /// </summary>
    TAG_INVISIBLE
}
/// <summary>
/// Represents the different views available for report content.
/// </summary>
public enum ReportContentView
{
    /// <summary>
    /// Represents the "CONTENT" view.
    /// </summary>
    CONTENT,
    /// <summary>
    /// Represents the "GLOBAL" view.
    /// </summary>
    GLOBAL,
    /// <summary>
    /// Represents the "CLOCK" view.
    /// </summary>
    CLOCK,
    /// <summary>
    /// Represents the "AREA" view.
    /// </summary>
    AREA,
    /// <summary>
    /// Represents the "ASSET" view.
    /// </summary>
    ASSET
}

/// Data models for the request/response
public class ReportTimeParameters
{
    /// <summary>
    /// Gets or sets the start time of the report.
    /// </summary>
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter)), Required] public DateTime? Start { get; set; }
    /// <summary>
    /// Gets or sets the end time of the report.
    /// </summary>
    [JsonConverter(typeof(DateTimeUnixEpochMillisecondsTimeConverter)), Required] public DateTime? End { get; set; }
    /// <summary>
    /// Gets or sets the time zone of the report.
    /// </summary>
    public SchedulerParameters? SchedulerParameters { get; set; }
}
/// <summary>
/// Represents the parameters for a report, including the time range and events.
/// </summary>
public class SchedulerParameters
{
    /// <summary>
    /// Gets or sets the frequency of the report.
    /// </summary>
    public string Frequency { get; set; } = "DAILY"; // "DAILY", "WEEKLY", "MONTHLY"
    /// <summary>
    /// Gets or sets the frequency amount for the report.
    /// </summary>
    public int FrequencyAmount { get; set; }
    /// <summary>
    /// Gets or sets the hour at which the report should be repeated.
    /// </summary>
    public string RepeatAtHour { get; set; } = "00:00"; // "HH:mm"
    /// <summary>
    /// Gets or sets a value indicating whether the report should end never.
    /// </summary>
    public bool? EndNever { get; set; }
    /// <summary>
    /// Gets or sets the number of times the report should be repeated.
    /// </summary>
    public int? EndRecurrences { get; set; }
    /// <summary>
    ///     Gets or sets the end date of the report.
    /// </summary>
    public long? EndDate { get; set; }
}
/// <summary>
/// Represents the configuration parameters for report areas.
/// </summary>
public class ReportAreaConfigParameters
{
    /// <summary>
    /// Gets or sets the minimum time spent on the area.
    /// </summary>
    [Required, JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan? MinTimeOnArea { get; set; }
    /// <summary>
    /// Gets or sets the time step for the area.
    /// </summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan TimeStep { get; set; }
    /// <summary>
    /// Gets or sets the activation time for the area.
    /// </summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan ActivationTime { get; set; }
    /// <summary>
    /// Gets or sets the deactivation time for the area.
    /// </summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan DeactivationTime { get; set; }
    /// <summary>
    /// Gets or sets the time after which a tag is considered to have disappeared from the area.
    /// </summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan DisappearTime { get; set; }
    /// <summary>
    /// Gets or sets the time after which a tag is considered to have re-entered the area.
    /// </summary>
    [JsonConverter(typeof(TimeSpanMillisecondsConverter))] public TimeSpan ReentryTime { get; set; }
}
/// <summary>
/// Represents the configuration for report areas.
/// </summary>
public class ReportAreaConfiguration
{
    /// <summary>
    /// Gets or sets the list of area identifiers to include in the report.
    /// </summary>
    public List<int> AreaIds { get; set; } = new();
    /// <summary>
    /// Gets or sets a value indicating whether overlapping areas are enabled.
    /// </summary>
    public bool EnableOverlapping { get; set; }
    /// <summary>
    /// Gets or sets the configuration parameters for the report areas.
    /// </summary>
    public ReportAreaConfigParameters ConfigParameters { get; set; } = new();
}
/// <summary>
/// Represents the configuration for tag status in the report.
/// </summary>
public class ReportTagStatusConfiguration
{
    /// <summary>
    /// Gets or sets the list of sub-events for the tag status.
    /// </summary>
    public List<ReportSubEvent> SubEvents { get; set; } = new();
}
/// <summary>
/// Represents the parameters for a report event.
/// </summary>
public class ReportNotificationEmail
{
    /// <summary>
    /// Gets or sets the list of email addresses to notify when the report is ready.
    /// </summary>
    public List<string> Emails { get; set; } = new();
}
/// <summary>
/// Represents a web hook configuration for report notifications.
/// </summary>
public class ReportWebHook
{
    /// <summary>
    ///     Gets or sets the URL of the web hook to notify when the report is ready.
    /// </summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the headers to include in the web hook notification.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
    /// <summary>
    /// Gets or sets the view type for the web hook notification.
    /// </summary>
    public string View { get; set; } = string.Empty; // "GLOBAL", "LEGACY", "RAW_DATA"
}
/// <summary>
/// Represents additional information for the report.
/// </summary>
public class ReportAdditionalInformation
{
    /// <summary>
    /// Gets or sets the email notification configuration for the report.
    /// </summary>
    public ReportNotificationEmail? NotificationEmail { get; set; }
    /// <summary>
    /// Gets or sets the list of web hooks for report notifications.
    /// </summary>
    public List<ReportWebHook> WebHooks { get; set; } = new();
}

/// <summary>
/// Represents a request to create a report with specified parameters and configurations.
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// Gets or sets the name of the report.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of asset identifiers to include in the report.
    /// </summary>
    public List<int>? AssetList { get; set; }

    /// <summary>
    /// Gets or sets the list of integration keys associated with the report.
    /// </summary>
    public List<string>? IntegrationKeys { get; set; }

    /// <summary>
    /// Gets or sets the time parameters for the report.
    /// </summary>
    public ReportTimeParameters TimeParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of events to be included in the report.
    /// </summary>
    public List<string> Events { get; set; } = new();

    /// <summary>
    /// Gets or sets the area configuration for the report.
    /// </summary>
    public ReportAreaConfiguration AreaConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the tag status configuration for the report.
    /// </summary>
    public ReportTagStatusConfiguration TagStatusConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets additional information to include in the report.
    /// </summary>
    public ReportAdditionalInformation AdditionalInformation { get; set; } = new();

    /// <summary>
    /// Gets or sets the identifier of the report template to use.
    /// </summary>
    public string? ReportTemplateId { get; set; }
}
/// <summary>
/// Represents the response for a create report request.
/// </summary>
public class ReportResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the created report.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of resource created.
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;
    /// <summary>
    ///     Gets or sets a value indicating whether the report was successfully created.
    /// </summary>
    public bool Created { get; set; }

    /// <summary>
    /// Gets or sets the type of the report creation.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time range for the report.
    /// </summary>
    public string DateTimeRange { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the date and time range for the report.
    /// </summary>
    public DateTime DateTimeRequestFor { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the state of the report creation.
    /// </summary>
    [JsonIgnore]
    public string State { get; set; } = string.Empty;
}
/// <summary>
/// Represents a report list item.
/// </summary>
public class ReportListItem
{
    /// <summary>
    ///     Gets or sets the unique identifier of the report.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the name of the report.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the type of the report.
    /// </summary>
    public ReportListParameters Parameters { get; set; } = new();
    /// <summary>
    ///     Gets or sets the status of the report.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the creator of the report.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the creation date of the report.
    /// </summary>
    public DateTime CreationDate { get; set; } = DateTime.MinValue;
}
/// <summary>
/// Represents the parameters for a report list item.
/// </summary>
public class ReportListParameters
{
    /// <summary>
    /// Gets or sets the time parameters for the report.
    /// </summary>
    public List<ReportEvent> Events { get; set; } = new();
}
/// <summary>
///     Represents a request for report result data.
/// </summary>
public class ReportResultDataRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the report.
    /// </summary>
    public ReportContentView ContentView { get; set; }
    /// <summary>
    /// Gets or sets the pagination information for the report content.
    /// </summary>
    public ReportContentPagination? GlobalPagination { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the asset.
    /// </summary>
    public string? AssetId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the clock.
    /// </summary>
    public ReportEvent? Event { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the area.
    /// </summary>
    public List<ReportSubEvent>? SubEvents { get; set; }
    /// <summary>
    /// Gets or sets the list of area identifiers to filter the report results.
    /// </summary>
    public List<string>? AreaIds { get; set; }
}
/// <summary>
/// Represents the pagination parameters for report content.
/// </summary>
public class ReportContentPagination
{
    /// <summary>
    /// Gets or sets the page number for pagination.
    /// </summary>
    public List<string>? TagStatuses { get; set; }
    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public List<string>? Areas { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the clock.
    /// </summary>
    public List<string>? Clocks { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the asset.
    /// </summary>
    public List<string>? Assets { get; set; }
    /// <summary>
    /// Gets or sets the list of integration keys to filter the report results.
    /// </summary>
    public List<string>? IntegrationKeys { get; set; }
    /// <summary>
    /// Gets or sets the start date for the report content.
    /// </summary>
    public long? DateFrom { get; set; }
    /// <summary>
    /// Gets or sets the end date for the report content.
    /// </summary>
    public long? DateTo { get; set; }
    /// <summary>
    /// Gets or sets the start time for the report content.
    /// </summary>
    public long? DurationFrom { get; set; }
    /// <summary>
    /// Gets or sets the end time for the report content.
    /// </summary>
    public long? DurationTo { get; set; }
}
/// <summary>
/// Represents a localized string (for i18n support).
/// </summary>
public class InternationalString
{
    /// <summary>
    /// Gets or sets the text of the localized string.
    /// </summary>
    public string Text { get; set; } = "";
    /// <summary>
    /// Gets or sets the localization key for the string.
    /// </summary>
    public string I18n { get; set; } = "";
}