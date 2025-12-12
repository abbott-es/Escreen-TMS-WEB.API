namespace WEB.GATEWAY.Models;

/// <summary>
/// Represents configuration settings for the Gateway routing and transformation.
/// </summary>
public class GatewaySettings
{
    /// <summary>
    /// The query parameter key used to identify the path.
    /// </summary>
    public string? QueryKey { get; set; }

    /// <summary>
    /// The query parameter key used for transformation logic.
    /// </summary>
    public string? TransformQueryKey { get; set; }

    /// <summary>
    /// The API endpoint path for gateway operations.
    /// </summary>
    public string? ApiPath { get; set; }

    /// <summary>
    /// The route path used to retrieve gateway URLs by key.
    /// </summary>
    public string? RoutePathByKey { get; set; }
}
