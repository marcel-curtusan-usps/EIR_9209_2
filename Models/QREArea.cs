/// <summary>
/// Represents an area in the QRE system.
/// </summary>
public class QREArea
{
    /// <summary>
    /// Unique identifier for the area.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Name of the area, e.g., "DIOSS-008".
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Type of the area, e.g., DEFAULT, RESTRICTED, etc.
    /// </summary>
    public required string Type { get; set; }
    /// <summary>
    /// Color code for the area, represented as a hex string.
    /// </summary>
    public required string Color { get; set; }
    /// <summary>
    /// RTLS Map ID for the area, used for mapping purposes.
    /// </summary>
    public int RtlsMapId { get; set; } = 0;
    /// <summary>
    /// Origin ID for the area, used for tracking or reference purposes.
    /// </summary>
    public int OriginId { get; set; } = 0;
}