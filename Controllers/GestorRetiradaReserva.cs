using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.Services;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace FerramentariaTest.Controllers
{
    public class GestorRetiradaReserva : Controller
    {

        protected IHttpContextAccessor httpContextAccessor;

        private readonly ILogger<GestorRetiradaReserva> _logger;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IConsultReservationRetirada _consultReservationRetirada;


        public GestorRetiradaReserva(IHttpContextAccessor httpCA,ILogger<GestorRetiradaReserva> logger, IUserContextService userContext, ICorrelationIdService correlationIdService, IConsultReservationRetirada consultReservationRetirada)
        {
            httpContextAccessor = httpCA;
            _logger = logger;
            _userContext = userContext;
            _correlationIdService = correlationIdService;
            _consultReservationRetirada = consultReservationRetirada;
        }

        //[PageAccessAuthorize("GestorRetiradaReserva.cs")]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> Index()
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: GestorRetiradaReserva.Index", user.Id);

                return View();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
                return View();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
                return View();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
                return View();
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
                return View();
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction(actionName: nameof(HomeController.Login), controllerName: nameof(HomeController).Replace("Controller", ""),
                    new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred";
                return View();
            }
        }

        //[PageAccessAuthorize("GestorRetiradaReserva.cs")]
        [HttpGet]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetGestorReservationRetirada(GestorRRFilterModel? filter)
        {
            try
            {
                if (filter == null) throw new ArgumentException("Filter is null for GetReservationEmployee");

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to GetReservationEmployee for Employee:{Chapa}", user.Id, filter.Chapa);

                List<CatalogDetail>? result = await _consultReservationRetirada.GetGestorListInformation(filter);

                if (result == null || result.Count == 0) return BadRequest("Nenhum resultado encontrado.");

                return Ok(result);

                //List<CatalogDetail>? result = await SearchItemListAsync(test);

                ////var sampleData = new { success = true, message = "Data retrieved successfully" };
                //return Json(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }     
        }


        //private async Task<List<CatalogDetail>?> SearchItemListAsync(GestorRRFilterModel? filter)
        //{

        //    List<CatalogDetail>? CatalogList = await (from produto in _context.Produto
        //                                              join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
        //                                              join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                                              join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                              join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
        //                                              where
        //                                                 catalogo.Ativo == 1
        //                                              && produto.Ativo == 1
        //                                              && produto.IdFerramentaria != 17
        //                                              && produto.Quantidade > 0
        //                                              && categoria.Ativo == 1
        //                                              && ferramentaria.Ativo == 1
        //                                              //&& categoria.Classe == IdCatalog
        //                                              && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
        //                                              && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
        //                                              && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
        //                                              && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
        //                                              && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
        //                                              && !catalogo.Nome.Contains("INUTILIZAR") // Exclude items with "INUTILIZAR" in Nome
        //                                              select new CatalogDetail
        //                                              {
        //                                                  Id = catalogo.Id,
        //                                                  IdCategoria = catalogo.IdCategoria,
        //                                                  Codigo = catalogo.Codigo,
        //                                                  Classe = categoria.Nome,
        //                                                  //Tipo = _context.Categoria.Where(cat => cat.Id == categoria.IdCategoria).Select(cat => cat.Nome).FirstOrDefault(),
        //                                                  Tipo = categoriaPai.Nome,
        //                                                  Nome = catalogo.Nome,
        //                                                  PorType = catalogo.PorType,
        //                                                  ClassType = categoria.ClassType,
        //                                                  Quantity = produto.Quantidade, // Adjusted quantity
        //                                                  IdFerramentaria = ferramentaria.Id,
        //                                                  Ferramentaria = ferramentaria.Nome,
        //                                                  Ferramentarias = new List<FerramentariaStockModel>
        //                                                                            {
        //                                                                                new FerramentariaStockModel
        //                                                                                {
        //                                                                                    Id = ferramentaria.Id,
        //                                                                                    Nome = ferramentaria.Nome,
        //                                                                                    Quantity = produto.Quantidade, // Raw quantity before grouping
        //                                                                                }
        //                                                                            }
        //                                              }).ToListAsync();

        //    List<CatalogDetail>? GroupedCatalogList = CatalogList
        //                                               .GroupBy(x => x.Id)
        //                                               .Select(group => new CatalogDetail
        //                                               {
        //                                                   Id = group.Key,
        //                                                   IdCategoria = group.First().IdCategoria,
        //                                                   Codigo = group.First().Codigo,
        //                                                   Classe = group.First().Classe,
        //                                                   Tipo = group.First().Tipo,
        //                                                   Nome = group.First().Nome,
        //                                                   PorType = group.First().PorType,
        //                                                   ClassType = group.First().ClassType,
        //                                                   Quantity = group.Sum(x => x.Quantity),
        //                                                   Ferramentarias = group
        //                                                                   .SelectMany(x => x.Ferramentarias)
        //                                                                   .GroupBy(f => f.Id)
        //                                                                   .Select(fGroup => new FerramentariaStockModel
        //                                                                   {
        //                                                                       Id = fGroup.Key,
        //                                                                       Nome = fGroup.First().Nome,
        //                                                                       Quantity = fGroup.Sum(f => f.Quantity),
        //                                                                   })
        //                                                                   .ToList(),
        //                                               }).ToList();

        //    var reservedQuantities = await _context.Reservations
        //                                .Where(r => r.Status != 7 && r.Status != 8 && r.Status != 3)
        //                                .GroupBy(r => new { r.IdCatalogo, r.IdFerramentaria })
        //                                .Select(g => new
        //                                {
        //                                    g.Key.IdCatalogo,
        //                                    g.Key.IdFerramentaria,
        //                                    TotalReserved = g.Sum(x => x.Quantidade)
        //                                })
        //                                .ToDictionaryAsync(x => new { x.IdCatalogo, x.IdFerramentaria }, x => x.TotalReserved);

        //    var finalCatalogList = GroupedCatalogList.Select(catalog =>
        //    {
        //        // Update quantities for each location
        //        var updatedFerramentarias = catalog.Ferramentarias.Select(ferramentaria =>
        //        {
        //            var key = new
        //            {
        //                IdCatalogo = catalog.Id,
        //                IdFerramentaria = ferramentaria.Id  // Changed from 'Id' to 'IdFerramentaria'
        //            };
        //            var reservedQty = reservedQuantities.TryGetValue(key, out var reserved) ? reserved : 0;

        //            return new FerramentariaStockModel
        //            {
        //                Id = ferramentaria.Id,
        //                Nome = ferramentaria.Nome,
        //                Quantity = ferramentaria.Quantity,
        //                ReservedQuantity = reservedQty,
        //                AvailableQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
        //                ferramentariaAllocatedQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
        //            };
        //        }).ToList();

        //        return new CatalogDetail
        //        {
        //            Id = catalog.Id,
        //            IdCategoria = catalog.IdCategoria,
        //            Codigo = catalog.Codigo,
        //            Classe = catalog.Classe,
        //            Tipo = catalog.Tipo,
        //            Nome = catalog.Nome,
        //            PorType = catalog.PorType,
        //            ClassType = catalog.ClassType,
        //            Quantity = catalog.Quantity,
        //            OverallQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
        //            Ferramentaria = string.Join(", ",
        //                                                updatedFerramentarias
        //                                                    .Where(f => f.AvailableQuantity > 0)
        //                                                    .Select(f => f.Nome)
        //                                                    .Distinct()),
        //            Ferramentarias = updatedFerramentarias,
        //            ReservedQuantity = updatedFerramentarias.Sum(f => f.ReservedQuantity),
        //            allocatedQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
        //        };
        //    }).ToList();

        //    if (filter.IsChecked == true) return finalCatalogList.Where(i => i.ReservedQuantity > 0).ToList();





        //    return finalCatalogList;

        //    //return CatalogList ?? new List<CatalogDetail>();
        //}


        //[PageAccessAuthorize("GestorRetiradaReserva.cs")]
        [HttpGet]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetReservationDetails(int IdCatalogo)
        {
            try
            {
                if (IdCatalogo <= 0) throw new ArgumentException("Filter is null for GetReservationEmployee");

                List<ConsultationReserveModel>? reservationResult = await _consultReservationRetirada.GetReservationForCatalog(IdCatalogo);

                if (reservationResult == null || reservationResult.Count == 0) return BadRequest("Nenhum resultado encontrado.");

                return Ok(reservationResult);

                //List<ConsultationReserveModel>? reservationResult = GetReservationForCatalog(IdCatalogo);

                //if (reservationResult.Count == 0) return Json(new { success = false, message = $"No Reservation found." });


                //return Json(new { success = true, reservationResult });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }  
        }

        //public List<ConsultationReserveModel> GetReservationForCatalog(int IdCatalogo)
        //{
        //    List<ConsultationReserveModel>? reservationResult = (from reserve in _context.Reservations
        //                                                         join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
        //                                                         join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
        //                                                         join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
        //                                                         join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
        //                                                         where reserve.IdCatalogo == IdCatalogo
        //                                                         select new ConsultationReserveModel
        //                                                         {
        //                                                             IdReservation = reserve.Id,
        //                                                             Ferramentaria = ferramentaria.Nome,
        //                                                             MemberCodPessoa = member.CodPessoa,
        //                                                             LeaderCodPessoa = leader.CodPessoa,
        //                                                             Quantidade = reserve.Quantidade,
        //                                                             OrderNo = reservationControl.Id,
        //                                                             ReservationType = reservationControl.TypeString,
        //                                                             StatusString = reserve.StatusString,
        //                                                             DateReservation = reserve.DataRegistro,
        //                                                             DateReservationString = reserve.DataRegistro.Value.ToShortDateString(),
        //                                                         }).ToList();

        //    if (reservationResult.Count == 0) return new List<ConsultationReserveModel>();


        //    Dictionary<int, Funcionario?> funcionarioDict = _contextBS.Funcionario.AsEnumerable()
        //                                                 .GroupBy(e => e.CodPessoa)
        //                                                 .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
        //                                                 .Where(f => f != null)  // Filter out nulls if any
        //                                                 .ToDictionary(
        //                                                     f => f.CodPessoa.Value,   // Key selector
        //                                                     f => f              // Value selector
        //                                                 );

        //    var enrichedReservations = reservationResult.Select(r =>
        //    {
        //        var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
        //        var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

        //        return new ConsultationReserveModel
        //        {
        //            IdReservation = r.IdReservation,
        //            Ferramentaria = r.Ferramentaria,
        //            MemberCodPessoa = r.MemberCodPessoa,
        //            LeaderCodPessoa = r.LeaderCodPessoa,
        //            Quantidade = r.Quantidade,
        //            OrderNo = r.OrderNo,
        //            ReservationType = r.ReservationType,
        //            StatusString = r.StatusString,
        //            DateReservation = r.DateReservation,
        //            DateReservationString = r.DateReservationString,
        //            MemberInfo = new employeeNewInformationModel()
        //            {
        //                Chapa = memberInfo.Chapa,
        //                Nome = memberInfo.Nome,
        //                CodSituacao = memberInfo.CodSituacao,
        //                CodColigada = memberInfo.CodColigada,
        //                Funcao = memberInfo.Funcao,
        //                Secao = memberInfo.Secao,
        //                CodPessoa = memberInfo.CodPessoa,
        //            },
        //            LeaderInfo = new employeeNewInformationModel()
        //            {
        //                Chapa = leaderInfo.Chapa,
        //                Nome = leaderInfo.Nome,
        //                CodSituacao = leaderInfo.CodSituacao,
        //                CodColigada = leaderInfo.CodColigada,
        //                Funcao = leaderInfo.Funcao,
        //                Secao = leaderInfo.Secao,
        //                CodPessoa = leaderInfo.CodPessoa,
        //            },
        //        };
        //    }).ToList();

        //    return enrichedReservations;

        //}



    }
}
