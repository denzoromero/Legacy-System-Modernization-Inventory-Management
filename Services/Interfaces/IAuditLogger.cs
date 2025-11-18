namespace FerramentariaTest.Services.Interfaces
{
    public interface IAuditLogger
    {
        void LogAuditInformation(int? userId, string message, string Action, string Outcome);
        void LogAuditDetailedInformation(string userId, string message, string Action, string Outcome, object? data = null);
        void LogAuditTransaction(int? userId, string message, string Action, string Outcome, string TransactionId);
    }
}
