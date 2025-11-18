using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Security.Policy;
using Microsoft.IdentityModel.Tokens;
using FerramentariaTest.Services;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using Serilog.Context;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;


namespace FerramentariaTest.Controllers
{
    public class HandoutRetirada : BaseController
    {

        protected IHttpContextAccessor httpContextAccessor;

        private readonly ILogger<HandoutRetirada> _logger;
        private readonly IAuditLogger _auditLogger;
        private readonly IReservationService _reservationService;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IFerramentariaService _ferramentariaService;
        private readonly IEmployeeService _employeeService;
        private readonly IRetiradaService _retiradaService;

        private readonly IAuditService _auditService;

        public HandoutRetirada(IHttpContextAccessor httpCA, ILogger<HandoutRetirada> logger, IRetiradaService retiradaService, IReservationService reservationService, IUserContextService userContext, ICorrelationIdService correlationIdService, IFerramentariaService ferramentariaService, IAuditLogger auditLogger, IEmployeeService employeeService, IAuditService auditService) : base(ferramentariaService, logger, correlationIdService)
        {
            httpContextAccessor = httpCA;
            _logger = logger;
            _reservationService = reservationService;
            _userContext = userContext;
            _correlationIdService = correlationIdService;
            _ferramentariaService = ferramentariaService;
            _auditLogger = auditLogger;
            _employeeService = employeeService;
            _retiradaService = retiradaService;
            _auditService = auditService;
        }


        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [Authorize(Roles = "Demo")]
        public async  Task<IActionResult> Index()
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: HandoutRetirada.Index", user.Id);

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
               
                if (TempData.ContainsKey("ShowSuccessAlertRetirada"))
                {
                    ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlertRetirada"]?.ToString();
                    TempData.Remove("ShowSuccessAlertRetirada"); // Remove it from TempData to avoid displaying it again
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

        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [OutputCache(Duration = 0, NoStore = true)]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> RetiradaOrderPage(string icard)
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: HandoutRetirada.RetiradaOrderPage with ICARD:{icard}", user.Id, icard);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) return RedirectToAction(nameof(Index), "HandoutRetirada");

                if (string.IsNullOrWhiteSpace(icard)) throw new ArgumentException("Número do crachá esta vazio.");

                fnRetornaColaboradorCracha? badgeCheck = await _employeeService.GetEmployeeCardInfo(icard);
                if (badgeCheck == null || string.IsNullOrEmpty(badgeCheck.MATRICULA)) throw new InvalidOperationException("Número do crachá não encontrado.");

                EmployeeInformationBS employeeInfo = await _employeeService.GetEmployeeInformationBS(badgeCheck.MATRICULA);
                if (employeeInfo == null || employeeInfo.CodPessoa.HasValue == false) throw new InvalidOperationException($"Nenhum funcionário encontrado com matrícula:{badgeCheck.MATRICULA}");

                List<newCatalogInformationModel>? OrderResult = await _retiradaService.GetRetiradaOrders(employeeInfo.CodPessoa.Value, FerramentariaValue.Value);
                if (OrderResult == null || OrderResult.Count == 0) throw new InvalidOperationException("Nenhuma retirada encontrada");

                TermsControlModel? checkTerms = await _employeeService.CheckTermsControl(badgeCheck.MATRICULA);
                if (checkTerms == null)
                {
                    ViewBag.OpenTermsModal = true;
                    return View(nameof(Index));
                }

                //TermsControlModel? checkTerms = await _employeeService.CheckTermsControl(badgeCheck.MATRICULA);
                //if (checkTerms == null)
                //{
                //    ViewBag.OpenTermsModal = true;
                //    return View(nameof(Index));
                //}


                //List<newCatalogInformationModel>? order = await _retiradaService.GetRetiradaOrders(employeeInfo.CodPessoa.Value, FerramentariaValue.Value);

                RetiradaOrderPageModel model = new RetiradaOrderPageModel()
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    RetiradaOrder = OrderResult
                };


                //List<newCatalogInformationModel>? order = getRetiradaOrders(entity.CodPessoa);
                //if (order == null || order.Count == 0)
                //{
                //    //ViewBag.Error = $"No Retirada found with Matricula: {check.MATRICULA}.";
                //    ViewBag.Error = $"No Retirada found with Matricula:  {check.MATRICULA}.";
                //    return View(nameof(Index));
                //}


                //ViewBag.RetiradaList = order;

