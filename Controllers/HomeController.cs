using FerramentariaTest.Models;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using FerramentariaTest.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using FerramentariaTest.Services.Interfaces;

namespace FerramentariaTest.Controllers
{
    public class HomeController : Controller
    {

        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "Home";

        private readonly ILogger<HomeController> _logger;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IUserService _userService;

        public HomeController(IHttpContextAccessor httpCA, IConfiguration configuration, ILogger<HomeController> logger, ICorrelationIdService correlationIdService, IUserService userService)
        {
            httpContextAccessor = httpCA;
            _configuration = configuration;
            _logger = logger;
            _correlationIdService = correlationIdService;
            _userService = userService;
        }


        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred";
                return View();
            }
        }

        public async Task<IActionResult> Login()
        {

            return View();
        }

        public IActionResult PreserveActionError(string? Error)
        {
            ViewBag.ErrorHandlerReturn = Error;
            return View(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind("SAMAccountName,Senha")] VW_Usuario_NewViewModel usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    FerramentariaTest.EntitiesBS.Usuario userInformation = await _userService.ValidateUser(usuario);

                    var sessionId = Guid.NewGuid().ToString();

                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, userInformation.Id.ToString()),
                                        new Claim(ClaimTypes.Name,  userInformation?.Chapa),
                                        new Claim("Chapa",  userInformation?.Chapa),
                                        new Claim(ClaimTypes.Role, "Demo"),
                                        new Claim("SessionId", sessionId),
                                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    _logger.LogInformation("User:{UserId} - Attempt Successful LoginAction - SessionId: {SessionId}", userInformation.Id, sessionId);

                    return RedirectToAction(nameof(Index));

                }
                else
                {
                    return View(usuario);
                }

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

		public IActionResult Logout()
        {
            httpContextAccessor?.HttpContext?.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.Clear();

            return RedirectToAction(nameof(Login));

   //         Log log = new Log();
			//log.LogWhat = pagina + "/Login";
			//log.LogWhere = pagina;
			//Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

   //         try
   //         {
			//	#region Authenticate User
			//	VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
			//	//usuario.Pagina = "Home/Index";
			//	usuariofer.Pagina = pagina;
			//	usuariofer.Pagina1 = log.LogWhat;
			//	usuariofer.Acesso = log.LogWhat;
			//	usuariofer = auxiliar.VerificaPermissao(usuariofer);

			//	if (usuariofer.Permissao == null)
			//	{
			//		usuariofer.Retorno = "Usuário sem permissão na página!";
			//		log.LogWhy = usuariofer.Retorno;
			//		auxiliar.GravaLogAlerta(log);
			//		return RedirectToAction("Login", "Home", usuariofer);
			//	}
			//	else
			//	{
			//		if (usuariofer.Permissao.Visualizar != 1)
			//		{
			//			usuariofer.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
			//			log.LogWhy = usuariofer.Retorno;
			//			auxiliar.GravaLogAlerta(log);
			//			return RedirectToAction("Login", "Home", usuariofer);
			//		}
			//	}
			//	#endregion

            
   //         }
			//catch (Exception ex)
			//{
			//	//return View(usuario);

			//	log.LogWhy = ex.Message;
			//	ErrorViewModel erro = new ErrorViewModel();
			//	erro.Tela = log.LogWhere;
			//	erro.Descricao = log.LogWhy;
			//	erro.Mensagem = log.LogWhat;
			//	erro.IdLog = auxiliar.GravaLogRetornoErro(log);
			//	return View();
			//}		
		}

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ErrorHandler(string? message)
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}