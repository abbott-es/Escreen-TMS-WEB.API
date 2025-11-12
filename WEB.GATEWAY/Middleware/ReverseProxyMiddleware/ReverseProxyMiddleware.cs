using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Caching;
using WEB.UTILITY.Enums;
using WEB.UTILITY.LanguageExt;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class ReverseProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICache _cache;
    private readonly IOutputCacheService _policyService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IProxyConfigService _routing;
    private readonly IAppLogger<ReverseProxyMiddleware> _logger;
    private readonly IRateLimitingStrategy _rateLimitStrategy;
    private readonly IErrorHandlingStrategy _errorHandlingStrat;
    private readonly GatewaySettings _gatewayEndpointSettings;
    private static readonly string[] _cacheAllowMethodsList = ["GET", "POST", "PUT"];

    public ReverseProxyMiddleware(
        RequestDelegate next,
        ICache cache,
        IOutputCacheService policyService,
        IRateLimitConfigServiceProvider rateLimitService,
        IProxyConfigService routingService,
        IRateLimitingStrategy rateLimitStrategy,
        IErrorHandlingStrategy errorHandlingStrategy,
        IOptions<GatewaySettings> settings,
        IAppLogger<ReverseProxyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _policyService = policyService;
        _rateLimitService = rateLimitService;
        _routing = routingService;
        _rateLimitStrategy = rateLimitStrategy;
        _errorHandlingStrat = errorHandlingStrategy;
        _logger = logger;
        _gatewayEndpointSettings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            IReadOnlyDictionary<string, RouteConfig> routeList = await _routing.GetRoutesAsync();

            if (!await IsRouteValidated(context, routeList))
            {
                return;
            }

            string path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;

            RouteConfig routeMatch = routeList
                .Where(r => r.Value?.Match?.Path != null)
                .FirstOrDefault(r =>
                {
                    return GetNormalizedPath(r.Value.Match.Path!).Equals(GetNormalizedPath(path),
                                                                         StringComparison.OrdinalIgnoreCase);
                }).Value;
            
            bool gatewayRouteMatchPath = context.Request.Path.HasValue &&
                         context.Request.Path.Value.Equals(_gatewayEndpointSettings.ApiPath, StringComparison.OrdinalIgnoreCase);

            if (gatewayRouteMatchPath)
            {
                bool isPostPutMethod = HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method);

                (string requestKeyNameValue, string gatewayUrl, string mappedMethod) = await GetRouteMatchFilterInput(context, isPostPutMethod);

                KeyValuePair<string, RouteConfig> gatewayRouteMatch = routeList.FirstOrDefault(r => IsRouteMatch(r.Value, _gatewayEndpointSettings.TransformQueryKey!, isPostPutMethod, requestKeyNameValue, mappedMethod, gatewayUrl));

                if (!await IsGatewayRouteValid(context, isPostPutMethod, gatewayRouteMatch))
                {
                    return;
                }

                _logger.LogDebug($"Gateway Crud MatchRoute: {gatewayRouteMatch} {gatewayRouteMatch.IsNull()}");

                if (!gatewayRouteMatch.IsNull() && !gatewayRouteMatch.Key.IsNull() && !isPostPutMethod)
                {
                    if (requestKeyNameValue != null)
                    {
                        KeyValuePair<string, RouteConfig> byKeyRoute = routeList.FirstOrDefault(r => r.Value.Match.Path != null && r.Value.Match.Path.Equals(_gatewayEndpointSettings.RoutePathByKey!, StringComparison.OrdinalIgnoreCase));

                        RoutePattern routePattern = RoutePatternFactory.Parse(byKeyRoute.Value.Match.Path!);
                        context.Request.HttpContext.Request.Path = byKeyRoute.Value.Match.Path!;

                        var query = context.Request.Query;
                        var queryDict = query.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Count > 0 ? kvp.Value[0] : null
                        );

                        string firstKey = queryDict.Keys.First();
                        string? firstValue = queryDict[firstKey];
                        queryDict.Remove(firstKey);
                        queryDict[_gatewayEndpointSettings.TransformQueryKey!] = firstValue;

                        var newQueryString = QueryHelpers.AddQueryString("", queryDict);

                        context.Request.QueryString = new QueryString(newQueryString);
                        context.Request.HttpContext.Request.QueryString = new QueryString(newQueryString);
                    }
                }
            }

            // endpoint matching
            if (routeMatch == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Route_Path_Not_Found";
                await context.Response.WriteAsync($"Route is not found for this request. Route: '{path}'", context.RequestAborted);
                return;
            }

            //cache policy matching
            string cachePolicyName = routeMatch.OutputCachePolicy ?? string.Empty;
            OutputCachePolicy? policy = _policyService.GetPolicy(cachePolicyName);
            if (policy == null)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Cache_Invalid";
                await context.Response.WriteAsync("Cache Policy is invalid for this request.", context.RequestAborted);
                return;
            }

            string ratePolicyName = routeMatch.RateLimiterPolicy ?? string.Empty;

            //rate limit policy matching
            if (string.IsNullOrEmpty(ratePolicyName) || string.IsNullOrEmpty(routeMatch?.RateLimiterPolicy))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Missing";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync("Rate Limit Policy is missing for this request.", context.RequestAborted);
                return;
            }

            if (!routeMatch.RateLimiterPolicy.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Invalid";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync($"Rate Limit Policy is invalid for this request. Policy: '{ratePolicyName}'", context.RequestAborted);
                return;
            }
            string requestBodyData = await WEB.UTILITY.Helper.StreamReaderHelper.ReadRequestBodyAsync(
        context);

            string clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!(await _rateLimitService.IsRequestAllowed(ratePolicyName, clientId, requestBodyData)))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Exceed";
                await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
                return;
            }

            var proxyFeature = context.Features.Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();

            if (proxyFeature == null)
            {
                _logger.LogDebug("ReverseProxyFeature is missing. Skipping Request.");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Service_Unavailable";
                await context.Response.WriteAsync("Gateway is unavailable for this request.", context.RequestAborted);
                return;
            }

            var destination = proxyFeature.AvailableDestinations.FirstOrDefault();

            if (destination == null)
            {
                _logger.LogWarning($"No available destination found for Route {routeMatch}.");
                await _errorHandlingStrat.HandleMissingDestinationAsync(context);
                return;
            }

            var isAllowedRateLimitPolicy = proxyFeature.Route.Config.RateLimiterPolicy?.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase) ?? false;

            if (!await IsAllowedRateLimitPolicyValid(context, proxyFeature, isAllowedRateLimitPolicy))
            {
                return;
            }

            string cacheKey = string.Empty;

            if (IsRequestCacheable(context))
            {
                cacheKey = await GenerateCacheKey(context, clientId, requestBodyData);
                var cache = await _cache.Get<string>(cacheKey);

                if (cache.IsSome)
                {
                    _logger.LogDebug($"Fetch from gateway cache. client: {clientId}");
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status200OK;

                    using var data = JsonDocument.Parse(cache.Value());
                    await context.Response.WriteAsync(JsonSerializer.Serialize(data.RootElement,
                        options: new JsonSerializerOptions
                        {
                            WriteIndented = true
                        })
                    , context.RequestAborted);
                    return;
                }
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context); // Proceed to next middleware

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();


            await _cache.Set(cacheKey, responseBody, policy.Duration);

            // Reset stream and copy to original response
            memoryStream.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBodyStream;
            await memoryStream.CopyToAsync(originalBodyStream);
            memoryStream.Close();
            memoryStream.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ReverseProxyServiceMiddleware.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error.");
        }

    }

    private static async Task<bool> IsGatewayRouteValid(
        HttpContext context, 
        bool isPostPutMethod, 
        KeyValuePair<string, RouteConfig> gatewayRouteMatch)
    {
        if (!gatewayRouteMatch.IsNull() && gatewayRouteMatch.Key.IsNull() && isPostPutMethod)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/plain";
            context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Route_Not_Available";
            await context.Response.WriteAsync("Gateway Service Not Available for this request.", context.RequestAborted);
            return false;
        }

        return true;
    }

    private static string GetNormalizedPath(string path)
    {
        return path?.Trim()?.ToLowerInvariant()!;
    }

    private async Task<bool> IsAllowedRateLimitPolicyValid(
        HttpContext context,
        Yarp.ReverseProxy.Model.IReverseProxyFeature proxyFeature,
        bool isAllowedRateLimitPolicy)
    {
        if (!isAllowedRateLimitPolicy)
        {
            _logger.LogDebug($"Rate limit policy: {proxyFeature.Route.Config.RateLimiterPolicy ?? "unknown"} is not isAllowedRateLimitPolicy for {proxyFeature.Route.Config.Match.Path}.");
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Exceed";
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
            return false;
        }

        return true;
    }

    private static async Task<bool> IsRouteValidated(
        HttpContext context,
        IReadOnlyDictionary<string, RouteConfig> routeList)
    {
        if (routeList == null)
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "text/plain";
            context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Timeout";
            await context.Response.WriteAsync("Gateway Timeout for this request.", context.RequestAborted);
            return false;
        }
        return true;
    }

    private static bool IsRequestCacheable(HttpContext context)
    {
        return _cacheAllowMethodsList.Contains(context.Request.Method);
    }

    private async Task<string> GenerateCacheKey(HttpContext context, string clientId, string body)
    {
        var keyBuilder = new StringBuilder($"{context.Request.Path.GetHashCode()}:{clientId.GetHashCode()}:{context.Request.Headers.UserAgent.GetHashCode()}:{context.Request.Headers.Authorization.GetHashCode()}:{body.GetHashCode()}");
        var key = keyBuilder.ToString();
        return key;
    }

    private static async Task<Option<RequestData>> ParseGatewayRequestBodyAsync(
        HttpContext context,
        IAppLogger<ReverseProxyMiddleware> logger)
    {
        try
        {
            var body = await WEB.UTILITY.Helper.StreamReaderHelper.ReadRequestBodyAsync(context);
            logger.LogDebug($"Parse Gateway Endpoints Request Body : {body}");
            var data = JsonSerializer.Deserialize<RequestData>(body);
            return Prelude.Optional(data);
        }
        catch (JsonException ex)
        {
            logger.LogWarning($"Failed to deserialize request body: {ex.Message}");
            return Option<RequestData>.None;
        }
    }

    private static string GetRequestKeyName(
        HttpContext context,
        Option<RequestData> requestDataOpt,
        string queryKey)
    {
        return requestDataOpt.Match(
            Some: data => data.keyName ?? string.Empty,
            None: () => context.Request.Query.TryGetValue(queryKey, out var values)
                ? values.FirstOrDefault() ?? string.Empty
                : string.Empty
        );
    }

    private static bool IsRouteMatch(
        RouteConfig route,
        string transformKey,
        bool isPostPutMethod,
        string requestKeyName,
        string mappedMethod,
        string gatewayUrl)
    {
        var metadataOpt = Prelude.Optional(route.Metadata);
        var methodsOpt = Prelude.Optional(route.Match.Methods);

        return metadataOpt.Match(
            Some: metadata =>
            {
                bool hasTransformKeyMatch = metadata.Any(m =>
                    !string.IsNullOrWhiteSpace(m.Key) &&
                    m.Key.Contains(transformKey, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(m.Value) &&
                    m.Value.Equals(requestKeyName, StringComparison.OrdinalIgnoreCase));

                bool methodMatch = methodsOpt.Match(
                    Some: methods => methods.Contains(mappedMethod),
                    None: () => false
                );

                bool pathMatch = route.Match.Path?.Equals(gatewayUrl, StringComparison.OrdinalIgnoreCase) ?? false;

                return isPostPutMethod
                    ? pathMatch && methodMatch && hasTransformKeyMatch
                    : hasTransformKeyMatch;
            },
            None: () => false
        );
    }

    private static string GetGatewayUrl(RequestData? data)
    {
        return data?.gatewayUrl ?? string.Empty;
    }

    private static string MapEnumMethod(int methodCode)
    {
        if (!Enum.TryParse<Method>(methodCode.ToString(), out var methodEnum))
        {
            return string.Empty;
        }

        string method = methodEnum switch
        {
            Method.Get => HttpMethods.Get,
            Method.Post => HttpMethods.Post,
            Method.Put => HttpMethods.Put,
            Method.Delete => HttpMethods.Delete,
            _ => string.Empty
        };

        return method;
    }

    private async Task<(
        string requestKeyNameValue,
        string gatewayUrl,
        string mappedMethod)> GetRouteMatchFilterInput(
        HttpContext context,
        bool isPostPutMethod)
    {
        Option<RequestData> requestDataOpt = !isPostPutMethod
            ? Option<RequestData>.None
            : await ParseGatewayRequestBodyAsync(context, _logger);

        string requestKeyNameValue = GetRequestKeyName(context, requestDataOpt,
            requestDataOpt.Match(Some: _ => _gatewayEndpointSettings.TransformQueryKey!, None: () => _gatewayEndpointSettings.QueryKey!));

        string gatewayUrl = requestDataOpt.Match(
            Some: data => GetGatewayUrl(data),
            None: () => string.Empty);

        string mappedMethod = requestDataOpt.Match(
            Some: data => MapEnumMethod(data.method),
            None: () => context.Request.Method
        );

        return (requestKeyNameValue, gatewayUrl, mappedMethod);
    }
}
