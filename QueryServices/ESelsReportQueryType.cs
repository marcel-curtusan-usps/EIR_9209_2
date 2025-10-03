using System.ComponentModel;

public enum ESelsReportQueryType
{
    /// <summary>
    /// This query type is used to get the total dwell time by person in a specific area.
    /// </summary>
    [Description("TAG_AGGREGATION")]
    TotalDwellTimeByPersonByOperationalArea,

    /// <summary>
    /// This query type is used to get the total dwell time by operational area.
    /// </summary>
    [Description("AREA_AGGREGATION")]
    TotalDwellTimeByOperationalAreaByPerson,

    /// <summary>
    /// This query type is used to get the total dwell time by person in a specific area.
    /// </summary>
    [Description("TAG_TIMELINE")]
    TimelineByPerson
}