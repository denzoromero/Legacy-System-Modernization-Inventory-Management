using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.EntitiesBS;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.Entities;
using System.Security.Cryptography;
using System.Text;
using AutoMapper.Execution;
using Microsoft.IdentityModel.Tokens;
using FerramentariaTest.Services.Interfaces;
using FerramentariaTest.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using Serilog.Context;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;

namespace FerramentariaTest.Controllers
{
    public class HandoutReservation : BaseController
    {

        protected IHttpContextAccessor httpContextAccessor;


        private readonly ILogger<HandoutReservation> _logger;
        private readonly IAuditLogger _auditLogger;
        private readonly IReservationService _reservationService;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IFerramentariaService _ferramentariaService;
        private readonly IEmployeeService _employeeService;
        private readonly IAuditService _auditService;

        public HandoutReservation(IHttpContextAccessor httpCA, ILogger<HandoutReservation> logger, IReservationService reservationService, IUserContextService userContext, ICorrelationIdService correlationIdService, IFerramentariaService ferramentariaService, IAuditLogger auditLogger, IEmployeeService employeeService, IAuditService auditService) : base(ferramentariaService, logger, correlationIdService)
        {
            httpContextAccessor = httpCA;
            _logger = logger;
            _reservationService = reservationService;
            _userContext = userContext;
            _correlationIdService = correlationIdService;
            _ferramentariaService = ferramentariaService;
            _auditLogger = auditLogger;
            _employeeService = employeeService;
            _auditService = auditService;
        }

        //[PageAccessAuthorize("HandoutReservation.cs")]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> Index()
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: HandoutReservation.Index", user.Id);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null)
                {
                    var ferramentariaItems = _ferramentariaService.GetFerramentariaList(user.Id);

                    if (ferramentariaItems != null)
                    {
                        ViewBag.FerramentariaItems = ferramentariaItems;
                        ViewBag.ViewPage = "PreparationIndex";
                    }

                    return PartialView("_FerramentariaPartialView");

                }

                if (TempData.ContainsKey("ShowSuccessAlertReservation"))
                {
                    ViewBag.SuccessAlertNew = TempData["ShowSuccessAlertReservation"]?.ToString();
                    TempData.Remove("ShowSuccessAlertReservation"); // Remove it from TempData to avoid displaying it again
                }

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

        //[PageAccessAuthorize("HandoutReservation.cs")]
        [OutputCache(Duration = 0, NoStore = true)]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> CompleteReservationPage(string icard)
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: HandoutReservation.CompleteReservationPage with ICARD:{Icard}", user.Id, icard);

                if (string.IsNullOrWhiteSpace(icard)) throw new ArgumentException("Número do crachá esta vazio.");

                fnRetornaColaboradorCracha? badgeCheck = await _employeeService.GetEmployeeCardInfo(icard);
                if (badgeCheck == null || string.IsNullOrEmpty(badgeCheck.MATRICULA)) throw new InvalidOperationException("Número do crachá não encontrado.");

                EmployeeInformationBS employeeInfo = await _employeeService.GetEmployeeInformationBS(badgeCheck.MATRICULA);
                if (employeeInfo == null || employeeInfo.CodPessoa.HasValue == false) throw new InvalidOperationException($"Nenhum funcionário encontrado com matrícula:{badgeCheck.MATRICULA}");

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("IdFerramentaria is null please refresh the page.");

                List<FinalReservationResult>? OrderResult = await _reservationService.GetFinalizedReservation(employeeInfo.CodPessoa.Value, FerramentariaValue.Value);
                if (OrderResult == null || OrderResult.Count == 0) throw new InvalidOperationException("Nenhuma reserva finalizada encontrada");

                TermsControlModel? checkTerms = await _employeeService.CheckTermsControl(badgeCheck.MATRICULA);
                if (checkTerms == null)
                {
                    ViewBag.OpenTermsModal = true;
                    return View(nameof(Index));
                }



