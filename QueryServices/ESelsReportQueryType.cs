using System.ComponentModel;

public enum ESelsReportQueryType
{
    [Description("AREA_AGGREGATION")]
    TotalDwellTimeByPersonByOperationalArea,
    [Description("TAG_AGGREGATION")]
    TotalDwellTimeByOperationalAreaByPerson,
    [Description("TAG_TIMELINE")]
    TimelineByPerson
}