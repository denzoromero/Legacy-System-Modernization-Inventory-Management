using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.Controllers
{
    public class AlertaVencimentoCA : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thItens.aspx";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        public AlertaVencimentoCA(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: AlertaVencimentoCA
        public IActionResult Index()
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuariofer.Pagina = pagina;
                //usuariofer.Pagina1 = log.LogWhat;
                //usuariofer.Acesso = log.LogWhat;
                //usuariofer = auxiliar.VerificaPermissao(usuariofer);

                //if (usuariofer.Permissao == null)
                //{
                //    usuariofer.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuariofer.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //}
                //else
                //{
                //    if (usuariofer.Permissao.Visualizar != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuariofer.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //    }
                //}
                //#endregion

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {


                            //if (LoggedUserDetails.Id == null)
                            //{
                            //    LoggedUserDetails = usuariofer;
                            //}

                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            int? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

                            if (FerramentariaValue == null)
                            {
                                var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                                          where ferramentaria.Ativo == 1 &&
                                                                !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                                _context.FerramentariaVsLiberador.Any(l => l.IdLogin == loggedUser.Id && l.IdFerramentaria == ferramentaria.Id)
                                                          orderby ferramentaria.Nome
                                                          select new
                                                          {
                                                              Id = ferramentaria.Id,
                                                              Nome = ferramentaria.Nome
                                                          }).ToList();

                                if (ferramentariaItems != null)
                                {
                                    ViewBag.FerramentariaItems = ferramentariaItems;
                                }

                                return PartialView("_FerramentariaPartialView");

                            }

                            var result = _context.AlertaAutomaticoVencimentoCA
                                  .Where(a => a.Destinatario == loggedUser.Email)
                                  .Select(a => a.Destinatario)
                                  .FirstOrDefault();

                            if (result != null)
                            {
                                ViewBag.Result = 1;
                            }
                            else
                            {
                                ViewBag.Result = 0;
                            }

                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

                            return View();



                        }
                        else
                        {
                            return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });
                        }
                    }
                    else
                    {
                        log.LogWhy = "Permission is Empty";
                        return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });
                    }
                }
                else
                {
                    log.LogWhy = "Session Expired";
                    return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });
                }

              
            }
            catch (Exception ex)
            {
                log.LogWhy = ex.Message;
                ErrorViewModel erro = new ErrorViewModel();
                erro.Tela = log.LogWhere;
                erro.Descricao = log.LogWhy;
                erro.Mensagem = log.LogWhat;
                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                return View();
            }

        }

        public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome)
        {
            if (Ferramentaria != null)
            {
                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult RefreshFerramentaria()
        {
            httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);
            httpContextAccessor.HttpContext.Session.Remove(Sessao.FerramentariaNome);
            return RedirectToAction(nameof(Index));
        }

        // GET: AlertaVencimentoCA/Details/5
        public ActionResult yourFormAction(int? AlertaValueTest)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuariofer.Pagina = pagina;
                //usuariofer.Pagina1 = log.LogWhat;
                //usuariofer.Acesso = log.LogWhat;
                //usuariofer = auxiliar.VerificaPermissao(usuariofer);

                //if (usuariofer.Permissao == null)
                //{
                //    usuariofer.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuariofer.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //}
                //else
                //{
                //    if (usuariofer.Permissao.Visualizar != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuariofer.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //    }
                //}
                //#endregion

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {

                            if (AlertaValueTest == 1)
                            {
                                //remove
                                var alertaToDelete = _context.AlertaAutomaticoVencimentoCA
                                                .FirstOrDefault(a => a.Destinatario == loggedUser.Email);

                                if (alertaToDelete != null)
                                {
                                    _context.AlertaAutomaticoVencimentoCA.Remove(alertaToDelete);
                                    _context.SaveChanges();
                                }

                                TempData["ShowSuccessAlert"] = true;
                            }
                            else
                            {
                                //add
                                var newAlerta = new AlertaAutomaticoVencimentoCA
                                {
                                    Destinatario = loggedUser.Email
                                };

                                _context.AlertaAutomaticoVencimentoCA.Add(newAlerta);
                                _context.SaveChanges();

                                TempData["ShowSuccessAlert"] = true;
                            }

                            return RedirectToAction(nameof(Index));
                            //return View();



                        }
                        else
                        {
                            return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });
                        }
                    }
                    else
                    {
                        log.LogWhy = "Permission is Empty";
                        return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });
                    }
                }
                else
                {
                    log.LogWhy = "Session Expired";
                    return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });
                }

         



            }
            catch (Exception ex)
            {
                log.LogWhy = ex.Message;
                ErrorViewModel erro = new ErrorViewModel();
                erro.Tela = log.LogWhere;
                erro.Descricao = log.LogWhy;
                erro.Mensagem = log.LogWhat;
                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                return View();
            }

        }

        // GET: AlertaVencimentoCA/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AlertaVencimentoCA/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuariofer.Pagina = pagina;
                //usuariofer.Pagina1 = log.LogWhat;
                //usuariofer.Acesso = log.LogWhat;
                //usuariofer = auxiliar.VerificaPermissao(usuariofer);

                //if (usuariofer.Permissao == null)
                //{
                //    usuariofer.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuariofer.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //}
                //else
                //{
                //    if (usuariofer.Permissao.Visualizar != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuariofer.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //    }
                //}
                //#endregion

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AlertaVencimentoCA/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AlertaVencimentoCA/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AlertaVencimentoCA/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AlertaVencimentoCA/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
