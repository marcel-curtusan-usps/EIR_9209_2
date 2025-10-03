namespace EIR_9209_2.Models
{
    /// <summary>
    /// Represents the dwell information of an employee within a specific area,
    /// including employee details and the duration spent in the area.
    /// </summary>
    public class AreaDwell
    {
        /// <summary>
        /// Gets or sets the full name of the employee.
        /// </summary>
        public required string EmployeeName { get; set; }

        /// <summary>
        /// Gets or sets the employee identification number (EIN).
        /// </summary>
        public required string Ein { get; set; }

        /// <summary>
        /// Gets or sets the name of the area where the employee dwelled.
        /// </summary>
        public required string AreaName { get; set; }

        /// <summary>
        /// Gets or sets the duration of time the employee spent in the area.
        /// </summary>
        public TimeSpan DwellTimeDurationInArea { get; set; }

        /// <summary>
        /// Gets or sets the type associated with the dwell record.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the first name of the employee.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the employee.
        /// </summary>
        public required string LastName { get; set; }
    }
}