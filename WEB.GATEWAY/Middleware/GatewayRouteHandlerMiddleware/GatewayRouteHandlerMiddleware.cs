using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Enums;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Middleware.GatewayRouteHandlerMiddleware;

public class GatewayRouteHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GatewaySettings _settings;
    private readonly IAppLogger<GatewayRouteHandlerMiddleware> _appLogger;
    private readonly IProxyConfigService _routing;
    public GatewayRouteHandlerMiddleware(RequestDelegate request,
        IOptions<GatewaySettings> settings,
        IProxyConfigService proxyConfigService,
        IAppLogger<GatewayRouteHandlerMiddleware> logger)
    {
        _next = request;
        _settings = settings.Value;
        _routing = proxyConfigService;
        _appLogger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isPostPutMethod = HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method);
        var isMatchPath = context.Request.Path.HasValue &&
                          context.Request.Path.Value.Equals(_settings.ApiPath, StringComparison.OrdinalIgnoreCase);

        if (isMatchPath)
        {
            var routeList = await _routing.GetRoutesAsync();

            (string requestKeyNameValue, string gatewayUrl, string mappedMethod) = await GetRouteMatchFilterInput(context, isPostPutMethod);

            var matchRoute = routeList
                .Where(r => IsRouteMatch(r.Value, _settings.TransformQueryKey!, isPostPutMethod, requestKeyNameValue, mappedMethod, gatewayUrl))
                .FirstOrDefault();

            _appLogger.LogDebug($"Gateway Crud MatchRoute: {matchRoute} {matchRoute.IsNull()}");

            if (!matchRoute.IsNull() && matchRoute.Key.IsNull() && isPostPutMethod)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Route_Not_Available";
                await context.Response.WriteAsync("Gateway Service Not Available for this request.", context.RequestAborted);
                return;
            }

            if (!matchRoute.IsNull() && !matchRoute.Key.IsNull() && !isPostPutMethod)
            {

                if (requestKeyNameValue != null)
                {
                    System.Collections.Generic.KeyValuePair<string, RouteConfig> byKeyRoute = routeList.FirstOrDefault(r => r.Value.Match.Path != null && r.Value.Match.Path.Equals(_settings.RoutePathByKey!, StringComparison.OrdinalIgnoreCase));

                    RoutePattern routePattern = RoutePatternFactory.Parse(byKeyRoute.Value.Match.Path!);

                    // Build a new endpoint from byKeyRoute
                    var newEndpoint = new RouteEndpoint(
                        async ctx => await _next(ctx), // Pass control to next middleware
                        routePattern,
                        order: 0,
                        new EndpointMetadataCollection(byKeyRoute.Value),
                        displayName: byKeyRoute.Key
                    );

                    // Transform the query key
                    IQueryCollection query = context.Request.Query;
                    System.Collections.Generic.Dictionary<string, string?> queryDict = query.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Count > 0 ? kvp.Value[0] : null
                    );

                    string firstKey = queryDict.Keys.First();
                    string? firstValue = queryDict[firstKey];
                    queryDict.Remove(firstKey);
                    queryDict[_settings.TransformQueryKey!] = firstValue;

                    var newQueryString = QueryHelpers.AddQueryString("", queryDict);
                    context.Request.QueryString = new QueryString(newQueryString);

                    // Forward logic
                    context.SetEndpoint(newEndpoint);
                }
                _appLogger.LogDebug($"Forwarding GET request to: {matchRoute.Value.Match.Path}");
            }
        }

        await _next(context);
    }

    private static async Task<Option<RequestData>> ParseRequestBodyAsync(HttpContext context, IAppLogger<GatewayRouteHandlerMiddleware> logger)
    {
        try
        {
            var body = await WEB.UTILITY.Helper.StreamReaderHelper.ReadRequestBodyAsync(context);
            logger.LogDebug($"ParseReqBody : {body}");
            var data = JsonSerializer.Deserialize<RequestData>(body);
            return Prelude.Optional(data);
        }
        catch (JsonException ex)
        {
            logger.LogWarning($"Failed to deserialize request body: {ex.Message}");
            return Option<RequestData>.None;
        }
    }

    private static string GetRequestKeyName(HttpContext context, Option<RequestData> requestDataOpt, string queryKey)
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

    private static string GetGatewayUrl(RequestData? data) => data?.gatewayUrl ?? string.Empty;

    private static string MapEnumMethod(int methodCode) =>
    Enum.TryParse<Method>(methodCode.ToString(), out var methodEnum)
        ? methodEnum switch
        {
            Method.Get => HttpMethods.Get,
            Method.Post => HttpMethods.Post,
            Method.Put => HttpMethods.Put,
            Method.Delete => HttpMethods.Delete,
            _ => string.Empty
        }
        : string.Empty;

    private async Task<(string requestKeyNameValue, string gatewayUrl, string mappedMethod)> GetRouteMatchFilterInput(
     HttpContext context,
     bool isPostPutMethod)
    {
        Option<RequestData> requestDataOpt = isPostPutMethod
            ? await ParseRequestBodyAsync(context, _appLogger)
            : Option<RequestData>.None;

        string requestKeyNameValue = GetRequestKeyName(context, requestDataOpt, 
            requestDataOpt.Match(Some: _ => _settings.TransformQueryKey!, None: () => _settings.QueryKey!));

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
