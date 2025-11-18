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
using AutoMapper.QueryableExtensions;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using X.PagedList;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class VW_Reativacao_ItemController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thProdutoReinclusao.aspx";
        private MapperConfiguration mapeamentoClasses;

        private static int? GlobalPagination;
        private static int? GlobalPageNumber;
        //private static int? UserEditPermission;

        private const string SessionKeyReactivacao = "Reactivacao";
        //private static List<VW_Reativacao_ItemViewModel?>? _ListVW_Reactivacao = new List<VW_Reativacao_ItemViewModel>();

        private const string SessionKeyProductToReactivate = "ProductToReactivate";
        //private static VW_Reativacao_ItemViewModel? ProductToReactivate = new VW_Reativacao_ItemViewModel();

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyCombinedModelVWReactivacao = "CombinedModelVWReactivacao";
        //private static CombinedInativoAtivo? combinedModel = new CombinedInativoAtivo();
        //private static VW_Reactivacao_SearchViewModel? SearchModel = new VW_Reactivacao_SearchViewModel();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public VW_Reativacao_ItemController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_Reativacao_Item, VW_Reativacao_ItemViewModel>();
                cfg.CreateMap<VW_Reativacao_ItemViewModel, VW_Reativacao_Item>();
            });
        }

        // GET: VW_Reativacao_Item
        public IActionResult Index(int? page)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                //usuario.Pagina1 = log.LogWhat;
                //usuario.Acesso = log.LogWhat;
                //usuario = auxiliar.VerificaPermissao(usuario);

                //if (usuario.Permissao == null)
                //{
                //    usuario.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Visualizar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //}

                //if (usuario.Permissao.Editar == 1)
                //{
                //    ViewBag.EditarValue = 1;
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
                return View(ex);
            }

        }

        public ActionResult SearchInativoProducts(CombinedInativoAtivo CombinedInativoAtivo)
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


                            List<VW_Reativacao_ItemViewModel>? Result = searches.SearchVWInativo(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel);
                            if (Result.Any())
                            {
                                //SearchModel = CombinedInativoAtivo.VW_Reactivacao_SearchViewModel;
                                //if (Result.Count > 1000)
                                //{

                                //}
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                HttpContext.Session.SetObject(SessionKeyReactivacao, Result);

                                //_ListVW_Reactivacao = Result;
                                GlobalPagination = CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Pagination;
                                int pageSize = GlobalPagination ?? 10;
                                int pageNumber = 1;
                                GlobalPagination = pageSize;
                                GlobalPageNumber = pageNumber;
                                IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = Result.ToPagedList(pageNumber, pageSize);
                                //UserEditPermission = usuariofer.Permissao.Editar;

                                var combinedViewModel = new CombinedInativoAtivo
                                {
                                    VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                                    VW_Reactivacao_SearchViewModel = CombinedInativoAtivo.VW_Reactivacao_SearchViewModel,
                                    LoggedUserDetails = checkPermission,
                                    ResultCount = Result.Count(),
                                };

                                CombinedInativoAtivoList? combinedViewModelSess = new CombinedInativoAtivoList
                                {
                                    VW_Reativacao_ItemViewModelList = Result,
                                    VW_Reactivacao_SearchViewModel = CombinedInativoAtivo.VW_Reactivacao_SearchViewModel,
                                    LoggedUserDetails = checkPermission,
                                    ResultCount = Result.Count(),
                                    PageNumber = pageNumber
                                };

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyCombinedModelVWReactivacao);
                                HttpContext.Session.SetObject(SessionKeyCombinedModelVWReactivacao, combinedViewModelSess);

                                //combinedModel = combinedViewModel;
                                return View(nameof(Index), combinedViewModel);
                            }
                            else
                            {
                                ViewBag.Error = "No Result Found.";
                                return View(nameof(Index));
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
                ViewBag.Error = ex.Message;
                return View("Index");
            }
            
        }


        public ActionResult GetProductToReactivate(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/GetProductToReactivate";
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


                        var CombinedModelReactivacao = HttpContext.Session.GetObject<CombinedInativoAtivoList>(SessionKeyCombinedModelVWReactivacao) ?? new CombinedInativoAtivoList();

                        int pageSize = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel.Pagination ?? 10;
                        int pageNumber = CombinedModelReactivacao.PageNumber ?? 1;

                        IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.ToPagedList(pageNumber, pageSize);

                        var combinedViewModel = new CombinedInativoAtivo
                        {
                            VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                            VW_Reactivacao_SearchViewModel = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel,
                            LoggedUserDetails = checkPermission,
                            ResultCount = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.Count(),
                        };


                        if (checkPermission.Pagina == pagina && checkPermission.Inserir == 1)
                        {
                            if (id != null)
                            {
                                var result = _context.VW_Reativacao_Item.Where(i => i.Id == id).OrderByDescending(i => i.DataInativacao).FirstOrDefault();
                                var mapper = mapeamentoClasses.CreateMapper();

                                VW_Reativacao_ItemViewModel? productResult = mapper.Map<VW_Reativacao_ItemViewModel>(result);

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductToReactivate);
                                HttpContext.Session.SetObject(SessionKeyProductToReactivate, productResult);

                                //ProductToReactivate = productResult;
                                ViewBag.ProductToReactivate = productResult;

                                return View(nameof(Index), combinedViewModel);
                            }
                            else
                            {
                                ViewBag.Error = "No Id Selected";
                                return View(nameof(Index), combinedViewModel);
                            }
                        }
                        else
                        {
                            ViewBag.Error = "O usuário não tem permissão para inserir dados";
                            return View(nameof(Index));
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

        public ActionResult GetProductToRequest(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/GetProductToRequest";
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


                        var CombinedModelReactivacao = HttpContext.Session.GetObject<CombinedInativoAtivoList>(SessionKeyCombinedModelVWReactivacao) ?? new CombinedInativoAtivoList();

                        int pageSize = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel.Pagination ?? 10;
                        int pageNumber = CombinedModelReactivacao.PageNumber ?? 1;

                        IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.ToPagedList(pageNumber, pageSize);

                        var combinedViewModel = new CombinedInativoAtivo
                        {
                            VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                            VW_Reactivacao_SearchViewModel = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel,
                            LoggedUserDetails = checkPermission,
                            ResultCount = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.Count(),
                        };

                        if (checkPermission.Pagina == pagina && checkPermission.Inserir == 1)
                        {
                            if (id != null)
                            {
                                var result = _context.VW_Reativacao_Item.Where(i => i.Id == id).OrderByDescending(i => i.DataInativacao).FirstOrDefault();
                                var mapper = mapeamentoClasses.CreateMapper();

                                VW_Reativacao_ItemViewModel? productResult = mapper.Map<VW_Reativacao_ItemViewModel>(result);

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductToReactivate);
                                HttpContext.Session.SetObject(SessionKeyProductToReactivate, productResult);

                                //ProductToReactivate = productResult;
                                ViewBag.ProductToRequest = productResult;

                                return View(nameof(Index), combinedViewModel);
                            }
                            else
                            {
                                ViewBag.Error = "No Id Selected";
                                return View(nameof(Index), combinedViewModel);
                            }
                        }
                        else
                        {
                            ViewBag.Error = "O usuário não tem permissão para inserir dados";
                            return View(nameof(Index));
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestToReactivate(VW_Reativacao_ItemViewModel? productDetails) 
        {
            Log log = new Log();
            log.LogWhat = pagina + "/RequestToReactivate";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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


                        var CombinedModelReactivacao = HttpContext.Session.GetObject<CombinedInativoAtivoList>(SessionKeyCombinedModelVWReactivacao) ?? new CombinedInativoAtivoList();



                        if (checkPermission.Pagina == pagina && checkPermission.Inserir == 1)
                        {
                            if (productDetails != null)
                            {
                                string? error = Validation(productDetails.Justificativa_Reativacao);
                                if (string.IsNullOrEmpty(error))
                                {
                                    var InsertToProdutoReincluido = new ProdutoReincluido
                                    {
                                        IdProduto = productDetails.Id,
                                        Observacao = productDetails.Justificativa_Reativacao,
                                        IdUsuario_Solicitante = loggedUser?.Id,
                                        IdUsuario_Aprovador = 0,
                                        Status = 1,
                                        DataRegistro = DateTime.Now
                                    };
                                    _context.ProdutoReincluido.Add(InsertToProdutoReincluido);
                                    _context.SaveChanges();

                                    VW_Reactivacao_SearchViewModel? search = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel != null ? CombinedModelReactivacao.VW_Reactivacao_SearchViewModel : new VW_Reactivacao_SearchViewModel();
                                    //Refresh List of Data
                                    List<VW_Reativacao_ItemViewModel?>? Result = searches.SearchVWInativo(search);
                                    if (Result.Any())
                                    {

                                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                        HttpContext.Session.SetObject(SessionKeyReactivacao, Result);

                                        //_ListVW_Reactivacao = Result;
                                        int pageSize = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel.Pagination ?? 10;
                                        int pageNumber = CombinedModelReactivacao.PageNumber ?? 1;
                                        IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = Result.ToPagedList(pageNumber, pageSize);


                                        var combinedViewModel = new CombinedInativoAtivo
                                        {
                                            VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                                            VW_Reactivacao_SearchViewModel = search,
                                            LoggedUserDetails = checkPermission,
                                            ResultCount = Result.Count(),
                                        };

                                        //combinedModel.VW_Reativacao_ItemViewModel = VW_Reativacao_Item;
                                        //combinedModel.ResultCount = Result.Count();

                                        ViewBag.ShowSuccessAlert = true;

                                        return View(nameof(Index), combinedViewModel);

                                    }
                                    else
                                    {
                                        ViewBag.Error = "Item not Found";
                                        return View(nameof(Index));
                                    }

                                }
                                else
                                {
                                    ViewBag.ErrorRequestToReactivate = error;
                                    var ProductToReactivate = HttpContext.Session.GetObject<VW_Reativacao_ItemViewModel>(SessionKeyProductToReactivate) ?? new VW_Reativacao_ItemViewModel();
                                    ViewBag.ProductToRequest = ProductToReactivate;

                                    int pageSize = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel.Pagination ?? 10;
                                    int pageNumber = CombinedModelReactivacao.PageNumber ?? 1;

                                    IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.ToPagedList(pageNumber, pageSize);

                                    var combinedViewModelout = new CombinedInativoAtivo
                                    {
                                        VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                                        VW_Reactivacao_SearchViewModel = CombinedModelReactivacao.VW_Reactivacao_SearchViewModel,
                                        LoggedUserDetails = checkPermission,
                                        ResultCount = CombinedModelReactivacao.VW_Reativacao_ItemViewModelList.Count(),
                                    };

                                    return View(nameof(Index), combinedViewModelout);
                                }

                            }
                            else
                            {
                                ViewBag.Error = "productDetails is error please report to IT.";
                                return View(nameof(Index));
                            }
                        }
                        else
                        {
                            ViewBag.Error = "O usuário não tem permissão para inserir dados";
                            return View(nameof(Index));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmReactivation(VW_Reativacao_ItemViewModel? productDetails)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);


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



                        if (checkPermission.Pagina == pagina && checkPermission.Inserir == 1)
                        {
                            if (productDetails != null)
                            {
                                var updateProdutoReincluido = _context.ProdutoReincluido.Where(i => i.IdProduto == productDetails.Id && i.Status == 1).FirstOrDefault();
                                if (updateProdutoReincluido != null)
                                {
                                    updateProdutoReincluido.Status = 2;
                                    updateProdutoReincluido.DataRegistro_Aprovacao = DateTime.Now;
                                    updateProdutoReincluido.IdUsuario_Aprovador = loggedUser?.Id;
                                    _context.SaveChanges();
                                }

                                var updateProduct = _context.Produto.FirstOrDefault(i => i.Id == productDetails.Id);
                                if (updateProduct != null)
                                {
                                    updateProduct.Ativo = 1;
                                    updateProduct.Observacao = updateProduct.Observacao + " ** Reat. Approv. Por => " + loggedUser?.Nome;
                                    _context.SaveChanges();
                                }

                                //VW_Reactivacao_SearchViewModel? search = combinedModel.VW_Reactivacao_SearchViewModel != null ? combinedModel.VW_Reactivacao_SearchViewModel : new VW_Reactivacao_SearchViewModel();
                                //List<VW_Reativacao_ItemViewModel?>? Result = searches.SearchVWInativo(search);
                                //if (Result.Any())
                                //{
                                //    _ListVW_Reactivacao = Result;
                                //    int pageSize = GlobalPagination ?? 10;
                                //    int pageNumber = GlobalPageNumber ?? 1;
                                //    IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = Result.ToPagedList(pageNumber, pageSize);

                                //    combinedModel.VW_Reativacao_ItemViewModel = VW_Reativacao_Item;
                                //    combinedModel.ResultCount = Result.Count();
                                //}

                                //combinedModel.VW_Reactivacao_SearchViewModel = SearchModel ?? new VW_Reactivacao_SearchViewModel();
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyCombinedModelVWReactivacao);
                                //_ListVW_Reactivacao.Clear();
                                //combinedModel.VW_Reativacao_ItemViewModel = null;

                                ViewBag.ShowSuccessAlert = true;

                                return View(nameof(Index));
                            }
                            else
                            {
                                return View(nameof(Index));
                            }
                        }
                        else
                        {
                            ViewBag.Error = "O usuário não tem permissão para inserir dados";
                            return View(nameof(Index));
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

        public ActionResult TestPage(int? page)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
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

            var ReactivacaoList = HttpContext.Session.GetObject<List<VW_Reativacao_ItemViewModel?>>(SessionKeyReactivacao) ?? new List<VW_Reativacao_ItemViewModel?>();

            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);
            GlobalPageNumber = pageNumber;
            IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = ReactivacaoList.ToPagedList(pageNumber, pageSize);

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
            PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);

            var combinedViewModel = new CombinedInativoAtivo
            {
                VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                LoggedUserDetails = checkPermission
            };

            return View("Index", combinedViewModel);
        }

        public string Validation(string? Observacao)
        {
            if (string.IsNullOrEmpty(Observacao))
            {
                return "Justificativa vázia.";
            }

            if (Observacao.Length > 250)
            {
                return "Justificativa ultrapassou os 250 caracteres .";
            }

            return null;
        }




        // GET: VW_Reativacao_Item_Extraviado/Edit/5
        public IActionResult Edit(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                //usuario.Pagina1 = log.LogWhat;
                //usuario.Acesso = log.LogWhat;
                //usuario = auxiliar.VerificaPermissao(usuario);

                //if (usuario.Permissao == null)
                //{
                //    usuario.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Editar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //}

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Editar == 1)
                        {



                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"].ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }


                            if (id == null || _context.VW_Reativacao_Item == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var vW_Reativacao_Item = _context.VW_Reativacao_Item.Find(id);
                            if (vW_Reativacao_Item == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            log.LogWhy = "Reativacao Item Inativado carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(vW_Reativacao_Item);

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



        // POST: VW_Reativacao_Item/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string Reactivate, int? IdProdutoAlocado, int? Status)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                //usuario.Pagina1 = log.LogWhat;
                //usuario.Acesso = log.LogWhat;
                //usuario = auxiliar.VerificaPermissao(usuario);

                //if (usuario.Permissao == null)
                //{
                //    usuario.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Editar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de Reactivacao Item!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
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


                            if (Reactivate == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                TempData["ErrorMessage"] = "Informe a Justificativa para Reativação obrigatorio";
                                return RedirectToAction("Edit", new { id = id });
                            }

                            if (Status == 1)
                            {
                                var produtoReincluido = _context.ProdutoReincluido.FirstOrDefault(i => i.IdProduto == id && i.Status == 1);
                                if (produtoReincluido != null)
                                {
                                    produtoReincluido.Status = 2;
                                    produtoReincluido.DataRegistro_Aprovacao = DateTime.Now;
                                    produtoReincluido.IdUsuario_Aprovador = loggedUser?.Id;
                                };

                                var EditProduto = _context.Produto.FirstOrDefault(i => i.Id == id);
                                if (EditProduto != null)
                                {
                                    EditProduto.Ativo = 1;
                                    EditProduto.Observacao = EditProduto.Observacao + " ** Reat. Aprov. Por => " + loggedUser?.Nome;
                                }

                                TempData["ShowSuccessAlert"] = true;
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                //_ListVW_Reactivacao.Clear();
                                //GlobalValues.ClearList(GlobalValues.ListVW_Reativacao_ItemViewModel);

                                log.LogWhy = "Reativacao Item com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                if (Reactivate != null)
                                {
                                    var insertToProdutoReincluido = new ProdutoReincluido
                                    {
                                        IdProduto = id,
                                        Observacao = Reactivate,
                                        IdUsuario_Solicitante = loggedUser?.Id,
                                        IdUsuario_Aprovador = 0,
                                        Status = 1
                                    };

                                    _context.ProdutoReincluido.Add(insertToProdutoReincluido);
                                    _context.SaveChanges();


                                    TempData["ShowSuccessAlert"] = true;
                                    //GlobalValues.ClearList(GlobalValues.ListVW_Reativacao_ItemViewModel);
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                    //_ListVW_Reactivacao.Clear();
                                    return RedirectToAction(nameof(Index));
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Informe a Justificativa para Reativação Obrigatorio";
                                    return RedirectToAction("Edit", new { id = id });
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
                return View(ex);
            }

           
        }



        public ActionResult GetInativoList(CombinedInativoAtivo CombinedInativoAtivo)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                //usuario.Pagina1 = log.LogWhat;
                //usuario.Acesso = log.LogWhat;
                //usuario = auxiliar.VerificaPermissao(usuario);

                //if (usuario.Permissao == null)
                //{
                //    usuario.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Visualizar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //}

                //if (usuario.Permissao.Editar == 1)
                //{
                //    ViewBag.EditarValue = 1;
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


                            List<VW_Reativacao_ItemViewModel> Result = new List<VW_Reativacao_ItemViewModel>();
                            Result = _context.VW_Reativacao_Item
                                            .Where(r =>
                                                (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Codigo == null || r.Codigo.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Codigo))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Descricao == null || r.Descricao.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Descricao))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.AF == null || r.AF.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.AF))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.PAT == null || r.PAT == CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.PAT)
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Controle == null || r.Controle.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Controle))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.LocalEmEstoque == null || r.LocalEmEstoque.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.LocalEmEstoque))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Motivo == null || r.Motivo.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Motivo))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Justificativa == null || r.Justificativa.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Justificativa))
                                                && (CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Usuario == null || r.Usuario.Contains(CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Usuario))
                                                && (!CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.De.HasValue || r.DataInativacao >= CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.De)
                                                && (!CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Ate.HasValue || r.DataInativacao <= CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Ate)
                                                ).ProjectTo<VW_Reativacao_ItemViewModel>(mapeamentoClasses)
                                                .ToList();

                            if (Result.Any())
                            {
                                var mapper = mapeamentoClasses.CreateMapper();

                                List<VW_Reativacao_ItemViewModel> VW_Reativacao_ItemViewModelResult = new List<VW_Reativacao_ItemViewModel>();
                                VW_Reativacao_ItemViewModelResult = mapper.Map<List<VW_Reativacao_ItemViewModel>>(Result);

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacao);
                                HttpContext.Session.SetObject(SessionKeyReactivacao, VW_Reativacao_ItemViewModelResult);

                                //_ListVW_Reactivacao = VW_Reativacao_ItemViewModelResult;

                                //GlobalValues.ListVW_Reativacao_ItemViewModel = VW_Reativacao_ItemViewModelResult;

                                GlobalPagination = CombinedInativoAtivo.VW_Reactivacao_SearchViewModel.Pagination;
                                int pageSize = GlobalPagination ?? 10;
                                int pageNumber = 1;
                                IPagedList<VW_Reativacao_ItemViewModel> VW_Reativacao_Item = VW_Reativacao_ItemViewModelResult.ToPagedList(pageNumber, pageSize);

                                //UserEditPermission = usuario.Permissao.Editar;

                                var combinedViewModel = new CombinedInativoAtivo
                                {
                                    VW_Reativacao_ItemViewModel = VW_Reativacao_Item,
                                    LoggedUserDetails = checkPermission
                                };

                                return View("Index", combinedViewModel);
                            }
                            else
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "No Searched has been found.";
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

           
            }
            catch (Exception ex)
            {
                log.LogWhy = ex.Message;
                ErrorViewModel erro = new ErrorViewModel();
                erro.Tela = log.LogWhere;
                erro.Descricao = log.LogWhy;
                erro.Mensagem = log.LogWhat;
                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                ViewBag.Error = ex.Message;
                return View("Index");
            }

        }

    }
}
