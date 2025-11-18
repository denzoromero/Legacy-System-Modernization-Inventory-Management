using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IAuditService
    {
        Task<AuditLogModel> GetAuditLogs(string TransactionId);

        Task<FinalAuditResultModel> MakeAuditModel(List<FinalSubmissionProcess> FinalProcessList, combineFixModel MoreInformation);
    }

}
