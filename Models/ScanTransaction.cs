namespace EIR_9209_2.Models
{
    /// <summary>
    /// Represents a scan transaction.
    /// </summary>
    public class ScanTransaction
    {
        /// <summary>
        /// Gets or sets the date and time of the scan.
        /// </summary>
        public DateTime ScanDateTime { get; set; }

        /// <summary>
        /// Gets or sets the caption of the controller.
        /// </summary>
        public string ControllerCaption { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the caption of the device.
        /// </summary>
        public string DeviceCaption { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the caption of the device type.
        /// </summary>
        public string DeviceTypeCaption { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the device.
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Gets or sets the EIN.
        /// </summary>
        public string EIN { get; set; } = string.Empty;
    }
}