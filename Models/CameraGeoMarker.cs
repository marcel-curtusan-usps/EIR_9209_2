using EIR_9209_2.Utilities.Extensions;

namespace EIR_9209_2.Models;
/// <summary>
/// Represents a geographic marker for a camera, including its geometry and camera properties.
/// </summary>
public class CameraGeoMarker
{
    /// <summary>
    /// Gets or sets the type of the geo marker, typically "Feature".
    /// </summary>
    public string Type { get; set; } = "Feature";

    /// <summary>
    /// Gets or sets the geometry information of the marker, such as type and coordinates.
    /// </summary>
    public MarkerGeometry Geometry { get; set; } = new MarkerGeometry();

    /// <summary>
    /// Gets or sets the camera properties associated with the marker.
    /// </summary>
    public Cameras Properties { get; set; } = new Cameras();

    /// <summary>
    /// Represents the geometry of the marker, including type and coordinates.
    /// </summary>
    public class MarkerGeometry
    {
        /// <summary>
        /// Gets or sets the geometry type, typically "Point".
        /// </summary>
        public string Type { get; set; } = "Point";

        /// <summary>
        /// Gets or sets the coordinates of the marker as a list of doubles.
        /// </summary>
        public List<double> Coordinates { get; set; } = new List<double>();
    }
}

/// <summary>
/// Represents the properties of a camera, including identification, location, and metadata.
/// </summary>
public class Cameras
{
    /// <summary>
    /// Gets or sets the unique identifier for the camera.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name of the camera.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Gets or sets the direction the camera is facing.
    /// </summary>
    public string CameraDirection { get; set; } = "";

    /// <summary>
    /// Gets or sets the base64-encoded image of the camera.
    /// </summary>
    public string Base64Image { get; set; } = "";

    /// <summary>
    /// Gets or sets the locale key for the camera.
    /// </summary>
    public string LocaleKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the model number of the camera.
    /// </summary>
    public string ModelNum { get; set; } = "";

    /// <summary>
    /// Gets or sets the physical address of the facility where the camera is located.
    /// </summary>
    public string FacilityPhysAddrTxt { get; set; } = "";

    /// <summary>
    /// Gets or sets the geographic processing region name.
    /// </summary>
    public string GeoProcRegionNm { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the facility subtype.
    /// </summary>
    public string FacilitySubtypeDesc { get; set; } = "";

    /// <summary>
    /// Gets or sets the geographic processing division name.
    /// </summary>
    public string GeoProcDivisionNm { get; set; } = "";

    /// <summary>
    /// Gets or sets the authorization key for the camera.
    /// </summary>
    public string AuthKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the latitude of the facility where the camera is located.
    /// </summary>
    public double FacilityLatitudeNum { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the longitude of the facility where the camera is located.
    /// </summary>
    public double FacilityLongitudeNum { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the name of the camera.
    /// </summary>
    public string CameraName { get; set; } = "";

    /// <summary>
    /// Gets or sets the id of the camera.
    /// </summary>
    public int BicamCameraId { get; set; } = 0;
    /// <summary>
    /// Gets or sets the Compression of the camera image.
    /// </summary>
    public int Compression { get; set; } = 25;

    /// <summary>
    /// Gets or sets the IP address of the camera.
    /// </summary>
    public string IP { get; set; } = "";

    /// <summary>
    /// Gets or sets the host name of the camera.
    /// </summary>
    public string HostName { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the camera.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the reachability status of the camera.
    /// If the incoming JSON has an empty string or null, it will be treated as false.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(EmptyStringToBooleanConverter))]
    public bool Reachable { get; set; } = false;

    /// <summary>
    /// Gets or sets the display name of the facility.
    /// </summary>
    public string FacilityDisplayName { get; set; } = "";

    /// <summary>
    /// Gets or sets the type of the camera.
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    ///  Gets or sets the configuration number of the image source.
    /// </summary>
    /// <value></value>
    public string Resolution { get; set; } = "";
    /// <summary>
    /// Gets or sets the number of image sources for the camera.
    /// </summary>

    public int NumberOfSources { get; set; } = 0;
    /// <summary>
    /// Gets or sets the list of image sources for the camera.
    /// </summary>
    /// <returns></returns>

    public List<CameraImageSource> ImageSource { get; set; } = new();
    /// <summary>
    /// Gets or sets the floor identifier where the camera is located.
    /// </summary>
    /// <value></value>
    public string FloorId { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether the camera is visible.
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the camera was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.MinValue;
}

/// <summary>
/// Represents the image source configuration for a camera.
/// </summary>
public class CameraImageSource
{
    /// <summary>
    /// Gets or sets a value indicating whether the image source is enabled.
    /// </summary>
    /// <value></value>
    public bool Enabled { get; set; } = false;
    /// <summary>
    /// Gets or sets the ID of the image source.
    /// </summary>
    /// <value></value>
    public string CameraId { get; set; } = "";
    /// <summary>
    /// Gets or sets the name of the image source.
    /// </summary>
    /// <value></value>
    public string Name { get; set; } = "";
    /// <summary>
    /// Gets or sets the source number of the image source.
    /// </summary>
    /// <value></value>
    public string Source { get; set; } = "";
    /// <summary>
    ///  Gets or sets the configuration number of the image source.
    /// </summary>
    /// <value></value>
    public string Resolution { get; set; } = "";
    /// <summary>
    ///  Gets or sets the rotation of the image source.
    /// </summary>
    /// <value></value>
    public int Rotation { get; set; } = 0;
    /// <summary>
    /// Gets or sets a value indicating whether MPEG signed video is enabled.
    /// </summary> 
    /// <value></value>
    public string MPEGSignedVideoEnabled { get; set; } = "";
    /// <summary>
    ///  Gets or sets the stream duration of the image source.
    /// </summary>
    /// <value></value>
    public string StreamDuration { get; set; } = "";
}
