using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace WEB.GATEWAY.Transforms
{
    public class GatewayTransformProvider : ITransformProvider
    {
        private readonly GatewaySettings _settings;
        private readonly IAppLogger<GatewayTransformProvider> _logger; 

        public GatewayTransformProvider(IOptions<GatewaySettings> settings, IAppLogger<GatewayTransformProvider> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // Required by ITransformProvider
        public void ValidateRoute(TransformRouteValidationContext context)
        {
            // Optional: validate route-level settings
            if (isValidSetttings())
            {
                context.Errors.Add(new ArgumentException("Gateway settings are not properly configured."));
                _logger.LogDebug("Gateway settings are not properly configured.");
            }

            bool isValidSetttings()
            {
                return string.IsNullOrEmpty(_settings.ApiPath) ||
                string.IsNullOrEmpty(_settings.QueryKey) ||
                string.IsNullOrEmpty(_settings.TransformQueryKey) ||
                string.IsNullOrEmpty(_settings.RoutePathByKey);
            }
        }

        // Required by ITransformProvider
        public void ValidateCluster(TransformClusterValidationContext context)
        {
            // Optional: validate cluster-level settings
            // For example, ensure cluster destinations exist
            if (context.Cluster?.Destinations == null || context.Cluster.Destinations.Count == 0)
            {
                context.Errors.Add(new ArgumentException("Cluster has no destinations configured."));
                _logger.LogDebug("Cluster has no destinations configured.");
            }
        }

        // Required by ITransformProvider
        public void Apply(TransformBuilderContext context)
        {
            context.AddRequestTransform(async transformContext =>
            {
                var req = transformContext.HttpContext.Request;

                bool isValidPath = req.Path.Equals(_settings.ApiPath, StringComparison.OrdinalIgnoreCase) &&
                    req.Query.ContainsKey(_settings.QueryKey!);

                SetsQueryTransform(transformContext, req, isValidPath);

                // Forward Authorization header safely
                if (req.Headers.TryGetValue("Authorization", out var auth))
                {
                    transformContext.ProxyRequest.Headers.Remove("Authorization");
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
                }

                await Task.CompletedTask;
            });
        }

        private void SetsQueryTransform(RequestTransformContext transformContext, HttpRequest req, bool isValidPath)
        {
            if (isValidPath)
            {
                // Rewrite path
                transformContext.Path = _settings.RoutePathByKey;

                // Rewrite query param
                var updated = new Dictionary<string, StringValues>();
                foreach (var p in req.Query)
                {
                    var key = p.Key == _settings.QueryKey ? _settings.TransformQueryKey : p.Key;
                    updated[key!] = p.Value;
                }
                transformContext.HttpContext.Request.Query = new QueryCollection(updated);
            }
        }
    }
}
