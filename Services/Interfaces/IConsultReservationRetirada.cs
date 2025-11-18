using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IConsultReservationRetirada
    {
        Task<List<ConsultationReserveModel>?> GetReservationDetailsByEmployee(GestorRRFilterModel filter);
        Task<ConsultationModel?> GetReservationRetiradaInformation(int orderNo);
        Task<List<CatalogDetail>?> GetGestorListInformation(GestorRRFilterModel filter);
        Task<List<ConsultationReserveModel>?> GetReservationForCatalog(int idCatalogo);
    }
}
