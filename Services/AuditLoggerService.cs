using FerramentariaTest.Services.Interfaces;
using Serilog.Context;

namespace FerramentariaTest.Services
{
    public class AuditLoggerService : IAuditLogger
    {
        private readonly Serilog.ILogger _auditLogger;

        public AuditLoggerService(Serilog.ILogger auditLogger) // Inject Serilog.ILogger
        {
            _auditLogger = auditLogger;
        }

        public void LogAuditInformation(int? userId, string message, string Action, string Outcome)
        {
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Action", Action))
            using (LogContext.PushProperty("Outcome", Outcome))
            {
                _auditLogger.Information(message);
            }
        }

        public void LogAuditDetailedInformation(string userId, string message, string Action, string Outcome, object? data = null)
        {
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Action", Action))
            using (LogContext.PushProperty("Outcome", Outcome))
            using (LogContext.PushProperty("Data", data, destructureObjects: true))
            {
                _auditLogger.Information(message);
            }
        }

        public void LogAuditTransaction(int? userId, string message, string Action, string Outcome, string TransactionId)
        {
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Action", Action))
            using (LogContext.PushProperty("Outcome", Outcome))
            using (LogContext.PushProperty("TransactionId", TransactionId))
            {
                _auditLogger.Information(message);
            }
        }


    }
}
