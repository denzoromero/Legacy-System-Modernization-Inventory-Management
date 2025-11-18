using FerramentariaTest.Services;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FerramentariaTest.Controllers
{
    public class BaseController : Controller
    {

        private readonly IFerramentariaService _ferramentariaService;
        private readonly ICorrelationIdService _correlationIdService;
        protected ILogger<BaseController> _baseLogger;

        public BaseController(IFerramentariaService ferramentariaService, ILogger<BaseController> logger, ICorrelationIdService correlationIdService)
        {
            _ferramentariaService = ferramentariaService;
            _baseLogger = logger;
            _correlationIdService = correlationIdService;
        }

        [HttpPost]
        public virtual async Task<IActionResult> SetFerramentariaValue(int ferramentaria, string selectedNome, string returnUrl = null)
        {
            try
            {

                if (ferramentaria == 0)
                {
                    _baseLogger.LogWarning("Ferramentaria is 0");
                    throw new ArgumentException("Ferramentaria is 0");
                }

                await _ferramentariaService.SetFerramentariaValue(ferramentaria);
               
                return Redirect(returnUrl ?? Request.Headers["Referer"].ToString() ?? "/");

            }
            catch (ArgumentException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                           new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
                           );
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                           new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable." }
                           );
            }
            catch (SqlException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                            new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred" }
                            );
            }
            catch (TimeoutException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                         new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out" }
                         );
            }
            catch (Exception ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                      new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Unexpected Error Occured" }
                      );
            }
        }

        [HttpGet]
        public virtual ActionResult RefreshFerramentaria()
        {
            try
            {
                _ferramentariaService.RefreshChosenFerramentaria();

                string referer = Request.Headers["Referer"].ToString();

                Uri refererUri = new Uri(referer);
                string path = refererUri.AbsolutePath;

                string[] segments = path.Trim('/').Split('/');

                if (segments.Length >= 2)
                {
                    string controller = segments[0];
                    string action = segments[1];
                    return RedirectToAction(action, controller);
                }
                else if (segments.Length == 1)
                {
                    // Only controller specified, redirect to default action
                    return RedirectToAction("Index", segments[0]);
                }
                else
                {
                    // Fallback to home
                    return RedirectToAction("Index", "Home");
                }


                //return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }
            catch (ArgumentException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                           new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
                           );
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                           new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable." }
                           );
            }
            catch (SqlException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                            new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred" }
                            );
            }
            catch (TimeoutException ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                         new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out" }
                         );
            }
            catch (Exception ex)
            {
                return RedirectToAction(actionName: nameof(HomeController.ErrorHandler), controllerName: nameof(HomeController).Replace("Controller", ""),
                      new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - Unexpected Error Occured" }
                      );
            }
        }



    }
}
