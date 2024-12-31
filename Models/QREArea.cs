public class QREArea
{
    //Example:
    //"id": "01H2GH3H040WYW9HF092ZJPBVH",
    //"name": "DIOSS-008",
    //"type": "DEFAULT",
    //"color": "#99ffff",
    //"rtlsMapId": 1

    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Color { get; set; }
    public int RtlsMapId { get; set; }
}