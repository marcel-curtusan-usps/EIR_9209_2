namespace EIR_9209_2.Models
{
    /// <summary>
    /// Represents information about an employee, including personal details, status, and identifiers.
    /// </summary>
    public class EmployeeInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the employee.
        /// </summary>
        public string EmployeeId { get; set; } = "";

        /// <summary>
        /// Gets or sets the pay week of the employee.
        /// </summary>
        public string PayWeek { get; set; } = "";

        /// <summary>
        /// Gets or sets the current status of the employee.
        /// </summary>
        public string CurrentStatus { get; set; } = "";

        /// <summary>
        /// Gets or sets the pay location of the employee.
        /// </summary>
        public string PayLocation { get; set; } = "";

        /// <summary>
        /// Gets or sets the last name of the employee.
        /// </summary>
        public string LastName { get; set; } = "";

        /// <summary>
        /// Gets or sets the first name of the employee.
        /// </summary>
        public string FirstName { get; set; } = "";

        /// <summary>
        /// Gets or sets the middle initial of the employee.
        /// </summary>
        public string MiddleInit { get; set; } = "";

        /// <summary>
        /// Gets or sets the designated action code for the employee.
        /// </summary>
        public string DesActCode { get; set; } = "";

        /// <summary>
        /// Gets or sets the title of the employee.
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the base operation of the employee.
        /// </summary>
        public string BaseOp { get; set; } = "";

        /// <summary>
        /// Gets or sets the tour number of the employee.
        /// </summary>
        public string TourNumber { get; set; } = "";

        /// <summary>
        /// Gets or sets the duty station FIN number of the employee.
        /// </summary>
        public string DutyStationFINNBR { get; set; } = "";

        /// <summary>
        /// Gets or sets the position of the employee.
        /// </summary>
        public string Position { get; set; } = "";

        /// <summary>
        /// Gets or sets the facility ID of the employee.
        /// </summary>
        public string FacilityID { get; set; } = "";

        /// <summary>
        /// Gets or sets the status of the employee.
        /// </summary>
        public string EmployeeStatus { get; set; } = "";

        /// <summary>
        /// Gets or sets the BLE ID of the employee.
        /// </summary>
        public string BleId { get; set; } = "";

        /// <summary>
        /// Gets or sets the encoded ID of the employee.
        /// </summary>
        public string EncodedId { get; set; } = "";

        /// <summary>
        /// Gets or sets the activation date of the employee.
        /// </summary>
        public DateTime Activation { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the employee.
        /// </summary>
        public DateTime Expiration { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the cardholder.
        /// </summary>
        /// <value>
        /// The unique identifier for the cardholder, represented as an integer.
        /// Defaults to 0 if not explicitly set.
        /// </value>
        public int CardholderId { get; set; } = 0;
    }
}
