namespace WEB.GATEWAY.Models;

public record RequestData
{
    public string? gatewayID { get; set; }
    public int method { get; set; }
    public string? keyName { get; set; }
    public string? gatewayUrl { get; set; }
}
