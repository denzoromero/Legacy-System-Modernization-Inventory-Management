using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IRetiradaService
    {
        Task<List<newCatalogInformationModel>> GetRetiradaOrders(int codpessoa, int idFerramentaria);
        Task CancelRetirada(int idReservation, string chapa, string observacao, string transactionId);
        Task TransferRetirada(int idReservation, string observacao, int IdFerramentariaTo, string transactionId);
        Task<Result> FinalizeProcessHandoutRetirada(List<FinalSubmissionProcess> submissions, string transactionId, int userId, int idFerramentaria);
    }

}
