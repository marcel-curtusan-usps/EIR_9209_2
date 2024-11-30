using Newtonsoft.Json;

public class Inventory
{
    //create properties for Inventory class
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("category")]
    public string Category { get; set; }
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }
    [JsonProperty("notes")]
    public string Notes { get; set; }
    [JsonProperty("barcode")]
    public string Barcode { get; set; }
    [JsonProperty("bleTag")]
    public string BLETag { get; set; }
    [JsonProperty("createDateTime")]
    public DateTime CreateDateTime { get; set; }
    [JsonProperty("moddifyDateTime")]
    public DateTime ModifyDateTime { get; set; }
    public DateTime CreatedDate { get; internal set; }
}