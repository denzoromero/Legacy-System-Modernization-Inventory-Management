using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Entities;
using Microsoft.EntityFrameworkCore;
using System;


using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.IdentityModel.Tokens;
using FerramentariaTest.EntitiesRM;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using FerramentariaTest.Services.Interfaces;
using AutoMapper;
using System.Linq;
using FerramentariaTest.Services;

namespace FerramentariaTest.Controllers
{
    public class AdminSide : Controller
    {

        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;

        private readonly ILogger<AdminSide> _logger;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IAdminService _adminService;
        private readonly ICatalogService _catalogService;




        private static string pagina = "AdminSide.cs";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public AdminSide(IHttpContextAccessor httpCA, IConfiguration configuration, ILogger<AdminSide> logger, IUserContextService userService, ICorrelationIdService correlationIdService, IAdminService adminService, ICatalogService catalogService)
        {
            httpContextAccessor = httpCA;
            _configuration = configuration;
            _logger = logger;
            _userContext = userService;
            _correlationIdService = correlationIdService;
            _adminService = adminService;
            _catalogService = catalogService;
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: Admin.Index", user.Id);

                if (TempData.ContainsKey("ShowSuccessAlertLeader"))
                {
                    ViewBag.SuccessAlertNew = TempData["ShowSuccessAlertLeader"]?.ToString();
                    TempData.Remove("ShowSuccessAlertLeader"); // Remove it from TempData to avoid displaying it again
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

        //[HttpPost]
        //public IActionResult UploadCatalogImage(IFormFile imgFile)
        //{
        //    int IdCatalogo = 1942;

        //    byte[] imageData;
        //    using (var stream = new MemoryStream())
        //    {
        //        imgFile.CopyTo(stream);
        //        imageData = stream.ToArray();
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        imgFile.CopyToAsync(memoryStream);

        //        Catalogo catalog = _context.Catalogo.FirstOrDefault(i => i.Id == IdCatalogo);

        //        catalog.ImageData = imageData;

        //        _context.Update(catalog);
        //        _context.SaveChanges();

        //    }

        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> InsertLeader(AddLeaderSubmitModel? LeaderInfo)
        {

            try
            {

                if (LeaderInfo == null) throw new ModifiedErrorException("LeaderInfo is null");

                var validations = new Dictionary<object?, string>
                {
                    [LeaderInfo.CodPessoaMember] = "CodPessoaMember",
                    [LeaderInfo.ChapaMember] = "ChapaMember",
                    [LeaderInfo.NomeMember] = "NomeMember",
                    [LeaderInfo.IdUser] = "IdUser"
                };

                var missingField = validations.FirstOrDefault(x => x.Key == null);
                if (missingField.Value != null) throw new ModifiedErrorException($"{missingField.Value} is empty");

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to InsertLeader for Matricula:{Matricula}", user.Id, LeaderInfo.ChapaMember);

                LeaderData leader = new LeaderData()
                {
                    CodPessoa = LeaderInfo.CodPessoaMember,
                    Chapa = LeaderInfo.ChapaMember,
                    Nome = LeaderInfo.NomeMember,
                    Ativo = 1,
                    DataRegistro = DateTime.Now,
                    IdUser = LeaderInfo.IdUser,
                };

                await _adminService.InsertNewLeader(leader);

                return Ok(new { success = true, message = "Leader Successfully inserted." });

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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

            //if (LeaderInfo == null)
            //{
            //    ViewBag.Error = "LeaderInfo is empty";
            //    return View(nameof(Index));
            //}

            //if (LeaderInfo.CodPessoaMember == null)
            //{
            //    ViewBag.Error = "CodPessoaMember is empty";
            //    return View(nameof(Index));
            //}
            //if (LeaderInfo.ChapaMember == null)
            //{
            //    ViewBag.Error = "ChapaMember is empty";
            //    return View(nameof(Index));
            //}
            //if (LeaderInfo.NomeMember == null)
            //{
            //    ViewBag.Error = "NomeMember is empty";
            //    return View(nameof(Index));
            //}
            //if (LeaderInfo.IdUser == null)
            //{
            //    ViewBag.Error = "IdUser is empty";
            //    return View(nameof(Index));
            //}


 

            //using (var transaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {

            //        LeaderData entity = new LeaderData()
            //        {
            //            CodPessoa = LeaderInfo.CodPessoaMember,
            //            Chapa = LeaderInfo.ChapaMember,
            //            Nome = LeaderInfo.NomeMember,
            //            Ativo = 1,
            //            DataRegistro = DateTime.Now,
            //            IdUser = LeaderInfo?.IdUser,
            //        };

            //        _context.Add(entity);

            //        _context.SaveChanges();
            //        transaction.Commit();

            //        TempData["ShowSuccessAlertLeader"] = "Success";
            //        return RedirectToAction(nameof(Index));

            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback();
            //        ViewBag.Error = $"SERVER ERROR: {ex.Message}";
            //        return View(nameof(Index));
            //    }
            //}




            //return View(nameof(Index));
        }




        public IActionResult ImageCatalog()
        {
            return View();
        }

        public async Task<IActionResult> SearchCatalog(CatalogoSearchModel filter,int CurrentPage)
        {
            try
            {

                List<CatalogoViewModel>? result = await _catalogService.SearchCatalog(filter);

                if (result == null || result.Count == 0) throw new ModifiedErrorException("No result found.");

                int totalPages = (int)Math.Ceiling((double)result.Count / 10);

                return Ok(new { catalogResult = result.Skip(CurrentPage * 10).Take(10), totalPages });

                //return Ok(result);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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

        public async Task<IActionResult> InsertCatalogImage(IFormFile imgFile, int IdCatalogo)
        {
            try
            {
                if (IdCatalogo <= 0) throw new ModifiedErrorException("IdTerms is 0");

                if (imgFile == null || imgFile.Length == 0) throw new ArgumentException("No file uploaded.");

                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/pjpeg" };
                if (!allowedContentTypes.Contains(imgFile.ContentType))
                    throw new ArgumentException("File must be a JPEG image.");

                if (imgFile.Length > 10 * 1024 * 1024) throw new ArgumentException("File size exceeds 10MB limit.");

                using (var memoryStream = new MemoryStream())
                {
                    await imgFile.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    await _catalogService.UpdateCatalogImage(fileBytes, IdCatalogo);

                    string ImageByteString = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}";

                    return Ok(ImageByteString);
                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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

        public async Task<IActionResult> CheckImage(int IdCatalogo)
        {
            try
            {

                if (IdCatalogo <= 0) throw new ModifiedErrorException("IdCatalogo is invalid.");

                return Ok(await _catalogService.GetImageString(IdCatalogo));

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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

        [HttpGet]
        public async Task<IActionResult> GetEmployeeInformation(string? givenInfo)
        {
            try
            {
                if (givenInfo == null) throw new ModifiedErrorException("Please provide chapa");

                FuncionarioModel result = await _adminService.GetEmployeeForLeader(givenInfo);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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


      

            //FuncionarioModel? result = await getEmployeeInformationAsync(givenInfo) ?? new FuncionarioModel();
            //if (result == null) return Json(new { success = false, message = $"No employee found with the given info {givenInfo}." });

            //UsuarioBS? checkuser = _contextBS.Usuario.FirstOrDefault(i => i.Chapa == result.Chapa && i.Ativo == 1);
            //if (checkuser == null) return Json(new { success = false, message = $"Chapa:{result.Chapa} - {result.Nome} is not yet registered in the SIB." });
            //result.IdUserSib = checkuser.Id;

            //LeaderData? leadercheck = _context.LeaderData.FirstOrDefault(i => i.CodPessoa == result.CodPessoa);
            //if (leadercheck != null) return Json(new { success = false, message = $"Chapa:{result.Chapa} - {result.Nome} is already listed as a Leader." });
       
            //return Json(new { success = true, memberinfo = result });

        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderInformation(string? givenInfo)
        {

            try
            {

                List<FuncionarioModel> testinfo = await _adminService.getLeaderList(givenInfo, 1);

                return Ok(testinfo);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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

          

            //if (givenInfo.IsNullOrEmpty()) return Json(new { success = true, message = "filter is empty." });

            //List<LeaderData>? leaderList = _context.LeaderData.Where(i => (i.Chapa == null || i.Chapa == givenInfo) || (i.Nome == null || i.Nome.Contains(givenInfo))).ToList();
            //if (leaderList == null) return Json(new { success = true, message = "No result found." });

            //List<FuncionarioModel>? completeInfo = (from leader in leaderList
            //                                        //join funcionario in _contextBS.Funcionario on leader.CodPessoa equals funcionario.CodPessoa
            //                                        join recentFunc in (
            //                                                from f in _contextBS.Funcionario
            //                                                group f by f.CodPessoa into g
            //                                                select g.OrderByDescending(x => x.DataMudanca).FirstOrDefault()
            //                                            ) on leader.CodPessoa equals recentFunc.CodPessoa
            //                                        select new FuncionarioModel()
            //                                        {
            //                                            IdLeader = leader.Id,
            //                                            IdUserSib = leader.IdUser,
            //                                            IdTerceiro = 0,
            //                                            CodPessoa = recentFunc.CodPessoa,
            //                                            CodColigada = recentFunc.CodColigada,
            //                                            Chapa = recentFunc.Chapa,
            //                                            Nome = recentFunc.Nome,
            //                                            CodSituacao = recentFunc.CodSituacao,
            //                                            Secao = recentFunc.Secao,
            //                                            Funcao = recentFunc.Funcao,
            //                                            AtivoLeader = leader.Ativo
            //                                        }).ToList();

            //if (completeInfo == null) return Json(new { success = true, message = "cant find result in Funcionario." });

            ////List<FuncionarioModel>? withImage = (from info in completeInfo
            ////                                     join pessoa in _contextRM.PPESSOA on info.CodPessoa equals pessoa.CODIGO
            ////                                     join gImagem in _contextRM.GIMAGEM on pessoa.IDIMAGEM equals gImagem.ID
            ////                                     select new FuncionarioModel()
            ////                                     {
            ////                                         IdLeader = info.IdLeader,
            ////                                         IdUserSib = info.IdUserSib,
            ////                                         IdTerceiro = 0,
            ////                                         CodPessoa = info.CodPessoa,
            ////                                         CodColigada = info.CodColigada,
            ////                                         Chapa = info.Chapa,
            ////                                         Nome = info.Nome,
            ////                                         CodSituacao = info.CodSituacao,
            ////                                         Secao = info.Secao,
            ////                                         Funcao = info.Funcao,
            ////                                         AtivoLeader = info.AtivoLeader,
            ////                                         ImageStringByte = gImagem.IMAGEM != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(gImagem.IMAGEM)}" : string.Empty,
            ////                                     }).ToList();

            //return Json(new { success = true, leaderInfo = completeInfo });


        }


        [HttpPost]
        public async Task<IActionResult> DeactiveLeader(int id)
        {

            try
            {
                if (id <= 0) throw new ModifiedErrorException("Id is invalid");

                await _adminService.DeactivateLeader(id);

                return Ok(true);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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


            //LeaderData? checkLeader = _context.LeaderData.FirstOrDefault(i => i.Id == id);
            //if (checkLeader == null) return Json(new { success = false, message = $"Leader Id:{id} not found." });

            //using (var transaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {

            //        checkLeader.Ativo = 0;

            //        _context.Update(checkLeader);

            //        _context.SaveChanges();
            //        transaction.Commit();

            //        return Json(new { success = true });

            //        //TempData["ShowSuccessAlertLeader"] = "Success";
            //        //return RedirectToAction(nameof(Index));

            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback();
            //        //ViewBag.Error = $"SERVER ERROR: {ex.Message}";
            //        return Json(new { success = false, message = $"SERVER ERROR: {ex.Message}" });
            //    }
            //}

        }

        [HttpPost]
        public async Task<IActionResult> ReactiveLeader(int id)
        {
            try
            {
                if (id <= 0) throw new ModifiedErrorException("Id is invalid");

                await _adminService.ReactivateLeader(id);

                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
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


            //LeaderData? checkLeader = _context.LeaderData.FirstOrDefault(i => i.Id == id);
            //if (checkLeader == null) return Json(new { success = false, message = $"Leader Id:{id} not found." });

            //using (var transaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {

            //        checkLeader.Ativo = 1;

            //        _context.Update(checkLeader);

            //        _context.SaveChanges();
            //        transaction.Commit();

            //        return Json(new { success = true });

            //        //TempData["ShowSuccessAlertLeader"] = "Success";
            //        //return RedirectToAction(nameof(Index));

            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback();
            //        //ViewBag.Error = $"SERVER ERROR: {ex.Message}";
            //        return Json(new { success = false, message = $"SERVER ERROR: {ex.Message}" });
            //    }
            //}

        }


        #endregion

        //private async Task<FuncionarioModel> getEmployeeInformationAsync(string? chapa)
        //{
        //    Funcionario? func = await _contextBS.Funcionario.Where(i => i.Chapa == chapa).OrderByDescending(i => i.DataMudanca).FirstOrDefaultAsync();
        //    if (func != null)
        //    {
        //        byte[]? base64Image = await (from pessoa in _contextRM.PPESSOA
        //                                     join gImagem in _contextRM.GIMAGEM
        //                                     on pessoa.IDIMAGEM equals gImagem.ID
        //                                     where pessoa.CODIGO == func.CodPessoa
        //                                     select gImagem.IMAGEM).FirstOrDefaultAsync();

        //        FuncionarioModel detailedFunc = new FuncionarioModel()
        //        {
        //            IdTerceiro = 0,
        //            CodPessoa = func.CodPessoa,
        //            CodColigada = func.CodColigada,
        //            Chapa = func.Chapa,
        //            Nome = func.Nome,
        //            CodSituacao = func.CodSituacao,
        //            Secao = func.Secao,
        //            Funcao = func.Funcao,
        //            ImageStringByte = $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}"
        //        };

        //        return detailedFunc ?? new FuncionarioModel();

        //    }


        //    return new FuncionarioModel();
        //}

    }
}