                return View(model);
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

        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [OutputCache(Duration = 0, NoStore = true)]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> FinalizeRetirada(List<FinalSubmissionProcess> FinalProcessList, string? CrachaNumber, string? TransactionId)        
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                using (LogContext.PushProperty("FinalHandoutRetirada", FinalProcessList, destructureObjects: true))
                {
                    _logger.LogInformation("User:{UserId} - Attempt to FinalizeRetirada for CrachaNumber:{CrachaNumber} with TransactionId:{TransactionId}", user.Id, CrachaNumber, TransactionId);
                }

                if (string.IsNullOrWhiteSpace(TransactionId)) throw new ArgumentException("TransactionId is empty.");
                if (string.IsNullOrWhiteSpace(CrachaNumber)) throw new ArgumentException("Número do crachá esta vazio.");
                if (FinalProcessList == null || FinalProcessList.Count == 0) throw new InvalidOperationException($"{_correlationIdService.GetCurrentCorrelationId()} - FinalProcessList is empty.");

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("IdFerramentaria is null please refresh the page.");

                fnRetornaColaboradorCracha? badgeCheck = await _employeeService.GetEmployeeCardInfo(CrachaNumber);
                if (badgeCheck == null || string.IsNullOrEmpty(badgeCheck.MATRICULA)) throw new InvalidOperationException("Número do crachá não encontrado.");

                EmployeeInformationBS employeeInfo = await _employeeService.GetEmployeeInformationBS(badgeCheck.MATRICULA);
                if (employeeInfo == null || employeeInfo.CodPessoa.HasValue == false) throw new InvalidOperationException($"Nenhum funcionário encontrado com matrícula:{badgeCheck.MATRICULA}");

                string CrachaType = badgeCheck.TIPOCRAC == 1 ? "P" : "T";

                string CrachaNo = $"{CrachaType}-{CrachaNumber}";


                combineFixModel combined = new combineFixModel()
                {
                    TransactionId = TransactionId,
                    CrachaNumber = CrachaNumber,
                    Balconista = user,
                    CrachaInformation = badgeCheck,
                    Employee = employeeInfo,
                };


                FinalProcessList.ForEach(i => i.CrachaNo = CrachaNo);


                FinalAuditResultModel auditResultModel = await _auditService.MakeAuditModel(FinalProcessList, combined);

                Result result = await _retiradaService.FinalizeProcessHandoutRetirada(FinalProcessList, TransactionId, user.Id, FerramentariaValue.Value);

                if (!result.IsSuccess) throw new InvalidOperationException(result.Error);

                //using var logContext = LogContext.PushProperty("HandedoutRetirada", FinalProcessList, destructureObjects: true);
                using (LogContext.PushProperty("FinalSubmissionProcess", FinalProcessList, destructureObjects: true))
                {
                    _logger.LogInformation("User:{UserId} - Successfully Handedout Retirada with TransactionId: {TransactionId}", user.Id, TransactionId);
                }

                using (LogContext.PushProperty("FinalAuditResultModel", auditResultModel, destructureObjects: true))
                {
                    _auditLogger.LogAuditTransaction(user.Id, $"User:{user.Nome} successfully handedout {FinalProcessList.Count} Retirada Item/s with TransactionId: {TransactionId} .", "FinalizeHandoutRetirada", "Success", TransactionId);
                } 

                return Ok(new { success = true, message = "A transação foi concluída." });



                //LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                //if (loggedUser == null)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });

                //PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                //if (checkPermission == null)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });

                //if (checkPermission.Inserir != 1)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });

                //if (CrachaNumber.IsNullOrEmpty())
                //{
                //    ViewBag.Error = "CrachaNumber is empty.";
                //    return View(nameof(Index));
                //}

                //fnRetornaColaboradorCracha? check = checkEmployeeCracha(CrachaNumber);
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
                //    ViewBag.Error = $"No employee found with Matricula: {check.MATRICULA}.";
                //    return View(nameof(Index));
                //}

                //List<newCatalogInformationModel>? order = getRetiradaOrders(entity.CodPessoa);
                //if (order == null || order.Count == 0)
                //{
                //    ViewBag.Error = $"No Retirada found with Matricula: {check.MATRICULA}.";
                //    return View(nameof(Index));
                //}

                //int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

                //List<CommonDataProduct>? CommonProductList = new List<CommonDataProduct>();

