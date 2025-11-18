using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;

namespace FerramentariaTest.Services
{
    public class ConsultReservationRetiradaService : IConsultReservationRetirada
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ILogger<ConsultReservationRetiradaService> _logger;

        public ConsultReservationRetiradaService(ContextoBanco context, ILogger<ConsultReservationRetiradaService> logger, ContextoBancoBS contextBS)
        {
            _context = context;
            _contextBS = contextBS;
            _logger = logger;
        }

        public async Task<List<ConsultationReserveModel>?> GetReservationDetailsByEmployee(GestorRRFilterModel? filter)
        {
            try
            {
                using var _ = LogContext.PushProperty("ReservationRetiradaFilter", filter, destructureObjects: true);
                _logger.LogInformation("Processing GetReservationDetailsByEmployee");

                if (filter == null) throw new ProcessErrorException("filter is null");

                List<ConsultationReserveModel>? listReservation = await (from reserve in _context.Reservations
                                                                   join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                                   join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                   join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                                   join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
                                                                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                   join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                                   where member.Chapa == filter.Chapa
                                                                    && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
                                                                    && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
                                                                    && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
                                                                    && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
                                                                    && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
                                                                   select new ConsultationReserveModel
                                                                   {
                                                                       Classe = categoria.ClassType,
                                                                       Codigo = catalogo.Codigo,
                                                                       itemNome = catalogo.Nome,
                                                                       MemberCodPessoa = member.CodPessoa,
                                                                       LeaderCodPessoa = leader.CodPessoa,
                                                                       Quantidade = reserve.Quantidade,
                                                                       Ferramentaria = ferramentaria.Nome,
                                                                       OrderNo = reservationControl.Id,
                                                                       ReservationType = reservationControl.TypeString,
                                                                       StatusString = reserve.StatusString,
                                                                   }).ToListAsync() ?? new List<ConsultationReserveModel>();

                if (listReservation == null || listReservation.Count == 0) throw new InvalidOperationException($"Nenhuma reserva/retirada encontrada para o funcionário:{filter.Chapa}");

                Dictionary<int, employeeNewInformationModel> funcionarioDict = await _contextBS.Funcionario.Where(f => f.CodPessoa != null)
                                                                                 .GroupBy(f => f.CodPessoa)
                                                                                 .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                                 .ToDictionaryAsync(
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

                var enrichedReservations = listReservation.Select(r =>
                {
                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new ConsultationReserveModel
                    {
                        Classe = r.Classe,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        Quantidade = r.Quantidade,
                        Ferramentaria = r.Ferramentaria,
                        OrderNo = r.OrderNo,
                        ReservationType = r.ReservationType,
                        StatusString = r.StatusString,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
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

        public async Task<ConsultationModel?> GetReservationRetiradaInformation(int orderNo)
        {
            try
            {
                if (orderNo <= 0) throw new ProcessErrorException("OrderNo is less than or equal to 0");

                _logger.LogInformation("Processing GetReservationRetiradaInformation with OrderNo:{orderNo}", orderNo);

                ConsultationModel? consultItems = await (from reserve in _context.Reservations
                                                   join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                   join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                   join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                   join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
                                                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                   join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                   where reserve.IdReservationControl == orderNo
                                                   group new
                                                   {
                                                       reserve,
                                                       reservationControl,
                                                       leader,
                                                       member,
                                                       catalogo,
                                                       categoria,
                                                       ferramentaria
                                                   } by new { reserve.IdReservationControl } into grouped
                                                   select new ConsultationModel
                                                   {
                                                       ControlId = grouped.Key.IdReservationControl,
                                                       LeaderName = grouped.First().leader.Nome,
                                                       ControlType = grouped.First().reservationControl.TypeString,
                                                       ControlStatusString = grouped.First().reservationControl.StatusString,
                                                       DateRegistration = grouped.First().reservationControl.DataRegistro.HasValue == true ? grouped.First().reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                       DateExpiration = grouped.First().reservationControl.ExpirationDate.HasValue == true ? grouped.First().reservationControl.ExpirationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                       ReservationList = grouped.Select(g => new ConsultationReserveModel
                                                       {
                                                           Classe = g.categoria.ClassType,
                                                           Codigo = g.catalogo.Codigo,
                                                           itemNome = g.catalogo.Nome,
                                                           Requester = g.member.Nome,
                                                           Ferramentaria = g.ferramentaria.Nome,
                                                           Quantidade = g.reserve.Quantidade,
                                                           StatusString = g.reserve.StatusString,
                                                       }).ToList()
                                                   }).FirstOrDefaultAsync();

                return consultItems;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
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

        public async Task<List<CatalogDetail>?> GetGestorListInformation(GestorRRFilterModel filter)
        {
            try
            {
                using var _ = LogContext.PushProperty("ReservationRetiradaFilter", filter, destructureObjects: true);
                _logger.LogInformation("Processing GetGestorListInformation");

                if (filter == null) throw new ProcessErrorException("filter is null");

                List<CatalogDetail>? CatalogList = await (from produto in _context.Produto
                                                          join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                          join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                          join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                          join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
                                                          where
                                                             catalogo.Ativo == 1
                                                          && produto.Ativo == 1
                                                          && produto.IdFerramentaria != 17
                                                          && produto.Quantidade > 0
                                                          && categoria.Ativo == 1
                                                          && ferramentaria.Ativo == 1
                                                          && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
                                                          && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
                                                          && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
                                                          && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
                                                          && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
                                                          && !catalogo.Nome!.Contains("INUTILIZAR") 
                                                          select new CatalogDetail
                                                          {
                                                              Id = catalogo.Id,
                                                              IdCategoria = catalogo.IdCategoria,
                                                              Codigo = catalogo.Codigo,
                                                              Classe = categoria.Nome,
                                                              Tipo = categoriaPai.Nome,
                                                              Nome = catalogo.Nome,
                                                              PorType = catalogo.PorType,
                                                              ClassType = categoria.ClassType,
                                                              Quantity = produto.Quantidade,
                                                              IdFerramentaria = ferramentaria.Id,
                                                              Ferramentaria = ferramentaria.Nome,
                                                              Ferramentarias = new List<FerramentariaStockModel>
                                                                                    {
                                                                                        new FerramentariaStockModel
                                                                                        {
                                                                                            Id = ferramentaria.Id,
                                                                                            Nome = ferramentaria.Nome,
                                                                                            Quantity = produto.Quantidade, // Raw quantity before grouping
                                                                                        }
                                                                                    }
                                                          }).ToListAsync();

                if (CatalogList == null || CatalogList.Count == 0) throw new InvalidOperationException("Nenhum resultado encontrado.");

                List<CatalogDetail>? GroupedCatalogList = CatalogList
                                                    .GroupBy(x => x.Id)
                                                    .Select(group => new CatalogDetail
                                                    {
                                                        Id = group.Key,
                                                        IdCategoria = group.First().IdCategoria,
                                                        Codigo = group.First().Codigo,
                                                        Classe = group.First().Classe,
                                                        Tipo = group.First().Tipo,
                                                        Nome = group.First().Nome,
                                                        PorType = group.First().PorType,
                                                        ClassType = group.First().ClassType,
                                                        Quantity = group.Sum(x => x.Quantity),
                                                        Ferramentarias = group
                                                                        .SelectMany(x => x.Ferramentarias)
                                                                        .GroupBy(f => f.Id)
                                                                        .Select(fGroup => new FerramentariaStockModel
                                                                        {
                                                                            Id = fGroup.Key,
                                                                            Nome = fGroup.First().Nome,
                                                                            Quantity = fGroup.Sum(f => f.Quantity),
                                                                        })
                                                                        .ToList(),
                                                    }).ToList();

                var reservedQuantities = await _context.Reservations
                                            .Where(r => r.Status != 7 && r.Status != 8 && r.Status != 3)
                                            .GroupBy(r => new { r.IdCatalogo, r.IdFerramentaria })
                                            .Select(g => new
                                            {
                                                g.Key.IdCatalogo,
                                                g.Key.IdFerramentaria,
                                                TotalReserved = g.Sum(x => x.Quantidade)
                                            })
                                            .ToDictionaryAsync(x => new { x.IdCatalogo, x.IdFerramentaria }, x => x.TotalReserved);

                var finalCatalogList = GroupedCatalogList.Select(catalog =>
                {
                    // Update quantities for each location
                    var updatedFerramentarias = catalog.Ferramentarias!.Select(ferramentaria =>
                    {
                        var key = new
                        {
                            IdCatalogo = catalog.Id,
                            IdFerramentaria = ferramentaria.Id  // Changed from 'Id' to 'IdFerramentaria'
                        };
                        var reservedQty = reservedQuantities.TryGetValue(key, out var reserved) ? reserved : 0;

                        return new FerramentariaStockModel
                        {
                            Id = ferramentaria.Id,
                            Nome = ferramentaria.Nome,
                            Quantity = ferramentaria.Quantity,
                            ReservedQuantity = reservedQty,
                            AvailableQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
                            ferramentariaAllocatedQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
                        };
                    }).ToList();

                    return new CatalogDetail
                    {
                        Id = catalog.Id,
                        IdCategoria = catalog.IdCategoria,
                        Codigo = catalog.Codigo,
                        Classe = catalog.Classe,
                        Tipo = catalog.Tipo,
                        Nome = catalog.Nome,
                        PorType = catalog.PorType,
                        ClassType = catalog.ClassType,
                        Quantity = catalog.Quantity,
                        OverallQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
                        Ferramentaria = string.Join(", ",
                                                            updatedFerramentarias
                                                                .Where(f => f.AvailableQuantity > 0)
                                                                .Select(f => f.Nome)
                                                                .Distinct()),
                        Ferramentarias = updatedFerramentarias,
                        ReservedQuantity = updatedFerramentarias.Sum(f => f.ReservedQuantity),
                        allocatedQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
                    };
                }).ToList();

                //if (filter.IsChecked == true) return finalCatalogList.Where(i => i.ReservedQuantity > 0).ToList();

                //if (filter.IsChecked == true) return finalCatalogList.Where(i => i.Ferramentarias!.Any(e => e.ReservedQuantity > 0)).ToList();

                if (filter.IsChecked == true)
                {
                    // First filter catalogs that have any reserved ferramentarias
                    var catalogsWithReserved = finalCatalogList
                        .Where(catalog => catalog.Ferramentarias!.Any(f => f.ReservedQuantity > 0))
                        .ToList();

                    // Then filter ferramentarias within those catalogs
                    return catalogsWithReserved.Select(catalog =>
                    {
                        var filteredFerramentarias = catalog.Ferramentarias!
                            .Where(ferramentaria => ferramentaria.ReservedQuantity > 0)
                            .ToList();

                        // Create a new catalog with only the filtered ferramentarias
                        return new CatalogDetail
                        {
                            // Copy all properties...
                            Id = catalog.Id,
                            IdCategoria = catalog.IdCategoria,
                            Codigo = catalog.Codigo,
                            Classe = catalog.Classe,
                            Tipo = catalog.Tipo,
                            Nome = catalog.Nome,
                            PorType = catalog.PorType,
                            ClassType = catalog.ClassType,
                            Quantity = catalog.Quantity,
                            // Recalculate quantities based on filtered ferramentarias
                            OverallQuantity = filteredFerramentarias.Sum(f => f.AvailableQuantity),
                            Ferramentaria = string.Join(", ",
                                filteredFerramentarias
                                    .Where(f => f.AvailableQuantity > 0)
                                    .Select(f => f.Nome)
                                    .Distinct()),
                            Ferramentarias = filteredFerramentarias,
                            ReservedQuantity = filteredFerramentarias.Sum(f => f.ReservedQuantity),
                            allocatedQuantity = filteredFerramentarias.Sum(f => f.AvailableQuantity),
                        };
                    }).ToList();
                }

                return finalCatalogList;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
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

        public async Task<List<ConsultationReserveModel>?> GetReservationForCatalog(int idCatalogo)
        {
            try
            {
                _logger.LogInformation("Processing GetReservationForCatalog with idCatalogo:{IdCatalogo}", idCatalogo);

                if (idCatalogo <= 0) throw new ProcessErrorException("idcatalogo is less than or equal to 0");

                List<ConsultationReserveModel>? reservationResult = await (from reserve in _context.Reservations
                                                                     join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                                     join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                     join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                                     join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                                     where reserve.IdCatalogo == idCatalogo
                                                                     select new ConsultationReserveModel
                                                                     {
                                                                         IdReservation = reserve.Id,
                                                                         Ferramentaria = ferramentaria.Nome,
                                                                         MemberCodPessoa = member.CodPessoa,
                                                                         LeaderCodPessoa = leader.CodPessoa,
                                                                         Quantidade = reserve.Quantidade,
                                                                         OrderNo = reservationControl.Id,
                                                                         ReservationType = reservationControl.TypeString,
                                                                         StatusString = reserve.StatusString,
                                                                         DateReservation = reserve.DataRegistro,
                                                                         DateReservationString = reserve.DataRegistro.HasValue == true ? reserve.DataRegistro.Value.ToShortDateString() : string.Empty,
                                                                     }).ToListAsync();

                if (reservationResult.Count == 0) throw new InvalidOperationException("Nenhum resultado encontrado");

                Dictionary<int, employeeNewInformationModel> funcionarioDict = await _contextBS.Funcionario.Where(f => f.CodPessoa != null)
                                                                           .GroupBy(f => f.CodPessoa)
                                                                           .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                           .ToDictionaryAsync(
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

                var enrichedReservations = reservationResult.Select(r =>
                {
                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new ConsultationReserveModel
                    {
                        IdReservation = r.IdReservation,
                        Ferramentaria = r.Ferramentaria,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        Quantidade = r.Quantidade,
                        OrderNo = r.OrderNo,
                        ReservationType = r.ReservationType,
                        StatusString = r.StatusString,
                        DateReservation = r.DateReservation,
                        DateReservationString = r.DateReservationString,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;


            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
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

    }
}
