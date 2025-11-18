using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IHistoryAlocadoService
    {
        //Task<IQueryable<HistoryAlocadoReportModel>> GetEmployeeItemHistory(string chapa, int codColigada, int year);
        Task<List<HistoryAlocadoReportModel>> GetEmployeeItemHistory(string chapa, int codColigada, int year);

        Task<List<HistoryAlocadoReportModel>> GetEmployeeItemAllocation(string chapa, int codColigada, int year);

        Task<List<HistoryAlocadoReportModel>> GetTerceiroItemHistory(int IdTerceiro, int year);
        Task<List<HistoryAlocadoReportModel>> GetTerceiroItemAllocation(int IdTerceiro, int year);
    }

}
