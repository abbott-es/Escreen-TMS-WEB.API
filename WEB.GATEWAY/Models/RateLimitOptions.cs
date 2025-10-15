namespace WEB.GATEWAY.Models;

/// <summary>
/// Represents the configuration options for a rate limiting policy.
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Gets or sets the name of the rate limiting policy.
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of permitted requests in the specified window.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the duration of the rate limiting window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of requests that can be queued while waiting for a permit.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the order in which queued requests are processed (e.g., FIFO, LIFO).
    /// </summary>
    public string QueueProcessingOrder { get; set; } = string.Empty;
}