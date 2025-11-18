using FerramentariaTest.Models;
using FerramentariaTest.Entities;
using System;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationsModel>?> GetGroupReservation(int? ferramentariaValue, int? controlStatus);
        Task<List<ItemReservationDetailModel>?> PrepareModel(int id, int? FerramentariaValue);
        Task<ReservationControl?> GetReservationControl(int id);
        Task UpdateReservationStatus(List<int?> IdReservations);
        Task<List<ReservationsControlModel>?> GetPreparingReservations(int IdReservationControl,int? FerramentariaValue);
        Task<List<ReservedProductModel>?> GetReservedProducts(int IdReservationControl, int FerramentariaValue);
        Task CancelReservation(int? IdReservation,string Chapa, string Observacao);
        Task<Reservations?> GetReservations(int id);
        Task TransferReservation(int IdReservation, string Observacao, int IdFerramentariaTo);
        Task FinalizeProcessReservation(List<ProductReservation> finalSubmissions);
        Task<List<ProductReservation>?> VerifyReservations(List<FinalSubmissionProcess> finalSubmissions);
        Task<bool> VerifyFinalizeTransactionId(string transactionId);



        Task<List<FinalReservationResult>?> GetFinalizedReservation(int codPessoa, int idFerramentaria);
        Task<Result> FinalizeProcessHandoutReservation(List<FinalSubmissionProcess> submissions,string transactionId, int userId, int idFerramentaria);
    }


}