                CompleteReservationPageModel model = new CompleteReservationPageModel()
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    FinalOrder = OrderResult
                };


                //ViewBag.ReservationList = OrderResult;

                return View(model);

                //fnRetornaColaboradorCracha? check = checkEmployeeCracha(icard);
                //if (check == null || string.IsNullOrEmpty(check.MATRICULA))
                //{
                //    ViewBag.Error = "ICard No not found on the database.";
                //    return View(nameof(Index));
                //}

                //employeeNewInformationModel entity = searches.searchEmployeeInformation(check.MATRICULA);
                ////employeeNewInformationModel entity = searches.searchEmployeeInformation("36369");
                //if (entity == null || entity.CodPessoa.HasValue == false)
                //{
                //    //ViewBag.Error = $"No employee found with Matricula: {check.MATRICULA}.";
                //    //ViewBag.Error = $"No employee found with Matricula: Matricula.";
                //    ViewBag.Error = $"No employee found with Matricula: {check.MATRICULA}.";
                //    return View(nameof(Index));
                //}

                //List<FinalReservationResult>? order = getFinalReservation(entity.CodPessoa);
                //if (order == null || order.Count == 0)
                //{
                //    //ViewBag.Error = $"No Reservation found for Employee with Matricula: Matricula.";
                //    ViewBag.Error = $"No Reservation found for Employee with Matricula: {check.MATRICULA}.";
                //    return View(nameof(Index));
                //}

                //ViewBag.ReservationList = order;

