using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Net;
using WEB.UTILITY.Helper;

namespace WEB.UTILITY.Logger
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppLogger(IHttpContextAccessor httpContextAccessor)
        {
            _logger = Log.ForContext<T>();
            _httpContextAccessor = httpContextAccessor;
        }

        private IDisposable PushRequestContext()
        {
            var context = _httpContextAccessor.HttpContext;
            var ip = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var host = context?.Request?.Host.Value ?? "Unknown";
            var path = context?.Request?.Path.Value ?? "Unknown";
            var correlationId = context?.Request?.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

            var traceId = Activity.Current?.TraceId.ToString() ?? "NoTrace";
            var spanId = Activity.Current?.SpanId.ToString() ?? "NoSpan";

            return new CompositeDisposable(
                LogContext.PushProperty("ClientIP", ip),
                LogContext.PushProperty("HostName", host),
                LogContext.PushProperty("RequestPath", path),
                LogContext.PushProperty("CorrelationId", correlationId),
                LogContext.PushProperty("TraceId", traceId),
                LogContext.PushProperty("SpanId", spanId)
            );

        }

        public void LogInformation(string message)
        {
            using (PushRequestContext())
            {
                _logger.Information(message);
            }
        }

        public void LogWarning(string message)
        {
            using (PushRequestContext())
            {
                _logger.Warning(message);
            }
        }

        public void LogError(Exception ex, string message)
        {
            using (PushRequestContext())
            {
                _logger.Error(ex, message);
            }
        }

        public void LogDebug(string message, params object[] args)
        {
            using (PushRequestContext())
            {
                _logger.Debug(message, args);
            }
        }

    }

}
