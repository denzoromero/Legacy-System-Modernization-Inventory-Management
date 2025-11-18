using FerramentariaTest.Controllers;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Packaging.Ionic.Zip;
using Serilog.Context;
using System;
using System.Linq;
using System.Net;

namespace FerramentariaTest.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(ContextoBanco context, ILogger<ReservationService> logger, ContextoBancoBS contextBS)
        {
            _context = context;
            _contextBS = contextBS;
            _logger = logger;           
        }

        public async Task<List<ReservationsModel>?> GetGroupReservation(int? ferramentariaValue, int? controlStatus)
        {
            try
            {
                _logger.LogInformation("Processing GetGroupReservation IdFerramentaria:{ferramentariaValue}, ControlStatus:{controlStatus}", ferramentariaValue, controlStatus);

                List<ReservationsControlModel>? reservations = await (from reservationControl in _context.ReservationControl
                                                                join reserve in _context.Reservations on reservationControl.Id equals reserve.IdReservationControl
                                                                join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                where reserve.IdFerramentaria == ferramentariaValue
                                                                && reservationControl.Type == 1
                                                                && (controlStatus == null || reserve.Status == controlStatus)
                                                                select new ReservationsControlModel
                                                                {
                                                                    ControlId = reservationControl.Id,
                                                                    Chave = reservationControl.Chave,
                                                                    LeadercodPessoa = leader.CodPessoa,
                                                                    LeaderName = leader.Nome,
                                                                    ControlStatusString = reservationControl.StatusString,
                                                                    ControlStatus = reservationControl.Status,
                                                                    reserveStatus = reserve.Status,
                                                                    reserveStatusString = reserve.StatusString,
                                                                    controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                }).ToListAsync();

                List<ReservationsModel>? groupReservation = reservations
                                                            .GroupBy(r => r.ControlId)
                                                            .Select(group => new ReservationsModel
                                                            {
                                                                ControlId = group.Key,
                                                                //itemCount = group.Count(),
                                                                itemCount = group.Count(r => r.reserveStatus == 0),
                                                                Chave = group.First().Chave,
                                                                LeaderName = group.First().LeaderName,
                                                                RegisteredCount = group.Count(),
                                                                ActualStatus = group.First().ControlStatusString,
                                                                controlDataRegistroString = group.First().controlDataRegistroString,
                                                                GroupStatus = group.All(r => r.reserveStatus == 1) ? 1 : 0
                                                            }).OrderBy(i => i.controlDataRegistroString).ToList();

                return groupReservation ?? new List<ReservationsModel>();

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }         
        }

        public async Task<List<ItemReservationDetailModel>?> PrepareModel(int id, int? FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing PrepareModel IdReservationControl:{id} IdFerramentaria:{FerramentariaValue}", id, FerramentariaValue);

                List<ItemReservationDetailModel> itemdetail = await (from reserv in _context.Reservations
                                                                   join catalogo in _context.Catalogo on reserv.IdCatalogo equals catalogo.Id
                                                                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                   join reservationControl in _context.ReservationControl on reserv.IdReservationControl equals reservationControl.Id
                                                                   join member in _context.LeaderMemberRel on reserv.IdLeaderMemberRel equals member.Id
                                                                   join leader in _context.LeaderData on member.IdLeader equals leader.Id
                                                                   join obra in _context.Obra on reserv.IdObra equals obra.Id
                                                                   where reserv.IdFerramentaria == FerramentariaValue
                                                                   && reserv.IdReservationControl == id
                                                                   && reservationControl.Type == 1
                                                                   select new ItemReservationDetailModel
                                                                   {
                                                                       IdReservation = reserv.Id,
                                                                       Classe = categoria.ClassType,
                                                                       Type = catalogo.PorType,
                                                                       Codigo = catalogo.Codigo,
                                                                       itemNome = catalogo.Nome,
                                                                       QuantidadeResquested = reserv.Quantidade,
                                                                       MemberCodPessoa = member.CodPessoa,
                                                                       LeaderCodPessoa = leader.CodPessoa,
                                                                       DataRegistro = reserv.DataRegistro.HasValue == true ? reserv.DataRegistro.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty, // Correct format
                                                                       Status = reserv.StatusString,
                                                                       IdObra = reserv.IdObra,
                                                                       ObraName = $"{obra.Codigo}-{obra.Nome}",
                                                                       intStatus = reserv.Status
                                                                   }).ToListAsync() ?? new List<ItemReservationDetailModel>();

                List<Funcionario?>? recentFuncionario = await _contextBS.Funcionario
                                                          .GroupBy(e => e.CodPessoa)
                                                          .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
                                                          .ToListAsync();

                List<ItemReservationDetailModel> completeDetail = (from item in itemdetail
                                                                   join memberinfo in recentFuncionario on item.MemberCodPessoa equals memberinfo.CodPessoa
                                                                   join leaderinfo in recentFuncionario on item.LeaderCodPessoa equals leaderinfo.CodPessoa
                                                                   select new ItemReservationDetailModel
                                                                   {
                                                                       IdReservation = item.IdReservation,
                                                                       Classe = item.Classe,
                                                                       Type = item.Type,
                                                                       Codigo = item.Codigo,
                                                                       itemNome = item.itemNome,
                                                                       QuantidadeResquested = item.QuantidadeResquested,
                                                                       MemberCodPessoa = item.MemberCodPessoa,
                                                                       MemberInfo = new employeeNewInformationModel
                                                                       {
                                                                           Chapa = memberinfo.Chapa,
                                                                           Nome = memberinfo.Nome,
                                                                           CodSituacao = memberinfo.CodSituacao,
                                                                           CodColigada = memberinfo.CodColigada,
                                                                           Funcao = memberinfo.Funcao,
                                                                           Secao = memberinfo.Secao,
                                                                       },
                                                                       LeaderCodPessoa = item.LeaderCodPessoa,
                                                                       LeaderInfo = new employeeNewInformationModel
                                                                       {
                                                                           Chapa = leaderinfo.Chapa,
                                                                           Nome = leaderinfo.Nome,
                                                                           CodSituacao = leaderinfo.CodSituacao,
                                                                           CodColigada = leaderinfo.CodColigada,
                                                                           Funcao = leaderinfo.Funcao,
                                                                           Secao = leaderinfo.Secao,
                                                                       },
                                                                       DataRegistro = item.DataRegistro,
                                                                       Status = item.Status,
                                                                       intStatus = item.intStatus,
                                                                       IdObra = item.IdObra,
                                                                       ObraName = item.ObraName,
                                                                   }).ToList();


                return completeDetail;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<ReservationControl?> GetReservationControl(int id)
        {
            try
            {
                _logger.LogInformation("Processing GetReservationControl IdReservationControl:{id}", id);
                return await _context.ReservationControl.FindAsync(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task UpdateReservationStatus(List<int?> IdReservations)
        {
            try
            {
                using var _ = LogContext.PushProperty("IdReservations", IdReservations, destructureObjects: true);
                {
                    _logger.LogInformation("Processing UpdateReservationStatus");
                }

                if (IdReservations == null || IdReservations.Count == 0) throw new ProcessErrorException("IdReservations is null or count 0.");

                await _context.Reservations.Where(r => IdReservations.Contains(r.Id)).ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, 1));

                await _context.SaveChangesAsync();

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<List<ReservationsControlModel>?> GetPreparingReservations(int IdReservationControl, int? FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing GetPreparingReservations IdReservationControl:{id}, IdFerramentaria:{IdFerramentaria}", IdReservationControl, FerramentariaValue);

                List<ReservationsControlModel>? reservations = await (from reservation in _context.Reservations
                                                                join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
                                                                where reservation.IdReservationControl == IdReservationControl
                                                                && reservation.IdFerramentaria == FerramentariaValue
                                                                && reservation.Status != 8
                                                                && reservation.Status != 3
                                                                select new ReservationsControlModel
                                                                {
                                                                    ControlId = reservationControl.Id,
                                                                    Chave = reservationControl.Chave,
                                                                    ControlStatusString = reservationControl.StatusString,
                                                                    ControlStatus = reservationControl.Status,
                                                                    reserveStatus = reservation.Status,
                                                                    controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                }).ToListAsync() ?? new List<ReservationsControlModel>();

                return reservations;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<List<ReservedProductModel>?> GetReservedProducts(int IdReservationControl, int FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing GetReservedProducts IdReservationControl:{id}, IdFerramentaria:{IdFerramentaria}", IdReservationControl, FerramentariaValue);



                List<ReservedProductModel>? reservedList = await (from reservation in _context.Reservations
                                                            join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
                                                            join member in _context.LeaderMemberRel on reservation.IdLeaderMemberRel equals member.Id
                                                            join leader in _context.LeaderData on member.IdLeader equals leader.Id
                                                            join obra in _context.Obra on reservation.IdObra equals obra.Id
                                                            join produto in _context.Produto on new { reservation.IdCatalogo, reservation.IdFerramentaria }
                                                                    equals new { produto.IdCatalogo, produto.IdFerramentaria }
                                                            join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                            join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                            where reservation.Status == 1
                                                            && produto.Ativo == 1
                                                            && catalogo.Ativo == 1
                                                            && categoria.Ativo == 1
                                                            && reservation.IdReservationControl == IdReservationControl
                                                            && reservation.IdFerramentaria == FerramentariaValue
                                                            select new ReservedProductModel
                                                            {
                                                                IdReservationControl = reservation.IdReservationControl,
                                                                IdReservation = reservation.Id,
                                                                IdCatalogo = reservation.IdCatalogo,
                                                                IdFerramentaria = reservation.IdFerramentaria,
                                                                IdProduto = produto.Id,

                                                                intClasse = categoria.Classe,
                                                                Classe = categoria.ClassType,
                                                                Type = catalogo.PorType,
                                                                Codigo = catalogo.Codigo,
                                                                itemNome = catalogo.Nome,
                                                                intStatus = reservation.Status,
                                                                Status = reservation.StatusString,
                                                                MemberCodPessoa = member.CodPessoa,
                                                                LeaderCodPessoa = leader.CodPessoa,
                                                                IdObra = obra.Id,
                                                                ObraName = $"{obra.Codigo}-{obra.Nome}",

                                                                QtyRequested = reservation.Quantidade,
                                                                QtyStock = produto.Quantidade,
                                                            }).ToListAsync() ?? new List<ReservedProductModel>();

                Dictionary<int, Funcionario?> funcionarioDict = _contextBS.Funcionario.AsEnumerable()
                                                .GroupBy(e => e.CodPessoa)
                                                .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
                                                .Where(f => f != null)  // Filter out nulls if any
                                                .ToDictionary(
                                                    f => f.CodPessoa.Value,   // Key selector
                                                    f => f              // Value selector
                                                );

                var enrichedReservations = reservedList.Select(r =>
                {
                    // Get member and leader information from dictionary
                    var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
                    var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

                    return new ReservedProductModel
                    {


                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        IdCatalogo = r.IdCatalogo,
                        IdFerramentaria = r.IdFerramentaria,
                        IdProduto = r.IdProduto,

                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        intStatus = r.intStatus,
                        Status = r.Status,
                        MemberCodPessoa = member.CodPessoa,
                        LeaderCodPessoa = leader.CodPessoa,
                        IdObra = r.IdObra,
                        ObraName = r.ObraName,

                        QtyRequested = r.QtyRequested,
                        QtyStock = r.QtyStock,
                        IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != r.IdFerramentaria && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

                        // Add new properties with funcionario data
                        MemberInfo = new employeeNewInformationModel()
                        {
                            Chapa = memberInfo.Chapa,
                            Nome = memberInfo.Nome,
                            CodSituacao = memberInfo.CodSituacao,
                            CodColigada = memberInfo.CodColigada,
                            Funcao = memberInfo.Funcao,
                            Secao = memberInfo.Secao,
                            CodPessoa = memberInfo.CodPessoa,
                        },
                        LeaderInfo = new employeeNewInformationModel()
                        {
                            Chapa = leaderInfo.Chapa,
                            Nome = leaderInfo.Nome,
                            CodSituacao = leaderInfo.CodSituacao,
                            CodColigada = leaderInfo.CodColigada,
                            Funcao = leaderInfo.Funcao,
                            Secao = leaderInfo.Secao,
                            CodPessoa = leaderInfo.CodPessoa,
                        },
                    };
                }).ToList();

                return enrichedReservations;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task CancelReservation(int? IdReservation, string Chapa, string Observacao)
        {
            try
            {
                _logger.LogInformation("Processing CancelReservation IdReservation:{id}, Chapa:{Chapa}, Observacao:{Observacao}", IdReservation, Chapa, Observacao);

                if (IdReservation == null) throw new ProcessErrorException("IdReservation is required");

                if (string.IsNullOrEmpty(Chapa)) throw new ProcessErrorException("Chapa is required", nameof(Chapa));

                Reservations? reservation = await _context.Reservations.FindAsync(IdReservation);
                if (reservation == null) throw new ProcessErrorException($"Reserva:{IdReservation} não encontrado.");

                if (reservation.Status == 8) throw new ProcessErrorException($"Reserva:{IdReservation} já está cancelada.");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 8;
                        reservation.Observacao = $"Cancellado por: {Chapa} - {Observacao}";


                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }

                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<Reservations?> GetReservations(int id)
        {
            try
            {
                _logger.LogInformation("Processing GetReservations id:{id}", id);
                return await _context.Reservations.FindAsync(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task TransferReservation(int IdReservation, string Observacao, int IdFerramentariaTo)
        {
            try
            {
                _logger.LogInformation("Processing TransferReservation IdReservation:{id}, Observacao:{Observacao}, FerramentariaTo:{IdFerramentariaTo}", IdReservation, Observacao, IdFerramentariaTo);

                if (IdReservation <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                if (string.IsNullOrWhiteSpace(Observacao)) throw new ProcessErrorException("Observation cannot be empty", nameof(Observacao));

                if (IdFerramentariaTo <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                Reservations? reservation = await _context.Reservations.FindAsync(IdReservation);

                if (reservation == null) throw new ProcessErrorException("Reservation is null");
              
                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 0;
                        reservation.Observacao = Observacao;
                        reservation.IdFerramentaria = IdFerramentariaTo;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task FinalizeProcessReservation(List<ProductReservation> finalSubmissions)
        {
            try
            {
                using var _ = LogContext.PushProperty("FinalReservation", finalSubmissions, destructureObjects: true);
                _logger.LogInformation("Processing FinalizeProcessReservation");

                if (finalSubmissions == null || finalSubmissions.Count == 0) throw new ProcessErrorException("ProcessList is empty");
                if (finalSubmissions.Any(i => i.IdReservation == null)) throw new ProcessErrorException("Some of the IdReservation is null");
                if (finalSubmissions.Any(i => i.IdProduto == null)) throw new ProcessErrorException("Some of the IdProduto is null");
                if (finalSubmissions.Any(i => i.FinalQuantity == null)) throw new ProcessErrorException("Some of the QtyRequested is null");


                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {
                        var reservationIds = finalSubmissions
                                            .Select(i => i.IdReservation.Value)
                                            .Distinct()
                                            .ToList();

                        await _context.Reservations.Where(r => reservationIds.Contains(r.Id.Value)).ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, 2));

                        _context.ProductReservation.AddRange(finalSubmissions);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<List<ProductReservation>?> VerifyReservations(List<FinalSubmissionProcess> finalSubmissions)
        {
            try
            {
                using var _ = LogContext.PushProperty("FinalReservation", finalSubmissions, destructureObjects: true);
                _logger.LogInformation("Processing VerifyReservations");

                if (finalSubmissions == null || finalSubmissions.Count == 0) throw new ProcessErrorException("ProcessList is empty");
                if (finalSubmissions.Any(i => i.IdReservation == null)) throw new ProcessErrorException("Some of the IdReservation is null");
                if (finalSubmissions.Any(i => i.IdProduto == null)) throw new ProcessErrorException("Some of the IdProduto is null");
                if (finalSubmissions.Any(i => i.QtyRequested == null)) throw new ProcessErrorException("Some of the QtyRequested is null");

                List<int>? reservationIds = finalSubmissions.Select(i => i.IdReservation!.Value).ToList();
                var existingReservations = await _context.Reservations.Where(r => r.Id.HasValue && reservationIds.Contains(r.Id.Value)).ToDictionaryAsync(r => r.Id.Value);

                var missingIds = reservationIds.Except(existingReservations.Keys).ToList();
                if (missingIds.Any())
                {
                    throw new InvalidOperationException($"Reservations not found: {string.Join(", ", missingIds)}");
                }

                var invalidStatus = existingReservations.Values.Where(r => r.Status != 1).ToList();
                if (invalidStatus.Any())
                {
                    throw new InvalidOperationException($"Invalid status for reservations: {string.Join(", ", invalidStatus.Select(r => r.Id))}");
                }



                List<ProductReservation>? entities = finalSubmissions.Select(item =>
                                                        new ProductReservation()
                                                        {
                                                            IdReservation = item.IdReservation,
                                                            IdProduto = item.IdProduto,
                                                            DataPrevistaDevolucao = item.DateReturn,
                                                            Observacao = item.Observacao,                                  
                                                            Status = 0,
                                                            DataRegistro = DateTime.Now,
                                                            FinalQuantity = item.QtyRequested,
                                                        }).ToList();

                return entities;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<bool> VerifyFinalizeTransactionId(string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing VerifyFinalizeTransactionId TransactionId:{transactionId}", transactionId);

                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("Transaction is empty.");

                return await _context.ProductReservation.AnyAsync(i => i.TransactionId == transactionId);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }




        public async Task<List<FinalReservationResult>?> GetFinalizedReservation(int codPessoa, int idFerramentaria)
        {
            try
            {
                _logger.LogInformation("Processing GetFinalizedReservation codPessoa:{codPessoa}, idFerramentaria:{idFerramentaria}", codPessoa, idFerramentaria);

                List<FinalReservationResult>? reservedproducts = await (from finalproduct in _context.ProductReservation
                                                                  join reservations in _context.Reservations on finalproduct.IdReservation equals reservations.Id
                                                                  join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
                                                                  join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
                                                                  join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
                                                                  join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
                                                                  join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                  join obra in _context.Obra on reservations.IdObra equals obra.Id
                                                                  where control.Type == 1
                                                                      && finalproduct.Status == 0
                                                                      && reservations.Status == 2
                                                                      && reservations.IdFerramentaria == idFerramentaria
                                                                      && member.CodPessoa == codPessoa
                                                                  select new FinalReservationResult
                                                                  {
                                                                      IdProductReservation = finalproduct.Id,
                                                                      IdReservationControl = reservations.IdReservationControl,
                                                                      IdReservation = reservations.Id,
                                                                      IdProduto = finalproduct.IdProduto,
                                                                      IdObra = reservations.IdObra,
                                                                      intClasse = categoria.Classe,
                                                                      Classe = categoria.ClassType,
                                                                      Type = catalogo.PorType,
                                                                      Codigo = catalogo.Codigo,
                                                                      Nome = catalogo.Nome,
                                                                      QtyFinal = finalproduct.FinalQuantity,
                                                                      DateReturn = finalproduct.DataPrevistaDevolucao.HasValue == true ? finalproduct.DataPrevistaDevolucao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                                      DateReturnProper = finalproduct.DataPrevistaDevolucao,
                                                                      Observacao = finalproduct.Observacao,
                                                                      MemberCodPessoa = member.CodPessoa,
                                                                      LeaderCodPessoa = leader.CodPessoa,
                                                                  }).ToListAsync() ?? new List<FinalReservationResult>();

                //if (reservedproducts == null || reservedproducts.Count == 0) throw new InvalidOperationException("Nenhuma reserva finalizada encontrada.");


                Dictionary<int, employeeNewInformationModel> funcionarioDict = _contextBS.Funcionario.Where(f => f.CodPessoa != null)
                                                                                .GroupBy(f => f.CodPessoa)
                                                                                .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                                .ToDictionary(
                                                                                    f => f.CodPessoa!.Value,
                                                                                    f => new employeeNewInformationModel
                                                                                    {
                                                                                        Chapa = f.Chapa,
                                                                                        Nome = f.Nome,
                                                                                        CodSituacao = f.CodSituacao,
                                                                                        CodColigada = f.CodColigada,
                                                                                        Funcao = f.Funcao,
                                                                                        Secao = f.Secao,
                                                                                        CodPessoa = f.CodPessoa,
                                                                                    }
                                                                                );

                var enrichedReservations = reservedproducts.Select(r =>
                {
                    // Get member and leader information from dictionary
                    //var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : new Funcionario();
                    //var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : new Funcionario();

                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new FinalReservationResult
                    {
                        IdProductReservation = r.IdProductReservation,
                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        IdProduto = r.IdProduto,
                        IdObra = r.IdObra,
                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        Nome = r.Nome,
                        QtyFinal = r.QtyFinal,
                        DateReturn = r.DateReturn,
                        DateReturnProper = r.DateReturnProper,
                        Observacao = r.Observacao,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<Result> FinalizeProcessHandoutReservation(List<FinalSubmissionProcess> submissions, string transactionId, int userId, int idFerramentaria)
        {
            try
            {
                using (LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true))
                {
                    _logger.LogInformation("Processing FinalizeProcessHandoutReservation with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                }

                if (submissions.Count == 0) throw new ProcessErrorException("submissions are empty", nameof(submissions));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is empty", nameof(transactionId));
                if (userId == 0) throw new ProcessErrorException("userId is 0", nameof(userId));
                if (idFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0", nameof(idFerramentaria));

                //using var _ = LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true);
                //{
                //    _logger.LogInformation("Processing FinalizeProcessHandoutReservation with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                //}
                

                List<string?> errors = new List<string?>();

                await using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {
                        foreach (var item in submissions)
                        {
                            Result result = await ProcessSubmissionItem(item, transactionId, userId, idFerramentaria);
                            if (result.IsFailure)
                            {
                                errors.Add(result.Error);
                            }
                        }

                        if (errors.Count > 0)
                        {
                            await transaction.RollbackAsync();
                            return Result.Failure(string.Join("<br>", errors));
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Result.Success();

                    }
                    catch (Exception)
                    {
                        _logger.LogError("Transaction Rollback - Insertion Fail.");
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Service Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Service Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }


        private async Task<Result> ProcessSubmissionItem(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            // Validate entities
            var reservation = await _context.Reservations.FirstOrDefaultAsync(i => i.Id == item.IdReservation);
            if (reservation == null)
                return Result.Failure($"IdReservation:{item.IdReservation} not found.");

            var productReservation = await _context.ProductReservation.FirstOrDefaultAsync(i => i.Id == item.IdProductReservation);
            if (productReservation == null)
                return Result.Failure($"Cannot find product reservation with the Id:{item.IdProductReservation}");

            var product = await _context.Produto.FirstOrDefaultAsync(x => x.Id == item.IdProduto);
            if (product == null)
                return Result.Failure($"Cannot find product with the Id:{item.IdProduto}");

            if (product.Quantidade < item.QtyRequested)
                return Result.Failure($"Insufficient stock for Product ID {item.IdProduto}. Available: {product.Quantidade}.");

            var historico = await _context.HistoricoAlocacao_2025.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
            if (historico != null)
                return Result.Failure($"IdReservation:{item.IdReservation} has already been allocated with TransactionId:{transactionId}. possible risk of duplication.");

            await CreateAllocationHistory(item, productReservation, transactionId, userId, idFerramentaria);
            UpdateProductStock(product, productReservation.FinalQuantity!.Value, transactionId);
            UpdateFinalReservationStatus(reservation, transactionId);
            UpdateProductReservationStatus(productReservation, userId, transactionId);

            return Result.Success();
        }

        private async Task CreateAllocationHistory(FinalSubmissionProcess item, ProductReservation productReservation, string transactionId, int userId, int idFerramentaria)
        {
            var historico = new HistoricoAlocacao_2025
            {
                IdProduto = item.IdProduto,
                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                Solicitante_CodColigada = item.CodColigadaSolicitante,
                Solicitante_Chapa = item.ChapaSolicitante,
                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                Liberador_CodColigada = item.CodColigadaLiberador,
                Liberador_Chapa = item.ChapaLiberador,
                Balconista_Emprestimo_IdLogin = userId,
                Balconista_Devolucao_IdLogin = userId,
                Observacao = $"Reservation: {productReservation.Observacao}",
                DataEmprestimo = DateTime.Now,
                DataDevolucao = productReservation.DataPrevistaDevolucao ?? DateTime.Now,
                IdObra = item.IdObra,
                Quantidade = productReservation.FinalQuantity,
                IdFerrOndeProdRetirado = idFerramentaria,
                IdControleCA = productReservation.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                EmprestimoTransactionId = transactionId,
                CrachaNo = item.CrachaNo
            };

            //using var _ = LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true);
            //{
            //    _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            //};

            using (LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true))
            {
                _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            //using var logScope = LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true);
            //_logger.LogInformation("Allocation - Transaction:{TransactionId}", transactionId);

            await _context.AddAsync(historico);
        }

        private void UpdateProductStock(Produto product, int finalQuantity, string transactionId)
        {
            int qtyBefore = product.Quantidade!.Value;
            int qtyAfter = product.Quantidade.Value - finalQuantity;

            _logger.LogInformation("StockUpdate - Transaction:{TransactionId} | ProductId:{ProductId} | Before:{QtyFrom} - After:{QtyAfter}", transactionId, product.Id, qtyBefore, qtyAfter);

            product.Quantidade = qtyAfter;
            _context.Update(product);
        }

        private void UpdateFinalReservationStatus(Reservations reservation, string transactionId)
        {
            _logger.LogInformation("ReservationUpdate - Transaction:{TransactionId} | ReservationId:{IdReservation} | From:{OldStatus} To:{NewStatus}", transactionId, reservation.Id, reservation.Status, 3);
            reservation.Status = 3;
            _context.Update(reservation);
        }

        private void UpdateProductReservationStatus(ProductReservation productReservation, int userId, string transactionId)
        {
            productReservation.Status = 3;
            productReservation.HandedBy = userId;
            productReservation.ModifiedTransactionId = transactionId;
            productReservation.ModifiedDate = DateTime.Now;
            _context.Update(productReservation);
        }




    }
}
