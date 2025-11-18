using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using AutoMapper;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using X.PagedList;
using System.Drawing.Printing;
using System.Web;

namespace FerramentariaTest.Controllers
{
    public class thBloqueioEmprestimoAoSolicitanteController : Controller
    {

        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thBloqueioEmprestimoAoSolicitante.aspx";
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        public thBloqueioEmprestimoAoSolicitanteController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
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

                cfg.CreateMap<Funcionario, FuncionarioBlockViewModel>();
                cfg.CreateMap<FuncionarioBlockViewModel, Funcionario>();
            });
        }

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
                //        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de BloqueioEmprestimoAoSolicitante!";
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

                            FuncionarioBlockViewModel? emptyData = new FuncionarioBlockViewModel();
                            UserViewModel? UsuarioModel = new UserViewModel();

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }


                            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                            //if (FuncionarioValue != null)
                            //{
                            //    UsuarioModel = searches.SearchEmployeeOnLoad();
                            //    //ViewBag.Messages = searches.SearchMensagem(UsuarioModel.Chapa);

                            //    var mapper = mapeamentoClasses.CreateMapper();
                            //    emptyData.UserViewModel = mapper.Map<UserViewModel>(UsuarioModel);
                            //    emptyData.Message = searches.SearchBloqueioMessage(UsuarioModel.Chapa);

                            //}
                            //else
                            //{
                            //    emptyData.UserViewModel = UsuarioModel;
                            //}

                            emptyData.UserViewModel = UsuarioModel;

                            log.LogWhy = "BloqueioEmprestimoAoSolicitante carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(emptyData);

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

                //if (LoggedUserDetails.Id == null)
                //{
                //    LoggedUserDetails = usuariofer;
                //}



             

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

        public ActionResult GetEmployee(string? filter)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
            FuncionarioBlockViewModel? funcionario = new FuncionarioBlockViewModel();
            UserViewModel? UsuarioModel = new UserViewModel();

            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            funcionario.UserViewModel = UsuarioModel;

            var mapper = mapeamentoClasses.CreateMapper();

            if (filter != null)
            {
                listTerceiroResult = searches.SearchTercerio(filter);
                TotalResult.AddRange(listTerceiroResult);

                listEmployeeResult = searches.SearchEmployeeChapa(filter);
                TotalResult.AddRange(listEmployeeResult);

                if (TotalResult.Count > 1)
                {
                    ViewBag.ListOfEmployees = TotalResult;
                    return View("Index", funcionario);
                }
                else if (TotalResult.Count == 1)
                {
                    //httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                    //httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, TotalResult[0].Chapa);

                   
                    UsuarioModel = searches.GetEmployeeDetails(TotalResult[0].Chapa);
                    List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();

                     messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);                   
                     ViewBag.Messages = messages.Count > 0 ? messages : null;

                    funcionario.UserViewModel = mapper.Map<UserViewModel>(UsuarioModel);
                    funcionario.Message = searches.SearchBloqueioMessage(UsuarioModel.Chapa);

                    return View("Index", funcionario);
                    //return RedirectToAction(nameof(Index));
                }
                else if (listEmployeeResult.Count == 0)
                {
                    ViewBag.Error = "No Searched has been found.";
                    return View("Index", funcionario);
                }

            }
            else
            {
                ViewBag.Error = "Please input Matricula/Nome";
                return View("Index", funcionario);
            }

            return View("Index", funcionario);

        }

        public ActionResult SelectedUser(string? chapa)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();

            FuncionarioBlockViewModel? funcionario = new FuncionarioBlockViewModel();

            var mapper = mapeamentoClasses.CreateMapper();

            if (chapa != null)
            {
                //httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                //httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, chapa);

                UsuarioModel = searches.GetEmployeeDetails(chapa);
                List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();

                messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                ViewBag.Messages = messages.Count > 0 ? messages : null;

                funcionario.UserViewModel = mapper.Map<UserViewModel>(UsuarioModel);
                funcionario.Message = searches.SearchBloqueioMessage(UsuarioModel.Chapa);

                return View("Index", funcionario);
            }

            return View(nameof(Index));
            //return RedirectToAction(nameof(Index));
        }


        // POST: thBloqueioEmprestimoAoSolicitanteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string? mensagem, string? chapa, int? coligada)
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
                //        usuariofer.Retorno = "Usuário sem permissão de Inserir a página de BloqueioEmprestimoAoSolicitante!";
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


                            FuncionarioBlockViewModel funcionario = new FuncionarioBlockViewModel();

                            if (chapa == null)
                            {
                                //ViewBag.Error = "Chapa is Empty";
                                log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                TempData["ErrorMessage"] = "Chapa is Empty";
                                //return RedirectToAction("GetEmployee");
                                return RedirectToAction(nameof(Index));
                            }

                            if (coligada == null)
                            {
                                TempData["ErrorMessage"] = "Chapa is Empty";

                                log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                //return RedirectToAction("GetEmployee");
                                return RedirectToAction(nameof(Index));
                            }

                            if (mensagem == null)
                            {
                                ViewBag.Error = "Mensagem is Empty";
                                TempData["ErrorMessage"] = "Mensagem is Empty";

                                log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction(nameof(Index));
                                //return RedirectToAction("GetEmployee", new { filter = chapa });
                            }

                            var infocheck = _context.BloqueioEmprestimoAoSolicitante.Where(i => i.Chapa == chapa).FirstOrDefault();

                            if (infocheck == null)
                            {
                                var InsertBlockSolicitante = new BloqueioEmprestimoAoSolicitante
                                {
                                    IdTerceiro = 0,
                                    CodColigada = coligada,
                                    Chapa = chapa,
                                    IdUsuario_Adicionou = loggedUser?.Id,
                                    Mensagem = mensagem,
                                    DataRegistro = DateTime.Now,
                                    Ativo = 1
                                };

                                _context.Add(InsertBlockSolicitante);
                                _context.SaveChanges();

                                TempData["ShowSuccessAlert"] = true;
                                ViewBag.ShowSuccessAlert = "Success";

                                log.LogWhy = "BloqueioEmprestimoAoSolicitante block com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                return RedirectToAction(nameof(Index));
                                //return RedirectToAction("GetEmployee", new { filter = chapa });
                            }
                            else
                            {
                                if (infocheck.Ativo != 0)
                                {
                                    infocheck.IdUsuario_Adicionou = loggedUser?.Id;
                                    infocheck.Mensagem = mensagem;

                                    TempData["ShowSuccessAlert"] = true;
                                    ViewBag.ShowSuccessAlert = "Success";
                                    _context.SaveChanges();

                                    return RedirectToAction(nameof(Index));
                                    //return RedirectToAction("GetEmployee", new { filter = chapa });
                                }
                                else
                                {
                                    infocheck.Ativo = 1;
                                    infocheck.IdUsuario_Adicionou = loggedUser?.Id;
                                    infocheck.Mensagem = mensagem;

                                    TempData["ShowSuccessAlert"] = true;
                                    ViewBag.ShowSuccessAlert = "Success";
                                    _context.SaveChanges();

                                    return RedirectToAction(nameof(Index));
                                    //return RedirectToAction("GetEmployee", new { filter = chapa });
                                }
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

        public ActionResult DeleteMessage(int? id)
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


        // POST: thBloqueioEmprestimoAoSolicitanteController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string? removeChapa)
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
                //    if (usuariofer.Permissao.Excluir != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de Excluir a página de BloqueioEmprestimoAoSolicitante!";
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


                            if (removeChapa != null)
                            {
                                var removeData = _context.BloqueioEmprestimoAoSolicitante.Where(o => o.Chapa == removeChapa).FirstOrDefault();

                                if (removeData != null)
                                {
                                    if (removeData.Ativo == 1)
                                    {
                                        removeData.Ativo = 0;
                                        removeData.IdUsuario_Excluiu = loggedUser?.Id;
                                        _context.SaveChanges();
                                    }

                                    TempData["ShowSuccessAlert"] = true;

                                    log.LogWhy = "BloqueioEmprestimoAoSolicitante deativar com sucesso!";
                                    auxiliar.GravaLogSucesso(log);

                                    return RedirectToAction(nameof(Index));
                                    //return RedirectToAction("GetEmployee", new { filter = removeChapa });
                                }
                                else
                                {
                                    //TempData["ShowErrorAlert"] = true;
                                    TempData["ErrorMessage"] = "No Data Found";

                                    log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                    auxiliar.GravaLogAlerta(log);

                                    return RedirectToAction(nameof(Index));
                                    //return RedirectToAction("GetEmployee", new { filter = removeChapa });
                                }

                            }
                            else
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Search Employee First";

                                log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction(nameof(Index));
                                //return RedirectToAction("GetEmployee", new { filter = removeChapa });
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

//if (infocheck.Ativo == 0)
//{
//    infocheck.Ativo = 1;
//    infocheck.IdUsuario_Adicionou = usuariofer.Id;
//    infocheck.Mensagem = mensagem;
//    infocheck.DataRegistro = DateTime.Now;

//    await _context.SaveChangesAsync();

//    TempData["ShowSuccessAlert"] = true;

//    usuariofer.Retorno = "BloqueioEmprestimoAoSolicitante block com sucesso";
//    log.LogWhy = usuariofer.Retorno;
//    auxiliar.GravaLogSucesso(log);

//    return RedirectToAction(nameof(Index));
//}

// GET: thBloqueioEmprestimoAoSolicitanteController
//public ActionResult Index(string? IdNumber)
//{
//    Log log = new Log();
//    log.LogWhat = pagina + "/Index";
//    log.LogWhere = pagina;
//    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);


//    try
//    {
//        #region Authenticate User
//        VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
//        //usuario.Pagina = "Home/Index";
//        usuariofer.Pagina = pagina;
//        usuariofer.Pagina1 = log.LogWhat;
//        usuariofer.Acesso = log.LogWhat;
//        usuariofer = auxiliar.VerificaPermissao(usuariofer);

//        if (usuariofer.Permissao == null)
//        {
//            usuariofer.Retorno = "Usuário sem permissão na página!";
//            log.LogWhy = usuariofer.Retorno;
//            auxiliar.GravaLogAlerta(log);
//            return RedirectToAction("Login", "Home", usuariofer);
//        }
//        else
//        {
//            if (usuariofer.Permissao.Visualizar != 1)
//            {
//                usuariofer.Retorno = "Usuário sem permissão de visualizar a página de BloqueioEmprestimoAoSolicitante!";
//                log.LogWhy = usuariofer.Retorno;
//                auxiliar.GravaLogAlerta(log);
//                return RedirectToAction("Login", "Home", usuariofer);
//            }
//        }
//        #endregion

//        ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

//        ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

//        if (TempData.ContainsKey("ErrorMessage"))
//        {
//            ViewBag.Error = TempData["ErrorMessage"].ToString();
//            TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
//        }


//        if (IdNumber == null)
//        {
//            var emptyData = new FuncionarioViewModel();

//            return View(emptyData);

//        }



//        // Ensure that the previous query is fully executed and the data reader is closed.
//        _contextBS.Database.CloseConnection();

//        var query = _contextBS.Funcionario
//                    .Where(e => e.Chapa == IdNumber)
//                    .OrderByDescending(e => e.DataMudanca)
//                    .FirstOrDefault();

//        if (query != null)
//        {

//            var emptyData = new FuncionarioViewModel();

//            return View(emptyData);
//        }

//        if (query.CodPessoa != null || query.CodPessoa != 0)
//        {
//            var result = (from pessoa in _contextRM.PPESSOA
//                          join gImagem in _contextRM.GIMAGEM
//                          on pessoa.IDIMAGEM equals gImagem.ID
//                          where pessoa.CODIGO == query.CodPessoa
//                          select gImagem.IMAGEM)
//                      .FirstOrDefault();

//            string base64Image = Convert.ToBase64String(result);

//            // Pass the base64Image to the view
//            ViewData["Base64Image"] = base64Image;

//        }


//        var mapper = mapeamentoClasses.CreateMapper();

//        var funcionarioViewModel = mapper.Map<FuncionarioViewModel>(query);

//        usuariofer.Retorno = "BloqueioEmprestimoAoSolicitante carregada com sucesso!";
//        log.LogWhy = usuariofer.Retorno;
//        auxiliar.GravaLogSucesso(log);

//        return View(funcionarioViewModel);

//    }
//    catch (Exception ex)
//    {
//        log.LogWhy = ex.Message;
//        ErrorViewModel erro = new ErrorViewModel();
//        erro.Tela = log.LogWhere;
//        erro.Descricao = log.LogWhy;
//        erro.Mensagem = log.LogWhat;
//        erro.IdLog = auxiliar.GravaLogRetornoErro(log);
//        return View(ex);
//    }


//}