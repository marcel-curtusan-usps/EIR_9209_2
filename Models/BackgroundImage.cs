namespace EIR_9209_2.Models
{
    public class OSLImage
    {
        public double widthMeter { get; set; } = 0.0;
        public double xMeter { get; set; } = 0.0;
        public bool visible { get; set; }
        public string otherCoordSys { get; set; } = "";
        public int rotation { get; set; } = 0;
        public string base64 { get; set; } = "";
        public double origoY { get; set; } = 0.0;
        public double origoX { get; set; } = 0.0;
        public double heightMeter { get; set; } = 0.0;
        public double yMeter { get; set; } = 0.0;
        public int alpha { get; set; } = 0;
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string fileName { get; set; } = "";
        public double metersPerPixelY { get; set; } = 0.0;
        public double metersPerPixelX { get; set; } = 0.0;
        public bool updateStatus { get; set; } = false;
        public string coordinateSystemId { get; set; } = "";
    }
}
