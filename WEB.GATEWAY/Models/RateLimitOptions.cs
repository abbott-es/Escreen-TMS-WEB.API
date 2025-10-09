namespace WEB.GATEWAY.Models;

public class RateLimitOptions
{
    public string PolicyName { get; set; } = string.Empty;
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int QueueLimit { get; set; }
    public string QueueProcessingOrder { get; set; } = string.Empty;
}