                //foreach (FinalSubmissionProcess item in FinalProcessList)
                //{
                //    CommonDataProduct common = new CommonDataProduct()
                //    {
                //        Classe = item.Classe,
                //        CatalogoType = item.PorType,
                //        IdReservation = item.IdReservation,
                //        IdProduto = item.IdProduto,
                //        Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                //        Solicitante_CodColigada = item.CodColigadaSolicitante,
                //        Solicitante_Chapa = item.ChapaSolicitante,
                //        Liberador_IdTerceiro = item.IdTerceiroLiberador,
                //        Liberador_CodColigada = item.CodColigadaLiberador,
                //        Liberador_Chapa = item.ChapaLiberador,
                //        Observacao = item.Observacao,
                //        DataEmprestimo = DateTime.Now,
                //        DataPrevistaDevolucao = item.DateReturn,
                //        IdObra = item.IdObra,
                //        Quantidade = item.QtyRequested,
                //        IdFerrOndeProdRetirado = FerramentariaValue,
                //        IdControleCA = item.IdControleCA,
                //    };

                //    CommonProductList.Add(common);
                //}


                //using (var transaction = _context.Database.BeginTransaction())
                //{
                //    try
                //    {
                //        foreach (CommonDataProduct product in CommonProductList)
                //        {
                //            Reservations? reserved = _context.Reservations.FirstOrDefault(i => i.Id == product.IdReservation);
                //            if (reserved == null)
                //            {
                //                ViewBag.Error = $"Cant find reservation with the Id:{product.IdReservation}";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == product.IdProduto);
                //            if (productToUpdate == null || productToUpdate.Quantidade < product.Quantidade)
                //            {
                //                ViewBag.Error = $"Insufficient stock for Product ID {product.IdProduto}. Available: {productToUpdate?.Quantidade ?? 0}.";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            if (product.Classe == 3)
                //            {
                //                HistoricoAlocacao_2025 historico = new HistoricoAlocacao_2025
                //                {
                //                    IdProduto = product.IdProduto,
                //                    Solicitante_IdTerceiro = product.Solicitante_IdTerceiro,
                //                    Solicitante_CodColigada = product.Solicitante_CodColigada,
                //                    Solicitante_Chapa = product.Solicitante_Chapa,
                //                    Liberador_IdTerceiro = product.Liberador_IdTerceiro,
                //                    Liberador_CodColigada = product.Liberador_CodColigada,
                //                    Liberador_Chapa = product.Liberador_Chapa,
                //                    Balconista_Emprestimo_IdLogin = loggedUser?.Id,
                //                    Balconista_Devolucao_IdLogin = loggedUser?.Id,
                //                    Observacao = $"Retirada: {product.Observacao}",
                //                    DataEmprestimo = DateTime.Now,
                //                    DataDevolucao = product.DataPrevistaDevolucao ?? DateTime.Now,
                //                    IdObra = product.IdObra,
                //                    Quantidade = product.Quantidade,
                //                    IdFerrOndeProdRetirado = FerramentariaValue,
                //                    IdControleCA = product.IdControleCA,
                //                    IdReservation = product.IdReservation
                //                };

                //                _context.Add(historico);
                //            }
                //            else
                //            {
                //                string key = $"{product.IdProduto}-{product.Solicitante_CodColigada}-{product.Solicitante_Chapa}-{loggedUser?.Id}-{DateTime.Now:dd/MM/yyyy HH:mm}-{product.IdObra}-{product.Quantidade}-{FerramentariaValue}";
                //                string hash = GenerateMD5Hash(key);

                //                // Check for duplicates if PorAferido or PorSerial
                //                if (product.CatalogoType == "PorAferido" || product.CatalogoType == "PorSerial")
                //                {
                //                    var existingAllocation = _context.ProdutoAlocado.Any(i => i.IdProduto == product.IdProduto);
                //                    if (existingAllocation)
                //                    {
                //                        ViewBag.Error = $"Product ID {product.IdProduto} is already allocated (PorAferido/PorSerial).";
                //                        transaction.Rollback();
                //                        return View(nameof(Index));
                //                    }
                //                }

