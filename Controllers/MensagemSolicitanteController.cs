using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesRM;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.Models;
using AutoMapper;
using FerramentariaTest.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.Helpers;
using System;

namespace FerramentariaTest.Controllers
{
    public class MensagemSolicitanteController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thMsg.aspx";
        private MapperConfiguration mapeamentoClasses;

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public MensagemSolicitanteController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Funcionario, FuncionarioViewModel>();
                cfg.CreateMap<FuncionarioViewModel, Funcionario>();
            });
        }


        // GET: MensagemSolicitante
        public ActionResult Index(string? IdNumber)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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

                            UserViewModel? UsuarioModel = new UserViewModel();
                            List<MensagemSolicitanteViewModel> messages = new List<MensagemSolicitanteViewModel>();

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                            //if (FuncionarioValue != null)
                            //{
                            //    UsuarioModel = searches.SearchEmployeeOnLoad();
                            //    //messages = searches.SearchMensagem(UsuarioModel.Chapa);

                            //    //if (messages.Count > 0)
                            //    //{
                            //    //    ViewBag.Messages = messages;
                            //    //}
                            //}


                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

                            return View(UsuarioModel);



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
                return View(ex);
            }

        }

        public ActionResult SearchEmployee(string? IdNumber)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchEmployee";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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

                        Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
                        List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
                        List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
                        List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
                        UserViewModel? UsuarioModel = new UserViewModel();

                        if (IdNumber != null)
                        {
                            listTerceiroResult = searches.SearchTercerio(IdNumber);
                            TotalResult.AddRange(listTerceiroResult);

                            listEmployeeResult = searches.SearchEmployeeChapa(IdNumber);
                            TotalResult.AddRange(listEmployeeResult);

                            if (TotalResult.Count > 1)
                            {
                                ViewBag.ListOfEmployees = TotalResult;
                                return View("Index", UsuarioModel);
                            }
                            else if (TotalResult.Count == 1)
                            {
                                //httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                                //httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, TotalResult[0].Chapa);

                                UsuarioModel = searches.GetEmployeeDetails(TotalResult[0].Chapa);
                                List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();

                                messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser?.Id);
                                ViewBag.Messages = messages.Count > 0 ? messages : null;

                                return View("Index", UsuarioModel);
                                //return RedirectToAction(nameof(Index));
                            }
                            else if (listEmployeeResult.Count == 0)
                            {
                                ViewBag.Error = "No Searched has been found.";
                                return View("Index", UsuarioModel);
                            }

                            return View("Index", UsuarioModel);
                        }
                        else
                        {
                            ViewBag.Error = "Matricula/Nome is Required";
                            return View("Index", UsuarioModel);
                        }




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


            //if (usuariofer.Permissao.Visualizar == 1)
            //{
               
            //}
            //else
            //{
            //    ViewBag.Error = "O usuário não tem permissão para visualizar dados";
            //    return View(nameof(Index));
            //}
     
        }

        public ActionResult SelectedUser(string? chapa)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();

            if (chapa != null)
            {
                httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, chapa);

                UsuarioModel = searches.GetEmployeeDetails(chapa);
                List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();

                messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                ViewBag.Messages = messages.Count > 0 ? messages : null;

                return View("Index", UsuarioModel);
            }

            return View(nameof(Index));
            //return RedirectToAction(nameof(Index));
        }

        public ActionResult DeleteMessage(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/DeleteMessage";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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
                    if (checkPermission.Excluir == 1)
                    {


                        if (id != null)
                        {
                            var DeleteMessage = _context.MensagemSolicitante.FirstOrDefault(i => i.Id == id);
                            if (DeleteMessage != null)
                            {
                                DeleteMessage.Ativo = 0;
                                _context.SaveChanges();
                            }

                            TempData["ShowSuccessAlert"] = true;
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Chapa is Empty";
                            return RedirectToAction(nameof(Index));
                        }



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

            //if (usuariofer.Permissao.Excluir == 1)
            //{
              
            //}
            //else
            //{
            //    ViewBag.Error = "O usuário não tem permissão para inserir dados";
            //    return View(nameof(Index));
            //}
                 
        }

        // POST: MensagemSolicitante/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string? chapa, int? coligada, int? fix, string? mensagem)
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
                //    if (usuariofer.Permissao.Inserir != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de Inserir a página de Mensagem Solicitante!";
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
                        if (checkPermission.Inserir == 1)
                        {



                            if (chapa == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Chapa is Empty";

                                log.LogWhy = "Erro na validação do modelo em criaçao ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Index");
                            }

                            if (coligada == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Please Search User";

                                log.LogWhy = "Erro na validação do modelo em criaçao ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Index");
                            }

                            if (mensagem == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Mensagem is Empty";

                                log.LogWhy = "Erro na validação do modelo em criaçao ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Index");
                            }

                            if (fix == null)
                            {
                                fix = 0;
                            }

                            var insertMensagem = new MensagemSolicitante
                            {
                                IdTerceiro = 0,
                                CodColigada = coligada,
                                Chapa = chapa,
                                IdUsuario_Adicionou = loggedUser?.Id,
                                Mensagem = mensagem,
                                Fixar = fix,
                                DataRegistro = DateTime.Now,
                                Ativo = 1
                            };

                            _context.Add(insertMensagem);
                            _context.SaveChanges();

                            log.LogWhy = "Mensagem Solicitante adicionada com sucesso";
                            auxiliar.GravaLogSucesso(log);

                            TempData["ShowSuccessAlert"] = true;
                            return RedirectToAction(nameof(Index));


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


    }
}