                //return View();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View(nameof(Index));
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
                return View(nameof(Index));
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
                return View(nameof(Index));
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
                return View(nameof(Index));
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
                return View(nameof(Index));
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
                return View(nameof(Index));
            }

        }

        //[PageAccessAuthorize("HandoutReservation.cs")]
        [OutputCache(Duration = 0, NoStore = true)]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> FinalizeHandoutReservation(List<FinalSubmissionProcess> FinalProcessList, string CrachaNumber, string TransactionId)   
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                //using var _ = LogContext.PushProperty("FinalHandoutReservation", FinalProcessList, destructureObjects: true);
                //{
                //    _logger.LogInformation("User:{UserId} - Attempt to FinalizeHandoutReservation for CrachaNumber:{CrachaNumber} with TransactionId:{TransactionId}", user.Id, CrachaNumber, TransactionId);
                //}

                using (LogContext.PushProperty("FinalHandoutReservation", FinalProcessList, destructureObjects: true))
                {
                    _logger.LogInformation("User:{UserId} - Attempt to FinalizeHandoutReservation for CrachaNumber:{CrachaNumber} with TransactionId:{TransactionId}", user.Id, CrachaNumber, TransactionId);
                }

                if (string.IsNullOrWhiteSpace(TransactionId)) throw new ArgumentException("TransactionId is empty.");

                if (string.IsNullOrWhiteSpace(CrachaNumber)) throw new ArgumentException("Número do crachá esta vazio.");

                if (FinalProcessList == null || FinalProcessList.Count == 0) throw new InvalidOperationException($"{_correlationIdService.GetCurrentCorrelationId()} - FinalProcessList is empty.");

                if (FinalProcessList.All(i => i.IsSelected == false)) throw new InvalidOperationException("Selecione pelo menos 1 item.");

                fnRetornaColaboradorCracha? badgeCheck = await _employeeService.GetEmployeeCardInfo(CrachaNumber);
                if (badgeCheck == null || string.IsNullOrEmpty(badgeCheck.MATRICULA)) throw new InvalidOperationException("Número do crachá não encontrado.");

                EmployeeInformationBS employeeInfo = await _employeeService.GetEmployeeInformationBS(badgeCheck.MATRICULA);
                if (employeeInfo == null || employeeInfo.CodPessoa.HasValue == false) throw new InvalidOperationException($"Nenhum funcionário encontrado com matrícula:{badgeCheck.MATRICULA}");

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("IdFerramentaria is null please refresh the page.");


                FinalProcessList = FinalProcessList.FindAll(i => i.IsSelected == true);

                string CrachaType = badgeCheck.TIPOCRAC == 1 ? "P" : "T";

                string CrachaNo = $"{CrachaType}-{CrachaNumber}";

                FinalProcessList.ForEach(i => i.CrachaNo = CrachaNo);

                combineFixModel combined = new combineFixModel()
                {
                    TransactionId = TransactionId,
                    CrachaNumber = CrachaNumber,
                    Balconista = user,
                    CrachaInformation = badgeCheck,
                    Employee = employeeInfo,
                };

                FinalAuditResultModel auditResultModel = await _auditService.MakeAuditModel(FinalProcessList, combined);

                Result result = await _reservationService.FinalizeProcessHandoutReservation(FinalProcessList, TransactionId, user.Id, FerramentariaValue.Value);

                if (!result.IsSuccess) throw new InvalidOperationException(result.Error);

                //using var logContext = LogContext.PushProperty("HandedoutReservation", FinalProcessList, destructureObjects: true);
                using (LogContext.PushProperty("FinalSubmissionProcess", FinalProcessList, destructureObjects: true))
                {
                    _logger.LogInformation("User:{UserId} - Successfully Handedout Reservations with TransactionId: {TransactionId}", user.Id, TransactionId);
                }

                using (LogContext.PushProperty("FinalAuditResultModel", auditResultModel, destructureObjects: true))
                {
                    _auditLogger.LogAuditTransaction(user.Id, $"User:{user.Nome} successfully handedout {FinalProcessList.Count} Reserved Item/s with TransactionId: {TransactionId} .", "FinalizeHandoutReservation", "Success", TransactionId);
                }
                 
                return Ok(new { success = true, message = "A transação foi concluída." });

                //using (var transaction = _context.Database.BeginTransaction())
                //{
                //    try
                //    {

                //        foreach (FinalSubmissionProcess item in SelectedItem)
                //        {
                //            Reservations? reserved = _context.Reservations.FirstOrDefault(i => i.Id == item.IdReservation);
                //            if (reserved == null)
                //            {
                //                ViewBag.Error = $"Cant find reservation with the Id:{item.IdReservation}";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            ProductReservation? productReservation = _context.ProductReservation.FirstOrDefault(i => i.Id == item.IdProductReservation);
                //            if (productReservation == null)
                //            {
                //                ViewBag.Error = $"Cant find product reservation with the Id:{item.IdProductReservation}";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                //            if (productToUpdate == null || productToUpdate.Quantidade < item.QtyRequested)
                //            {
                //                ViewBag.Error = $"Insufficient stock for Product ID {item.IdProduto}. Available: {productToUpdate?.Quantidade ?? 0}.";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            HistoricoAlocacao_2025 historico = new HistoricoAlocacao_2025
                //            {
                //                IdProduto = item.IdProduto,
                //                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                //                Solicitante_CodColigada = item.CodColigadaSolicitante,
                //                Solicitante_Chapa = item.ChapaSolicitante,
                //                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                //                Liberador_CodColigada = item.CodColigadaLiberador,
                //                Liberador_Chapa = item.ChapaLiberador,
                //                Balconista_Emprestimo_IdLogin = loggedUser?.Id,
                //                Balconista_Devolucao_IdLogin = loggedUser?.Id,
                //                Observacao = $"Reservation: {productReservation.Observacao}",
                //                DataEmprestimo = DateTime.Now,
                //                DataDevolucao = productReservation.DataPrevistaDevolucao ?? DateTime.Now,
                //                IdObra = item.IdObra,
                //                Quantidade = productReservation.FinalQuantity,
                //                IdFerrOndeProdRetirado = FerramentariaValue,
                //                IdControleCA = productReservation.IdControleCA,
                //                IdReservation = item.IdReservation
                //            };

                //            _context.Add(historico);

                //            productToUpdate.Quantidade -= productReservation.FinalQuantity;
                //            _context.Update(productToUpdate);

                //            reserved.Status = 3;
                //            _context.Update(reserved);

                //            productReservation.Status = 3;
                //            productReservation.HandedBy = loggedUser?.Id;
                //            _context.Update(productReservation);

                //        }

                //        _context.SaveChanges();
                //        transaction.Commit();

                //        TempData["ShowSuccessAlertReservation"] = "Success";
                //        return RedirectToAction(nameof(Index));

                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        ViewBag.Error = $"SERVER ERROR: {ex.Message}";
                //        return View(nameof(Index));
                //    }
                //}

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


        #region Javascript

        //[HttpPost]
        //public IActionResult verifyCracha(string icard, int codPessoa)
        //{
        //    fnRetornaColaboradorCracha? verifyEmployee = _contextBS.GetColaboradorCracha(icard).FirstOrDefault();
        //    if (verifyEmployee != null)
        //    {
        //        if (verifyEmployee.MATRICULA != null)
        //        {
        //            employeeNewInformationModel? member = searches.searchEmployeeInformationUsingCodPessoa(codPessoa);

        //            if (verifyEmployee.MATRICULA == member.Chapa)
        //            {
        //                return Json(new { success = true, member });
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Matricula from ID Card is different from reserved item." });
        //            }
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "ICARD of the Employee is not found. Please report to IT." });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "ICARD of the Employee is not found." });
        //    }
        //}

        public async Task<IActionResult> AddTermsControl(string icard)
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - AddTermsControl with ICARD:{Icard}", user.Id, icard);

                fnRetornaColaboradorCracha? badgeCheck = await _employeeService.GetEmployeeCardInfo(icard);
                if (badgeCheck == null || string.IsNullOrEmpty(badgeCheck.MATRICULA)) throw new InvalidOperationException("Número do crachá não encontrado.");

                EmployeeInformationBS employeeInfo = await _employeeService.GetEmployeeInformationBS(badgeCheck.MATRICULA);
                if (employeeInfo == null || employeeInfo.CodPessoa.HasValue == false) throw new InvalidOperationException($"Nenhum funcionário encontrado com matrícula:{badgeCheck.MATRICULA}");

                TermsControlModel? checkTerms = await _employeeService.CheckTermsControl(badgeCheck.MATRICULA);
                if (checkTerms != null) throw new ArgumentException("Already Signed the Terms and Condition");

                string TransactionId = Guid.NewGuid().ToString();

                TermsControl terms = new TermsControl()
                {
                    Balconista = user.Id,
                    Chapa = badgeCheck.MATRICULA,
                    TransactionId = TransactionId,
                    DataRegistro = DateTime.Now
                };

                Result result = await _employeeService.AddToTermsControl(terms);

                if (!result.IsSuccess) throw new InvalidOperationException(result.Error);

                using var logContext = LogContext.PushProperty("TermsControl", terms, destructureObjects: true);
                _logger.LogInformation("User:{UserId} - Successfully Handedout Reservations with TransactionId: {TransactionId}", user.Id, TransactionId);
                _auditLogger.LogAuditTransaction(user.Id, $"Employee:{badgeCheck.NOME} successfully signed Terms and Condition initiated by User:{user.Nome} with TransactionId:{TransactionId} .", "AddTermsControl", "Success", TransactionId);

                return Ok(new { success = true, message = "A transação foi concluída." });

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

        #endregion


        #region Functions

        //public fnRetornaColaboradorCracha checkEmployeeCracha(string Icard)
        //{
        //    fnRetornaColaboradorCracha? verifyEmployee = _contextBS.GetColaboradorCracha(Icard).FirstOrDefault();

        //    return verifyEmployee ?? new fnRetornaColaboradorCracha();
        //}

        //public List<FinalReservationResult>? getFinalReservation(int? codpessoa)
        //{
        //    int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

        //    List<FinalReservationResult>? reservedproducts = (from finalproduct in _context.ProductReservation
        //                                                         join reservations in _context.Reservations on finalproduct.IdReservation equals reservations.Id
        //                                                         join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
        //                                                         join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
        //                                                         join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
        //                                                         join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
        //                                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                                         join obra in _context.Obra on reservations.IdObra equals obra.Id
        //                                                         where control.Type == 1
        //                                                             && finalproduct.Status == 0
        //                                                             && reservations.Status == 2
        //                                                             && reservations.IdFerramentaria == FerramentariaValue
        //                                                             && member.CodPessoa == codpessoa
        //                                                         select new FinalReservationResult
        //                                                         {
        //                                                             IdProductReservation = finalproduct.Id,
        //                                                             IdReservationControl = reservations.IdReservationControl,
        //                                                             IdReservation = reservations.Id,
        //                                                             IdProduto = finalproduct.IdProduto,
        //                                                             IdObra = reservations.IdObra,
        //                                                             intClasse = categoria.Classe,
        //                                                             Classe = categoria.ClassType,
        //                                                             Type = catalogo.PorType,
        //                                                             Codigo = catalogo.Codigo,
        //                                                             Nome = catalogo.Nome,
        //                                                             QtyFinal = finalproduct.FinalQuantity,
        //                                                             DateReturn = finalproduct.DataPrevistaDevolucao.HasValue == true ? finalproduct.DataPrevistaDevolucao.Value.ToString("dd/MM/yyyy") : string.Empty,
        //                                                             DateReturnProper = finalproduct.DataPrevistaDevolucao,
        //                                                             Observacao = finalproduct.Observacao,
        //                                                             MemberCodPessoa = member.CodPessoa,
        //                                                             LeaderCodPessoa = leader.CodPessoa,
        //                                                         }
        //                                                        ).ToList();


        //    if (reservedproducts.Count == 0)
        //    {
        //        return new List<FinalReservationResult>();
        //    }


        //    Dictionary<int, Funcionario?> funcionarioDict = _contextBS.Funcionario.AsEnumerable()
        //                                                        .GroupBy(e => e.CodPessoa)
        //                                                        .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
        //                                                        .Where(f => f != null)  // Filter out nulls if any
        //                                                        .ToDictionary(
        //                                                            f => f.CodPessoa.Value,   // Key selector
        //                                                            f => f              // Value selector
        //                                                        );

        //    var enrichedReservations = reservedproducts.Select(r =>
        //    {
        //        // Get member and leader information from dictionary
        //        var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
        //        var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

        //        return new FinalReservationResult
        //        {
        //            IdProductReservation = r.IdProductReservation,
        //            IdReservationControl = r.IdReservationControl,
        //            IdReservation = r.IdReservation,
        //            IdProduto = r.IdProduto,
        //            IdObra = r.IdObra,
        //            intClasse = r.intClasse,
        //            Classe = r.Classe,
        //            Type = r.Type,
        //            Codigo = r.Codigo,
        //            Nome = r.Nome,
        //            QtyFinal = r.QtyFinal,
        //            DateReturn = r.DateReturn,
        //            DateReturnProper = r.DateReturnProper,
        //            Observacao = r.Observacao,
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


        //    return enrichedReservations ?? new List<FinalReservationResult>();


        //}


        #endregion


        #region Ferramentaria Actions

        //public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome)
        //{
        //    var currentController = RouteData.Values["controller"]?.ToString();

        //    if (Ferramentaria != null)
        //    {
        //        httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
        //        httpContextAccessor.HttpContext.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        //public ActionResult RefreshFerramentaria()
        //{
        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);
        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.FerramentariaNome);
        //    return RedirectToAction(nameof(Index));
        //}

        #endregion

    }
}
