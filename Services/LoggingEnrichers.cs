using FerramentariaTest.Services.Interfaces;
using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace FerramentariaTest.Services
{
    public class CorrelationIdLogEnricher : ILogEventEnricher
    {
        private readonly ICorrelationIdService _correlationIdService;

        public CorrelationIdLogEnricher(ICorrelationIdService correlationIdService)
        {
            _correlationIdService = correlationIdService;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            try
            {
                var correlationId = _correlationIdService.GetCurrentCorrelationId();

                if (!string.IsNullOrEmpty(correlationId))
                {
                    var correlationIdProperty = propertyFactory.CreateProperty("TraceIdGuid", correlationId);
                    logEvent.AddPropertyIfAbsent(correlationIdProperty);
                }
            }
            catch
            {
                // Silent fail - don't let logging break the application
            }
        }
    }


    public class RequestContextEnricher : ILogEventEnricher
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        //public RequestContextEnricher(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpContextAccessor = httpContextAccessor;
        //}

        public RequestContextEnricher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            using var scope = _serviceProvider.CreateScope();
            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();

            if (httpContextAccessor == null)
                return;

            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
                return;

            // Add IP Address
            var ipAddress = GetClientIpAddress(httpContext);
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var ipProperty = propertyFactory.CreateProperty("ClientIp", ipAddress);
                logEvent.AddPropertyIfAbsent(ipProperty);
            }

            // Add Session ID
            //var sessionId = httpContext.Session?.Id;
            //if (!string.IsNullOrEmpty(sessionId))
            //{
            //    var sessionProperty = propertyFactory.CreateProperty("SessionId", sessionId);
            //    logEvent.AddPropertyIfAbsent(sessionProperty);
            //}

            //var sessionIdClaim = httpContext.User.FindFirst("SessionId");
            //if (sessionIdClaim != null && !string.IsNullOrEmpty(sessionIdClaim.Value))
            //{
            //    var sessionProperty = propertyFactory.CreateProperty("SessionId", sessionIdClaim.Value);
            //    logEvent.AddPropertyIfAbsent(sessionProperty);
            //}

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var sessionIdClaim = httpContext.User.FindFirst("SessionId");
                if (sessionIdClaim != null && !string.IsNullOrEmpty(sessionIdClaim.Value))
                {
                    var sessionProperty = propertyFactory.CreateProperty("SessionId", sessionIdClaim.Value);
                    logEvent.AddPropertyIfAbsent(sessionProperty);
                }

                // You can also add other user context here
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userIdProperty = propertyFactory.CreateProperty("UserId", userIdClaim.Value);
                    logEvent.AddPropertyIfAbsent(userIdProperty);
                }
            }

        }

        private string GetClientIpAddress(HttpContext httpContext)
        {
            var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                var ips = forwardedHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return ips.FirstOrDefault()?.Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
