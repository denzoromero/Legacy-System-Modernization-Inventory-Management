using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Text.Json;
using Microsoft.Net.Http.Headers;
using AutoMapper.Execution;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;
using Azure.Core;
using FerramentariaTest.Services;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using Serilog.Context;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;

namespace FerramentariaTest.Controllers
{
    public class PrepareReservation : BaseController
    {
        protected IHttpContextAccessor httpContextAccessor;

        private readonly ILogger<PrepareReservation> _logger;
        private readonly IAuditLogger _auditLogger;
        private readonly IReservationService _reservationService;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IFerramentariaService _ferramentariaService;

        public PrepareReservation(IHttpContextAccessor httpCA, ILogger<PrepareReservation> logger, IReservationService reservationService, IUserContextService userContext, ICorrelationIdService correlationIdService, IFerramentariaService ferramentariaService, IAuditLogger auditLogger) : base(ferramentariaService, logger, correlationIdService)
        {
            httpContextAccessor = httpCA;
            _logger = logger;
            _reservationService = reservationService;
            _userContext = userContext;
            _correlationIdService = correlationIdService;
            _ferramentariaService = ferramentariaService;
            _auditLogger = auditLogger;
        }

        //[PageAccessAuthorize("PrepareReservation.cs")]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> Index()
        {
            try
            {

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Landed on Page: PrepareReservation.Index", userId);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null)
                {
                    var ferramentariaItems = _ferramentariaService.GetFerramentariaList(userId);

                    if (ferramentariaItems != null)
                    {
                        ViewBag.FerramentariaItems = ferramentariaItems;
                        ViewBag.ViewPage = "PreparationIndex";
                    }

                    return PartialView("_FerramentariaPartialView");

                }

                if (TempData.ContainsKey("ShowSuccessAlertproceedPrepare"))
                {
                    ViewBag.SuccessAlertNew = TempData["ShowSuccessAlertproceedPrepare"]?.ToString();
                    TempData.Remove("ShowSuccessAlertproceedPrepare"); // Remove it from TempData to avoid displaying it again
                }

                List<ReservationsModel>? groupReservation = await _reservationService.GetGroupReservation(FerramentariaValue, 0) ?? new List<ReservationsModel>();

                //List<ReservationsModel>? groupReservation = getGroupReservation(FerramentariaValue, 0).Where(i => i.itemCount > 0).OrderBy(i => i.ControlId).ToList() ?? new List<ReservationsModel>();

                ViewBag.ReservationList = groupReservation;

                return View();

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
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

        #region PrepareItems

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> PrepareItems(int id)
        {
            try
            {

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Landed on Page: PrepareReservation.PrepareItems with Id:{id}", userId, id);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null)
                {
                    var ferramentariaItems = _ferramentariaService.GetFerramentariaList(userId);

                    if (ferramentariaItems != null)
                    {
                        ViewBag.FerramentariaItems = ferramentariaItems;
                        ViewBag.ViewPage = "PreparationIndex";
                    }

                    return PartialView("_FerramentariaPartialView");

                }

                if (TempData.ContainsKey("ShowSuccessAlertPrepareAction"))
                {
                    ViewBag.SuccessAlertNew = TempData["ShowSuccessAlertPrepareAction"]?.ToString();
                    TempData.Remove("ShowSuccessAlertPrepareAction"); // Remove it from TempData to avoid displaying it again
                }

                ReservationControl? control = await _reservationService.GetReservationControl(id);
                if (control == null)
                {
                    throw new InvalidOperationException($"No ReservationControl Found on Id:{id}");
                }

                List<ItemReservationDetailModel>? listitems = await _reservationService.PrepareModel(id, FerramentariaValue);
                if (listitems == null || listitems.Count == 0)
                {
                    throw new InvalidOperationException($"No ReservationList Found on ControleId:{id}");
                }
           
                PrepareItemsPageModel itemlist = new PrepareItemsPageModel()
                {
                    ControlId = id,
                    RegisteredDate = control?.DataRegistro.HasValue == true ? control.DataRegistro.Value.ToString("dd/MM/yyyy") : string.Empty,
                    ExpiryDateString = control?.ExpirationDate.HasValue == true ? control?.ExpirationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                    items = listitems ?? new List<ItemReservationDetailModel>()
                };

                ViewBag.itemList = itemlist;

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

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> PrepareAction(List<ItemReservationDetailModel>? selectedItems, int Id, string? RegisteredDate, string? ExpiryDate)
        {
            try
            {
                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Attempt to PrepareAction", userId);

                if (selectedItems == null || selectedItems.Count == 0)
                {
                    throw new InvalidOperationException($"Nenhum item para preparar.");
                }

                selectedItems = selectedItems.Where(i => i.IsSelected == true).ToList();
                if (selectedItems.Count == 0)
                {
                    throw new InvalidOperationException($"Nenhum item selecionado para preparar.");
                }

                await _reservationService.UpdateReservationStatus(selectedItems.Select(item => item.IdReservation).ToList());

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                string ReceiptHtml = generateReceipt(selectedItems, Id, RegisteredDate, ExpiryDate);

                ViewBag.ReceiptHtml = ReceiptHtml;
                ViewBag.SuccessAlertNew = "Sucesso!";

                ReservationControl? control = await _reservationService.GetReservationControl(Id);
                if (control == null)
                {
                    throw new InvalidOperationException($"No ReservationControl Found on Id:{Id}");
                }

                List<ItemReservationDetailModel>? listitems = await _reservationService.PrepareModel(Id, FerramentariaValue);
                if (listitems == null || listitems.Count == 0)
                {
                    throw new InvalidOperationException($"No ReservationList Found on ControleId:{Id}");
                }

                PrepareItemsPageModel itemlist = new PrepareItemsPageModel()
                {
                    ControlId = Id,
                    RegisteredDate = control?.DataRegistro.HasValue == true ? control.DataRegistro.Value.ToString("dd/MM/yyyy") : string.Empty,
                    ExpiryDateString = control?.ExpirationDate.HasValue == true ? control?.ExpirationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                    items = listitems ?? new List<ItemReservationDetailModel>(),
                };

                var userName = HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                using var _ = LogContext.PushProperty("PreparedItems", selectedItems, destructureObjects: true);
                _logger.LogInformation("User:{UserId} - Attempt Successful for PrepareAction", userId);
                _auditLogger.LogAuditInformation(userId, $"User:{userName} successfully Prepared {selectedItems.Count} Item/s with Control:{control.Chave}.", "PrepareReservation", "Success");

                ViewBag.itemList = itemlist;
                return View(nameof(PrepareItems));

                //if (selectedItems != null)
                //{
                //    List<ItemReservationDetailModel>? selected = selectedItems.Where(i => i.IsSelected == true).ToList();
                //    if (selected.Count > 0)
                //    {
                //        int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                //        //ViewBag.ReceiptHtml = generateReceipt(selected);
                //        string ReceiptHtml = generateReceipt(selected, Id, RegisteredDate, ExpiryDate);

                //        //TempData["ShowSuccessAlertPrepareAction"] = "Sucesso!";
                //        //TempData["ReceiptHtml"] = ReceiptHtml;
                //        //return RedirectToAction(nameof(PrepareItems), new { id = Id });

                //        ViewBag.ReceiptHtml = ReceiptHtml;
                //        ViewBag.SuccessAlertNew = "Sucesso!";
                //        //ViewBag.itemList = prepareModel(Id ?? 0, FerramentariaValue);

                //        List<ItemReservationDetailModel>? listitems = prepareModel(Id, FerramentariaValue);

                //        ReservationControl? control = _context.ReservationControl.FirstOrDefault(i => i.Id == Id);

                //        PrepareItemsPageModel itemlist = new PrepareItemsPageModel()
                //        {
                //            ControlId = Id,
                //            RegisteredDate = control?.DataRegistro.Value.ToString("dd/MM/yyyy") ?? string.Empty,
                //            ExpiryDateString = control?.ExpirationDate.Value.ToString("dd/MM/yyyy") ?? string.Empty,
                //            items = listitems ?? new List<ItemReservationDetailModel>()
                //        };

                //        ViewBag.itemList = itemlist;

                //        ViewBag.SuccessAlertNew = "Success!";
                //        return View(nameof(PrepareItems));
                //    }
                //    else
                //    {
                //        ViewBag.Error = "No Selected Items";
                //        return View(nameof(PrepareItems));
                //    }
                //}
                //else
                //{
                //    ViewBag.Error = "selectedItems is null";
                //    return View(nameof(PrepareItems));
                //}
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View(nameof(PrepareItems));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View(nameof(PrepareItems));
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
                return View(nameof(PrepareItems));
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
                return View(nameof(PrepareItems));
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
                return View(nameof(PrepareItems));
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
                return View(nameof(PrepareItems));
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
                return View(nameof(PrepareItems));
            }
        }

        #endregion

        [Authorize(Roles = "Demo")]
        [OutputCache(Duration = 0, NoStore = true)]
        public async Task<IActionResult> ProcessListReservation(int id)
        {
            try
            {
                //LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                //if (loggedUser == null)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });

                //PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                //if (checkPermission == null)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });

                //if (checkPermission.Visualizar != 1)
                //    return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });

                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Landed on Page: PrepareReservation.ProcessListReservation with Id:{id}", userId, id);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                

                //List<ReservedProductModel>? reservedItem = await _reservationService.GetReservedProducts(id, FerramentariaValue);

                ProcessListReservationModel model = new ProcessListReservationModel()
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    ReservedItems = await _reservationService.GetReservedProducts(id, FerramentariaValue.Value) ?? new List<ReservedProductModel>()
                };

                    //List<ReservedProductModel>? reservedItem = getCompleteItemDetail(id);

                return View(model);
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

        [Authorize(Roles = "Demo")]
        [OutputCache(Duration = 0, NoStore = true)]
        public async Task<IActionResult> FinalizeReservations(List<FinalSubmissionProcess> FinalProcessList, string? TransactionId)
        {           
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                using var _ = LogContext.PushProperty("FinalReservation", FinalProcessList, destructureObjects: true);
                _logger.LogInformation("User:{UserId} - Attempt to FinalizeReservations", user.Id);

                if (FinalProcessList == null || FinalProcessList.Count == 0) throw new InvalidOperationException("ProcessList is empty");
                if (FinalProcessList.Any(i => i.IdReservation == null)) throw new InvalidOperationException("Some of the IdReservation is null");
                if (FinalProcessList.Any(i => i.IdProduto == null)) throw new InvalidOperationException("Some of the IdProduto is null");
                if (FinalProcessList.Any(i => i.QtyRequested == null)) throw new InvalidOperationException("Some of the QtyRequested is null");
                if (string.IsNullOrEmpty(TransactionId)) throw new InvalidOperationException("TransactionId is null");

                if (await _reservationService.VerifyFinalizeTransactionId(TransactionId)) throw new InvalidOperationException("Transaction already inserted");

                List<ProductReservation>? finalResult = await _reservationService.VerifyReservations(FinalProcessList);
                if (finalResult == null || finalResult.Count == 0) throw new InvalidOperationException("Verification of the final submission is null");

                finalResult.ForEach(i =>
                {
                    i.PreparedBy = user.Id;
                    i.TransactionId = TransactionId;
                });

                await _reservationService.FinalizeProcessReservation(finalResult);

                using var another = LogContext.PushProperty("finalResult", finalResult, destructureObjects: true);
                _logger.LogInformation("User:{UserId} - Successfully FinalizeReservations with TransactionId{TransactionId}", user.Id, TransactionId);
                _auditLogger.LogAuditTransaction(user.Id, $"User:{user.Nome} successfully Finalize {finalResult.Count} Item/s.", "FinalizeReservation", "Success", TransactionId);

                return Ok(new { success = true, message = "As reservas foram finalizadas." });

                //using var transaction = _context.Database.BeginTransaction();
                //{
                //    try
                //    {

                //        List<ProductReservation> Entities = new List<ProductReservation>();
                //        foreach (FinalSubmissionProcess item in FinalProcessList)
                //        {
                //            Reservations? reserved = _context.Reservations.FirstOrDefault(i => i.Id == item.IdReservation);
                //            if (reserved == null)
                //            {
                //                ViewBag.Error = $"Cant find reservation with the Id:{item.IdReservation}";
                //                transaction.Rollback();
                //                return View(nameof(Index));
                //            }

                //            ProductReservation entity = new ProductReservation()
                //            {
                //                IdReservation = item.IdReservation,
                //                IdProduto = item.IdProduto,
                //                DataPrevistaDevolucao = item.DateReturn,
                //                Observacao = item.Observacao,
                //                PreparedBy = loggedUser.Id,
                //                Status = 0,
                //                DataRegistro = DateTime.Now,
                //                FinalQuantity = item.QtyRequested,
                //            };

                //            reserved.Status = 2;
                //            _context.Update(reserved);

                //            Entities.Add(entity);
                //        }

                //        _context.ProductReservation.AddRange(Entities);
                //        _context.SaveChanges();

                //        transaction.Commit();

                //        TempData["ShowSuccessAlertproceedPrepare"] = "Success!";
                //        return RedirectToAction(nameof(Index));
                //        //return View(nameof(ProcessListReservation));
                //    }
                //    catch (Exception ex)
                //    {
                //        ViewBag.Error = ex.Message;
                //        return View(nameof(ProcessListReservation));
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

        #region javascript

        [HttpGet]
        public async Task<IActionResult> getItemDetail(int id)
        {
            try
            {
                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Attempt to getItemDetail with Id:{id}", userId, id);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("Nenhuma Ferramentaria selecionada. Atualize a página para selecionar a Ferramentaria.");

                List<ReservationsControlModel>? reservations = await _reservationService.GetPreparingReservations(id, FerramentariaValue);

                if (reservations == null || reservations.Count == 0)
                {
                    throw new InvalidOperationException($"Nenhum resultado encontrado com numero do pedido:{id}");
                }

                if (reservations.All(i => i.reserveStatus == 2))
                {
                    throw new InvalidOperationException($"Reservas com o número do pedido: {id} já estão com o status Aguardando para serem retiradas.");
                }

                if (reservations.All(i => i.reserveStatus != 1))
                {
                    throw new InvalidOperationException($"Reservas com o número do pedido: {id} ainda não estão no status Preparando.");
                }

                return Ok(id);



                //int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

                //Reservations? checkReservation = _context.Reservations.FirstOrDefault(i => i.IdReservationControl == id && i.IdFerramentaria == FerramentariaValue);

                //List<ReservationsControlModel>? reservations = (from reservation in _context.Reservations
                //                                                join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
                //                                                where reservation.IdReservationControl == id
                //                                                && reservation.IdFerramentaria == FerramentariaValue
                //                                                && reservation.Status != 8
                //                                                && reservation.Status != 3
                //                                                select new ReservationsControlModel
                //                                                {
                //                                                    ControlId = reservationControl.Id,
                //                                                    Chave = reservationControl.Chave,
                //                                                    ControlStatusString = reservationControl.StatusString,
                //                                                    ControlStatus = reservationControl.Status,
                //                                                    reserveStatus = reservation.Status,
                //                                                    controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                //                                                }).ToList();

                //if (reservations != null && reservations.Count > 0)
                //{

                //    if (reservations.All(i => i.reserveStatus == 1))
                //    {
                //        return Json(new { success = true, id });
                //    }
                //    else
                //    {
                //        if (reservations.All(i => i.reserveStatus == 2))
                //        {
                //            return Json(new { success = false, message = $"Reservations with Order No:{id} is already on Awaiting to be picked-up status." });
                //        }
                //        else
                //        {
                //            return Json(new { success = false, message = $"Reservations with Order No:{id} is not yet on Preparing status." });
                //        }
                
                //    }
                    
                //}
                //else
                //{
                //    return Json(new { success = false, message = "No data found." });
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


            //try
            //{
            //    ItemReservationDetailModel? itemdetail = getReservationDetail(key);

            //    if (itemdetail != null)
            //    {
            //        int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
            //        if (itemdetail.IdFerramentaria == FerramentariaValue)
            //        {
            //            if (itemdetail.intStatus == 1)
            //            {

            //                if (itemdetail.intClasse == 2)
            //                {
            //                    itemdetail.listCA = GetCAList(itemdetail.IdCatalogo);
            //                }

            //                itemdetail.MemberInfo = searches.searchEmployeeInformationUsingCodPessoa(itemdetail.MemberCodPessoa);
            //                itemdetail.LeaderInfo = searches.searchEmployeeInformationUsingCodPessoa(itemdetail.LeaderCodPessoa);

            //                itemdetail.ferramentariacount = countFerramentaria(itemdetail.IdCatalogo);

            //                List<productDetail> product = checkProduct(itemdetail);

            //                return Json(new { success = true, itemdetail, product });
            //            }
            //            else if (itemdetail.intStatus == 2)
            //            {
            //                return Json(new { success = false, message = "Item is already in prepared status." });
            //            }
            //            else if (itemdetail.intStatus == 0)
            //            {
            //                return Json(new { success = false, message = "Please prepare the Item first, Item is not in preparing status." });
            //            }
            //            else if (itemdetail.intStatus == 5)
            //            {
            //                return Json(new { success = false, message = "This reservation is pending for Cancellation/Transfer." });
            //            }
            //            else if (itemdetail.intStatus == 7)
            //            {
            //                return Json(new { success = false, message = "This reservation is cancelled due to expiration." });
            //            }
            //            else
            //            {
            //                return Json(new { success = false, message = "Item is not in any of the status" });
            //            }
            //        }
            //        else
            //        {
            //            return Json(new { success = false, message = "Item is not on your section." });
            //        }
            //    }
            //    else
            //    {
            //        return Json(new { success = false, message = "No data found." });
            //    }

            //}
            //catch (Exception ex)
            //{
            //    return Json(new { success = false, message = $"getItemDetail: {ex.Message}" });
            //}


        }

        [HttpGet]
        public async Task<IActionResult> getControlList(int? status)
        {
            try
            {
                int? userId = _userContext.GetUserId();

                _logger.LogInformation("User:{UserId} - Attempt to getControlList with Status:{status}", userId, status);

                int? FerramentariaValue = _ferramentariaService.GetChosenFerramentariaValue();
                if (FerramentariaValue == null) throw new ArgumentException("Nenhuma Ferramentaria selecionada. Atualize a página para selecionar a Ferramentaria.");

                List<ReservationsModel>? groupReservation = await _reservationService.GetGroupReservation(FerramentariaValue, status) ?? new List<ReservationsModel>();

                return Ok(groupReservation);

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

        [HttpPost]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> CancelTransaction(int? IdReservation, string? cancelObservacao)
        {
            try
            {
                
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to CancelTransaction with IdReservation:{id} - {cancellation}", user.Id, IdReservation, cancelObservacao);

                if (IdReservation == null) throw new ArgumentException("IdReservation is required");

                if (string.IsNullOrEmpty(cancelObservacao)) throw new ArgumentException("Por favor insira Observação");

                await _reservationService.CancelReservation(IdReservation, user.Chapa, cancelObservacao);

                _logger.LogInformation("User:{UserId} - Successfully cancelled the reservation:{id}", user.Id, IdReservation);
                _auditLogger.LogAuditInformation(user.Id, $"User:{user.Nome} successfully Cancelled Reservation Id: {IdReservation} - Reason: {cancelObservacao}.", "CancelReservation", "Success");

                return Ok(new { success = true, message = "Reserva cancelada com sucesso." });

                //LoggedUserData ? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                //if (loggedUser.Chapa == null) return Json(new { success = false, message = $"Session Expired: loggedUser.Nome is empty." });

                //Reservations? checkReserve = _context.Reservations.FirstOrDefault(i => i.Id == IdReservation);

                //if (checkReserve == null) return Json(new { success = false, message = $"CancelTransaction: Id: {IdReservation} not found." });

                //using var transaction = _context.Database.BeginTransaction();
                //{
                //    try
                //    {
                //        checkReserve.Status = 8;
                //        checkReserve.Observacao = $"Cancellado por: {loggedUser.Chapa} - {cancelObservacao}";
                //        _context.Reservations.Update(checkReserve);
                //        _context.SaveChanges();
                //        transaction.Commit();

                //        return Ok(new { success = true, message = "Cancelled." });
                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        return Json(new { success = false, message = ex.Message });
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

        [HttpGet]
        public async Task<IActionResult> GetFerramentariaTransferList(int? IdCatalogo, int? IdFerramentaria)
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to GetFerramentariaTransferList for IdCatalogo:{IdCatalogo}.", user.Id, IdCatalogo);

                if (IdCatalogo == null) throw new ArgumentException("IdCatalogo is required");

                if (IdFerramentaria == null) throw new ArgumentException("IdFerramentaria is required");

                List<FerramentariaStockModel>? listFerramentaria = await _ferramentariaService.GetAvailableFerramentaria(IdCatalogo.Value, IdFerramentaria.Value);

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

        [HttpPost]
        //[PageAccessAuthorize("PrepareReservation.cs")]
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> TransferReservation([FromForm] int? OrderNo, [FromForm] int? IdFerramentaria, [FromForm] string? Observacao)
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to TransferReservation for IdReservation:{OrderNo} to IdFerramentaria:{IdFerramentaria} IdFerramentaria - {Observacao}.", user.Id, OrderNo, IdFerramentaria, Observacao);

                if (OrderNo == null) throw new ArgumentException("OrderNo is required.");
                if (IdFerramentaria == null) throw new ArgumentException("IdFerramentaria is required.");
                if (string.IsNullOrEmpty(Observacao)) throw new ArgumentException("Observacao is required.");

                Reservations? reservation = await _reservationService.GetReservations(OrderNo.Value);
                if (reservation == null) throw new InvalidOperationException($"Cannot find ReservationId:{OrderNo}");

                string? FerramentariaFrom = await _ferramentariaService.GetFerramentariaName(reservation.IdFerramentaria!.Value);
                string? FerramentariaTo = await _ferramentariaService.GetFerramentariaName(IdFerramentaria.Value);

                string ModifiedObs = $"Transfer: {FerramentariaFrom} -> {FerramentariaTo} by {user.Chapa} Obs:{Observacao}";

                await _reservationService.TransferReservation(OrderNo.Value, ModifiedObs, IdFerramentaria.Value);


                _logger.LogInformation("User:{UserId} - Successfully transferred the reservation:{id}", user.Id, OrderNo);
                _auditLogger.LogAuditInformation(user.Id, $"User:{user.Nome} successfully transferred Reservation Id: {OrderNo} FROM:{FerramentariaFrom} -> TO:{FerramentariaTo}", "TransferReservation", "Success");

                return Ok(new { success = true, message = "Reserva transferida com sucesso." });


                //LoggedUserData ? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
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

        #region functions

        //public List<ReservationsModel>? getGroupReservation(int? FerramentariaValue,int? controlStatus)
        //{

        //    List<ReservationsControlModel>? reservations = (from reservationControl in _context.ReservationControl
        //                                                    join reserve in _context.Reservations on reservationControl.Id equals reserve.IdReservationControl
        //                                                    join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
        //                                                    where reserve.IdFerramentaria == FerramentariaValue 
        //                                                    && reservationControl.Type == 1
        //                                                    //&& (controlStatus == null || reservationControl.Status == controlStatus)
        //                                                    && (controlStatus == null || reserve.Status == controlStatus)
        //                                                    select new ReservationsControlModel
        //                                                    {
        //                                                        ControlId = reservationControl.Id,
        //                                                        Chave = reservationControl.Chave,
        //                                                        LeadercodPessoa = leader.CodPessoa,
        //                                                        LeaderName = leader.Nome,
        //                                                        ControlStatusString = reservationControl.StatusString,
        //                                                        ControlStatus = reservationControl.Status,
        //                                                        reserveStatus = reserve.Status,
        //                                                        reserveStatusString = reserve.StatusString,
        //                                                        controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
        //                                                    }).ToList();

        //    List<ReservationsModel>? groupReservation = reservations
        //                                                .GroupBy(r => r.ControlId)
        //                                                .Select(group => new ReservationsModel
        //                                                {
        //                                                    ControlId = group.Key,
        //                                                    //itemCount = group.Count(),
        //                                                    itemCount = group.Count(r => r.reserveStatus == 0),
        //                                                    Chave = group.First().Chave,
        //                                                    LeaderName = group.First().LeaderName,
        //                                                    RegisteredCount = group.Count(),
        //                                                    ActualStatus = group.First().ControlStatusString,
        //                                                    controlDataRegistroString = group.First().controlDataRegistroString,
        //                                                    GroupStatus = group.All(r => r.reserveStatus == 1) ? 1 : 0
        //                                                }).OrderBy(i => i.controlDataRegistroString).ToList();


        //    return groupReservation ?? new List<ReservationsModel>();
        //}

        //public List<ReservedProductModel>? getCompleteItemDetail(int id)
        //{

        //    List<ReservedProductModel>? reservedList = (from reservation in _context.Reservations
        //                                                join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
        //                                                join member in _context.LeaderMemberRel on reservation.IdLeaderMemberRel equals member.Id
        //                                                join leader in _context.LeaderData on member.IdLeader equals leader.Id
        //                                                join obra in _context.Obra on reservation.IdObra equals obra.Id
        //                                                join produto in _context.Produto on new { reservation.IdCatalogo, reservation.IdFerramentaria }
        //                                                        equals new { produto.IdCatalogo, produto.IdFerramentaria }
        //                                                join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                                                join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                                where reservation.Status == 1
        //                                                && produto.Ativo == 1
        //                                                && catalogo.Ativo == 1
        //                                                && categoria.Ativo == 1
        //                                                && reservation.IdReservationControl == id
        //                                                select new ReservedProductModel
        //                                                {
        //                                                    IdReservationControl = reservation.IdReservationControl,
        //                                                    IdReservation = reservation.Id,
        //                                                    IdCatalogo = reservation.IdCatalogo,
        //                                                    IdFerramentaria = reservation.IdFerramentaria,
        //                                                    IdProduto = produto.Id,

        //                                                    intClasse = categoria.Classe,
        //                                                    Classe = categoria.ClassType,
        //                                                    Type = catalogo.PorType,
        //                                                    Codigo = catalogo.Codigo,
        //                                                    itemNome = catalogo.Nome,
        //                                                    intStatus = reservation.Status,
        //                                                    Status = reservation.StatusString,
        //                                                    MemberCodPessoa = member.CodPessoa,
        //                                                    LeaderCodPessoa = leader.CodPessoa,
        //                                                    IdObra = obra.Id,
        //                                                    ObraName = $"{obra.Codigo}-{obra.Nome}",

        //                                                    QtyRequested = reservation.Quantidade,
        //                                                    QtyStock = produto.Quantidade,                                                  
        //                                                }).ToList();

        //    if (reservedList == null || reservedList.Count == 0)
        //                return new List<ReservedProductModel>();
                  

        //    //foreach (ReservedProductModel item in reservedList)
        //    //{
        //    //    item.listCA = GetCAList(item.IdCatalogo) ?? new List<ControleCA>();
        //    //}

        //    Dictionary<int, Funcionario?> funcionarioDict = _contextBS.Funcionario.AsEnumerable()
        //                                             .GroupBy(e => e.CodPessoa)
        //                                             .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
        //                                             .Where(f => f != null)  // Filter out nulls if any
        //                                             .ToDictionary(
        //                                                 f => f.CodPessoa.Value,   // Key selector
        //                                                 f => f              // Value selector
        //                                             );

        //    var enrichedReservations = reservedList.Select(r =>
        //    {
        //        // Get member and leader information from dictionary
        //        var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
        //        var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

        //        return new ReservedProductModel
        //        {


        //            IdReservationControl = r.IdReservationControl,
        //            IdReservation = r.IdReservation,
        //            IdCatalogo = r.IdCatalogo,
        //            IdFerramentaria = r.IdFerramentaria,
        //            IdProduto = r.IdProduto,

        //            intClasse = r.intClasse,
        //            Classe = r.Classe,
        //            Type = r.Type,
        //            Codigo = r.Codigo,
        //            itemNome = r.itemNome,
        //            intStatus = r.intStatus,
        //            Status = r.Status,
        //            MemberCodPessoa = member.CodPessoa,
        //            LeaderCodPessoa = leader.CodPessoa,
        //            IdObra = r.IdObra,
        //            ObraName = r.ObraName,

        //            QtyRequested = r.QtyRequested,
        //            QtyStock = r.QtyStock,
        //            IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != r.IdFerramentaria && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

        //            // Add new properties with funcionario data
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

        //    return enrichedReservations ?? new List<ReservedProductModel>();

        //    //ItemReservationDetailModel itemdetail = (from reserv in _context.Reservations
        //    //                                          join catalogo in _context.Catalogo on reserv.IdCatalogo equals catalogo.Id
        //    //                                          join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //    //                                          join reservationControl in _context.ReservationControl on reserv.IdReservationControl equals reservationControl.Id
        //    //                                          join member in _context.LeaderMemberRel on reserv.IdLeaderMemberRel equals member.Id
        //    //                                          join leader in _context.LeaderData on member.IdLeader equals leader.Id
        //    //                                          join obra in _context.Obra on reserv.IdObra equals obra.Id
        //    //                                          where reserv.Id == id
        //    //                                          //&& reserv.Status == 1
        //    //                                          select new ItemReservationDetailModel
        //    //                                          {
        //    //                                              IdFerramentaria = reserv.IdFerramentaria,
        //    //                                              IdCategoria = categoria.Id,
        //    //                                              IdCatalogo = reserv.IdCatalogo,
        //    //                                              IdReservation = reserv.Id,
        //    //                                              intClasse = categoria.Classe,
        //    //                                              Classe = categoria.ClassType,
        //    //                                              Type = catalogo.PorType,
        //    //                                              Codigo = catalogo.Codigo,
        //    //                                              itemNome = catalogo.Nome,
        //    //                                              QuantidadeResquested = reserv.Quantidade,
        //    //                                              DataRegistro = reserv.DataRegistro.HasValue == true ? reserv.DataRegistro.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty,
        //    //                                              intStatus = reserv.Status,
        //    //                                              Status = reserv.StatusString,
        //    //                                              DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
        //    //                                              MemberCodPessoa = member.CodPessoa,
        //    //                                              LeaderCodPessoa = leader.CodPessoa,
        //    //                                              ExpiryDate = reservationControl.ExpirationDate,
        //    //                                              IdObra = obra.Id,
        //    //                                              ObraName = $"{obra.Codigo}-{obra.Nome}",
        //    //                                              ExpiryDateString = reservationControl.ExpirationDate.HasValue == true ? reservationControl.ExpirationDate.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty
        //    //                                          }).FirstOrDefault() ?? new ItemReservationDetailModel();

        //    //if (itemdetail.intClasse == 2)
        //    //{
        //    //    itemdetail.listCA = GetCAList(itemdetail.IdCatalogo);
        //    //}

        //    //itemdetail.MemberInfo = searches.searchEmployeeInformationUsingCodPessoa(itemdetail.MemberCodPessoa);
        //    //itemdetail.LeaderInfo = searches.searchEmployeeInformationUsingCodPessoa(itemdetail.LeaderCodPessoa);



        //    ////itemdetail.ferramentariacount = countFerramentaria(itemdetail.IdCatalogo);

        //    //itemdetail.listFerramentaria = getFerramentariaList(itemdetail.IdCatalogo,itemdetail.IdFerramentaria);

        //    //itemdetail.listProduto = checkProduct(itemdetail);


        //    //return itemdetail ?? new ItemReservationDetailModel();
        //}

        public String generateReceipt(List<ItemReservationDetailModel> selected, int Id,string? RegisteredDate, string? ExpiryDate)
        {
            StringBuilder sb = new StringBuilder();


            sb.AppendLine("<html><head><title>Receipt</title>");
            sb.AppendLine(GenerateCSS());
            sb.AppendLine("</head><body>");

            sb.AppendLine("<div class='receipt'>");

            sb.AppendLine($"<b> Reserve No:{Id} </b>");
            sb.AppendLine("<br>");
            sb.AppendLine($"<b> Member: {selected[0].MemberInfo.Nome} </b>");
            sb.AppendLine("<br>");
            sb.AppendLine($"<b> Leader: {selected[0].LeaderInfo.Nome} </b>");
            sb.AppendLine("<br>");
            sb.AppendLine($"Registered Date:{RegisteredDate}");
            sb.AppendLine("<br>");
            sb.AppendLine($"Expiry Date:{ExpiryDate}");

            foreach (ItemReservationDetailModel? item in selected)
            {
                sb.AppendLine("<br>");
                sb.AppendLine($"Codigo:{item.Codigo}");
                sb.AppendLine("<br>");
                sb.AppendLine($"Item:{item.itemNome}");
                sb.AppendLine("<br>");
                sb.AppendLine($"Qtd:{item.QuantidadeResquested}");
                sb.AppendLine("<br>");
                sb.AppendLine("-------------------------------------------");     
            }

            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        public string GenerateCSS()
        {
            string? csscode = @"<style type='text/css'>
                                @media print { 
                                    body, html {
                                                 margin: 0;
                                                 padding: 0;
                                                 width: 7.9cm;
                                                }
                                    .receipt {
                                        width: 7.9cm;
                                        font-family: 'Courier New', monospace;
                                         font-size: 12px;
                                     }
                                    @page {
                                        margin: 0;
                                        }
                              }
                                    body {
                                    font-family: Arial, sans-serif;
                                    }

                                    .receipt {
                                    width: 100%;
                                    font-family: 'Courier New', monospace;
                                    font-size: 12px;
                                    }
                            </style>
                            ";

            return csscode;
        }

        //public List<ItemReservationDetailModel>? prepareModel(int id, int? FerramentariaValue)
        //{

        //    List<ItemReservationDetailModel> itemdetail = (from reserv in _context.Reservations
        //                                                   join catalogo in _context.Catalogo on reserv.IdCatalogo equals catalogo.Id
        //                                                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                                   join reservationControl in _context.ReservationControl on reserv.IdReservationControl equals reservationControl.Id
        //                                                   join member in _context.LeaderMemberRel on reserv.IdLeaderMemberRel equals member.Id
        //                                                   join leader in _context.LeaderData on member.IdLeader equals leader.Id
        //                                                   join obra in _context.Obra on reserv.IdObra equals obra.Id
        //                                                   where reserv.IdFerramentaria == FerramentariaValue
        //                                                   && reserv.IdReservationControl == id
        //                                                   && reservationControl.Type == 1
        //                                                   select new ItemReservationDetailModel
        //                                                   {
        //                                                       IdReservation = reserv.Id,
        //                                                       Classe = categoria.ClassType,
        //                                                       Type = catalogo.PorType,
        //                                                       Codigo = catalogo.Codigo,
        //                                                       itemNome = catalogo.Nome,
        //                                                       QuantidadeResquested = reserv.Quantidade,
        //                                                       MemberCodPessoa = member.CodPessoa,
        //                                                       LeaderCodPessoa = leader.CodPessoa,
        //                                                       DataRegistro = reserv.DataRegistro.HasValue == true ? reserv.DataRegistro.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty, // Correct format
        //                                                       Status = reserv.StatusString,
        //                                                       IdObra = reserv.IdObra,
        //                                                       ObraName = $"{obra.Codigo}-{obra.Nome}",
        //                                                       intStatus = reserv.Status
        //                                                   }).ToList();

        //    List<Funcionario?>? recentFuncionario = _contextBS.Funcionario
        //                                              .GroupBy(e => e.CodPessoa)
        //                                              .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
        //                                              .ToList();

        //    List<ItemReservationDetailModel> completeDetail = (from item in itemdetail
        //                                                       join memberinfo in recentFuncionario on item.MemberCodPessoa equals memberinfo.CodPessoa
        //                                                       join leaderinfo in recentFuncionario on item.LeaderCodPessoa equals leaderinfo.CodPessoa
        //                                                       select new ItemReservationDetailModel
        //                                                       {
        //                                                           IdReservation = item.IdReservation,
        //                                                           Classe = item.Classe,
        //                                                           Type = item.Type,
        //                                                           Codigo = item.Codigo,
        //                                                           itemNome = item.itemNome,
        //                                                           QuantidadeResquested = item.QuantidadeResquested,
        //                                                           MemberCodPessoa = item.MemberCodPessoa,
        //                                                           MemberInfo = new employeeNewInformationModel
        //                                                           {
        //                                                               Chapa = memberinfo.Chapa,
        //                                                               Nome = memberinfo.Nome,
        //                                                               CodSituacao = memberinfo.CodSituacao,
        //                                                               CodColigada = memberinfo.CodColigada,
        //                                                               Funcao = memberinfo.Funcao,
        //                                                               Secao = memberinfo.Secao,
        //                                                           },
        //                                                           LeaderCodPessoa = item.LeaderCodPessoa,
        //                                                           LeaderInfo = new employeeNewInformationModel
        //                                                           {
        //                                                               Chapa = leaderinfo.Chapa,
        //                                                               Nome = leaderinfo.Nome,
        //                                                               CodSituacao = leaderinfo.CodSituacao,
        //                                                               CodColigada = leaderinfo.CodColigada,
        //                                                               Funcao = leaderinfo.Funcao,
        //                                                               Secao = leaderinfo.Secao,
        //                                                           },
        //                                                           DataRegistro = item.DataRegistro,
        //                                                           Status = item.Status,
        //                                                           intStatus = item.intStatus,
        //                                                           IdObra = item.IdObra,
        //                                                           ObraName = item.ObraName,
        //                                                       }).ToList();



        //    return completeDetail ?? new List<ItemReservationDetailModel>();

        //}

        #endregion

        #region Ferramentaria Actions

        //public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome, string? returnUrl)
        //{
        //    var currentController = RouteData.Values["controller"]?.ToString();

        //    if (Ferramentaria != null)
        //    {
        //        httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
        //        httpContextAccessor.HttpContext.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
        //    }

        //    //return RedirectToAction(nameof(Index));
        //    return RedirectToAction(returnUrl);
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