                //                ProdutoAlocado produtoAlocado = new ProdutoAlocado
                //                {
                //                    IdProduto = product.IdProduto,
                //                    IdObra = product.IdObra,
                //                    IdFerrOndeProdRetirado = FerramentariaValue,
                //                    Solicitante_IdTerceiro = product.Solicitante_IdTerceiro,
                //                    Solicitante_CodColigada = product.Solicitante_CodColigada,
                //                    Solicitante_Chapa = product.Solicitante_Chapa,
                //                    Balconista_IdLogin = loggedUser?.Id,
                //                    Liberador_IdTerceiro = product.Liberador_IdTerceiro,
                //                    Liberador_CodColigada = product.Liberador_CodColigada,
                //                    Liberador_Chapa = product.Liberador_Chapa,
                //                    Observacao = $"Retirada:{product.Observacao}",
                //                    DataPrevistaDevolucao = product.DataPrevistaDevolucao,
                //                    DataEmprestimo = DateTime.Now,
                //                    Quantidade = product.Quantidade,
                //                    Chave = hash,
                //                    IdControleCA = product.IdControleCA,
                //                    IdReservation = product.IdReservation
                //                };

                //                _context.Add(produtoAlocado);
                //            }

                //            productToUpdate.Quantidade -= product.Quantidade;
                //            _context.Update(productToUpdate);

                //            reserved.Status = 3;
                //            _context.Update(reserved);

                //        }

                //        _context.SaveChanges();
                //        transaction.Commit();

                //        TempData["ShowSuccessAlertRetirada"] = true;
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
        //public IActionResult verifyCracha(string icard)
        //{
        //    fnRetornaColaboradorCracha? verifyEmployee = _contextBS.GetColaboradorCracha(icard).FirstOrDefault();
        //    if (verifyEmployee != null)
        //    {
        //        if (verifyEmployee.MATRICULA != null)
        //        {
        //            return Json(new { success = true, verifyEmployee });
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


        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [HttpPost]
        [Authorize(Roles = "Demo")]
        public async  Task<IActionResult> CancelTransaction(int? IdReservation,string? Observacao, string? IdTransaction)
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to CancelTransaction with IdReservation:{id} - {cancellation} - TransactionId:{IdTransaction}", user.Id, IdReservation, Observacao, IdTransaction);

                if (IdReservation == null) throw new ArgumentException("IdReservation is required");
                if (string.IsNullOrEmpty(IdTransaction)) throw new ArgumentException("IdTransaction is required");
                if (string.IsNullOrEmpty(Observacao)) throw new ArgumentException("Por favor insira Observação");


                await _retiradaService.CancelRetirada(IdReservation.Value, user.Chapa, Observacao, IdTransaction);

                _logger.LogInformation("User:{UserId} - Successfully cancelled the Retirada:{id} with TransactionId:{IdTransaction}", user.Id, IdReservation, IdTransaction);
                _auditLogger.LogAuditTransaction(user.Id, $"User:{user.Nome} successfully Cancelled Retirada Id: {IdReservation} - Reason: {Observacao} with TransactionId: {IdTransaction} .", "CancelRetirada", "Success", IdTransaction);
                
                return Ok(new { success = true, message = "Retirada cancelada com sucesso." });


                //Reservations? checkReserve = _context.Reservations.FirstOrDefault(i => i.Id == IdReservation);

                //if (checkReserve == null) return Json(new { success = false, message = $"CancelTransaction: Id: {IdReservation} not found." });

                //using var transaction = _context.Database.BeginTransaction();
                //{
                //    try
                //    {
                //        checkReserve.Status = 8;
                //        checkReserve.Observacao = Observacao;
                //        _context.Reservations.Update(checkReserve);
                //        _context.SaveChanges();
                //        transaction.Commit();

                //        return Ok(new { success = true, message = "Cancelled." });
                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        return StatusCode(500, new { success = false, message = ex.Message });
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

        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [HttpGet]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetFerramentariaList(int? IdCatalogo)
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to GetFerramentariaTransferList for IdCatalogo:{IdCatalogo}.", user.Id, IdCatalogo);

                if (IdCatalogo == null) throw new ArgumentException("IdCatalogo is required");

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("IdFerramentaria is required, please refresh page");

                List<FerramentariaStockModel>? listFerramentaria = await _ferramentariaService.GetAvailableFerramentaria(IdCatalogo.Value, FerramentariaValue.Value);

                if (listFerramentaria == null || listFerramentaria.Count == 0) throw new InvalidOperationException("Não há ferramentaria disponível para transferência");

                return Ok(listFerramentaria);           

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

