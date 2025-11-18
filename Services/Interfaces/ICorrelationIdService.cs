using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface ICorrelationIdService
    {
        string GetCurrentCorrelationId();
        string GenerateNewCorrelationId();
        IDisposable BeginScope(string correlationId = null);
    }

    public interface IUserContextService
    {
        int? GetUserId();
        UserClaimModel GetUserClaimData();
    }


}
