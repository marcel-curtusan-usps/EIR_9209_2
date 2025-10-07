using Newtonsoft.Json;

namespace EIR_9209_2.Models;
/// <summary>
/// Represents a geographical zone as a GeoJSON Feature, including its geometry and properties.
/// </summary>
public class GeoZone
{
    /// <summary>
    /// The GeoJSON feature type. Default is "Feature".
    /// </summary>
    public string Type { get; set; } = "Feature";

    /// <summary>
    /// The geometry of the zone, typically a polygon with coordinates.
    /// </summary>
    public Geometry Geometry { get; set; } = new Geometry();

    /// <summary>
    /// Additional properties describing the zone, such as identifiers, visibility, color, and metadata.
    /// </summary>
    public Properties Properties { get; set; } = new Properties();
}

/// <summary>
/// Contains metadata and attributes for a geographical zone.
/// </summary>
public class Properties
{
    /// <summary>
    /// Unique identifier for the zone.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Identifier for the floor associated with the zone.
    /// </summary>
    public string FloorId { get; set; } = "";

    /// <summary>
    /// Indicates whether the zone is visible.
    /// </summary>
    public bool Visible { get; set; } = false;

    /// <summary>
    /// Color code representing the zone.
    /// </summary>
    public string Color { get; set; } = "";

    /// <summary>
    /// Name of the zone.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Type of the zone.
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// MPE (Machine Processing Equipment) name associated with the zone.
    /// </summary>
    public string MpeName { get; set; } = "";

    /// <summary>
    /// MPE number associated with the zone.
    /// </summary>
    public string MpeNumber { get; set; } = "";

    /// <summary>
    /// MPE group associated with the zone.
    /// </summary>
    public string MpeGroup { get; set; } = "";

    /// <summary>
    /// LDC (Local Distribution Center) identifier.
    /// </summary>
    public string LDC { get; set; } = "";

    /// <summary>
    /// Payment location identifier.
    /// </summary>
    public string PayLocation { get; set; } = "";

    /// <summary>
    /// Color code for the payment location.
    /// </summary>
    public string PayLocationColor { get; set; } = "";

    /// <summary>
    /// IP address of the MPE.
    /// </summary>
    public string MpeIpAddress { get; set; } = "";

    /// <summary>
    /// Performance data for the MPE run, if available.
    /// </summary>
    public MPERunPerformance? MPERunPerformance { get; set; }

    /// <summary>
    /// Data source for the zone information.
    /// </summary>
    public string? DataSource { get; set; } = "";

    /// <summary>
    /// Email addresses associated with the zone.
    /// </summary>
    public string? Emails { get; set; } = "";

    /// <summary>
    /// Bin identifiers associated with the zone.
    /// </summary>
    public string Bins { get; set; } = "";

    /// <summary>
    /// Bin identifiers for rejected items.
    /// </summary>
    public string RejectBins { get; set; } = "";

    /// <summary>
    /// External URL related to the zone.
    /// </summary>
    public string ExternalUrl { get; set; } = "";
}

/// <summary>
/// Represents the geometric shape of a zone, typically as a polygon with coordinates.
/// </summary>
public class Geometry
{
    /// <summary>
    /// The geometry type, usually "Polygon".
    /// </summary>
    public string Type { get; set; } = "Polygon";

    /// <summary>
    /// The coordinates defining the polygon geometry of the zone.
    /// </summary>
    public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
}