        //[PageAccessAuthorize("HandoutRetirada.cs")]
        [HttpPost]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> TransferReservation([FromForm] int? OrderNo, [FromForm] int? IdFerramentaria, [FromForm] string? Observacao, [FromForm] string? IdTransaction)
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to TransferReservation for IdReservation:{OrderNo} to IdFerramentaria:{IdFerramentaria} IdFerramentaria - {Observacao}.", user.Id, OrderNo, IdFerramentaria, Observacao);

                if (OrderNo == null) throw new ArgumentException("OrderNo is required.");
                if (IdFerramentaria == null) throw new ArgumentException("IdFerramentaria is required.");
                if (string.IsNullOrEmpty(Observacao)) throw new ArgumentException("Observacao is required.");
                if (string.IsNullOrEmpty(IdTransaction)) throw new ArgumentException("IdTransaction is required.");

                Reservations? reservation = await _reservationService.GetReservations(OrderNo.Value);
                if (reservation == null) throw new InvalidOperationException($"Cannot find ReservationId:{OrderNo}");

                string? FerramentariaFrom = await _ferramentariaService.GetFerramentariaName(reservation.IdFerramentaria!.Value);
                string? FerramentariaTo = await _ferramentariaService.GetFerramentariaName(IdFerramentaria.Value);

                string ModifiedObs = $"Transfer: {FerramentariaFrom} -> {FerramentariaTo} by {user.Chapa} Obs:{Observacao}";

                await _reservationService.TransferReservation(OrderNo.Value, ModifiedObs, IdFerramentaria.Value);

                _logger.LogInformation("User:{UserId} - Successfully transferred the reservation:{id}", user.Id, OrderNo);
                _auditLogger.LogAuditInformation(user.Id, $"User:{user.Nome} successfully transferred Reservation Id: {OrderNo} FROM:{FerramentariaFrom} -> TO:{FerramentariaTo}", "TransferRetirada", "Success");

                return Ok(new { success = true, message = "Reserva transferida com sucesso." });

                //LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                //if (loggedUser.Chapa == null) return Json(new { success = false, message = $"Session Expired: loggedUser.Nome is empty." });

                //Reservations? checkReservation = _context.Reservations.FirstOrDefault(i => i.Id == OrderNo);
                //if (checkReservation == null) return Json(new { success = false, message = $"Cannot find Reservation with the Id {OrderNo}." });

                //string? FerramentariaFrom = _context.Ferramentaria.Where(i => i.Id == checkReservation.IdFerramentaria).Select(i => i.Nome).FirstOrDefault();
                //string? FerramentariaTo = _context.Ferramentaria.Where(i => i.Id == IdFerramentaria).Select(i => i.Nome).FirstOrDefault();

                //using var transaction = _context.Database.BeginTransaction();
                //{
                //    try
                //    {
                //        checkReservation.Status = 0;
                //        checkReservation.Observacao = $"Transfer: {FerramentariaFrom} -> {FerramentariaTo} by {loggedUser.Chapa} Obs:{Observacao}";
                //        checkReservation.IdFerramentaria = IdFerramentaria;

                //        _context.Reservations.Update(checkReservation);
                //        _context.SaveChanges();
                //        transaction.Commit();

                //        return Json(new { success = true, message = "Sucesso na transferência." });
                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        return Json(new { success = false, message = $"Transaction Failed: {ex.Message}" });
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

        #endregion

        #region Functions

        //public fnRetornaColaboradorCracha checkEmployeeCracha(string Icard)
        //{
        //    fnRetornaColaboradorCracha? verifyEmployee = _contextBS.GetColaboradorCracha(Icard).FirstOrDefault();

        //    return verifyEmployee ?? new fnRetornaColaboradorCracha();
        //}


        //public List<newCatalogInformationModel>? getRetiradaOrders(int? codpessoa)
        //{
        //    int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

        //    //List<newCatalogInformationModel>? reservedproducts = (from reservations in _context.Reservations                                                               
        //    //                                                       join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
        //    //                                                       join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
        //    //                                                       join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
        //    //                                                       join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
        //    //                                                       join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //    //                                                       join obra in _context.Obra on reservations.IdObra equals obra.Id
        //    //                                                       where
        //    //                                                       control.Type == 2
        //    //                                                       && reservations.Status == 0
        //    //                                                       && reservations.IdFerramentaria == FerramentariaValue
        //    //                                                       && member.CodPessoa == codpessoa
        //    //                                                       select new newCatalogInformationModel
        //    //                                                       {
        //    //                                                           IdCatalogo = reservations.IdCatalogo,
        //    //                                                           IdCategoria = catalogo.IdCategoria,
        //    //                                                           intClasse = categoria.Classe,
        //    //                                                           Classe = categoria.ClassType,
        //    //                                                           Type = catalogo.PorType,
        //    //                                                           Codigo = catalogo.Codigo,
        //    //                                                           itemNome = catalogo.Nome,
        //    //                                                           DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                                                                                                         
        //    //                                                           IdObra = reservations.IdObra,
        //    //                                                           ObraName = $"{obra.Codigo}-{obra.Nome}",
        //    //                                                           IdReservationControl = control.Id,
        //    //                                                           IdReservation = reservations.Id,
        //    //                                                           QuantidadeResquested = reservations.Quantidade,

