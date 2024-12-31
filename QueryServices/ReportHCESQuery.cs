using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class ReportHCESQuery
{
    [JsonPropertyName("appid")]
    public string Appid { get; set; } = "";

    [JsonPropertyName("pagenumber")]
    public int? Pagenumber { get; set; } = 0;

    [JsonPropertyName("pagesize")]
    public int? Pagesize { get; set; } = 0;

    [JsonPropertyName("filter")]
    public List<Filter> Filter { get; set; } = new List<Filter>();
}
public class Filter
{
    [JsonPropertyName("query")]
    public Query query;
}

public class Query
{
    [JsonPropertyName("fieldname")]
    [JsonProperty(Order = 1)]
    public string fieldname { get; set; } = "";

    [JsonPropertyName("operand")]
    [JsonProperty(Order = 2)]
    public string operand = "=";

    [JsonPropertyName("fieldvalue")]
    [JsonProperty(Order = 3)]
    public string fieldvalue { get; set; } = "";
}