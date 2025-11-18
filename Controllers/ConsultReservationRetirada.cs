using AutoMapper.Execution;
using Azure.Core;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using FerramentariaTest.Services;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace FerramentariaTest.Controllers
{
    public class ConsultReservationRetirada : Controller
    {

        protected IHttpContextAccessor httpContextAccessor;

        private readonly ILogger<ConsultReservationRetirada> _logger;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IConsultReservationRetirada _consultReservationRetirada;

        public ConsultReservationRetirada(IHttpContextAccessor httpCA, ILogger<ConsultReservationRetirada> logger, IUserContextService userContext, ICorrelationIdService correlationIdService, IConsultReservationRetirada consultReservationRetirada)
        {
            httpContextAccessor = httpCA;
            _logger = logger;
            _userContext = userContext;
            _correlationIdService = correlationIdService;
            _consultReservationRetirada = consultReservationRetirada;
        }

        //[PageAccessAuthorize("ConsultReservationRetirada.cs")]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> Index()
        {
            try
            {

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Landed on Page: ConsultReservationRetirada.Index", userId);

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

        //[PageAccessAuthorize("ConsultReservationRetirada.cs")]
        [HttpGet]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetReservationEmployee(GestorRRFilterModel? filter)
        {
            try
            {
                if (filter == null) throw new ArgumentException("Filter is null for GetReservationEmployee");

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Attempt to GetReservationEmployee for Employee:{Chapa}", userId, filter.Chapa);

                List<ConsultationReserveModel>? listReservation = await _consultReservationRetirada.GetReservationDetailsByEmployee(filter);

                if (listReservation == null || listReservation.Count == 0) return BadRequest("Nenhum resultado encontrado.");

                return Ok(listReservation);

                //List<ConsultationReserveModel>? listReservation = getReservationDetailsByEmployee(filter) ?? new List<ConsultationReserveModel>();
                //if (listReservation.Count == 0) return Json(new { success = false, message = "Nenhum resultado encontrado." });


                //return Json(new { success = true, listReservation });
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

        //public List<ConsultationReserveModel>? getReservationDetailsByEmployee(GestorRRFilterModel filter)
        //{
        //    List<ConsultationReserveModel>? listReservation = (from reserve in _context.Reservations
        //                                                       join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
        //                                                       join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
        //                                                       join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
        //                                                       join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
        //                                                       join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                                       join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
        //                                                       where member.Chapa == filter.Chapa
        //                                                        && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
        //                                                        && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
        //                                                        && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
        //                                                        && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
        //                                                        && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
        //                                                       select new ConsultationReserveModel
        //                                                       {
        //                                                           Classe = categoria.ClassType,
        //                                                           Codigo = catalogo.Codigo,
        //                                                           itemNome = catalogo.Nome,
        //                                                           MemberCodPessoa = member.CodPessoa,
        //                                                           LeaderCodPessoa = leader.CodPessoa,
        //                                                           Quantidade = reserve.Quantidade,
        //                                                           Ferramentaria = ferramentaria.Nome,
        //                                                           OrderNo = reservationControl.Id,
        //                                                           ReservationType = reservationControl.TypeString,
        //                                                           StatusString = reserve.StatusString,
        //                                                       }).ToList() ?? new List<ConsultationReserveModel>();

        //    if (listReservation.Count == 0) return listReservation;

        //    Dictionary<int, Funcionario?> funcionarioDict = _contextBS.Funcionario.AsEnumerable()
        //                                    .GroupBy(e => e.CodPessoa)
        //                                    .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
        //                                    .Where(f => f != null)  // Filter out nulls if any
        //                                    .ToDictionary(
        //                                        f => f.CodPessoa.Value,   // Key selector
        //                                        f => f              // Value selector
        //                                    );

        //    var enrichedReservations = listReservation.Select(r =>
        //    {
        //        var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
        //        var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

        //        return new ConsultationReserveModel
        //        {
        //            Classe = r.Classe,
        //            Codigo = r.Codigo,
        //            itemNome = r.itemNome,
        //            MemberCodPessoa = r.MemberCodPessoa,
        //            LeaderCodPessoa = r.LeaderCodPessoa,
        //            Quantidade = r.Quantidade,
        //            Ferramentaria = r.Ferramentaria,
        //            OrderNo = r.OrderNo,
        //            ReservationType = r.ReservationType,
        //            StatusString = r.StatusString,
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


        //[PageAccessAuthorize("ConsultReservationRetirada.cs")]
        [HttpGet]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetOrderInformation(int OrderNo)
        {
            try
            {
                if (OrderNo <= 0) throw new ArgumentException("OrderNo is less than or equal to 0 for GetOrderInformation");

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Attempt to GetOrderInformation for OrderNo:{IdReservationControl}", userId, OrderNo);

                ConsultationModel? resultList = await _consultReservationRetirada.GetReservationRetiradaInformation(OrderNo);

                if (resultList == null) throw new InvalidOperationException($"Nenhum resultado encontrado com ControlNo:{OrderNo}"); 

                return Ok(resultList);

                //ConsultationModel? resultList = getGroupReservation(OrderNo);

                //return Json(resultList);
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

        //public ConsultationModel? getGroupReservation(int OrderNo)
        //{

        //    ConsultationModel? consultItems = (from reserve in _context.Reservations
        //                                       join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
        //                                       join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
        //                                       join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
        //                                       join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
        //                                       join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                       join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
        //                                       where reserve.IdReservationControl == OrderNo
        //                                       group new
        //                                       {
        //                                          reserve,
        //                                          reservationControl,
        //                                          leader,
        //                                          member,
        //                                          catalogo,
        //                                          categoria,
        //                                          ferramentaria
        //                                       } by new { reserve.IdReservationControl } into grouped
        //                                       select new ConsultationModel
        //                                       {
        //                                          ControlId = grouped.Key.IdReservationControl,
        //                                          LeaderName = grouped.First().leader.Nome,
        //                                          ControlType = grouped.First().reservationControl.TypeString,
        //                                          ControlStatusString = grouped.First().reservationControl.StatusString,
        //                                          DateRegistration = grouped.First().reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy"),
        //                                          DateExpiration = grouped.First().reservationControl.ExpirationDate.Value.ToString("dd/MM/yyyy"),
        //                                          ReservationList = grouped.Select(g => new ConsultationReserveModel
        //                                          {
        //                                             Classe = g.categoria.ClassType,
        //                                             Codigo = g.catalogo.Codigo,
        //                                             itemNome = g.catalogo.Nome,
        //                                             Requester = g.member.Nome,
        //                                             Ferramentaria = g.ferramentaria.Nome,
        //                                             Quantidade = g.reserve.Quantidade,
        //                                             StatusString = g.reserve.StatusString,
        //                                          }).ToList()
        //                                       }).FirstOrDefault();

        //    return consultItems;
        //}


     


    }
}