        //    //                                                           MemberCodPessoa = member.CodPessoa,
        //    //                                                           LeaderCodPessoa = leader.CodPessoa,
        //    //                                                       }).ToList();


        //    List<newCatalogInformationModel> reservedproducts = (from reservations in _context.Reservations
        //                                                            join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
        //                                                            join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
        //                                                            join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
        //                                                            join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
        //                                                            join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                                            join obra in _context.Obra on reservations.IdObra equals obra.Id
        //                                                            where control.Type == 2
        //                                                                && reservations.Status == 0
        //                                                                && reservations.IdFerramentaria == FerramentariaValue
        //                                                                && member.CodPessoa == codpessoa
        //                                                            group new
        //                                                            {
        //                                                                reservations,
        //                                                                control,
        //                                                                member,
        //                                                                leader,
        //                                                                catalogo,
        //                                                                categoria,
        //                                                                obra
        //                                                            } by new { reservations.IdCatalogo, reservations.IdReservationControl } into grouped
        //                                                            select new newCatalogInformationModel
        //                                                            {
        //                                                                IdCatalogo = grouped.Key.IdCatalogo,
        //                                                                IdCategoria = grouped.First().catalogo.IdCategoria,
        //                                                                intClasse = grouped.First().categoria.Classe,
        //                                                                Classe = grouped.First().categoria.ClassType,
        //                                                                Type = grouped.First().catalogo.PorType,
        //                                                                Codigo = grouped.First().catalogo.Codigo,
        //                                                                itemNome = grouped.First().catalogo.Nome,
        //                                                                DataDeRetornoAutomatico = grouped.First().catalogo.DataDeRetornoAutomatico,
        //                                                                IdObra = grouped.First().reservations.IdObra,
        //                                                                ObraName = $"{grouped.First().obra.Codigo}-{grouped.First().obra.Nome}",
        //                                                                IdReservationControl = grouped.First().control.Id,
        //                                                                IdReservation = grouped.First().reservations.Id,
        //                                                                QuantidadeResquested = grouped.Sum(x => x.reservations.Quantidade),
        //                                                                MemberCodPessoa = grouped.First().member.CodPessoa,
        //                                                                LeaderCodPessoa = grouped.First().leader.CodPessoa,
        //                                                            }
        //                                                        ).ToList();





        //    if (reservedproducts.Count == 0) return new List<newCatalogInformationModel>();



        //    foreach (newCatalogInformationModel item in reservedproducts)
        //    {
        //        List<Produto>? listProducts = _context.Produto.Where(i => i.IdCatalogo == item.IdCatalogo 
        //                                                            && i.IdFerramentaria == FerramentariaValue 
        //                                                            && i.Ativo == 1 
        //                                                            && i.Quantidade > 0).ToList();

        //        if (listProducts.Count == 1)
        //        {
        //            item.IdProdutoSelected = listProducts[0].Id;
        //        }

            

        //        //item.listProducts = listProducts.Select(p => new newProductInformation
        //        //{
        //        //    IdProduto = p.Id,
        //        //    IdFerramentaria = p.IdFerramentaria,
        //        //    AF = p.AF,
        //        //    PAT = p.PAT,
        //        //    StockQuantity = p.Quantidade,
        //        //    DataVencimento = p.DataVencimento
        //        //}).ToList();

        //        //item.listProducts = listProducts.Select(p =>
        //        //{
        //        //    var productInfo = new newProductInformation
        //        //    {
        //        //        IdProduto = p.Id,
        //        //        IdFerramentaria = p.IdFerramentaria,
        //        //        AF = p.AF,
        //        //        PAT = p.PAT,
        //        //        StockQuantity = p.Quantidade,
        //        //        DataVencimento = p.DataVencimento
        //        //    };

