using System;

namespace EIR_9209_2.Models;
/// <summary>
/// Represents information about a QRE badge.
/// </summary>
public class QreBadgeInfo
{
    /// <summary>
    /// Unique identifier for the badge info record.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Employee's unique identifier.
    /// </summary>
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Employee's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Activation timestamp (Unix epoch milliseconds).
    /// </summary>
    public long Activation { get; set; } = 0;

    /// <summary>
    /// Indicates if the badge is blocked.
    /// </summary>
    public bool Blocked { get; set; } = false;

    /// <summary>
    /// Current status of the badge (e.g., END_TOUR).
    /// </summary>
    public string BadgeStatus { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the last badge status update (Unix epoch milliseconds).
    /// </summary>
    public long BadgeStatusUpdate { get; set; } = 0;

    /// <summary>
    /// Timestamp of the previous badge status update (Unix epoch milliseconds).
    /// </summary>
    public long BadgeStatusPreviousUpdate { get; set; } = 0;

    /// <summary>
    /// Tag identifier associated with the badge.
    /// </summary>
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the badge holder (e.g., Clerk).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Badge identifier.
    /// </summary>
    public string BadgeId { get; set; } = string.Empty;
}
