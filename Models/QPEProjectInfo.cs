namespace EIR_9209_2.Models
{
    public class BackgroundImage
    {
        public double widthMeter { get; set; }
        public double heightMeter { get; set; }
        public double xMeter { get; set; }
        public bool visible { get; set; }
        public double yMeter { get; set; }
        public int rotation { get; set; }
        public int alpha { get; set; }
        public double origoY { get; set; }
        public string id { get; set; }
        public double origoX { get; set; }
        public double metersPerPixelY { get; set; }
        public double metersPerPixelX { get; set; }
    }

    public class CoordinateSystem
    {
        public List<Locator> locators { get; set; }
        public List<OSLImage> backgroundImages { get; set; }
        public double relativeZ { get; set; }
        public List<Polygon> polygons { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public List<Zone> zones { get; set; }
    }

    public class Locator
    {
        public bool locationLocked { get; set; }
        public List<double> orientation { get; set; }
        public bool visible { get; set; }
        public string notes { get; set; }
        public string color { get; set; }
        public bool isOrientationSetManually { get; set; }
        public List<string> associatedAreas { get; set; }
        public bool locationLockedX { get; set; }
        public string locatorChannel { get; set; }
        public bool locationLockedZ { get; set; }
        public bool locationLockedY { get; set; }
        public string locatorType { get; set; }
        public string name { get; set; }
        public List<double> location { get; set; }
        public string id { get; set; }
    }
    public class Polygon
    {
        public bool locationLocked { get; set; }
        public string notes { get; set; }
        public bool visible { get; set; }
        public string color { get; set; }
        public List<string> openingIds { get; set; }
        public string name { get; set; }
        public string polygonData { get; set; }
        public string id { get; set; }
        public List<PolygonHole> polygonHoles { get; set; }
    }

    public class PolygonHole
    {
        public bool locationLocked { get; set; }
        public string notes { get; set; }
        public bool visible { get; set; }
        public string name { get; set; }
        public string polygonData { get; set; }
        public string id { get; set; }
    }

    public class QPEProjectInfo
    {
        public List<CoordinateSystem> coordinateSystems { get; set; }
        public int code { get; set; }
        public long responseTS { get; set; }
        public string message { get; set; }
        public string version { get; set; }
        public string command { get; set; }
        public string status { get; set; }
    }


    public class TrackingArea
    {
        public string notes { get; set; }
        public int maxZ { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public int minZ { get; set; }
        public bool track3d { get; set; }
        public string trackingAreaGroup { get; set; }
    }

    public class TrackingAreaGroup
    {
        public string notes { get; set; }
        public string color { get; set; }
        public TrackTagGroups trackTagGroups { get; set; }
        public string name { get; set; }
        public int radiusThreshold { get; set; }
        public double snapToPolygonDistanceZ { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public double snapToPolygonDistance { get; set; }
        public string mode3d { get; set; }
    }

    public class TrackTagGroups
    {
        public object tagGroups { get; set; }
        public bool allTags { get; set; }
    }

    public class Zone
    {
        public bool locationLocked { get; set; }
        public string notes { get; set; }
        public bool visible { get; set; }
        public string color { get; set; }
        public string zoneGroupId { get; set; }
        public string name { get; set; }
        public string polygonData { get; set; }
        public string id { get; set; }
        public List<PolygonHole> polygonHoles { get; set; }
    }
}
