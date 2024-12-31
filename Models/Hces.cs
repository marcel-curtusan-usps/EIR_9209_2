using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EIR_9209_2.Models
{
    public class Field
    {
        [JsonPropertyName("name")]
        public string name;

        [JsonPropertyName("value")]
        public string value;
    }

    public class Hces
    {
        [JsonPropertyName("appId")]
        public string appId;

        [JsonPropertyName("pagenumber")]
        public int? pagenumber;

        [JsonPropertyName("pagesize")]
        public int? pagesize;

        [JsonPropertyName("totalrecordcount")]
        public int? totalrecordcount;

        [JsonPropertyName("status")]
        public string status;

        [JsonPropertyName("row")]
        public List<Row> row;
    }

    public class Row
    {
        [JsonPropertyName("field")]
        public List<Field> field;
    }

}
