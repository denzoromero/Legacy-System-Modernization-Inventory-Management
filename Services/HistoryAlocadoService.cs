using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FerramentariaTest.Services
{
    public class HistoryAlocadoService : IHistoryAlocadoService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoSeek _contextSeek;
        private readonly ILogger<EmployeeService> _logger;

        public HistoryAlocadoService(ContextoBanco context, ILogger<EmployeeService> logger, ContextoBancoBS contextBS, ContextoBancoSeek contextSeek)
        {
            _context = context;
            _contextBS = contextBS;
            _contextSeek = contextSeek;
            _logger = logger;
        }

        public async Task<List<HistoryAlocadoReportModel>> GetEmployeeItemHistory(string chapa, int codColigada, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory Chapa:{chapa}, Year:{year}", chapa, year);

                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("chapa is empty.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                string tableName = $"HistoricoAlocacao_{year}";

                var dbSetProperties = _context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                if (tableProperty == null) throw new ProcessErrorException($"Table:{tableName} not found on the database.");

                var table = tableProperty.GetValue(_context, null);

                //_context.Database.SetCommandTimeout(300);

                List<HistoryAlocadoReportModel> historyResult = await (from history in (IQueryable<HistoricoAlocacao>)table
                                                                     join product in _context.Produto on history.IdProduto equals product.Id
                                                                     join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                     join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                     join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                                                                     join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                                                                     from destination in ferrDevGroup.DefaultIfEmpty()
                                                                     join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                                                                     from controle in controlGroup.DefaultIfEmpty()
                                                                     where history.Solicitante_Chapa == chapa
                                                                            && history.Solicitante_CodColigada == codColigada
                                                                            && history.DataDevolucao >= startDate
                                                                            && history.DataDevolucao <= endDate
                                                                     select new HistoryAlocadoReportModel
                                                                     {
                                                                         IdCatalogo = product.IdCatalogo,
                                                                         Code = catalog.Codigo,
                                                                         Description = catalog.Nome,
                                                                         IdProduto = product.Id,
                                                                         BorrowedDate = history.DataEmprestimo,
                                                                         BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                         ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                         Qty = history.Quantidade.ToString(),
                                                                         LocationOrigin = origin.Nome,
                                                                         LocationReturn = destination != null ? destination.Nome : string.Empty,
                                                                         ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                         IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                                                                         IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                         //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                         IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                                                                         IdTransactionEmprestimo = year >= 2025 ? history.EmprestimoTransactionId : string.Empty,
                                                                         ItemClass = category.ClassType,
                                                                         CrachaNo = year >= 2025 ? history.CrachaNo : string.Empty,
                                                                     }).AsNoTracking().ToListAsync();


                //var historyResult = (from history in (IQueryable<HistoricoAlocacao>)table
                //                                                        join product in _context.Produto on history.IdProduto equals product.Id
                //                                                        join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                //                                                        join category in _context.Categoria on catalog.IdCategoria equals category.Id
                //                                                        join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                //                                                        join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                //                                                        from destination in ferrDevGroup.DefaultIfEmpty()
                //                                                        join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                //                                                        from controle in controlGroup.DefaultIfEmpty()
                //                                                        where history.Solicitante_Chapa == chapa
                //                                                               && history.Solicitante_CodColigada == codColigada
                //                                                               && history.DataDevolucao >= startDate
                //                                                               && history.DataDevolucao <= endDate
                //                                                        select new HistoryAlocadoReportModel
                //                                                        {
                //                                                            IdCatalogo = product.IdCatalogo,
                //                                                            Code = catalog.Codigo,
                //                                                            Description = catalog.Nome,
                //                                                            IdProduto = product.Id,
                //                                                            BorrowedDate = history.DataEmprestimo,
                //                                                            BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy") : string.Empty,
                //                                                            ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy") : string.Empty,
                //                                                            Qty = history.Quantidade.ToString(),
                //                                                            LocationOrigin = origin.Nome,
                //                                                            LocationReturn = destination != null ? destination.Nome : string.Empty,
                //                                                            ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                //                                                            IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                //                                                            IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                //                                                            //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                //                                                            IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                //                                                        });

                //if (historyResult == null || historyResult.Count == 0) throw new ProcessErrorException($"No Result Found for Employee:{chapa}.");
                if (historyResult == null || historyResult.Count == 0) return new List<HistoryAlocadoReportModel>();

                var userIds = historyResult
                               .Select(x => new
                               {
                                   x.IdBalconistaBorrow,
                                   x.IdBalconistaReturn
                               })
                               .Distinct()
                               .ToList();

                // Step 2: Extract and prepare user IDs for lookup
                var borrowUserIds = userIds.Select(x => x.IdBalconistaBorrow).Distinct().ToList();
                var returnUserIds = userIds.Select(x => x.IdBalconistaReturn).Where(id => id.HasValue).Distinct().ToList();
                var allUserIds = borrowUserIds.Union(returnUserIds).Distinct().ToList();

                // Step 3: Get user data from second context
                var funcionarioDict = await _contextBS.VW_Usuario.Where(u => allUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                historyResult = historyResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdBalconistaReturn = x.IdBalconistaReturn,
                                    BalconistaReturn = x.IdBalconistaReturn.HasValue && funcionarioDict.ContainsKey(x.IdBalconistaReturn.Value) ? funcionarioDict[x.IdBalconistaReturn.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    IdTransactionEmprestimo = x.IdTransactionEmprestimo != null ? x.IdTransactionEmprestimo : string.Empty,
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo,
                                }).ToList();

                return historyResult;

                //var result =  (from completeHistory in historyResult
                //                                          join userBorrow in _contextBS.VW_Usuario on completeHistory.IdBalconistaBorrow equals userBorrow.Id
                //                                          join userReturn in _contextBS.VW_Usuario on completeHistory.IdBalconistaReturn equals userReturn.Id
                //                                          select new HistoryAlocadoReportModel
                //                                          {
                //                                              IdCatalogo = completeHistory.IdCatalogo,
                //                                              Code = completeHistory.Code,
                //                                              Description = completeHistory.Description,
                //                                              IdProduto = completeHistory.IdProduto,
                //                                              BorrowedDate = completeHistory.BorrowedDate,
                //                                              BorrowedDateString = completeHistory.BorrowedDateString,
                //                                              ReturnedDateString = completeHistory.ReturnedDateString,
                //                                              Qty = completeHistory.Qty,
                //                                              LocationOrigin = completeHistory.LocationOrigin,
                //                                              LocationReturn = completeHistory.LocationReturn,
                //                                              ControlCA = completeHistory.ControlCA,
                //                                              IdBalconistaBorrow = completeHistory.IdBalconistaBorrow,
                //                                              BalconistaBorrow = userBorrow.Nome,
                //                                              IdBalconistaReturn = completeHistory.IdBalconistaReturn,
                //                                              BalconistaReturn = userReturn.Nome,
                //                                              IdTransaction = completeHistory.IdTransaction,
                //                                          }).AsQueryable();

                //List<HistoryAlocadoReportModel> resulttest = new List<HistoryAlocadoReportModel>();

                //resulttest.AddRange(result);

                //List<CatalogGroupModel> FinalResult = historyResult.GroupBy(i => i.IdCatalogo)
                //                                      .Select(catalog => new CatalogGroupModel
                //                                      {
                //                                          IdCatalogo = catalog.Key,
                //                                          Description = catalog.FirstOrDefault()!.Description,
                //                                          Code = catalog.FirstOrDefault()!.Code,
                //                                          ItemAllocation = catalog.OrderByDescending(x => x.BorrowedDate).ToList()
                //                                      }).ToList();



                //return FinalResult;
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

        public async Task<List<HistoryAlocadoReportModel>> GetEmployeeItemAllocation(string chapa, int codColigada, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory Chapa:{chapa}, Year:{year}", chapa, year);

                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("chapa is empty.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                List<HistoryAlocadoReportModel> allocationResult = await(from allocation in _context.ProdutoAlocado
                                                                      join product in _context.Produto on allocation.IdProduto equals product.Id
                                                                      join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                      join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                      join origin in _context.Ferramentaria on allocation.IdFerrOndeProdRetirado equals origin.Id
                                                                      join controle in _context.ControleCA on allocation.IdControleCA equals controle.Id into controlGroup
                                                                      from controle in controlGroup.DefaultIfEmpty()
                                                                      where allocation.Solicitante_Chapa == chapa
                                                                             && allocation.Solicitante_CodColigada == codColigada
                                                                             && allocation.DataEmprestimo >= startDate
                                                                             && allocation.DataEmprestimo <= endDate
                                                                      select new HistoryAlocadoReportModel
                                                                      {
                                                                          IdProdutoAlocado = allocation.Id,
                                                                          IdCatalogo = product.IdCatalogo,
                                                                          Code = catalog.Codigo,
                                                                          Description = catalog.Nome,
                                                                          IdProduto = product.Id,
                                                                          BorrowedDate = allocation.DataEmprestimo,
                                                                          BorrowedDateString = allocation.DataEmprestimo.HasValue ? allocation.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                          ReturnedDateString = string.Empty,
                                                                          Qty = allocation.Quantidade.ToString(),
                                                                          LocationOrigin = origin.Nome,
                                                                          ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                          IdBalconistaBorrow = allocation.Balconista_IdLogin,
                                                                          //IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                          //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                          IdTransaction = allocation.TransactionId ?? string.Empty,
                                                                          ItemClass = category.ClassType,
                                                                          CrachaNo = allocation.CrachaNo,
                                                                      }).AsNoTracking().ToListAsync();

                if (allocationResult == null || allocationResult.Count == 0) return new List<HistoryAlocadoReportModel>();


                var borrowUserIds = allocationResult.Select(x => x.IdBalconistaBorrow).Distinct().ToList();

                var funcionarioDict = await _contextBS.VW_Usuario.Where(u => borrowUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                allocationResult = allocationResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdProdutoAlocado = x.IdProdutoAlocado,
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    //ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    LostItems = SearchProdutoExtraviadoQuantity(x.IdProdutoAlocado),
                                    BalconistaReturn = GetBalconista(x.IdProdutoAlocado),
                                    LostDateString = LostDate(x.IdProdutoAlocado),
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo
                                }).ToList();

                return allocationResult;

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

        public async Task<List<HistoryAlocadoReportModel>> GetTerceiroItemHistory(int IdTerceiro, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetTerceiroItemHistory IdTerceiro:{IdTerceiro}, Year:{year}", IdTerceiro, year);

                if (IdTerceiro <= 0) throw new ProcessErrorException("IdTerceiro is 0.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                string tableName = $"HistoricoAlocacao_{year}";

                var dbSetProperties = _context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                if (tableProperty == null) throw new ProcessErrorException($"Table:{tableName} not found on the database.");

                var table = tableProperty.GetValue(_context, null);

                List<HistoryAlocadoReportModel> historyResult = await (from history in (IQueryable<HistoricoAlocacao>)table
                                                                       join product in _context.Produto on history.IdProduto equals product.Id
                                                                       join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                       join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                       join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                                                                       join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                                                                       from destination in ferrDevGroup.DefaultIfEmpty()
                                                                       join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                                                                       from controle in controlGroup.DefaultIfEmpty()
                                                                       where history.Solicitante_IdTerceiro == IdTerceiro
                                                                              && history.Solicitante_CodColigada == 0
                                                                              && history.DataDevolucao >= startDate
                                                                              && history.DataDevolucao <= endDate
                                                                       select new HistoryAlocadoReportModel
                                                                       {
                                                                           IdCatalogo = product.IdCatalogo,
                                                                           Code = catalog.Codigo,
                                                                           Description = catalog.Nome,
                                                                           IdProduto = product.Id,
                                                                           BorrowedDate = history.DataEmprestimo,
                                                                           BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           Qty = history.Quantidade.ToString(),
                                                                           LocationOrigin = origin.Nome,
                                                                           LocationReturn = destination != null ? destination.Nome : string.Empty,
                                                                           ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                           IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                                                                           IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                           //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                           IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                                                                           IdTransactionEmprestimo = year >= 2025 ? history.EmprestimoTransactionId : string.Empty,
                                                                           ItemClass = category.ClassType,
                                                                           CrachaNo = year >= 2025 ? history.CrachaNo : string.Empty,
                                                                       }).AsNoTracking().ToListAsync();


                //if (historyResult == null || historyResult.Count == 0) throw new ProcessErrorException($"No Result Found for Employee:{chapa}.");
                if (historyResult == null || historyResult.Count == 0) return new List<HistoryAlocadoReportModel>();

                var userIds = historyResult
                               .Select(x => new
                               {
                                   x.IdBalconistaBorrow,
                                   x.IdBalconistaReturn
                               })
                               .Distinct()
                               .ToList();

                // Step 2: Extract and prepare user IDs for lookup
                var borrowUserIds = userIds.Select(x => x.IdBalconistaBorrow).Distinct().ToList();
                var returnUserIds = userIds.Select(x => x.IdBalconistaReturn).Where(id => id.HasValue).Distinct().ToList();
                var allUserIds = borrowUserIds.Union(returnUserIds).Distinct().ToList();

                // Step 3: Get user data from second context
                var funcionarioDict = await _contextBS.VW_Usuario.Where(u => allUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                historyResult = historyResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdBalconistaReturn = x.IdBalconistaReturn,
                                    BalconistaReturn = x.IdBalconistaReturn.HasValue && funcionarioDict.ContainsKey(x.IdBalconistaReturn.Value) ? funcionarioDict[x.IdBalconistaReturn.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    IdTransactionEmprestimo = x.IdTransactionEmprestimo != null ? x.IdTransactionEmprestimo : string.Empty,
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo,
                                }).ToList();

                return historyResult;

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

        public async Task<List<HistoryAlocadoReportModel>> GetTerceiroItemAllocation(int IdTerceiro, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory IdTerceiro:{IdTerceiro}, Year:{year}", IdTerceiro, year);

                if (IdTerceiro <= 0) throw new ProcessErrorException("IdTerceiro is 0.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                List<HistoryAlocadoReportModel> allocationResult = await (from allocation in _context.ProdutoAlocado
                                                                          join product in _context.Produto on allocation.IdProduto equals product.Id
                                                                          join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                          join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                          join origin in _context.Ferramentaria on allocation.IdFerrOndeProdRetirado equals origin.Id
                                                                          join controle in _context.ControleCA on allocation.IdControleCA equals controle.Id into controlGroup
                                                                          from controle in controlGroup.DefaultIfEmpty()
                                                                          where allocation.Solicitante_IdTerceiro == IdTerceiro
                                                                                 && allocation.Solicitante_CodColigada == 0
                                                                                 && allocation.DataEmprestimo >= startDate
                                                                                 && allocation.DataEmprestimo <= endDate
                                                                          select new HistoryAlocadoReportModel
                                                                          {
                                                                              IdProdutoAlocado = allocation.Id,
                                                                              IdCatalogo = product.IdCatalogo,
                                                                              Code = catalog.Codigo,
                                                                              Description = catalog.Nome,
                                                                              IdProduto = product.Id,
                                                                              BorrowedDate = allocation.DataEmprestimo,
                                                                              BorrowedDateString = allocation.DataEmprestimo.HasValue ? allocation.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                              ReturnedDateString = string.Empty,
                                                                              Qty = allocation.Quantidade.ToString(),
                                                                              LocationOrigin = origin.Nome,
                                                                              ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                              IdBalconistaBorrow = allocation.Balconista_IdLogin,
                                                                              //IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                              //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                              IdTransaction = allocation.TransactionId ?? string.Empty,
                                                                              ItemClass = category.ClassType,
                                                                              CrachaNo = allocation.CrachaNo,
                                                                          }).AsNoTracking().ToListAsync();

                if (allocationResult == null || allocationResult.Count == 0) return new List<HistoryAlocadoReportModel>();


                var borrowUserIds = allocationResult.Select(x => x.IdBalconistaBorrow).Distinct().ToList();

                var funcionarioDict = await _contextBS.VW_Usuario.Where(u => borrowUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                allocationResult = allocationResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdProdutoAlocado = x.IdProdutoAlocado,
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    //ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    LostItems = SearchProdutoExtraviadoQuantity(x.IdProdutoAlocado),
                                    BalconistaReturn = GetBalconista(x.IdProdutoAlocado),
                                    LostDateString = LostDate(x.IdProdutoAlocado),
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo
                                }).ToList();

                return allocationResult;

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


        private int? SearchProdutoExtraviadoQuantity(int? IdProdutoAlocado)
        {

            List<int?> result = (
                                    from produtoExtraviado in _context.ProdutoExtraviado
                                    join produtoAlocado in _context.ProdutoAlocado on produtoExtraviado.IdProdutoAlocado equals produtoAlocado.Id
                                    join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
                                    where produtoExtraviado.IdProdutoAlocado == IdProdutoAlocado && produtoExtraviado.Ativo == 1
                                    select produtoExtraviado.Quantidade
                                  ).ToList();

            int? totalExtraviado = result.Sum();

            return totalExtraviado;
        }

        private string? GetBalconista(int? IdProdutoAlocado)
        {
            int? IdBalconista = _context.ProdutoExtraviado.Where(x => x.IdProdutoAlocado == IdProdutoAlocado && x.Ativo == 1).Select(i => i.IdUsuario).FirstOrDefault();

            if (IdBalconista == null) return string.Empty;

            return _contextBS.VW_Usuario.Where(i => i.Id == IdBalconista).Select(x => x.Nome).FirstOrDefault();

        }

        private string? LostDate(int? IdProdutoAlocado)
        {

           return _context.ProdutoExtraviado.Where(x => x.IdProdutoAlocado == IdProdutoAlocado && x.Ativo == 1)
                                .Select(i => i.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm"))
                                .FirstOrDefault();

        }


    }
}
