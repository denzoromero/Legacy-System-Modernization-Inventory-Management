using FerramentariaTest.Controllers;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using System;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace FerramentariaTest.Services
{
    public class RetiradaService : IRetiradaService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ILogger<RetiradaService> _logger;


        public RetiradaService(ContextoBanco context, ILogger<RetiradaService> logger, ContextoBancoBS contextBS)
        {
            _context = context;
            _contextBS = contextBS;
            _logger = logger;
        }

        public async Task<List<newCatalogInformationModel>> GetRetiradaOrders(int codpessoa, int idFerramentaria)
        {
            try
            {
                if (codpessoa == 0) throw new ProcessErrorException("codpessoa is 0");
                if (idFerramentaria == 0) throw new ProcessErrorException("idFerramentaria is 0");

                List<newCatalogInformationModel> reservedproducts = await (from reservations in _context.Reservations
                                                                     join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
                                                                     join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
                                                                     join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
                                                                     join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
                                                                     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                     join obra in _context.Obra on reservations.IdObra equals obra.Id
                                                                     where control.Type == 2
                                                                         && reservations.Status == 0
                                                                         && reservations.IdFerramentaria == idFerramentaria
                                                                         && member.CodPessoa == codpessoa
                                                                     group new
                                                                     {
                                                                         reservations,
                                                                         control,
                                                                         member,
                                                                         leader,
                                                                         catalogo,
                                                                         categoria,
                                                                         obra
                                                                     } by new { reservations.IdCatalogo, reservations.IdReservationControl } into grouped
                                                                     select new newCatalogInformationModel
                                                                     {
                                                                         IdCatalogo = grouped.Key.IdCatalogo,
                                                                         IdCategoria = grouped.First().catalogo.IdCategoria,
                                                                         intClasse = grouped.First().categoria.Classe,
                                                                         Classe = grouped.First().categoria.ClassType,
                                                                         Type = grouped.First().catalogo.PorType,
                                                                         Codigo = grouped.First().catalogo.Codigo,
                                                                         itemNome = grouped.First().catalogo.Nome,
                                                                         DataDeRetornoAutomatico = grouped.First().catalogo.DataDeRetornoAutomatico,
                                                                         IdObra = grouped.First().reservations.IdObra,
                                                                         ObraName = $"{grouped.First().obra.Codigo}-{grouped.First().obra.Nome}",
                                                                         IdReservationControl = grouped.First().control.Id,
                                                                         IdReservation = grouped.First().reservations.Id,
                                                                         QuantidadeResquested = grouped.Sum(x => x.reservations.Quantidade),
                                                                         MemberCodPessoa = grouped.First().member.CodPessoa,
                                                                         LeaderCodPessoa = grouped.First().leader.CodPessoa,
                                                                     }
                                                                     ).ToListAsync() ?? new List<newCatalogInformationModel>();

                if (reservedproducts.Count == 0) throw new InvalidOperationException("Nenhuma retirada encontrada.");

                foreach (newCatalogInformationModel item in reservedproducts)
                {
                    List<Produto>? listProducts = _context.Produto.Where(i => i.IdCatalogo == item.IdCatalogo
                                                                        && i.IdFerramentaria == idFerramentaria
                                                                        && i.Ativo == 1
                                                                        && i.Quantidade > 0).ToList();

                    if (listProducts.Count == 1)
                    {
                        item.IdProdutoSelected = listProducts[0].Id;
                    }

                    item.listProducts = listProducts
                                        .Where(p => item.Type != "PorAferido" || p.DataVencimento > DateTime.Now)
                                        .Select(p => new newProductInformation
                                        {
                                            IdProduto = p.Id,
                                            IdFerramentaria = p.IdFerramentaria,
                                            AF = p.AF,
                                            PAT = p.PAT,
                                            StockQuantity = p.Quantidade,
                                            DataVencimento = p.DataVencimento,
                                            AllowedToBorrow = item.Type == "PorAferido"
                                                ? p.DataVencimento > DateTime.Now
                                                : true,
                                            Reason = item.Type == "PorAferido"
                                                ? (p.DataVencimento > DateTime.Now ? "Valid" : "Expired")
                                                : string.Empty
                                        })
                                        .ToList();

                    if (item.intClasse == 2)
                    {
                        List<ControleCA>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == item.IdCatalogo && i.Ativo == 1 && i.Validade > DateTime.Now).OrderByDescending(i => i.Validade).ToList() ?? new List<ControleCA>();

                        if (controleCAData.Count > 0)
                        {
                            item.listCA = controleCAData;
                        }

                        if (item.DataDeRetornoAutomatico.HasValue && item.DataDeRetornoAutomatico != 0)
                        {
                            item.DataReturn = DateTime.Now.AddDays(item.DataDeRetornoAutomatico.Value);
                        }

                    }

                }

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

                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new newCatalogInformationModel
                    {
                        IdCatalogo = r.IdCatalogo,
                        IdCategoria = r.IdCategoria,
                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        DataDeRetornoAutomatico = r.DataDeRetornoAutomatico,
                        IdObra = r.IdObra,
                        ObraName = r.ObraName,
                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        QuantidadeResquested = r.QuantidadeResquested,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        listProducts = r.listProducts,
                        listCA = r.listCA,
                        IdProdutoSelected = r.IdProdutoSelected,
                        DataReturn = r.DataReturn,
                        IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != idFerramentaria && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

                        // Add new properties with funcionario data
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel()
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

        public async Task CancelRetirada(int idReservation, string chapa, string observacao, string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing CancelRetirada IdReservation:{idReservation}, Chapa:{chapa}, Observacao:{observacao}, TransactionId:{transactionId}", idReservation, chapa, observacao, transactionId);

                if (idReservation <= 0) throw new ProcessErrorException("IdReservation is 0");
                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("Chapa is null or whitespace", nameof(chapa));
                if (string.IsNullOrWhiteSpace(observacao)) throw new ProcessErrorException("observacao is null or whitespace", nameof(observacao));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is null or whitespace", nameof(transactionId));

                Reservations? reservation = await _context.Reservations.FindAsync(idReservation);
                if (reservation == null) throw new ProcessErrorException($"Reserva:{idReservation} não encontrado.");

                if (reservation.Status == 8) throw new ProcessErrorException($"Reserva:{idReservation} já está cancelada.");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 8;
                        reservation.Observacao = $"Cancellado por: {chapa} - {observacao}";

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

        public async Task TransferRetirada(int idReservation, string observacao, int IdFerramentariaTo, string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing TransferRetirada IdReservation:{id}, Observacao:{Observacao}, FerramentariaTo:{IdFerramentariaTo}, TransactionId:{transactionId}", idReservation, observacao, IdFerramentariaTo, transactionId);


                if (idReservation <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));
                if (string.IsNullOrWhiteSpace(observacao)) throw new ProcessErrorException("Observation cannot be empty", nameof(observacao));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("Observation cannot be empty", nameof(transactionId));
                if (IdFerramentariaTo <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                Reservations? reservation = await _context.Reservations.FindAsync(idReservation);

                if (reservation == null) throw new ProcessErrorException("Reservation is null");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 0;
                        reservation.Observacao = observacao;
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

        public async Task<Result> FinalizeProcessHandoutRetirada(List<FinalSubmissionProcess> submissions, string transactionId, int userId, int idFerramentaria)
        {
            try
            {
                using (LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true))
                {
                    _logger.LogInformation("Processing FinalizeProcessHandoutRetirada with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                }

                if (submissions.Count == 0) throw new ProcessErrorException("submissions are empty", nameof(submissions));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is empty", nameof(transactionId));
                if (userId == 0) throw new ProcessErrorException("userId is 0", nameof(userId));
                if (idFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0", nameof(idFerramentaria));

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



        private async Task<Result> ProcessSubmissionItem(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(i => i.Id == item.IdReservation);
            if (reservation == null) return Result.Failure($"IdReservation:{item.IdReservation} not found.");

            var product = await _context.Produto.FirstOrDefaultAsync(x => x.Id == item.IdProduto);
            if (product == null) return Result.Failure($"Cannot find product with the Id:{item.IdProduto}");

            if (product.Quantidade < item.QtyRequested) return Result.Failure($"Insufficient stock for Product ID {item.IdProduto}. Available: {product.Quantidade}.");

            if (item.Classe == 3)
            {
                var historico = await _context.HistoricoAlocacao_2025.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
                if (historico != null) return Result.Failure($"IdReservation:{item.IdReservation} has already been History allocated with TransactionId:{transactionId}. possible risk of duplication.");

                await CreateAllocationHistory(item, transactionId, userId, idFerramentaria);
            }
            else
            {
                var produtoAlocado = await _context.ProdutoAlocado.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
                if (produtoAlocado != null) return Result.Failure($"IdReservation:{item.IdReservation} has already been product allocated with TransactionId:{transactionId}. possible risk of duplication.");

                await CreateProdutoAlocado(item, transactionId, userId, idFerramentaria);
            }

            UpdateProductStock(product, item.QtyRequested!.Value, transactionId);
            UpdateFinalReservationStatus(reservation, transactionId);

            return Result.Success();
        }

        private async Task CreateAllocationHistory(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
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
                Observacao = $"Retirada: {item.Observacao}",
                DataEmprestimo = DateTime.Now,
                DataDevolucao = item.DateReturn ?? DateTime.Now,
                IdObra = item.IdObra,
                Quantidade = item.QtyRequested,
                IdFerrOndeProdRetirado = idFerramentaria,
                IdControleCA = item.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                EmprestimoTransactionId = transactionId,
                CrachaNo = item.CrachaNo,
            };

            using (LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true))
            {
                _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            await _context.AddAsync(historico);
        }

        private async Task CreateProdutoAlocado(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            string key = $"{item.IdProduto}-{item.CodColigadaSolicitante}-{item.ChapaSolicitante}-{userId}-{DateTime.Now:dd/MM/yyyy HH:mm}-{item.IdObra}-{item.QtyRequested}-{idFerramentaria}";
            string hash;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(key);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            ProdutoAlocado produtoAlocado = new ProdutoAlocado
            {
                IdProduto = item.IdProduto,
                IdObra = item.IdObra,
                IdFerrOndeProdRetirado = idFerramentaria,
                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                Solicitante_CodColigada = item.CodColigadaSolicitante,
                Solicitante_Chapa = item.ChapaSolicitante,
                Balconista_IdLogin = userId,
                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                Liberador_CodColigada = item.CodColigadaLiberador,
                Liberador_Chapa = item.ChapaLiberador,
                Observacao = $"Retirada:{item.Observacao}",
                DataPrevistaDevolucao = item.DateReturn,
                DataEmprestimo = DateTime.Now,
                Quantidade = item.QtyRequested,
                Chave = hash,
                IdControleCA = item.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                CrachaNo = item.CrachaNo,
            };

            using (LogContext.PushProperty("ProdutoAlocado", produtoAlocado, destructureObjects: true))
            {
                _logger.LogInformation("ProdutoAlocado - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            await _context.AddAsync(produtoAlocado);

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


    }
}