        //        //    if (item.Type == "PorAferido")
        //        //    {
        //        //        productInfo.AllowedToBorrow = p.DataVencimento >= DateTime.Now;
        //        //        productInfo.Reason = productInfo.AllowedToBorrow ? "Valid" : "Expired";
        //        //    }
        //        //    else
        //        //    {
        //        //        productInfo.AllowedToBorrow = true;
        //        //    }

        //        //    return productInfo;
        //        //}).ToList();

        //        item.listProducts = listProducts
        //                            .Where(p => item.Type != "PorAferido" || p.DataVencimento > DateTime.Now)
        //                            .Select(p => new newProductInformation
        //                            {
        //                                IdProduto = p.Id,
        //                                IdFerramentaria = p.IdFerramentaria,
        //                                AF = p.AF,
        //                                PAT = p.PAT,
        //                                StockQuantity = p.Quantidade,
        //                                DataVencimento = p.DataVencimento,
        //                                AllowedToBorrow = item.Type == "PorAferido"
        //                                    ? p.DataVencimento > DateTime.Now
        //                                    : true,
        //                                Reason = item.Type == "PorAferido"
        //                                    ? (p.DataVencimento > DateTime.Now ? "Valid" : "Expired")
        //                                    : string.Empty
        //                            })
        //                            .ToList();


        //        if (item.intClasse == 2)
        //        {
        //            List<ControleCA>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == item.IdCatalogo && i.Ativo == 1 && i.Validade > DateTime.Now).OrderByDescending(i => i.Validade).ToList() ?? new List<ControleCA>();

        //            if (controleCAData.Count > 0)
        //            {
        //                item.listCA = controleCAData;
        //            }

        //            if (item.DataDeRetornoAutomatico.HasValue && item.DataDeRetornoAutomatico != 0)
        //            {
        //                item.DataReturn = DateTime.Now.AddDays(item.DataDeRetornoAutomatico.Value);
        //            }

        //        }


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

        //        return new newCatalogInformationModel
        //        {
        //            IdCatalogo = r.IdCatalogo,
        //            IdCategoria = r.IdCategoria,
        //            intClasse = r.intClasse,
        //            Classe = r.Classe,
        //            Type = r.Type,
        //            Codigo = r.Codigo,
        //            itemNome = r.itemNome,
        //            DataDeRetornoAutomatico = r.DataDeRetornoAutomatico,
        //            IdObra = r.IdObra,
        //            ObraName = r.ObraName,
        //            IdReservationControl = r.IdReservationControl,
        //            IdReservation = r.IdReservation,
        //            QuantidadeResquested = r.QuantidadeResquested,
        //            MemberCodPessoa = r.MemberCodPessoa,
        //            LeaderCodPessoa = r.LeaderCodPessoa,
        //            listProducts = r.listProducts,
        //            listCA = r.listCA,
        //            IdProdutoSelected = r.IdProdutoSelected,
        //            DataReturn = r.DataReturn,
        //            IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != FerramentariaValue && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

        //            // Add new properties with funcionario data
        //            MemberInfo = new employeeNewInformationModel()
        //                         {
        //                            Chapa = memberInfo.Chapa,
        //                            Nome = memberInfo.Nome,
        //                            CodSituacao = memberInfo.CodSituacao,
        //                            CodColigada = memberInfo.CodColigada,
        //                            Funcao = memberInfo.Funcao,
        //                            Secao = memberInfo.Secao,
        //                            CodPessoa = memberInfo.CodPessoa,
        //                         },
        //            LeaderInfo = new employeeNewInformationModel()
        //                        {
        //                            Chapa = leaderInfo.Chapa,
        //                            Nome = leaderInfo.Nome,
        //                            CodSituacao = leaderInfo.CodSituacao,
        //                            CodColigada = leaderInfo.CodColigada,
        //                            Funcao = leaderInfo.Funcao,
        //                            Secao = leaderInfo.Secao,
        //                            CodPessoa = leaderInfo.CodPessoa,
        //                        },
        //        };
        //    }).ToList();


        //    return enrichedReservations ?? new List<newCatalogInformationModel>();

            
        //}


        //private string GenerateMD5Hash(string input)
        //{
        //    using (MD5 md5 = MD5.Create())
        //    {
        //        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        //        byte[] hashBytes = md5.ComputeHash(inputBytes);
        //        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        //    }
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
