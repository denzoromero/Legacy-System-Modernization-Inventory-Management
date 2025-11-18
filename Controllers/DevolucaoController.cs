using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using X.PagedList;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Management;
using System.Globalization;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Drawing.Printing;
using System.Drawing;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using System;
using System.Collections.Generic;
using FerramentariaTest.EntitySeek;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ESC_POS_USB_NET.Printer;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    //public class GlobalDataDevolucao
    //{
    //    public static List<DevolucaoViewModel> listDevolucao { get; set; }

    //    public static List<int> BatchUploadList { get; set; }

    //}

    public class DevolucaoController : Controller
    {
        private const string SessionKeyDevolucaoList = "DevolucaoList";
        private const string SessionKeySearchModel = "SearchModel";
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thDevolucao.aspx";
        private MapperConfiguration mapeamentoClasses;
        private StreamReader _streamToPrint;
        private Font _printFont;
        private readonly ICompositeViewEngine _viewEngine;

        private const string SessionKeyPaginationDevolucao = "PaginationDevolucao";
        //private static int? GlobalPagination;
        private const string SessionKeyPageNumberDevolucao = "PageNumberDevolucao";
        //private static int? GlobalPageNumber;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static UserViewModel? SolicitanteEmployee = new UserViewModel();
        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();
        //private static SearchDevolucaoViewModel? SearchedFilterToReturn = new SearchDevolucaoViewModel();

        //private static List<DevolucaoViewModel?> ListOfProducts = new List<DevolucaoViewModel?>();

        public DevolucaoController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: DevolucaoController
        public ActionResult Index(int? page)
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
                //        usuariofer.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
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


                            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];
                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

                            var combinedViewModel = new CombinedDevolucao();
                            SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                            UserViewModel? UsuarioModel = new UserViewModel();
                            //IPagedList<DevolucaoViewModel> DevolucaoPagedList = new StaticPagedList<DevolucaoViewModel>(
                            //                                                        new List<DevolucaoViewModel>(),  // empty list for now
                            //                                                        1,  // default to page 1
                            //                                                        1,    // items per page
                            //                                                        0   // total number of items (initialize as 0)
                            //                                                        );

                            //if (LoggedUserDetails.Id == null)
                            //{
                            //    LoggedUserDetails = usuariofer;
                            //}

                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.SolicitanteDevolucao);
                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);
                            HttpContext.Session.Remove(SessionKeyDevolucaoList);
                            HttpContext.Session.Remove(SessionKeySearchModel);

                            int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
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
                            else
                            {
                                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.IdFerramentaria, (int)FerramentariaValue);
                            }



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


                            //Batch Upload Modal
                            //if (TempData.ContainsKey("OpenModalBatchUpload"))
                            //{
                            //    ViewBag.ListForUpload = GlobalDataDevolucao.BatchUploadList;

                            //    ViewBag.OpenBatchUploadModal = TempData["OpenModalBatchUpload"];
                            //    TempData.Remove("OpenModalBatchUpload");
                            //}




                            ViewBag.FerramentariaValue = FerramentariaValue;
                            combinedViewModel.SearchDevolucaoViewModel = searchfilter;

                            //usuariofer.Retorno =;
                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

                            return View(combinedViewModel);



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

        public ActionResult SearchProductToReturn(CombinedDevolucao? FromUserCombinedDevolucao)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchProductToReturn";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
            SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
            UserViewModel? UsuarioModel = new UserViewModel();

            try
            {
                UserViewModel? SolicitanteModel = new UserViewModel();
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                }

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


                            //Get the Search value from the front-end
                            combinedViewModel.SearchDevolucaoViewModel = FromUserCombinedDevolucao.SearchDevolucaoViewModel;

                            HttpContext.Session.Remove(SessionKeySearchModel);
                            HttpContext.Session.SetObject(SessionKeySearchModel, FromUserCombinedDevolucao.SearchDevolucaoViewModel);

                            if (FromUserCombinedDevolucao.SearchDevolucaoViewModel != null)
                            {
                                if (FromUserCombinedDevolucao.UserViewModel != null)
                                {
                                    List<DevolucaoViewModel>? Result = new List<DevolucaoViewModel>();
                                    SearchFilters searchFilters = new SearchFilters
                                    {
                                        Chapa = FromUserCombinedDevolucao.UserViewModel.Chapa,
                                        CodColigada = FromUserCombinedDevolucao.UserViewModel.CodColigada,
                                        Observacao = FromUserCombinedDevolucao.SearchDevolucaoViewModel.Observacao != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.Observacao : null,
                                        TransacoesDe = FromUserCombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe : null,
                                        TransacoesAte = FromUserCombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte : null,
                                        PrevisaoDe = FromUserCombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe : null,
                                        PrevisaoAte = FromUserCombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte : null,
                                        Codigo = FromUserCombinedDevolucao.SearchDevolucaoViewModel.Codigo != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.Codigo : null,
                                        Catalogo = FromUserCombinedDevolucao.SearchDevolucaoViewModel.Catalogo != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.Catalogo : null,
                                        CatalogoList = FromUserCombinedDevolucao.SearchDevolucaoViewModel.CatalogoList != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.CatalogoList : null,
                                        AF = FromUserCombinedDevolucao.SearchDevolucaoViewModel.AF != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.AF : null,
                                        PAT = FromUserCombinedDevolucao.SearchDevolucaoViewModel.PAT != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.PAT : null,
                                        DataDeValidade = FromUserCombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade != null ? FromUserCombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade : null
                                    };

                                    Result = searches.SearchDevolucaoList(searchFilters);

                                    if (Result.Any())
                                    {
                                        //Result = from results in Result
                                        //         join user in _contextBS.VW_Usuario_New on results.Balconista_IdLogin equals user.Id into userGroup
                                        //         from user in userGroup.DefaultIfEmpty()

                                        //foreach (var item in Result)
                                        //{
                                        //    //int? balconista = item.Balconista_IdLogin;

                                        //    //var usuario = await _contextBS.VW_Usuario_New
                                        //    //    .FirstOrDefaultAsync(u => u.Id == balconista);

                                        //    //if (usuario != null)
                                        //    //{
                                        //    //    item.Balconista_IdLogin = int.Parse(usuario.Chapa);
                                        //    //}
                                        //    //else
                                        //    //{
                                        //    //    var usuarioOld = await _contextBS.VW_Usuario
                                        //    //    .FirstOrDefaultAsync(u => u.Id == balconista);

                                        //    //    item.Balconista_IdLogin = int.Parse(usuarioOld.Chapa);
                                        //    //}

                                        //    //int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);
                                        //    //item.QuantidadeExtraviada = extraviadoQuantity;

                                        //    var GetListImage = _context.Arquivo.Where(im => im.IdProdutoAlocado == item.IdProdutoAlocado).ToList();
                                        //    if (GetListImage.Count > 0)
                                        //    {
                                        //        item.FileFound = true;
                                        //    }
                                        //}

                                        if (FromUserCombinedDevolucao.SearchDevolucaoViewModel.ItensExtraviados == true)
                                        {
                                            Result = Result.Where(item => item.QuantidadeExtraviada != 0).ToList();
                                        }

                                        //var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
                                        //model = Result;
                                        HttpContext.Session.SetObject(SessionKeyDevolucaoList, Result);

                                        //GlobalValues.DevolucaoViewModel = Result;
                                        //ListOfProducts = Result;

                                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPaginationDevolucao);
                                        HttpContext.Session.SetInt32(SessionKeyPaginationDevolucao, FromUserCombinedDevolucao.SearchDevolucaoViewModel.Pagination ?? 10);
                                        //GlobalPagination = FromUserCombinedDevolucao.SearchDevolucaoViewModel.Pagination;

                                        //int? testget = GlobalPagination;
                                        int pageSize = FromUserCombinedDevolucao.SearchDevolucaoViewModel.Pagination ?? 10;
                                        int pageNumber = 1;

                                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPageNumberDevolucao);
                                        HttpContext.Session.SetInt32(SessionKeyPageNumberDevolucao, pageNumber);
                                        //GlobalPageNumber = 1;

                                        IPagedList<DevolucaoViewModel> DevolucaoPagedList = Result.ToPagedList(pageNumber, pageSize);

                                        combinedViewModel.DevolucaoViewModel = DevolucaoPagedList;

                                        int? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                        //if (FerramentariaValue == usuariofer.IdFerramentaria)
                                        //{
                                            
                                        //}
                                        combinedViewModel.IdFerramentariaUser = FerramentariaValue;
                                        combinedViewModel.UserViewModel = SolicitanteModel;
                                        combinedViewModel.ResultCount = Result.Count;
                                        ViewBag.FerramentariaValue = FerramentariaValue;
                                        return View(nameof(Index), combinedViewModel);
                                    }
                                    else
                                    {
                                        //TempData["ShowErrorAlert"] = true;
                                        //TempData["ErrorMessage"] = "No Searched has been found.";
                                        ViewBag.Error = "No Searched has been found.";
                                        FromUserCombinedDevolucao.UserViewModel = SolicitanteModel;
                                        return View(nameof(Index), FromUserCombinedDevolucao);
                                    }

                                }
                                else
                                {
                                    //TempData["ShowErrorAlert"] = true;
                                    //TempData["ErrorMessage"] = "Please search employee first.";
                                    ViewBag.Error = "Por favor procure o funcionário primeiro.";
                                    return View(nameof(Index), combinedViewModel);
                                }
                            }

                            return View(nameof(Index));



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

        //Not Used
        //public async Task<ActionResult> SearchDevolucao(CombinedDevolucao? CombinedDevolucao)
        //{
        //    Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
        //    CombinedDevolucao? combinedViewModel = new CombinedDevolucao();

        //    if (TempData.ContainsKey("ShowSuccessAlert"))
        //    {
        //        ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"].ToString();
        //        TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
        //    }

        //    if (TempData.ContainsKey("ErrorMessage"))
        //    {
        //        ViewBag.Error = TempData["ErrorMessage"].ToString();
        //        TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
        //    }

        //    SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
        //    UserViewModel? UsuarioModel = new UserViewModel();

        //    combinedViewModel.SearchDevolucaoViewModel = CombinedDevolucao.SearchDevolucaoViewModel;

        //    HttpContext.Session.Remove(SessionKeySearchModel);
        //    HttpContext.Session.SetObject(SessionKeySearchModel, CombinedDevolucao.SearchDevolucaoViewModel);

        //    if (CombinedDevolucao != null)
        //    {
        //        if (CombinedDevolucao.UserViewModel != null)
        //        {
        //            combinedViewModel.UserViewModel = searches.SearchEmployeeOnLoad();
                    
        //            List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();
        //            SearchFilters searchFilters = new SearchFilters
        //            {
        //                Chapa = CombinedDevolucao.UserViewModel.Chapa,
        //                CodColigada = CombinedDevolucao.UserViewModel.CodColigada,
        //                Observacao = CombinedDevolucao.SearchDevolucaoViewModel.Observacao != null ? CombinedDevolucao.SearchDevolucaoViewModel.Observacao : null,
        //                TransacoesDe = CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe != null ? CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe : null,
        //                TransacoesAte = CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte != null ? CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte : null,
        //                PrevisaoDe = CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe != null ? CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe : null,
        //                PrevisaoAte = CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte != null ? CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte : null,
        //                Codigo = CombinedDevolucao.SearchDevolucaoViewModel.Codigo != null ? CombinedDevolucao.SearchDevolucaoViewModel.Codigo : null,
        //                Catalogo = CombinedDevolucao.SearchDevolucaoViewModel.Catalogo != null ? CombinedDevolucao.SearchDevolucaoViewModel.Catalogo : null,
        //                AF = CombinedDevolucao.SearchDevolucaoViewModel.AF != null ? CombinedDevolucao.SearchDevolucaoViewModel.AF : null,
        //                DataDeValidade = CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade != null ? CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade : null                 
        //            };

        //            Result = searches.SearchDevolucaoList(searchFilters);

        //            if (Result.Any())
        //            {
        //                foreach (var item in Result)
        //                {
        //                    int? balconista = item.Balconista_IdLogin;

        //                    var usuario = await _contextBS.VW_Usuario_New
        //                        .FirstOrDefaultAsync(u => u.Id == balconista);

        //                    if (usuario != null)
        //                    {
        //                        item.Balconista_IdLogin = int.Parse(usuario.Chapa);
        //                    }
        //                    else
        //                    {
        //                        var usuarioOld = await _contextBS.VW_Usuario
        //                        .FirstOrDefaultAsync(u => u.Id == balconista);

        //                        item.Balconista_IdLogin = int.Parse(usuarioOld.Chapa);
        //                    }

        //                    int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);

        //                    item.QuantidadeExtraviada = extraviadoQuantity;

        //                }

        //                GlobalValues.DevolucaoViewModel = Result;
        //                //GlobalPagination = CombinedDevolucao.SearchDevolucaoViewModel.Pagination;

        //                //int? testget = GlobalPagination;
        //                //int pageSize = GlobalPagination ?? 10;
        //                int pageNumber = 1;

        //                IPagedList<DevolucaoViewModel> DevolucaoPagedList = Result.ToPagedList(pageNumber, pageSize);

        //                combinedViewModel.DevolucaoViewModel = DevolucaoPagedList;
        //            }
        //            else
        //            {
        //                //TempData["ShowErrorAlert"] = true;
        //                //TempData["ErrorMessage"] = "No Searched has been found.";
        //                ViewBag.Error = "No Searched has been found.";
        //            }
        //        }
        //        else
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            //TempData["ErrorMessage"] = "Please search employee first.";
        //            ViewBag.Error = "Please search employee first.";
        //        }
        //    }

        //    int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
        //    ViewBag.FerramentariaValue = FerramentariaValue;
        //    return View("Index", combinedViewModel);
        //    //return RedirectToAction("Edit", new { id = id });
        //}

        public ActionResult SearchEmployee(CombinedDevolucao? CombinedDevolucao)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchEmployee";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
            UserViewModel? UsuarioModel = new UserViewModel();

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

            try
            {
                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {

                            if (CombinedDevolucao?.UserViewModel != null && CombinedDevolucao?.UserViewModel.filter != null)
                            {
                                listTerceiroResult = searches.SearchTercerio(CombinedDevolucao.UserViewModel.filter);
                                TotalResult.AddRange(listTerceiroResult);

                                listEmployeeResult = searches.SearchEmployeeChapa(CombinedDevolucao.UserViewModel.filter);
                                TotalResult.AddRange(listEmployeeResult);

                                //if (ListOfProducts.Count > 0)
                                //{
                                //    ListOfProducts.Clear();
                                //}
                                HttpContext.Session.Remove(SessionKeyDevolucaoList);

                                if (TotalResult.Count > 1)
                                {
                                    ViewBag.ListOfEmployees = TotalResult;
                                    return View(nameof(Index));
                                }
                                else if (TotalResult.Count == 1)
                                {
                                    httpContextAccessor.HttpContext.Session.Remove(Sessao.SolicitanteDevolucao);
                                    httpContextAccessor.HttpContext.Session.SetString(Sessao.SolicitanteDevolucao, TotalResult[0].Chapa);

                                    UsuarioModel = searches.GetEmployeeDetails(TotalResult[0].Chapa);
                                    //SolicitanteEmployee = UsuarioModel;
                                    List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();
                                    messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                                    ViewBag.Messages = messages.Count > 0 ? messages : null;

                                    string? blockMessage = searches.SearchBloqueioMessage(UsuarioModel.Chapa);
                                    if (!string.IsNullOrEmpty(blockMessage))
                                    {
                                        ViewBag.BlockMessage = blockMessage;
                                    }

                                    var combinedViewModel = new CombinedDevolucao();
                                    SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                                    combinedViewModel.UserViewModel = UsuarioModel;
                                    combinedViewModel.SearchDevolucaoViewModel = searchfilter;

                                    return View(nameof(Index), combinedViewModel);
                                    //return RedirectToAction(nameof(Index));
                                }
                                else if (TotalResult.Count == 0)
                                {
                                    //if (ListOfProducts.Count > 0)
                                    //{
                                    //    ListOfProducts.Clear();
                                    //}
                                    ViewBag.Error = "No Searched has been found.";
                                    return View(nameof(Index));
                                }

                            }
                            else
                            {
                                ModelState.AddModelError("UserViewModel.filter", "Matricula/Nome is Required");
                                return View(nameof(Index));
                            }

                            return View(nameof(Index));



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
                ViewBag.Error = ex.Message;
                return View(nameof(Index));
            }

        
        }


     

        public ActionResult DeleteMessage(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            if (id != null)
            {
                MensagemSolicitante? DeleteMessage = _context.MensagemSolicitante.FirstOrDefault(i => i.Id == id) ?? new MensagemSolicitante();
                if (DeleteMessage != null)
                {
                    DeleteMessage.Ativo = 0;
                    _context.SaveChanges();
                }

                UserViewModel? UsuarioModel = searches.GetEmployeeDetails(DeleteMessage.Chapa);

                ViewBag.ShowSuccessAlert = true;

                var combinedViewModel = new CombinedDevolucao();
                SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                combinedViewModel.UserViewModel = UsuarioModel;
                combinedViewModel.SearchDevolucaoViewModel = searchfilter;

                return View(nameof(Index), combinedViewModel);

                //TempData["ShowSuccessAlert"] = true;
                //return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Chapa is Empty";
                return RedirectToAction(nameof(Index));
            }

        }

        public ActionResult SelectedUser(string? chapa)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SelectedUser";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();

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

            try
            {
                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {

                            if (chapa != null)
                            {
                                //httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                                //httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, chapa);

                                httpContextAccessor.HttpContext?.Session.Remove(Sessao.SolicitanteDevolucao);
                                httpContextAccessor.HttpContext?.Session.SetString(Sessao.SolicitanteDevolucao, chapa);

                                UsuarioModel = searches.GetEmployeeDetails(chapa);
                                //SolicitanteEmployee = UsuarioModel;

                                List<MensagemSolicitanteViewModel>? messages = new List<MensagemSolicitanteViewModel>();
                                messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                                ViewBag.Messages = messages.Count > 0 ? messages : null;

                                string? blockMessage = searches.SearchBloqueioMessage(UsuarioModel.Chapa);
                                if (!string.IsNullOrEmpty(blockMessage))
                                {
                                    ViewBag.BlockMessage = blockMessage;
                                }

                                var combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                                combinedViewModel.UserViewModel = UsuarioModel;
                                combinedViewModel.SearchDevolucaoViewModel = searchfilter;

                                return View(nameof(Index), combinedViewModel);
                            }

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
                ViewBag.Error = ex.Message;
                return View(nameof(Index)); 
            }


        

       
        }


        #region Table Functions

        public ActionResult GetObservacao(int? id)
        {
            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            var observacao = model
             .Where(devolucao => devolucao.IdProdutoAlocado == id)           
             .FirstOrDefault(); 

            if (observacao != null)
            {
                //ViewBag.Observacao = observacao;

                //ViewBag.OpenObsModal = true;

                TempData["GetCodigo"] = observacao.CodigoCatalogo;
                TempData["GetObs"] = observacao.Observacao;

                TempData["OpenModal"] = true;
            }
            
            return RedirectToAction(nameof(Index));
        }

        public ActionResult GetLib(string? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/GetLib";
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

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            var queryliberador = _contextBS.Funcionario
                           .Where(e => e.Chapa == id)
                           .OrderByDescending(e => e.DataMudanca)
                           .FirstOrDefault();

            //TempData["LibChapa"] = queryliberador.Chapa;
            //TempData["LibNome"] = queryliberador.Nome;
            //TempData["LibCodPessoa"] = queryliberador.CodPessoa;
            //TempData["OpenModalLib"] = true;

            //int? CodPessoa = TempData["LibCodPessoa"] as int?;


            var result = (from pessoa in _contextRM.PPESSOA
                          join gImagem in _contextRM.GIMAGEM
                          on pessoa.IDIMAGEM equals gImagem.ID
                          where pessoa.CODIGO == queryliberador.CodPessoa
                          select gImagem.IMAGEM)
                     .FirstOrDefault();

            ViewData["Base64ImageLib"] = Convert.ToBase64String(result);

            ViewBag.LibChapa = queryliberador.Chapa;
            ViewBag.LibNome = queryliberador.Nome;
            ViewBag.OpenLibModal = true;
            //ViewBag.OpenLibModal = TempData["OpenModalLib"];

            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
            combinedViewModel.UserViewModel = SolicitanteModel;
            combinedViewModel.ResultCount = model.Count;



            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
            IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

            combinedViewModel.DevolucaoViewModel = devoPagedList;

            return View(nameof(Index), combinedViewModel);
            //return RedirectToAction(nameof(Index));
        }

        public ActionResult GetBalc(string? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/GetBalc";
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

            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            var querybalc = _contextBS.Funcionario
                          .Where(e => e.Chapa == id)
                          .OrderByDescending(e => e.DataMudanca)
                          .FirstOrDefault();

            var result = (from pessoa in _contextRM.PPESSOA
                          join gImagem in _contextRM.GIMAGEM
                          on pessoa.IDIMAGEM equals gImagem.ID
                          where pessoa.CODIGO == querybalc.CodPessoa
                          select gImagem.IMAGEM)
                     .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(result);

            ViewBag.BalcChapa = querybalc.Chapa;
            ViewBag.BalcNome = querybalc.Nome;
            ViewBag.OpenBalcModal = true;

            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
            combinedViewModel.UserViewModel = SolicitanteModel;
            combinedViewModel.ResultCount = model.Count;

            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
            IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

            combinedViewModel.DevolucaoViewModel = devoPagedList;

            return View(nameof(Index), combinedViewModel);
        }

        public ActionResult OpenSingleUpload(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/OpenSingleUpload";
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

            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            if (id != null)
            {
                ViewBag.IdHolder = id;
                var GetListImage = _context.Arquivo.Where(im => im.IdProdutoAlocado == id).ToList();

                ViewBag.AlocadoImages = GetListImage;
                ViewBag.OpenSingle = true;

                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                combinedViewModel.UserViewModel = SolicitanteModel;
                combinedViewModel.ResultCount = model.Count;

                int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

                combinedViewModel.DevolucaoViewModel = devoPagedList;

                return View(nameof(Index), combinedViewModel);
            }
            else
            {
                return RedirectToAction(nameof(Index));

            }

            //return View(nameof(Index), combinedViewModel);
            //return RedirectToAction(nameof(Index));
        }

        #endregion

        #region buttons

        [HttpPost]
        public ActionResult executarDevolucao(CombinedDevolucao model, int? PrintTicket)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de devolucao!";
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
                        if (checkPermission.Inserir == 1)
                        {

                            UserViewModel? SolicitanteModel = new UserViewModel();
                            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                            if (!string.IsNullOrEmpty(SolicitanteChapa))
                            {
                                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                            }

                            var modelDevolucao = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

                            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();

                            SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                            UserViewModel? UsuarioModel = new UserViewModel();
                            List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();


                            if (model != null)
                            {
                                combinedViewModel.SearchDevolucaoViewModel = model.SearchDevolucaoViewModel;
                                combinedViewModel.UserViewModel = model.UserViewModel;

                                List<int?>? historicoId = new List<int?>();

                                var selectedIds = model.PassedDevolucao.Where(row => row.selectedIds != null).ToList();
                                if (selectedIds.Count > 0)
                                {
                                    //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                                    List<PassedDevolucaoModel>? ListPassedValues = new List<PassedDevolucaoModel>();

                                    ListPassedValues = selectedIds;
                                    foreach (var item in ListPassedValues)
                                    {
                                        var GetDatas = modelDevolucao.FirstOrDefault(e => e.IdProduto == item.selectedIds && e.IdProdutoAlocado == item.IdProdutoAlocado);
                                        var qtddevValue = item.QuantidadeInput;
                                        var getFerr = item.ddlFerramentaria;

                                        int? HisIdProdutoAlocado = GetDatas.IdProdutoAlocado;
                                        int? HisIdProduto = GetDatas.IdProduto;
                                        string? HisFerrNome = item.NomeFerramentaria;
                                        int? HisReturnFerr = item.ddlFerramentaria;
                                        int? QtdDevolucao = item.QuantidadeInput;
                                        int? QtdEmprestada = item.Quantidade;
                                        int? QtdExtraviada = searches.SearchProdutoExtraviadoQuantity(HisIdProdutoAlocado);

                                        var GetProdutoAlocado = _context.ProdutoAlocado.FirstOrDefault(e => e.Id == GetDatas.IdProdutoAlocado);
                                        //if (GetProdutoAlocado.Quantidade > 0)
                                        if (item.QuantidadeInput > 0 && QtdEmprestada >= item.QuantidadeInput)
                                        {
                                            if (GetProdutoAlocado.Quantidade >= 1)
                                            {
                                                int ano = DateTime.Now.Year;
                                                string tableName = $"HistoricoAlocacao_{ano}";

                                                var tableExists = _context.Database.SqlQueryRaw<int>(
                                                                    "IF OBJECT_ID('" + tableName + "') IS NOT NULL " +
                                                                    "SELECT 1 " +
                                                                    "ELSE " +
                                                                    "SELECT 0"
                                                                ).AsEnumerable().FirstOrDefault();

                                                if (tableExists != 0)
                                                {
                                                    var InsertHistoricoAlocacao2024 = new HistoricoAlocacao_2025
                                                    {
                                                        IdProduto = GetProdutoAlocado.IdProduto,
                                                        Solicitante_IdTerceiro = GetProdutoAlocado.Solicitante_IdTerceiro,
                                                        Solicitante_CodColigada = GetProdutoAlocado.Solicitante_CodColigada,
                                                        Solicitante_Chapa = GetProdutoAlocado.Solicitante_Chapa,
                                                        //GetProdutoAlocado.Solicitante_Chapa,
                                                        Liberador_IdTerceiro = GetProdutoAlocado.Liberador_IdTerceiro,
                                                        Liberador_CodColigada = GetProdutoAlocado.Liberador_CodColigada,
                                                        Liberador_Chapa = GetProdutoAlocado.Liberador_Chapa,
                                                        Balconista_Emprestimo_IdLogin = GetProdutoAlocado.Balconista_IdLogin,
                                                        Balconista_Devolucao_IdLogin = loggedUser.Id,
                                                        Observacao = GetProdutoAlocado.Observacao,
                                                        DataEmprestimo = GetProdutoAlocado.DataEmprestimo,
                                                        DataPrevistaDevolucao = GetProdutoAlocado.DataPrevistaDevolucao,
                                                        DataDevolucao = DateTime.Now,
                                                        IdObra = GetProdutoAlocado.IdObra,
                                                        Quantidade = QtdDevolucao,
                                                        IdFerrOndeProdRetirado = GetProdutoAlocado.IdFerrOndeProdRetirado,
                                                        IdFerrOndeProdDevolvido = getFerr,
                                                        IdControleCA = GetProdutoAlocado.IdControleCA
                                                    };

                                                    _context.Add(InsertHistoricoAlocacao2024);
                                                    _context.SaveChanges();

                                                    historicoId.Add(InsertHistoricoAlocacao2024.Id);

                                                    var CheckArquivo = _context.ArquivoVsProdutoAlocado.FirstOrDefault(e => e.IdProdutoAlocado == GetDatas.IdProdutoAlocado);
                                                    if (CheckArquivo != null)
                                                    {
                                                        var InsertArquivoHistorico = new ArquivoVsHistorico
                                                        {
                                                            IdArquivo = CheckArquivo.IdArquivo,
                                                            IdHistoricoAlocacao = InsertHistoricoAlocacao2024.Id,
                                                            Ano = ano,
                                                            DataRegistro = DateTime.Now
                                                        };
                                                        _context.Add(InsertArquivoHistorico);
                                                        _context.SaveChanges();
                                                    }

                                                    if (getFerr != 17)
                                                    {
                                                        var updateproduct = _context.Produto.Where(i => i.Id == GetDatas.IdProduto).FirstOrDefault();
                                                        if (updateproduct != null)
                                                        {
                                                            if (GetDatas.CatalogoPorAferido == 1 || GetDatas.CatalogoPorSerial == 1)
                                                            {
                                                                updateproduct.Quantidade = 1;
                                                                _context.SaveChanges();

                                                                //if (updateproduct.Quantidade >= 1)
                                                                //{
                                                                //    updateproduct.Quantidade = 1;
                                                                //    _context.SaveChanges();
                                                                //}
                                                                //else
                                                                //{
                                                                //    updateproduct.Quantidade = updateproduct.Quantidade + QtdDevolucao;
                                                                //    _context.SaveChanges();
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                updateproduct.Quantidade = updateproduct.Quantidade + QtdDevolucao;
                                                                _context.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (GetDatas.CatalogoPorAferido != 1 || GetDatas.CatalogoPorSerial != 1)
                                                        {
                                                            int? OutsideIdProduto;
                                                            DevolucaoViewModel? ToSearch = new DevolucaoViewModel();
                                                            ToSearch.IdCatalogo = GetDatas.IdCatalogo;
                                                            ToSearch.IdFerramentaria = HisReturnFerr;
                                                            ProdutoCompleteViewModel? objLocalizado = searches.VerifyProduct(ToSearch);
                                                            if (objLocalizado != null)
                                                            {
                                                                int? OldQuantity = objLocalizado.Quantidade;
                                                                int? OldQuatidadeMinima = objLocalizado.QuantidadeMinima;
                                                                string? OldObservacao = objLocalizado.Observacao;

                                                                int? localizadoQuantity = objLocalizado.Quantidade + QtdDevolucao;
                                                                int? localizadoQuantityMinima = GetDatas.QuantidadeMinima;
                                                                string? Localizacao = objLocalizado.Localizacao;

                                                                int? id = objLocalizado.IdProduto;
                                                                OutsideIdProduto = id;

                                                                DevolucaoViewModel? NewSearch = new DevolucaoViewModel();
                                                                NewSearch.IdProduto = GetDatas.IdProduto;

                                                                ProdutoCompleteViewModel? prodDebito = searches.VerifyProduct(NewSearch);
                                                                if (prodDebito != null)
                                                                {
                                                                    var InsertToLogProduto = new LogProduto
                                                                    {
                                                                        IdProduto = prodDebito.IdProduto,
                                                                        IdUsuario = loggedUser.Id,
                                                                        QuantidadeDe = prodDebito.Quantidade + QtdDevolucao,
                                                                        QuantidadePara = prodDebito.Quantidade,
                                                                        Acao = 2,
                                                                        DataRegistro = DateTime.Now
                                                                    };

                                                                    _context.LogProduto.Add(InsertToLogProduto);
                                                                    _context.SaveChanges();
                                                                }

                                                                var AnotherInsertToLogProduto = new LogProduto
                                                                {
                                                                    IdProduto = id,
                                                                    IdUsuario = loggedUser.Id,
                                                                    QuantidadeDe = OldQuantity,
                                                                    QuantidadePara = localizadoQuantity,
                                                                    RfmDe = objLocalizado.RFM,
                                                                    RfmPara = "TRANSFERENCIA",
                                                                    ObservacaoDe = OldObservacao.Length > 250 ? OldObservacao.PadRight(250) : OldObservacao,
                                                                    ObservacaoPara = OldObservacao.Length > 250 ? OldObservacao.PadRight(250) : OldObservacao,
                                                                    Acao = 2,
                                                                    DataRegistro = DateTime.Now
                                                                };

                                                                _context.LogProduto.Add(AnotherInsertToLogProduto);
                                                                _context.SaveChanges();

                                                                //EditarSaldo
                                                                //EditarRFM
                                                                var UpdateSaldo = _context.Produto.Where(i => i.Id == id).FirstOrDefault();
                                                                if (UpdateSaldo != null)
                                                                {
                                                                    UpdateSaldo.Quantidade = localizadoQuantity;
                                                                    UpdateSaldo.RFM = "TRANSFERENCIA";
                                                                    _context.SaveChanges();
                                                                }

                                                            }
                                                            else
                                                            {

                                                                var InsertToProduto = new Produto
                                                                {
                                                                    IdCatalogo = GetDatas.IdCatalogo,
                                                                    IdFerramentaria = HisReturnFerr,
                                                                    Quantidade = QtdDevolucao,
                                                                    QuantidadeMinima = GetDatas.QuantidadeMinima,
                                                                    Localizacao = GetDatas.Localizacao,
                                                                    RFM = "TRANSFERENCIA",
                                                                    Observacao = GetDatas.Observacao,
                                                                };

                                                                _context.Produto.Add(InsertToProduto);
                                                                _context.SaveChanges();

                                                                int? Id = InsertToProduto.Id;
                                                                OutsideIdProduto = Id;

                                                                DevolucaoViewModel? NewSearch = new DevolucaoViewModel();
                                                                NewSearch.IdProduto = GetDatas.IdProduto;

                                                                ProdutoCompleteViewModel? productDetails = searches.VerifyProduct(NewSearch);
                                                                if (productDetails != null)
                                                                {
                                                                    var InsertToLogProduto = new LogProduto
                                                                    {
                                                                        IdProduto = productDetails.IdProduto,
                                                                        IdUsuario = loggedUser.Id,
                                                                        QuantidadeDe = productDetails.Quantidade + QtdDevolucao,
                                                                        QuantidadeMinimaPara = productDetails.Quantidade,
                                                                        Acao = 2,
                                                                        DataRegistro = DateTime.Now
                                                                    };

                                                                    _context.LogProduto.Add(InsertToLogProduto);
                                                                    _context.SaveChanges();
                                                                }

                                                                var InsertToLogProdutoAgain = new LogProduto
                                                                {
                                                                    IdProduto = Id,
                                                                    IdUsuario = loggedUser.Id,
                                                                    QuantidadePara = QtdDevolucao,
                                                                    Acao = 1,
                                                                    DataRegistro = DateTime.Now
                                                                };

                                                                _context.LogProduto.Add(InsertToLogProdutoAgain);
                                                                _context.SaveChanges();

                                                            }

                                                            var InsertToHistoricoTransferencia = new HistoricoTransferencia
                                                            {
                                                                IdProduto = OutsideIdProduto,
                                                                IdUsuario = loggedUser.Id,
                                                                IdFerramentariaOrigem = GetDatas.IdFerramentaria,
                                                                IdFerramentariaDestino = HisReturnFerr,
                                                                DataOcorrencia = DateTime.Now,
                                                                Quantidade = QtdDevolucao
                                                            };

                                                            _context.HistoricoTransferencia.Add(InsertToHistoricoTransferencia);
                                                            _context.SaveChanges();

                                                        }
                                                    }

                                                    int? newquantity = QtdEmprestada - QtdDevolucao;
                                                    if (newquantity == 0 && QtdExtraviada == 0)
                                                    {
                                                        var arquivoVsProdutoAlocadoToDelete = _context.ArquivoVsProdutoAlocado
                                                                                        .FirstOrDefault(a => a.IdProdutoAlocado == GetDatas.IdProdutoAlocado);

                                                        var produtoAlocadoToDelete = _context.ProdutoAlocado
                                                                                     .FirstOrDefault(p => p.Id == GetDatas.IdProdutoAlocado);

                                                        if (arquivoVsProdutoAlocadoToDelete != null)
                                                        {
                                                            _context.ArquivoVsProdutoAlocado.Remove(arquivoVsProdutoAlocadoToDelete);
                                                        }

                                                        if (produtoAlocadoToDelete != null)
                                                        {
                                                            _context.ProdutoAlocado.Remove(produtoAlocadoToDelete);
                                                        }

                                                        _context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        var newedit = _context.ProdutoAlocado.Where(i => i.Id == GetDatas.IdProdutoAlocado).FirstOrDefault();
                                                        if (newedit != null)
                                                        {
                                                            newedit.Quantidade = newquantity;
                                                            _context.SaveChanges();
                                                        }

                                                    }

                                                }
                                                else
                                                {
                                                    TempData["ShowErrorAlert"] = true;
                                                    TempData["ErrorMessage"] = $"Please Contact IT Department, Because {tableName} doesnt exist ";

                                                    return RedirectToAction(nameof(Index));
                                                }
                                            }
                                            else
                                            {
                                                TempData["ShowErrorAlert"] = true;
                                                TempData["ErrorMessage"] = $"Saldo INSUFICIENTE para operação de devolução! o SALDO ATUALMENTE EMPRESTADO é de {GetProdutoAlocado.Quantidade}. E a QUANTIDADE informada para devolução é de {QtdDevolucao} para Empréstimo ID {GetDatas.IdProdutoAlocado}";

                                                return RedirectToAction(nameof(Index));
                                            }


                                        }
                                    }


                                    if (PrintTicket == 1)
                                    {
                                        DateTime dateTimeOneMinuteAgo = DateTime.Now.AddMinutes(-1);
                                        var sb = new StringBuilder();
                                        List<HistoricoAlocacaoViewModel> historicoAlocacaoList = new List<HistoricoAlocacaoViewModel>();
                                        historicoAlocacaoList = (from hist in _context.HistoricoAlocacao_2025
                                                                 join produto in _context.Produto on hist.IdProduto equals produto.Id
                                                                 join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                                 join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                 join ferrOrigem in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrigem.Id
                                                                 join ferrDevolucao in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDevolucao.Id into devolucaoGroup
                                                                 from devolucao in devolucaoGroup.DefaultIfEmpty()
                                                                 where hist.Balconista_Devolucao_IdLogin == loggedUser.Id &&
                                                                       hist.Solicitante_Chapa == SolicitanteModel.Chapa &&
                                                                       //hist.Solicitante_CodColigada == SolicitanteModel.CodColigada &&
                                                                       historicoId.Contains(hist.Id)
                                                                 //hist.Solicitante_IdTerceiro == 0 &&
                                                                 //hist.DataDevolucao >= dateTimeOneMinuteAgo
                                                                 select new HistoricoAlocacaoViewModel
                                                                 {
                                                                     IdHistoricoAlocacao = hist.Id,
                                                                     IdProdutoAlocado = 0,
                                                                     IdProduto = hist.IdProduto,
                                                                     Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                                     Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                                     Solicitante_Chapa = hist.Solicitante_Chapa,
                                                                     Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                                     Liberador_CodColigada = hist.Liberador_CodColigada,
                                                                     Liberador_Chapa = hist.Liberador_Chapa,
                                                                     Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                                     Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                                     Observacao = hist.Observacao,
                                                                     DataEmprestimo = hist.DataEmprestimo,
                                                                     DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                                     DataDevolucao = hist.DataDevolucao,
                                                                     IdObra = hist.IdObra,
                                                                     Quantidade = hist.Quantidade,
                                                                     QuantidadeEmprestada = catalogo.PorAferido == 0 && catalogo.PorSerial == 0
                                                                         ? (_context.ProdutoAlocado
                                                                                .Where(pa =>
                                                                                    pa.IdProduto == hist.IdProduto &&
                                                                                    pa.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                                    pa.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                                    pa.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                                .OrderBy(pa => pa.Id)
                                                                                .Select(pa => pa.Quantidade)
                                                                                .FirstOrDefault() ?? 0)
                                                                         : 0,
                                                                     IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                                     IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                                     CodigoCatalogo = catalogo.Codigo,
                                                                     NomeCatalogo = catalogo.Nome,
                                                                     FerrOrigem = ferrOrigem.Nome,
                                                                     FerrDevolucao = devolucao.Nome,
                                                                     AFProduto = produto.AF,
                                                                     Serie = produto.Serie,
                                                                     PATProduto = produto.PAT,
                                                                     IdControleCA = hist.IdControleCA
                                                                 }).ToList();

                                        if (historicoAlocacaoList.Count > 0)
                                        {
                                            DateTime dataDevolucao = DateTime.Now;


                                            sb.AppendLine("<html><head><title>Receipt</title>");
                                            sb.AppendLine("<style type='text/css'>");
                                            sb.AppendLine("@media print {");
                                            sb.AppendLine("  body, html {");
                                            sb.AppendLine("    margin: 0;");
                                            sb.AppendLine("    padding: 0;");
                                            sb.AppendLine("    width: 7.9cm;");
                                            sb.AppendLine("  }");
                                            sb.AppendLine("  .receipt {");
                                            sb.AppendLine("    width: 7.9cm;");
                                            sb.AppendLine("    font-family: 'Courier New', monospace;");
                                            sb.AppendLine("    font-size: 12px;");
                                            sb.AppendLine("  }");
                                            sb.AppendLine("  @page {");
                                            sb.AppendLine("    margin: 0;");
                                            sb.AppendLine("  }");
                                            sb.AppendLine("}");
                                            sb.AppendLine("body {");
                                            sb.AppendLine("  font-family: Arial, sans-serif;");
                                            sb.AppendLine("}");
                                            sb.AppendLine(".receipt {");
                                            sb.AppendLine("  width: 100%;");
                                            sb.AppendLine("  font-family: 'Courier New', monospace;");
                                            sb.AppendLine("  font-size: 12px;");
                                            sb.AppendLine("}");
                                            sb.AppendLine("</style>");
                                            sb.AppendLine("</head><body>");
                                            sb.AppendLine("<div class='receipt'>");

                                            //                        sb.AppendLine(" _");
                                            //sb.AppendFormat("{0}{0}{0}", Environment.NewLine);
                                            //sb.AppendLine(" **************************************");
                                            //sb.AppendLine(" COMPROVANTE DE DEVOLUÇÃO");
                                            //sb.AppendLine("@FERRAMENTARIA");
                                            //sb.AppendLine(" **************************************");
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendLine(QuebraTexto(String.Format(" OPER. REALIZADA: {0}", ((DateTime)dataDevolucao).ToString("dd/MM/yyyy HH:mm:ss"))));
                                            ////sb.AppendLine(QuebraTexto($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}"));
                                            ////sb.AppendLine(QuebraTexto($"FUNCIONARIO:{usuario.Chapa.ToString().Trim()}/{usuario.Nome.ToString().Trim()}"));

                                            //                        sb.AppendLine(QuebraTexto(String.Format(" POR BALCONISTA : {0}", usuario.Nome).TrimEnd()));
                                            //                        sb.AppendLine(QuebraTexto(String.Format(" P/ FUNCIONÁRIO : {0} / {1}", SolicitanteModel.Chapa, SolicitanteModel.Nome.TrimEnd())));

                                            //foreach (var viewModel in historicoAlocacaoList)
                                            //{
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" ID TRANSAÇÃO   : {0}", viewModel.IdHistoricoAlocacao).TrimEnd()));
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" CÓDIGO         : {0}", viewModel.CodigoCatalogo).TrimEnd()));
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" QTD. DEVOLVIDA : {0}", viewModel.Quantidade).TrimEnd()));
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" SALDO RESTANTE : {0}", viewModel.QuantidadeEmprestada).TrimEnd()));
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" DAT. EMPRÉSTIMO: {0}", viewModel.DataEmprestimo).TrimEnd()));
                                            //                            sb.AppendLine(QuebraTexto(String.Format(" DESCRIÇÃO      : {0}", viewModel.NomeCatalogo).TrimEnd()));

                                            //	if (viewModel.AFProduto != null)
                                            //	{
                                            //		//sb.AppendLine($"AF: {viewModel.AFProduto?.ToString().Trim()}");
                                            //                                sb.AppendLine(QuebraTexto(String.Format(" AF             : {0}", viewModel.AFProduto).TrimEnd()));
                                            //	}
                                            //	if (viewModel.PATProduto != 0)
                                            //	{
                                            //                                sb.AppendLine(QuebraTexto(String.Format(" PAT            : {0}", viewModel.PATProduto).TrimEnd()));
                                            //	}
                                            //	if (viewModel.Observacao != null)
                                            //	{
                                            //                                //sb.AppendLine($"Obs: {viewModel.Observacao?.ToString().Trim()}");
                                            //                                sb.AppendLine(QuebraTexto(String.Format(" NOTA BALCONISTA: {0}", viewModel.Observacao).TrimEnd()));
                                            //                                sb.AppendFormat("{0}", Environment.NewLine);
                                            //	}

                                            //                            sb.Replace("@FERRAMENTARIA", QuebraTexto(String.Format(" {0}", viewModel?.FerrOrigem.ToUpper()).TrimEnd()));
                                            //}

                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendLine(" **************************************");
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendLine(QuebraTexto((String.Format(" {0}", "O empregado está ciente de que, na eventualidade de perda, extravio ou dano do Rádio, Equipamento / EPI / Kit / Ferramenta, assim como a não devolução do mesmo quando requisitada, o valor correspondente à perda será descontado dos próximos vencimentos do empregado, nos termos do Artigo 462-1º/CLT."))));
                                            //sb.AppendLine(" **************************************");
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendFormat("{0}", Environment.NewLine);
                                            //sb.AppendLine(" _");

                                            sb.AppendLine(" _");
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(" **************************************");
                                            sb.AppendLine(" COMPROVANTE DE DEVOLUCAO");
                                            sb.AppendLine("@FERRAMENTARIA");
                                            sb.AppendLine(" **************************************");
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(QuebraTexto(String.Format(" OPER. REALIZADA: {0}", ((DateTime)dataDevolucao).ToString("dd/MM/yyyy HH:mm:ss"))));
                                            //sb.AppendLine(QuebraTexto($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}"));
                                            //sb.AppendLine(QuebraTexto($"FUNCIONARIO:{usuario.Chapa.ToString().Trim()}/{usuario.Nome.ToString().Trim()}"));
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(QuebraTexto(String.Format(" POR BALCONISTA : {0}", loggedUser.Nome).TrimEnd()));
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(QuebraTexto(String.Format(" P/ FUNCIONARIO : {0} / {1}", SolicitanteModel.Chapa, SolicitanteModel.Nome.TrimEnd())));
                                            sb.AppendFormat("<br>");
                                            foreach (var viewModel in historicoAlocacaoList)
                                            {
                                                sb.AppendLine(QuebraTexto(String.Format(" ID TRANSACAO   : {0}", viewModel.IdHistoricoAlocacao).TrimEnd()));
                                                sb.AppendFormat("<br>");
                                                sb.AppendLine(QuebraTexto(String.Format(" CODIGO         : {0}", viewModel.CodigoCatalogo).TrimEnd()));
                                                sb.AppendFormat("<br>");
                                                sb.AppendLine(QuebraTexto(String.Format(" QTD. DEVOLVIDA : {0}", viewModel.Quantidade).TrimEnd()));
                                                sb.AppendFormat("<br>");
                                                sb.AppendLine(QuebraTexto(String.Format(" SALDO RESTANTE : {0}", viewModel.QuantidadeEmprestada).TrimEnd()));
                                                sb.AppendFormat("<br>");
                                                sb.AppendLine(QuebraTexto(String.Format(" DAT. EMPRESTIMO: {0}", viewModel.DataEmprestimo).TrimEnd()));
                                                sb.AppendFormat("<br>");
                                                sb.AppendLine(QuebraTexto(String.Format(" DESCRICAO      : {0}", viewModel.NomeCatalogo).TrimEnd()));
                                                sb.AppendFormat("<br>");

                                                if (!string.IsNullOrEmpty(viewModel.AFProduto))
                                                {
                                                    //sb.AppendLine($"AF: {viewModel.AFProduto?.ToString().Trim()}");
                                                    sb.AppendLine(QuebraTexto(String.Format(" AF             : {0}", viewModel.AFProduto).TrimEnd()));
                                                    sb.AppendFormat("<br>");
                                                }
                                                if (viewModel.PATProduto != 0)
                                                {
                                                    sb.AppendLine(QuebraTexto(String.Format(" PAT            : {0}", viewModel.PATProduto).TrimEnd()));
                                                    sb.AppendFormat("<br>");
                                                }
                                                if (!string.IsNullOrEmpty(viewModel.Observacao))
                                                {
                                                    //sb.AppendLine($"Obs: {viewModel.Observacao?.ToString().Trim()}");
                                                    sb.AppendLine(QuebraTexto(String.Format(" NOTA BALCONISTA: {0}", viewModel.Observacao).TrimEnd()));
                                                    sb.AppendFormat("<br>");
                                                }

                                                sb.Replace("@FERRAMENTARIA", QuebraTexto(String.Format(" {0}", viewModel?.FerrOrigem.ToUpper()).TrimEnd()));
                                            }

                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(" **************************************");
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(QuebraTexto((String.Format(" {0}", "O empregado esta ciente de que, na eventualidade de perda, extravio ou dano do Radio, Equipamento / EPI / Kit / Ferramenta, assim como a nao devolucao do mesmo quando requisitada, o valor correspondente a perda sera descontado dos proximos vencimentos do empregado, nos termos do Artigo 462-1/CLT."))));
                                            sb.AppendLine(" **************************************");
                                            sb.AppendFormat("<br>");
                                            sb.AppendFormat("<br>");
                                            sb.AppendFormat("<br>");
                                            sb.AppendFormat("<br>");
                                            sb.AppendLine(" _");

                                            sb.AppendLine("</div>");
                                            sb.AppendLine("</body></html>");

                                            //string caminhoFisico = @"D:\Ferramentaria\Diebold\" + httpContextAccessor.["REMOTE_ADDR"] + @"\";
                                            string remoteAddr = HttpContext.Connection.RemoteIpAddress.ToString();
                                            //string? FolderPath = "C:\\Ferramentaria\\Diebold\\" + remoteAddr + "\\";
                                            string? FolderPath = "D:\\Ferramentaria\\Diebold\\" + remoteAddr + "\\";

                                            //string? FolderPath = "C:\\Repositorio\\SIB-Ferramentaria\\\\Receipts";
                                            //string? VirtualPathBase = "http://brkfbagapp27:2022/RepositorioHomolog/Diebold/";

                                            string? caminho = String.Format("{0}{1}{2}{3}{4}.txt", FolderPath, loggedUser.Nome, loggedUser.Chapa, loggedUser.CodColigada, DateTime.Now.ToString().Replace("-", "").Replace(":", "").Replace(" ", "").Replace("/", ""));
                                            //string fileName = $"{usuario.Nome}{usuario.Chapa}{usuario.CodColigada}{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";

                                            //// Combine to get the full physical path
                                            //string? caminho = Path.Combine(FolderPath, fileName);

                                            // Combine to get the full virtual path
                                            //string? caminhoVirtual = $"{VirtualPathBase}{remoteAddr}/{fileName}";

                                            //string? caminhoVirtual = "http://brkfbagapp27:2022/RepositorioHomolog/Diebold/10.188.19.18/DABY JOHN PONCE ROMERO3611985202425514PM.txt";

                                            //string? caminhoVirtual = "http://brkfbagapp27:2022/RepositorioHomolog/Diebold/";

                                            if (!Directory.Exists(FolderPath))
                                            {
                                                Directory.CreateDirectory(FolderPath);
                                            }

                                            using (StreamWriter outfile = new StreamWriter(caminho))
                                            {
                                                outfile.Write(sb.ToString());
                                            }

                                            // Pass the receipt HTML to the view or return as JSON/HTML
                                            string receiptHtml = sb.ToString();

                                            ViewBag.ReceiptHtml = sb.ToString();

                                            // Option 1: Pass the receipt to the view


                                            //ViewBag.ReceiptContent = sb.ToString();

                                            //PrintReceipt(sb.ToString());

                                            //string content = sb.ToString();

                                            //string htmlContent = $"<html><body><pre>{content}</pre></body></html>";
                                            //string PdfFileName = $"{usuario.Nome}{usuario.Chapa}{usuario.CodColigada}{DateTime.Now}";            
                                            //var pdf = auxiliar.GeneratePdfHtml(htmlContent);
                                            //return File(pdf, "application/pdf", PdfFileName);

                                            //TempData.Remove("PdfContent");
                                            //TempData.Remove("PdfFileName");
                                            //TempData.Clear();

                                            //TempData["PdfContent"] = sb.ToString();
                                            //TempData["PdfFileName"] = $"{usuario.Nome}{usuario.Chapa}{usuario.CodColigada}{DateTime.Now}";

                                            //Printer printer = new Printer("Generic / Text Only");

                                            //var oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
                                            //oManagementScope.Connect();

                                            //var oSelectQuery = new SelectQuery { QueryString = @"SELECT Name FROM Win32_Printer" };
                                            //var oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
                                            //ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

                                            //string targetPrinterName = "Generic / Text Only";
                                            //string printerName = null;
                                            //foreach (ManagementObject printers in oObjectCollection)
                                            //{
                                            //    if (printers["Name"].ToString().Equals(targetPrinterName, StringComparison.OrdinalIgnoreCase))
                                            //    {
                                            //        printerName = printers["Name"].ToString();
                                            //        break;
                                            //    }
                                            //}

                                            //var pd = new PrintDocument
                                            //{
                                            //    PrinterSettings = { PrinterName = @ConfigurationManager.AppSettings["PrinterName"] },
                                            //    DefaultPageSettings = { Landscape = true }
                                            //};

                                            //Printer printer = new Printer(printerName);

                                            //// Print a test receipt
                                            //printer.Append(sb.ToString());
                                            //printer.FullPaperCut();
                                            //printer.PrintDocument();

                                            //try
                                            //{

                                            //    _streamToPrint = new StreamReader(caminho);

                                            //    _printFont = new Font("Arial", 7);

                                            //    PrintDocument pd = new PrintDocument();
                                            //    pd.DefaultPageSettings.Margins = new Margins(
                                            //                                 Convert.ToInt32(0.147 * 100),   // Left margin in hundredths of an inch
                                            //                                 Convert.ToInt32(0.157 * 100),   // Right margin in hundredths of an inch
                                            //                                 Convert.ToInt32(0 * 100),       // Top margin in hundredths of an inch
                                            //                                 Convert.ToInt32(0.004 * 100)    // Bottom margin in hundredths of an inch
                                            //                                );

                                            //    pd.PrintPage += ImprimirPagina;

                                            //    pd.Print();

                                            //    _streamToPrint.Close();
                                            //}
                                            //catch (IOException ioEx)
                                            //{
                                            //    Console.WriteLine($"IOException: {ioEx.Message}");
                                            //    log.LogWhy = ioEx.Message;
                                            //    ErrorViewModel erro = new ErrorViewModel();
                                            //    erro.Tela = log.LogWhere;
                                            //    erro.Descricao = log.LogWhy;
                                            //    erro.Mensagem = log.LogWhat;
                                            //    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                            //}
                                            //catch (UnauthorizedAccessException authEx)
                                            //{
                                            //    Console.WriteLine($"UnauthorizedAccessException: {authEx.Message}");
                                            //    log.LogWhy = authEx.Message;
                                            //    ErrorViewModel erro = new ErrorViewModel();
                                            //    erro.Tela = log.LogWhere;
                                            //    erro.Descricao = log.LogWhy;
                                            //    erro.Mensagem = log.LogWhat;
                                            //    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                            //}
                                            //catch (Exception ex)
                                            //{
                                            //    Console.WriteLine($"Unexpected Exception: {ex.Message}");
                                            //    log.LogWhy = ex.Message;
                                            //    ErrorViewModel erro = new ErrorViewModel();
                                            //    erro.Tela = log.LogWhere;
                                            //    erro.Descricao = log.LogWhy;
                                            //    erro.Mensagem = log.LogWhat;
                                            //    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                            //}
                                        }
                                    }

                                    log.LogWhy = "Devolucao adicionada com sucesso";
                                    auxiliar.GravaLogSucesso(log);

                                    //TempData["ShowSuccessAlert"] = true;

                                    Result = RefreshDevolucao(combinedViewModel);
                                    //modelDevolucao = Result;

                                    HttpContext.Session.SetObject(SessionKeyDevolucaoList, Result);

                                    //int? testget = GlobalPagination;
                                    int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                    int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;

                                    IPagedList<DevolucaoViewModel> DevolucaoPagedList = Result.ToPagedList(pageNumber, pageSize);

                                    combinedViewModel.DevolucaoViewModel = DevolucaoPagedList;

                                    var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                                    //UsuarioModel = searches.SearchEmployeeOnLoad();
                                    combinedViewModel.UserViewModel = SolicitanteModel;
                                    combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                    combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                    combinedViewModel.ResultCount = Result.Count;

                                    //int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
                                    //ViewBag.FerramentariaValue = FerramentariaValue;

                                    ViewBag.ShowSuccessAlert = true;
                                    ViewBag.GeneratePdfAfterView = true;
                                    //ViewBag.ReceiptHtml = sb != null ? sb.ToString() : null;

                                    return View("Index", combinedViewModel);

                                }
                                else
                                {
                                    TempData["ShowErrorAlert"] = true;
                                    TempData["ErrorMessage"] = "Não foi possível completar a operação!, No selected Id";

                                    return RedirectToAction(nameof(Index));
                                }

                            }
                            else
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Não foi possível completar a operação!, Model is empty";

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
                return View(nameof(Index));
            }

        }


        [HttpGet]
        public IActionResult GeneratePdf()
        {
            try
            {
                Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
                string? content = TempData["PdfContent"]?.ToString();
                string? PdfFileName = TempData["PdfFileName"]?.ToString();

                if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(PdfFileName))
                {
                    return BadRequest("Invalid PDF data.");
                }

                string htmlContent = $@"
                                    <html>
                                    <head>
                                        <meta charset='utf-8'>
                                        <style>
                                            body {{
                                                font-family: Arial, sans-serif; /* Specify the font here */
                                            }}
                                        </style>
                                    </head>
                                    <body>
                                        <pre>{content}</pre>
                                    </body>
                                    </html>";

                var pdf = auxiliar.GeneratePdfHtml(htmlContent);

                TempData.Clear();
                TempData.Remove("PdfContent");
                TempData.Remove("PdfFileName");

                return File(pdf, "application/pdf", PdfFileName);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        private async Task<string> RenderViewToStringAsync(string viewPath)
        {
            var actionContext = new ActionContext
            {
                HttpContext = HttpContext,
                RouteData = RouteData,
                ActionDescriptor = ControllerContext.ActionDescriptor
            };

            var viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);

            //var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //dir = Path.Combine(dir, "Templates");

            //var viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: true);

            if (!viewResult.Success)
            {
                var message = $"View not found: {viewPath}. Check that the view file exists and the path is correct.";
                throw new FileNotFoundException(message, viewPath);
            }

            var cssPath = $"{Request.Scheme}://{Request.Host}/css/PdfTemplateStyle.css";


            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                viewContext.ViewData["Layout"] = cssPath;

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        public void PrintReceipt(string content)
        {
            // Define the printer settings
            PrinterSettings printerSettings = new PrinterSettings();
            PrintDocument printDocument = new PrintDocument();

            // Configure the printer
            printDocument.PrinterSettings = printerSettings;

            // Event handler to provide the content to print
            printDocument.PrintPage += (sender, e) =>
            {
                // Set font and print content
                Font printFont = new Font("Arial", 7);
                e.Graphics.DrawString(content, printFont, Brushes.Black, 10, 10);
            };

            // Print the document
            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
                // Handle exceptions (e.g., no printer installed)
                //Console.WriteLine($"Print failed: {ex.Message}");
                Log log = new Log();
                log.LogWhy = ex.Message;
                ErrorViewModel erro = new ErrorViewModel();
                erro.Tela = log.LogWhere;
                erro.Descricao = log.LogWhy;
                erro.Mensagem = log.LogWhat;
                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
            }
        }

        private void ImprimirPagina(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height / _printFont.GetHeight(ev.Graphics);

            // Print each line of the file.
            while (count < linesPerPage)
            {
                line = _streamToPrint.ReadLine();
                if (line == null)
                {
                    break;
                }
                yPos = topMargin + count * _printFont.GetHeight(ev.Graphics);
                ev.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

		private string QuebraTexto(string t)
		{
			string texto = t.TrimEnd();
			StringBuilder sb = new StringBuilder();

			double stepFor = texto.Length / 40.0;
			int startIndex = 0;

			if (stepFor > 1)
			{
				for (int index = 0; index <= stepFor; index++)
				{
					int resto = texto.Length - (startIndex + 40);

					if (resto >= 40)
					{
						if (index != 0)
						{
							startIndex += 40;
							sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, 40).TrimStart(), Environment.NewLine);
						}
						else
						{
							sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
						}
					}
					else if (resto >= 0)
					{
						if (index != 0)
						{
							startIndex += 40;
							sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
						}
						else
						{
							sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
						}
					}
					else if (resto < 0)
					{
						resto = (texto.Length - 1) - startIndex;
						if (sb.ToString().IndexOf(texto.Substring(startIndex, resto), StringComparison.Ordinal) == -1)
						{
							sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
						}
					}
				}

				return sb.ToString();
			}
			else
			{
				return string.Format(" {0}{1}", texto.TrimStart(), Environment.NewLine);
			}
		}

        public ActionResult NextPage(int? page)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
            int pageNumber = page ?? 1;

            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPaginationDevolucao);
            HttpContext.Session.SetInt32(SessionKeyPaginationDevolucao, pageSize);

            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPageNumberDevolucao);
            HttpContext.Session.SetInt32(SessionKeyPageNumberDevolucao, pageNumber);

            //GlobalPageNumber = pageNumber;

            IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

            CombinedDevolucao combined = new CombinedDevolucao
            {
                SearchDevolucaoViewModel = Searchmodel,
                UserViewModel = SolicitanteModel,
                DevolucaoViewModel = devoPagedList,
                IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                ResultCount = model.Count
            };

            return View(nameof(Index), combined);
        }

 

        public IActionResult justificarExtravio(CombinedDevolucao model)
        //public ActionResult justificarExtravio(List<int>? selectedIds)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/justificarExtravio";
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

                        var modelDevolucao = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
                        var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                        UserViewModel? SolicitanteModel = new UserViewModel();
                        string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                        if (!string.IsNullOrEmpty(SolicitanteChapa))
                        {
                            SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                        }

                        var selectedIds = model.PassedDevolucao.Where(row => row.selectedIds != null).ToList();
                        List<PassedDevolucaoModel>? ListPassedValues = new List<PassedDevolucaoModel>();

                        ListPassedValues = selectedIds;

                        if (ListPassedValues.Count == 0)
                        {
                            //TempData["ShowErrorAlert"] = true;
                            //TempData["ErrorMessage"] = "Não foi possível completar a operação!";
                            //return RedirectToAction(nameof(Index));

                            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                            combinedViewModel.UserViewModel = SolicitanteModel;
                            combinedViewModel.ResultCount = modelDevolucao.Count;

                            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                            IPagedList<DevolucaoViewModel> devoPagedList = modelDevolucao.ToPagedList(pageNumber, pageSize);

                            combinedViewModel.DevolucaoViewModel = devoPagedList;

                            ViewBag.Error = "Não foi possível completar a operação!";

                            return View(nameof(Index), combinedViewModel);
                            //return View(nameof(Index));

                        }
                        else if (ListPassedValues.Count > 1)
                        {
                            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                            combinedViewModel.UserViewModel = SolicitanteModel;
                            combinedViewModel.ResultCount = modelDevolucao.Count;

                            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                            IPagedList<DevolucaoViewModel> devoPagedList = modelDevolucao.ToPagedList(pageNumber, pageSize);

                            combinedViewModel.DevolucaoViewModel = devoPagedList;

                            ViewBag.Error = "Favor selecionar apenas 1 registro.";

                            return View(nameof(Index), combinedViewModel);
                        }
                        else
                        {

                            int? justificarId = ListPassedValues[0].IdProdutoAlocado;

                            if (justificarId != null)
                            {
                                var justificar = modelDevolucao.Where(dev => dev.IdProdutoAlocado == justificarId).FirstOrDefault();
                                ViewBag.Justificar = justificar;
                                ViewBag.OpenJustificarModal = true;
                            }

                            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                            combinedViewModel.UserViewModel = SolicitanteModel;
                            combinedViewModel.ResultCount = modelDevolucao.Count;

                            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                            IPagedList<DevolucaoViewModel> devoPagedList = modelDevolucao.ToPagedList(pageNumber, pageSize);

                            combinedViewModel.DevolucaoViewModel = devoPagedList;


                            return View(nameof(Index), combinedViewModel);

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

         

            //if (ListPassedValues.Count > 1)
            //{
            //    TempData["ShowErrorAlert"] = true;
            //    TempData["ErrorMessage"] = "Favor selecionar apenas 1 registro.";

            //    return RedirectToAction(nameof(Index));
            //}

            //int? selected = ListPassedValues[0].IdProdutoAlocado;

            ////TempData["justificarId"] = selected;
            ////TempData["OpenModalJustificar"] = true;

            //return RedirectToAction(nameof(Index), new { IdJustificar = selected });
        }

        public IActionResult exportar(List<int>? selectedIds)
        {
            //if (selectedIds.Count == 0)
            //{
            //    TempData["ShowErrorAlert"] = true;
            //    TempData["ErrorMessage"] = "Não foi possível completar a operação!, please check";

            //    return RedirectToAction(nameof(Index));
            //}

            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();

            if (model.Count == 0)
            {
                TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "List is Empty";

                return RedirectToAction(nameof(Index));
            }


            var listDevolucao = model;

            DataTable dataTable = new DataTable();
            // Add columns to the dataTable
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Catalogo");
            dataTable.Columns.Add("C.A");
            dataTable.Columns.Add("AF/Serial");
            dataTable.Columns.Add("Pat");
            dataTable.Columns.Add("FerrOrigem");
            dataTable.Columns.Add("Quantidade");
            dataTable.Columns.Add("Data do Emprestimo");
            dataTable.Columns.Add("Data Prevista para Devolução");
            dataTable.Columns.Add("Observação");
            dataTable.Columns.Add("Liberador");
            dataTable.Columns.Add("Balconista");
            dataTable.Columns.Add("Data Validade");
            dataTable.Columns.Add("Solicitante Chapa");
            dataTable.Columns.Add("Solicitante Nome");
            dataTable.Columns.Add("Extraviado");


            // Add data rows from listDevolucao
            foreach (var item in listDevolucao)
            {
                //var balconistaInfo = GetBalconistaInfo(item.Balconista_IdLogin);

                var solicitacaoinfo = GetSolicitacaoInfo(item.Solicitante_Chapa);

                var row = dataTable.NewRow();
                row["Codigo"] = item.CodigoCatalogo; // Replace with the actual property name
                row["Catalogo"] = item.NomeCatalogo; // Replace with the actual property name
                row["C.A"] = "Numero CA: " + item.NumeroControleCA + " Validade: " + item.ValidadeControlCA?.ToString("dd/MM/yyyy") + "";
                row["AF/Serial"] = item.AFProduto;
                row["Pat"] = item.PATProduto;
                row["FerrOrigem"] = item.NomeFerramentaria;
                row["Quantidade"] = item.Quantidade;
                row["Data do Emprestimo"] = item.DataEmprestimo?.ToString("dd/MM/yyyy");
                row["Data Prevista para Devolução"] = item.DataPrevistaDevolucao?.ToString("dd/MM/yyyy");
                row["Observação"] = item.Observacao;
                row["Liberador"] = item.Liberador_Chapa;
                row["Balconista"] = item.Balconista_IdLogin;
                    //balconistaInfo.Chapa; //balconista chapa
                row["Data Validade"] = item.DataVencimento?.ToString("dd/MM/yyyy"); 
                row["Solicitante Chapa"] = item.Solicitante_Chapa;
                row["Solicitante Nome"] = solicitacaoinfo.Nome; //nome
                row["Extraviado"] = item.DC_DataAquisicao?.ToString("dd/MM/yyyy");

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {

                    var worksheet = package.Workbook.Worksheets.Add("ITENS PARA DEVOLUÇÃO");

                    // Merge cells A1 to P1 and set the content to "ITENS PARA DEVOLUÇÃO"
                    worksheet.Cells["A1:P1"].Merge = true;
                    worksheet.Cells["A1"].Value = "ITENS PARA DEVOLUÇÃO";
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.Font.Name = "Arial";
                    worksheet.Cells["A1"].Style.Font.Size = 16;

                    using (var range = worksheet.Cells["A2:P2"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    // Get the columns starting from A2
                    var currentColumn = 'A'; // Start from column A
                   

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        worksheet.Cells[$"{currentColumn}2"].Value = column.ColumnName;
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Name = "Arial";
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Bold = true;
                        currentColumn++;
                    }


                    // Load data into the worksheet starting from A3
                    worksheet.Cells["A3"].LoadFromDataTable(dataTable, PrintHeaders: false);

                    var dataRange = worksheet.Cells["A3:P" + (dataTable.Rows.Count + 2)]; // Adjust the range to include all data rows

                    using (var dataCells = dataRange)
                    {
                        dataCells.Style.Font.Name = "Arial";
                        dataCells.Style.Font.Size = 10;
                    }

                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;


                    package.Save();
                }

                memoryStream.Position = 0;
                byte[] content = memoryStream.ToArray();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "DEVOLUCAO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
       
                FileContentResult fileResult = File(content, contentType, fileName);

                //string? basePath = "C:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";
                //string? basePath = "C:\\Ferramentaria\\";
                string? basePath = "D:\\Ferramentaria\\";

                if (!Directory.Exists(Path.Combine(basePath, "Relatorio")))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Path.Combine(basePath, "Relatorio"));
                }

                string caminhoFisico = basePath + "Relatorio\\" + fileName;
                System.IO.File.WriteAllBytes(caminhoFisico, content);


                return File(content, contentType, fileName);

                //// Save the Excel file to the physical path
                //string fileName = "DEVOLUCAO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                //string caminhoFisico = "D:\\Ferramentaria\\Relatorio\\" + fileName;
                //System.IO.File.WriteAllBytes(caminhoFisico, content);

                //// Define the content type
                //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //// Trigger the download
                //return File(System.IO.File.ReadAllBytes(caminhoFisico), contentType, fileName);

            }

            //return RedirectToAction(nameof(Index));
        }

        public IActionResult uploadEmLote(CombinedDevolucao model)
        //public ActionResult uploadEmLote(List<int>? selectedIds)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
            SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            var modelDevolucao = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();


            combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
            combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
            combinedViewModel.UserViewModel = SolicitanteModel;
            combinedViewModel.ResultCount = modelDevolucao.Count;

            int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
            IPagedList<DevolucaoViewModel> devoPagedList = modelDevolucao.ToPagedList(pageNumber, pageSize);

            combinedViewModel.DevolucaoViewModel = devoPagedList;

            var selectedIds = model.PassedDevolucao.Where(row => row.selectedIds != null).Select(row => row.IdProdutoAlocado).ToList();

            List<int> filteredIds = selectedIds
                                    .Where(id => id.HasValue)
                                    .Select(id => id.Value)
                                    .ToList();

            if (selectedIds.Count == 0)
            {
                ViewBag.Error = "Não foi possível completar a operação!, please check";
                return View(nameof(Index), combinedViewModel);
                //return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.ListForUpload = filteredIds;
                ViewBag.OpenBatchUploadModal = true;
                return View(nameof(Index), combinedViewModel);
            }


            //GlobalDataDevolucao.BatchUploadList = filteredIds;

            //TempData["OpenModalBatchUpload"] = true;

            //return RedirectToAction(nameof(Index));

        }

        #endregion


        #region Actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmExtravio(int? justificarQtdInput, int? justificarQtd, string? justificar, int? ProdutoId, int? AlocadoId)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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
                //    //auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de Devolucao!";
                //        log.LogWhy = usuario.Retorno;
                //        //auxiliar.GravaLogAlerta(log);
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
                        if (checkPermission.Inserir == 1)
                        {


                            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
                            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                            UserViewModel? SolicitanteModel = new UserViewModel();
                            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                            if (!string.IsNullOrEmpty(SolicitanteChapa))
                            {
                                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                            }

                            string? error = ValidateJustificativar(justificarQtdInput, justificarQtd, justificar);
                            if (string.IsNullOrEmpty(error))
                            {
                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                int? TipoExclusao = 1;
                                string? observacao = String.Concat("INCLUSÃO AUTOMÁTICA PELA TELA DE JUSTIFICATIVA DE EXTRAVIO:", justificar);

                                //insert Produto Excluido
                                var InsertProdutoExcluido = new ProdutoExcluido
                                {
                                    IdTipoExclusao = TipoExclusao,
                                    IdProduto = ProdutoId,
                                    Observacao = observacao,
                                    IdUsuario = loggedUser.Id,
                                    DataRegistro = DateTime.Now
                                };
                                _context.Add(InsertProdutoExcluido);
                                _context.SaveChanges();
                                int? insertedProdutoExcluidoId = InsertProdutoExcluido.Id;

                                //update Alocado
                                var AlocadoToMinus = _context.ProdutoAlocado.Where(alo => alo.Id == AlocadoId).FirstOrDefault();
                                if (AlocadoToMinus != null)
                                {
                                    AlocadoToMinus.Quantidade = AlocadoToMinus.Quantidade - justificarQtdInput;
                                    //_context.SaveChanges();
                                }

                                //insert extraviado
                                var InsertProdutoExtraviado = new ProdutoExtraviado
                                {
                                    IdProdutoExcluido = insertedProdutoExcluidoId,
                                    IdProdutoAlocado = AlocadoId,
                                    Quantidade = justificarQtdInput,
                                    Observacao = justificar,
                                    IdUsuario = loggedUser.Id,
                                    DataRegistro = DateTime.Now,
                                    Ativo = 1
                                };
                                _context.Add(InsertProdutoExtraviado);
                                _context.SaveChanges();

                                //update produto ativo
                                var ToCheck = model.Where(ch => ch.IdProduto == ProdutoId && ch.IdProdutoAlocado == AlocadoId).FirstOrDefault();
                                if (ToCheck.CatalogoPorAferido == 1 || ToCheck.CatalogoPorSerial == 1)
                                {
                                    var ProdutoToInactivate = _context.Produto.Where(pro => pro.Id == ProdutoId).FirstOrDefault();
                                    if (ProdutoToInactivate != null)
                                    {
                                        ProdutoToInactivate.Ativo = 0;
                                        //_context.Update(ProdutoToInactivate);
                                        _context.SaveChanges();
                                    }
                                }

                                //_context.SaveChanges();

                                log.LogWhy = "Devolucao Confrima Extravio adicionada com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();
                                Result = RefreshDevolucao(combinedViewModel);

                                HttpContext.Session.SetObject(SessionKeyDevolucaoList, Result);

                                //ListOfProducts = Result;

                                ViewBag.ShowSuccessAlert = true;

                                int pageSizeNew = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumberNew = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> NewDevoPagedList = Result.ToPagedList(pageNumberNew, pageSizeNew);
                                combinedViewModel.DevolucaoViewModel = NewDevoPagedList;

                                return View(nameof(Index), combinedViewModel);
                            }
                            else
                            {
                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);
                                combinedViewModel.DevolucaoViewModel = devoPagedList;

                                DevolucaoViewModel? JustificarValue = model.Where(dev => dev.IdProdutoAlocado == AlocadoId).FirstOrDefault();

                                log.LogWhy = "Erro na validação do modelo em criaçao devolucao!";
                                auxiliar.GravaLogAlerta(log);

                                ViewBag.ErrorJustificativar = error;
                                ViewBag.Justificar = JustificarValue;
                                ViewBag.OpenJustificarModal = true;

                                return View(nameof(Index), combinedViewModel);
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
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult UploadAction(IFormFile file, int? ProdutoAloId)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de devolucao!";
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


                            UserViewModel? SolicitanteModel = new UserViewModel();
                            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                            if (!string.IsNullOrEmpty(SolicitanteChapa))
                            {
                                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                            }

                            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
                            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                            string? error = ValidateInputsForSingleUpload(file);
                            if (string.IsNullOrEmpty(error))
                            {
                                byte[] imageData;
                                using (var stream = new MemoryStream())
                                {
                                    file.CopyTo(stream);
                                    imageData = stream.ToArray();
                                }
                                var GetSolicitanteDetails = model.FirstOrDefault(so => so.IdProdutoAlocado == ProdutoAloId);

                                var InsertToArquivo = new Arquivo
                                {
                                    IdProdutoAlocado = ProdutoAloId,
                                    Ano = DateTime.Now.Year,
                                    Solicitante_IdTerceiro = GetSolicitanteDetails.Solicitante_IdTerceiro,
                                    Solicitante_CodColigada = GetSolicitanteDetails.Solicitante_CodColigada,
                                    Solicitante_Chapa = GetSolicitanteDetails.Solicitante_Chapa,
                                    IdUsuario = loggedUser.Id,
                                    Tipo = 4,
                                    ArquivoNome = file.FileName,
                                    DataRegistro = DateTime.Now,
                                    Ativo = 1,
                                    ImageData = imageData,
                                    Responsavel = loggedUser.Nome
                                };
                                _context.Add(InsertToArquivo);
                                //_context.SaveChangesAsync();

                                var InsertToArquivoVsProdutoAlocado = new ArquivoVsProdutoAlocado
                                {
                                    IdArquivo = InsertToArquivo.Id,
                                    IdProdutoAlocado = InsertToArquivo.IdProdutoAlocado,
                                    DataRegistro = DateTime.Now
                                };
                                _context.Add(InsertToArquivoVsProdutoAlocado);
                                _context.SaveChanges();

                                //usuario.Retorno = ;
                                log.LogWhy = "Devolucao upload com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();
                                Result = RefreshDevolucao(combinedViewModel);
                                HttpContext.Session.SetObject(SessionKeyDevolucaoList, Result);
                                //ListOfProducts = Result;

                                ViewBag.ShowSuccessAlert = true;

                                int pageSizeNew = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumberNew = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> NewDevoPagedList = Result.ToPagedList(pageNumberNew, pageSizeNew);
                                combinedViewModel.DevolucaoViewModel = NewDevoPagedList;

                                return View(nameof(Index), combinedViewModel);

                                //return RedirectToAction("Index");
                            }
                            else
                            {
                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

                                combinedViewModel.DevolucaoViewModel = devoPagedList;

                                var GetListImage = _context.Arquivo.Where(im => im.IdProdutoAlocado == ProdutoAloId).ToList();

                                ViewBag.ErrorSingleUpload = error;
                                ViewBag.IdHolder = ProdutoAloId;
                                ViewBag.AlocadoImages = GetListImage;
                                ViewBag.OpenSingle = true;
                                return View(nameof(Index), combinedViewModel);
                            }


                            #region Validation
                            //if (file == null)
                            //{


                            //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                            //    log.LogWhy = usuario.Retorno;
                            //    auxiliar.GravaLogAlerta(log);


                            //    //return RedirectToAction(nameof(Index));
                            //}

                            //if (file.Length > 1048576) // 1MB (1024 bytes * 1024)
                            //{
                            //    ViewBag.ErrorSingleUpload = "File size should not exceed 1MB.";
                            //    ViewBag.IdHolder = ProdutoAloId;
                            //    ViewBag.AlocadoImages = GetListImage;
                            //    ViewBag.OpenSingle = true;

                            //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                            //    log.LogWhy = usuario.Retorno;
                            //    auxiliar.GravaLogAlerta(log);

                            //    return View(nameof(Index), combinedViewModel);
                            //    //return RedirectToAction(nameof(Index));
                            //}

                            //if (!file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) && !file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                            //{
                            //    TempData["ShowErrorAlert"] = true;
                            //    TempData["ErrorMessage"] = "Invalid file format. Only JPG images are allowed.";

                            //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                            //    log.LogWhy = usuario.Retorno;
                            //    auxiliar.GravaLogAlerta(log);

                            //    return RedirectToAction(nameof(Index));
                            //}

                            //if (file.FileName.Length > 250)
                            //{
                            //    TempData["ShowErrorAlert"] = true;
                            //    TempData["ErrorMessage"] = "Filename should not exceed 250 characters.";

                            //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                            //    log.LogWhy = usuario.Retorno;
                            //    auxiliar.GravaLogAlerta(log);

                            //    return RedirectToAction(nameof(Index));
                            //}
                            #endregion



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

        [HttpPost]
        public IActionResult BatchUploadAction(IFormFile file,List<int>? ListProdutoAlocadoId)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de devolucao!";
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

                            UserViewModel? SolicitanteModel = new UserViewModel();
                            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.SolicitanteDevolucao);
                            if (!string.IsNullOrEmpty(SolicitanteChapa))
                            {
                                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                            }

                            var model = HttpContext.Session.GetObject<List<DevolucaoViewModel>>(SessionKeyDevolucaoList) ?? new List<DevolucaoViewModel>();
                            var Searchmodel = HttpContext.Session.GetObject<SearchDevolucaoViewModel?>(SessionKeySearchModel) ?? new SearchDevolucaoViewModel();

                            string? error = ValidateInputsForSingleUpload(file);
                            if (string.IsNullOrEmpty(error))
                            {
                                byte[] imageData;

                                using (var stream = new MemoryStream())
                                {
                                    file.CopyTo(stream);
                                    imageData = stream.ToArray();
                                }

                                string? Filename = file.FileName;

                                foreach (int id in ListProdutoAlocadoId)
                                {
                                    //inserting in Arquivo
                                    var GetDetail = model.FirstOrDefault(de => de.IdProdutoAlocado == id);

                                    var InsertToArquivo = new Arquivo
                                    {
                                        IdProdutoAlocado = GetDetail.IdProdutoAlocado,
                                        Ano = DateTime.Now.Year,
                                        Solicitante_IdTerceiro = GetDetail.Solicitante_IdTerceiro,
                                        Solicitante_CodColigada = GetDetail.Solicitante_CodColigada,
                                        Solicitante_Chapa = GetDetail.Solicitante_Chapa,
                                        IdUsuario = loggedUser.Id,
                                        Tipo = 4,
                                        ArquivoNome = Filename,
                                        DataRegistro = DateTime.Now,
                                        Ativo = 1,
                                        ImageData = imageData,
                                        Responsavel = loggedUser.Nome
                                    };
                                    _context.Add(InsertToArquivo);
                                    //_context.SaveChanges();

                                    //inserting in ArquivoVsProdutoAlocado
                                    var InsertToArquivoVsProdutoAlocado = new ArquivoVsProdutoAlocado
                                    {
                                        IdArquivo = InsertToArquivo.Id,
                                        IdProdutoAlocado = InsertToArquivo.IdProdutoAlocado,
                                        DataRegistro = DateTime.Now
                                    };
                                    _context.Add(InsertToArquivoVsProdutoAlocado);
                                    _context.SaveChanges();

                                }

                                log.LogWhy = "Devolucao upload com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();
                                Result = RefreshDevolucao(combinedViewModel);

                                HttpContext.Session.SetObject(SessionKeyDevolucaoList, Result);
                                //ListOfProducts = Result;

                                ViewBag.ShowSuccessAlert = true;

                                int pageSizeNew = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumberNew = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> NewDevoPagedList = Result.ToPagedList(pageNumberNew, pageSizeNew);
                                combinedViewModel.DevolucaoViewModel = NewDevoPagedList;

                                return View(nameof(Index), combinedViewModel);
                            }
                            else
                            {
                                CombinedDevolucao? combinedViewModel = new CombinedDevolucao();
                                SearchDevolucaoViewModel? searchModel = new SearchDevolucaoViewModel();

                                combinedViewModel.IdFerramentariaUser = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                                combinedViewModel.SearchDevolucaoViewModel = Searchmodel;
                                combinedViewModel.UserViewModel = SolicitanteModel;
                                combinedViewModel.ResultCount = model.Count;

                                int pageSize = HttpContext.Session.GetInt32(SessionKeyPaginationDevolucao) ?? 10;
                                int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumberDevolucao) ?? 1;
                                IPagedList<DevolucaoViewModel> devoPagedList = model.ToPagedList(pageNumber, pageSize);

                                ViewBag.ErrorBatchModal = error;
                                ViewBag.ListForUpload = ListProdutoAlocadoId;
                                ViewBag.OpenBatchUploadModal = true;
                                return View(nameof(Index), combinedViewModel);
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

          

                //#region Validation
                //if (file == null)
                //{
                //    TempData["ShowErrorAlert"] = true;
                //    TempData["ErrorMessage"] = "No File Selected.";

                //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);

                //    return RedirectToAction(nameof(Index));
                //}

                //if (file.Length > 1048576) // 1MB (1024 bytes * 1024)
                //{
                //    TempData["ShowErrorAlert"] = true;
                //    TempData["ErrorMessage"] = "File size should not exceed 1MB.";

                //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);

                //    return RedirectToAction(nameof(Index));
                //}

                //if (!file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) && !file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                //{
                //    TempData["ShowErrorAlert"] = true;
                //    TempData["ErrorMessage"] = "Invalid file format. Only JPG images are allowed.";

                //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);

                //    return RedirectToAction(nameof(Index));
                //}

                //if (file.FileName.Length > 250)
                //{
                //    TempData["ShowErrorAlert"] = true;
                //    TempData["ErrorMessage"] = "Filename should not exceed 250 characters.";

                //    usuario.Retorno = "Erro na validação do modelo em criaçao devolucao!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);

                //    return RedirectToAction(nameof(Index));
                //}
                //#endregion

             

                //return RedirectToAction(nameof(Index));
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


        #endregion

        public string ValidateInputsForSingleUpload(IFormFile file)
        {
            if (file == null)
            {
                return "No File Selected.";
            }

            if (file.Length > 1048576)
            {
                return "File size should not exceed 1MB.";
            }

            if (!file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) && !file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return "Invalid file format. Only JPG images are allowed.";
            }

            if (file.FileName.Length > 250)
            {
               return "Filename should not exceed 250 characters.";
            }

            return null;
        }

        public string ValidateJustificativar(int? justificarQtdInput, int? justificarQtd, string? justificar)
        {
            if (justificarQtdInput > justificarQtd)
            {
                return "Input cannot be greater than Qtd.";
            }

            if (justificar == null)
            {
                return "Justificar cannot be null.";
            }

            if (justificar.Length > 250)
            {
                return "Justificativa de Extravio excedeu o limite de 250 caracteres.";
            }

            return null;
        }

        public List<DevolucaoViewModel> RefreshDevolucao(CombinedDevolucao? CombinedDevolucao)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            List<DevolucaoViewModel> Result = new List<DevolucaoViewModel>();

            if (CombinedDevolucao != null)
            {
                if (CombinedDevolucao.UserViewModel != null)
                {                    
                    SearchFilters searchFilters = new SearchFilters
                    {
                        Chapa = CombinedDevolucao.UserViewModel.Chapa,
                        CodColigada = CombinedDevolucao.UserViewModel.CodColigada,
                        Observacao = CombinedDevolucao.SearchDevolucaoViewModel.Observacao != null ? CombinedDevolucao.SearchDevolucaoViewModel.Observacao : null,
                        TransacoesDe = CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe != null ? CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe : null,
                        TransacoesAte = CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte != null ? CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte : null,
                        PrevisaoDe = CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe != null ? CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe : null,
                        PrevisaoAte = CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte != null ? CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte : null,
                        Codigo = CombinedDevolucao.SearchDevolucaoViewModel.Codigo != null ? CombinedDevolucao.SearchDevolucaoViewModel.Codigo : null,
                        Catalogo = CombinedDevolucao.SearchDevolucaoViewModel.Catalogo != null ? CombinedDevolucao.SearchDevolucaoViewModel.Catalogo : null,
                        AF = CombinedDevolucao.SearchDevolucaoViewModel.AF != null ? CombinedDevolucao.SearchDevolucaoViewModel.AF : null,
                        DataDeValidade = CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade != null ? CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade : null
                    };
                    Result = searches.SearchDevolucaoList(searchFilters);

                    if (Result.Any())
                    {
                        foreach (var item in Result)
                        {
                            //int? balconista = item.Balconista_IdLogin;

                            //var usuario =  _contextBS.VW_Usuario_New
                            //    .FirstOrDefault(u => u.Id == balconista);

                            //if (usuario != null)
                            //{
                            //    item.Balconista_IdLogin = int.Parse(usuario.Chapa);
                            //}
                            //else
                            //{
                            //    var usuarioOld = _contextBS.VW_Usuario
                            //    .FirstOrDefault(u => u.Id == balconista);

                            //    item.Balconista_IdLogin = int.Parse(usuarioOld.Chapa);
                            //}

                            int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);
                            item.QuantidadeExtraviada = extraviadoQuantity;

                            var GetListImage = _context.Arquivo.Where(im => im.IdProdutoAlocado == item.IdProdutoAlocado).ToList();
                            if (GetListImage.Count > 0)
                            {
                                item.FileFound = true;
                            }

                        }
                      
                    }
                    else
                    {
                        ViewBag.Error = "No Searched has been found.";
                    }
                }

            }

            return Result;

        }

        //public List<DevolucaoViewModel> RefreshDevolucao(CombinedDevolucao? CombinedDevolucao)
        //{
        //    List<DevolucaoViewModel> Result = null;

        //    Result = (from produtoAlocado in _context.ProdutoAlocado
        //              join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
        //              join ferrOndeProdRetirado in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOndeProdRetirado.Id
        //              join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //              join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //              join obra in _context.Obra on produtoAlocado.IdObra equals obra.Id
        //              join controleca in _context.ControleCA on produtoAlocado.IdControleCA equals controleca.Id into controlecaGroup
        //              from controleca in controlecaGroup.DefaultIfEmpty()
        //              where (CombinedDevolucao.UserViewModel.CodColigada == null || produtoAlocado.Solicitante_CodColigada == CombinedDevolucao.UserViewModel.CodColigada)
        //                  && (CombinedDevolucao.UserViewModel.Chapa == null || produtoAlocado.Solicitante_Chapa == CombinedDevolucao.UserViewModel.Chapa)
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.Observacao == null || produtoAlocado.Observacao.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Observacao))
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe == null || produtoAlocado.DataEmprestimo >= CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe)
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte == null || produtoAlocado.DataEmprestimo >= CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte)
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe == null || produtoAlocado.DataPrevistaDevolucao >= CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe)
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte == null || produtoAlocado.DataPrevistaDevolucao >= CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte)
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.Codigo == null || catalogo.Codigo.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Codigo))
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.Catalogo == null || catalogo.Nome.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Catalogo))
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.AF == null || produto.AF.Contains(CombinedDevolucao.SearchDevolucaoViewModel.AF))
        //                  && (CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade == null || produto.DataVencimento == CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade)
        //              orderby produtoAlocado.DataEmprestimo descending
        //              select new DevolucaoViewModel
        //              {
        //                  IdProdutoAlocado = produtoAlocado.Id,
        //                  Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
        //                  Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
        //                  Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
        //                  Balconista_IdLogin = produtoAlocado.Balconista_IdLogin,
        //                  Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
        //                  Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
        //                  Liberador_Chapa = produtoAlocado.Liberador_Chapa,
        //                  Observacao = produtoAlocado.Observacao,
        //                  DataEmprestimo = produtoAlocado.DataEmprestimo,
        //                  DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
        //                  Quantidade = produtoAlocado.Quantidade,
        //                  IdFerramentaria = ferrOndeProdRetirado.Id,
        //                  NomeFerramentaria = ferrOndeProdRetirado.Nome,
        //                  IdObra = obra.Id,
        //                  NomeObra = obra.Nome,
        //                  IdProduto = produto.Id,
        //                  AFProduto = produto.AF,
        //                  PATProduto = produto.PAT,
        //                  DataVencimento = produto.DataVencimento,
        //                  DC_DataAquisicao = produto.DC_DataAquisicao,
        //                  DC_Valor = produto.DC_Valor,
        //                  CodigoCatalogo = catalogo.Codigo,
        //                  NomeCatalogo = catalogo.Nome,
        //                  CatalogoPorAferido = catalogo.PorAferido,
        //                  CatalogoPorSerial = catalogo.PorSerial,
        //                  ImpedirDescarte = catalogo.ImpedirDescarte,
        //                  HabilitarDescarteEPI = catalogo.HabilitarDescarteEPI,
        //                  IdCategoria = categoria.Id,
        //                  ClasseCategoria = categoria.Classe,
        //                  NomeCategoria = categoria.Nome,
        //                  ProdutoAtivo = produto.Ativo,
        //                  IdControleCA = produtoAlocado.IdControleCA,
        //                  NumeroControleCA = controleca.NumeroCA,
        //                  ValidadeControlCA = controleca.Validade,
        //              }).ToList();

        //    if (Result.Any())
        //    {
        //        foreach (var item in Result)
        //        {
        //            int? balconista = item.Balconista_IdLogin;

        //            var usuario = await _contextBS.VW_Usuario_New
        //                .FirstOrDefaultAsync(u => u.Id == balconista);

        //            if (usuario != null)
        //            {
        //                item.Balconista_IdLogin = int.Parse(usuario.Chapa);
        //            }
        //            else
        //            {
        //                var usuarioOld = await _contextBS.VW_Usuario
        //                .FirstOrDefaultAsync(u => u.Id == balconista);

        //                item.Balconista_IdLogin = int.Parse(usuarioOld.Chapa);
        //            }

        //        }

        //        GlobalValues.DevolucaoViewModel = Result;

        //        GlobalPagination = 10;
        //    }
        //    else
        //    {
        //        TempData["ShowErrorAlert"] = true;
        //        TempData["ErrorMessage"] = "No Searched has been found.";
        //    }

        //    return Result;
        //}

        public VW_Usuario_New GetBalconistaInfo(int? balconistaId)
        {
            return _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == balconistaId);
        }

        public Funcionario GetSolicitacaoInfo(string? solicitanteChapa)
        {
            return _contextBS.Funcionario.FirstOrDefault(u => u.Chapa == solicitanteChapa);
        }


        #region Not Used

        //public async Task<IActionResult> executarDevolucao(List<int>? selectedIds, int? PrintTicket)
        //{
        //    Log log = new Log();
        //    log.LogWhat = pagina + "/Index";
        //    log.LogWhere = pagina;
        //    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
        //    try
        //    {
        //        #region Authenticate User
        //        VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
        //        //usuario.Pagina = "Home/Index";
        //        usuario.Pagina = pagina;
        //        usuario.Pagina1 = log.LogWhat;
        //        usuario.Acesso = log.LogWhat;
        //        usuario = auxiliar.VerificaPermissao(usuario);

        //        if (usuario.Permissao == null)
        //        {
        //            usuario.Retorno = "Usuário sem permissão na página!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);
        //            return RedirectToAction("Login", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Inserir != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de Inserir a página de devolucao!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("Login", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        if (selectedIds.Count == 0)
        //        {
        //            TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Não foi possível completar a operação!, please check";

        //            return RedirectToAction(nameof(Index));
        //        }

        //        string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);

        //        foreach (int id in selectedIds)
        //        {
        //            var GetDatas = GlobalDataDevolucao.listDevolucao.FirstOrDefault(e => e.IdProduto == id);

        //            var qtddevValue = Request.Form["qtddevValues_" + GetDatas.IdProdutoAlocado];

        //            var getFerr = Request.Form["ferrdev_" + GetDatas.IdProdutoAlocado];



        //            var GetProdutoAlocado = _context.ProdutoAlocado.FirstOrDefault(e => e.Id == GetDatas.IdProdutoAlocado);

        //            if (GetProdutoAlocado.Quantidade >= 1)
        //            {


        //                int ano = DateTime.Now.Year;
        //                string tableName = $"HistoricoAlocacao_{ano}";

        //                var tableExists = _context.Database.SqlQueryRaw<int>(
        //                                    "IF OBJECT_ID('" + tableName + "') IS NOT NULL " +
        //                                    "SELECT 1 " +
        //                                    "ELSE " +
        //                                    "SELECT 0"
        //                                ).AsEnumerable().FirstOrDefault();

        //                if (tableExists == 0)
        //                {
        //                    TempData["ShowErrorAlert"] = true;
        //                    TempData["ErrorMessage"] = $"Please Contact IT Department, Because {tableName} doesnt exist ";

        //                    return RedirectToAction(nameof(Index));
        //                }
        //                else
        //                {
        //                    var InsertHistoricoAlocacao2024 = new HistoricoAlocacao_2024
        //                    {
        //                        IdProduto = GetProdutoAlocado.IdProduto,
        //                        Solicitante_IdTerceiro = GetProdutoAlocado.Solicitante_IdTerceiro,
        //                        Solicitante_CodColigada = GetProdutoAlocado.Solicitante_CodColigada,
        //                        Solicitante_Chapa = GetProdutoAlocado.Solicitante_Chapa,
        //                        //GetProdutoAlocado.Solicitante_Chapa,
        //                        Liberador_IdTerceiro = GetProdutoAlocado.Liberador_IdTerceiro,
        //                        Liberador_CodColigada = GetProdutoAlocado.Liberador_CodColigada,
        //                        Liberador_Chapa = GetProdutoAlocado.Liberador_Chapa,
        //                        Balconista_Emprestimo_IdLogin = GetProdutoAlocado.Balconista_IdLogin,
        //                        Balconista_Devolucao_IdLogin = usuario.Id,
        //                        Observacao = GetProdutoAlocado.Observacao,
        //                        DataEmprestimo = GetProdutoAlocado.DataEmprestimo,
        //                        DataPrevistaDevolucao = GetProdutoAlocado.DataPrevistaDevolucao,
        //                        DataDevolucao = DateTime.Now,
        //                        IdObra = GetProdutoAlocado.IdObra,
        //                        Quantidade = int.Parse(qtddevValue),
        //                        IdFerrOndeProdRetirado = GetProdutoAlocado.IdFerrOndeProdRetirado,
        //                        IdFerrOndeProdDevolvido = int.Parse(getFerr),
        //                        IdControleCA = GetProdutoAlocado.IdControleCA
        //                    };

        //                    _context.Add(InsertHistoricoAlocacao2024);
        //                    await _context.SaveChangesAsync();


        //                    var CheckArquivo = _context.ArquivoVsProdutoAlocado.FirstOrDefault(e => e.IdProdutoAlocado == GetDatas.IdProdutoAlocado);

        //                    if (CheckArquivo != null)
        //                    {
        //                        var InsertArquivoHistorico = new ArquivoVsHistorico
        //                        {
        //                            IdArquivo = CheckArquivo.IdArquivo,
        //                            IdHistoricoAlocacao = InsertHistoricoAlocacao2024.Id,
        //                            Ano = ano,
        //                            DataRegistro = DateTime.Now
        //                        };

        //                        _context.Add(InsertArquivoHistorico);
        //                        await _context.SaveChangesAsync();

        //                        var arquivoVsProdutoAlocadoToDelete = _context.ArquivoVsProdutoAlocado
        //                            .FirstOrDefault(a => a.IdProdutoAlocado == GetDatas.IdProdutoAlocado);

        //                        // Find the entity with Id equal to 2596431 in ProdutoAlocado
        //                        var produtoAlocadoToDelete = _context.ProdutoAlocado
        //                            .FirstOrDefault(p => p.Id == GetDatas.IdProdutoAlocado);

        //                        if (arquivoVsProdutoAlocadoToDelete != null)
        //                        {
        //                            _context.ArquivoVsProdutoAlocado.Remove(arquivoVsProdutoAlocadoToDelete);
        //                        }

        //                        if (produtoAlocadoToDelete != null)
        //                        {
        //                            _context.ProdutoAlocado.Remove(produtoAlocadoToDelete);
        //                        }

        //                        _context.SaveChanges();

        //                    }

        //                }

        //            }


        //        }


        //        if (PrintTicket == 1)
        //        {


        //            DateTime dateTimeOneMinuteAgo = DateTime.Now.AddMinutes(-1);

        //            var result = from hist in _context.HistoricoAlocacao_2023
        //                         join produto in _context.Produto on hist.IdProduto equals produto.Id
        //                         join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                         join ferrOrigem in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrigem.Id
        //                         join ferrDevolucao in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDevolucao.Id into devolucaoGroup
        //                         from devolucao in devolucaoGroup.DefaultIfEmpty()
        //                         where hist.Balconista_Devolucao_IdLogin == usuario.Id &&
        //                               hist.Solicitante_Chapa == FuncionarioValue &&
        //                               hist.Solicitante_CodColigada == 2 &&
        //                               hist.Solicitante_IdTerceiro == 0 &&
        //                               hist.DataDevolucao >= dateTimeOneMinuteAgo
        //                         select new HistoricoAlocacaoViewModel
        //                         {
        //                             IdHistoricoAlocacao = hist.Id,
        //                             IdProdutoAlocado = 0,
        //                             IdProduto = hist.IdProduto,
        //                             Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
        //                             Solicitante_CodColigada = hist.Solicitante_CodColigada,
        //                             Solicitante_Chapa = hist.Solicitante_Chapa,
        //                             Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
        //                             Liberador_CodColigada = hist.Liberador_CodColigada,
        //                             Liberador_Chapa = hist.Liberador_Chapa,
        //                             Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
        //                             Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
        //                             Observacao = hist.Observacao,
        //                             DataEmprestimo = hist.DataEmprestimo,
        //                             DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
        //                             DataDevolucao = hist.DataDevolucao,
        //                             IdObra = hist.IdObra,
        //                             Quantidade = hist.Quantidade,
        //                             QuantidadeEmprestada = catalogo.PorAferido == 0 && catalogo.PorSerial == 0
        //                                 ? (_context.ProdutoAlocado
        //                                        .Where(pa =>
        //                                            pa.IdProduto == hist.IdProduto &&
        //                                            pa.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
        //                                            pa.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
        //                                            pa.Solicitante_Chapa == hist.Solicitante_Chapa)
        //                                        .OrderBy(pa => pa.Id)
        //                                        .Select(pa => pa.Quantidade)
        //                                        .FirstOrDefault() ?? 0)
        //                                 : 0,
        //                             IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
        //                             IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
        //                             CodigoCatalogo = catalogo.Codigo,
        //                             NomeCatalogo = catalogo.Nome,
        //                             FerrOrigem = ferrOrigem.Nome,
        //                             FerrDevolucao = devolucao.Nome,
        //                             AFProduto = produto.AF,
        //                             Serie = produto.Serie,
        //                             PATProduto = produto.PAT,
        //                             IdControleCA = hist.IdControleCA
        //                         };


        //            //var sb = new StringBuilder();
        //            //sb.AppendLine(" ____");
        //            //sb.AppendLine();
        //            //sb.AppendLine();
        //            //sb.AppendLine();
        //            //sb.AppendLine("**************************************");
        //            //sb.AppendLine("COMPROVANTE DE DEVOLUÇÃO");
        //            //sb.AppendLine("@FERRAMENTARIA");
        //            //sb.AppendLine(" **************************************");
        //            //sb.AppendLine($"OPER. REALIZADA: {DateTime.Now}");
        //            //sb.AppendLine($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}");
        //            //sb.AppendLine($"FUNCIONARIO: {usuario.Nome.ToString().Trim()}");

        //            ////foreach (int ReceiptId in selectedIds)
        //            ////{


        //            ////}


        //            //foreach (HistoricoAlocacaoViewModel viewModel in result)
        //            //{
        //            //    sb.AppendLine($"ID TRANSAÇÃO: {viewModel.IdHistoricoAlocacao?.ToString().Trim()}");
        //            //    sb.AppendLine($"CODIGO: {viewModel.CodigoCatalogo?.ToString().Trim()}");
        //            //    sb.AppendLine($"QTD. DEVOLVIDA: {viewModel.Quantidade?.ToString().Trim()}");
        //            //    sb.AppendLine($"SALDO RESTANTE: {viewModel.QuantidadeEmprestada?.ToString().Trim()}");
        //            //    sb.AppendLine($"DAT. EMPRESTIMO: {viewModel.DataEmprestimo?.ToString().Trim()}");
        //            //    sb.AppendLine($"DESCRIÇÃO: {viewModel.NomeCatalogo?.ToString().Trim()}");

        //            //}

        //            //sb.AppendLine();
        //            //sb.AppendLine(" **************************************");
        //            //sb.AppendLine((String.Format(" {0}", "O empregado está ciente de que, na eventualidade de perda, extravio ou dano do Rádio, Equipamento / EPI / Kit / Ferramenta, assim como a não devolução do mesmo quando requisitada, o valor correspondente à perda será descontado dos próximos vencimentos do empregado, nos termos do Artigo 462-1º/CLT.")));
        //            //sb.AppendLine(" **************************************");
        //            //sb.AppendLine();
        //            //sb.AppendLine();
        //            //sb.AppendLine();
        //            //sb.AppendLine(" ____");

        //            #region Different Print

        //            //PrintDocument printDocument = new PrintDocument();

        //            //Margins margins = new Margins(30, 30, 20, 20); // Left: 0.3, Right: 0.3, Top: 0.2, Bottom: 0.2
        //            //printDocument.DefaultPageSettings.Margins = margins;

        //            //PaperSize pageSize = new PaperSize();
        //            //pageSize.Width = 284;
        //            //printDocument.DefaultPageSettings.PaperSize = pageSize;

        //            //printDocument.PrintPage += (sender, e) =>
        //            //{
        //            //    Graphics graphics = e.Graphics;
        //            //    Font printFont = new Font("Arial", 10); // Adjust the font size

        //            //    float yPos = e.MarginBounds.Top; // Adjust yPos to consider the top margin
        //            //    float leftMargin = e.MarginBounds.Left;

        //            //    graphics.DrawString("ESTALEIRO BRASFELS LTDA", printFont, Brushes.Black, 20, 10);
        //            //    graphics.DrawString("COMPROVANTE", printFont, Brushes.Black, 30, 30);
        //            //    graphics.DrawString("**********************", printFont, Brushes.Black, 110, 50);
        //            //    graphics.DrawLine(Pens.Black, 80, 70, 320, 70);

        //            //    graphics.DrawLine(Pens.Black, 80, 100, 320, 100);

        //            //    graphics.DrawString($"OPER. REALIZADA: {DateTime.Now}", printFont, Brushes.Black, 30, 30);
        //            //    graphics.DrawString($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}", printFont, Brushes.Black, 30, 30);
        //            //    graphics.DrawString($"FUNCIONARIO: {usuario.Nome.ToString().Trim()}", printFont, Brushes.Black, 30, 30);

        //            //    // Your receipt content
        //            //    string[] receiptContent = new string[]
        //            //    {
        //            //        " ____",
        //            //        "",
        //            //        "",
        //            //        "",
        //            //        "**************************************",
        //            //        "COMPROVANTE DE DEVOLUÇÃO",
        //            //        "@FERRAMENTARIA",
        //            //        "**************************************",
        //            //        $"OPER. REALIZADA: {DateTime.Now}",
        //            //        $"POR BALCONISTA: {usuario.Nome.ToString().Trim()}",
        //            //        $"FUNCIONARIO: {usuario.Nome.ToString().Trim()}",
        //            //        // Add other receipt content
        //            //    };

        //            //    // Print each line of the receipt content
        //            //    foreach (string line in receiptContent)
        //            //    {
        //            //        graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos);
        //            //        yPos += printFont.GetHeight();
        //            //    }

        //            //    If more content exists, set HasMorePages to true
        //            //    e.HasMorePages = false;
        //            //};

        //            //Use PrintDocument's Print method to start the printing process
        //            //printDocument.Print();

        //            #endregion

        //            PrintDocument printDocument = new PrintDocument();

        //            // Set the margins in hundredths of an inch
        //            Margins margins = new Margins(30, 30, 20, 20); // Left: 0.3, Right: 0.3, Top: 0.2, Bottom: 0.2
        //            printDocument.DefaultPageSettings.Margins = margins;

        //            // Your receipt content
        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine(" ____");
        //            sb.AppendLine("\r\n");
        //            sb.AppendLine("\r\n");
        //            sb.AppendLine("\r\n");
        //            //sb.AppendLine("*");
        //            sb.AppendLine("*****************************************");
        //            sb.AppendLine("COMPROVANTE DE DEVOLUCAO");
        //            sb.AppendLine("@FERRAMENTARIA");
        //            sb.AppendLine("*****************************************");
        //            sb.AppendLine($"OPER. REALIZADA: {DateTime.Now}");
        //            sb.AppendLine($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}");
        //            sb.AppendLine($"FUNCIONARIO:{usuario.Chapa.ToString().Trim()}/{usuario.Nome.ToString().Trim()}");

        //            foreach (HistoricoAlocacaoViewModel viewModel in result)
        //            {
        //                sb.AppendLine($"ID TRANSACAO: {viewModel.IdHistoricoAlocacao?.ToString().Trim()}");
        //                sb.AppendLine($"CODIGO: {viewModel.CodigoCatalogo?.ToString().Trim()}");
        //                sb.AppendLine($"QTD. DEVOLVIDA: {viewModel.Quantidade?.ToString().Trim()}");
        //                sb.AppendLine($"SALDO RESTANTE: {viewModel.QuantidadeEmprestada?.ToString().Trim()}");
        //                sb.AppendLine($"DAT. EMPRESTIMO: {viewModel.DataEmprestimo?.ToString().Trim()}");
        //                //sb.AppendLine($"DESCRICAO: {viewModel.NomeCatalogo?.ToString().Trim()}");


        //                string descricao = viewModel.NomeCatalogo?.ToString().Trim();

        //                if (!string.IsNullOrEmpty(descricao))
        //                {
        //                    // Check if the description is longer than 25 characters
        //                    if (descricao.Length > 25)
        //                    {
        //                        // Break the description into lines with a maximum length of 25 characters
        //                        int maxLineLength = 25;
        //                        for (int i = 0; i < descricao.Length; i += maxLineLength)
        //                        {
        //                            int length = Math.Min(maxLineLength, descricao.Length - i);
        //                            string line = descricao.Substring(i, length);
        //                            sb.AppendLine($"DESCRICAO: {line}");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        sb.AppendLine($"DESCRICAO: {descricao}");
        //                    }
        //                }

        //                sb.Replace("@FERRAMENTARIA", viewModel.FerrOrigem?.ToString().ToUpper().Trim());
        //            }

        //            sb.AppendLine();
        //            sb.AppendLine();
        //            sb.AppendLine();
        //            sb.AppendLine("*****************************************");
        //            sb.AppendLine("O empregado esta ciente de que,");
        //            sb.AppendLine("na eventualidade de perda,");
        //            sb.AppendLine("extravio ou dano do Rádio,");
        //            sb.AppendLine("Equipamento/EPI/Kit/Ferramenta,");
        //            sb.AppendLine("assim como a não devolução do mesmo quando requisitada,");
        //            sb.AppendLine("o valor correspondente à perda será");
        //            sb.AppendLine("descontado dos próximos vencimentos do empregado,");
        //            sb.AppendLine("nos termos do Artigo 462-1º/CLT.");
        //            sb.AppendLine(" **************************************");
        //            sb.AppendLine();
        //            sb.AppendLine();
        //            sb.AppendLine();
        //            sb.AppendLine(" ____");
        //            // Add other receipt content

        //            string[] receiptContent = sb.ToString().Split('\n');

        //            int currentLine = 0;

        //            float lineHeight = 0.5f; // Adjust the spacing factor as needed

        //            printDocument.PrintPage += (sender, e) =>
        //            {
        //                Graphics graphics = e.Graphics;
        //                Font printFont = new Font("Arial", 8); // Adjust the font size

        //                float yPos = 0.2f * 55; // Assuming 100 DPI and 1.8 inches of white space
        //                float leftMargin = e.MarginBounds.Left;

        //                // Print each line of the receipt content
        //                while (currentLine < receiptContent.Length)
        //                {
        //                    graphics.DrawString(receiptContent[currentLine], printFont, Brushes.Black, leftMargin, yPos);
        //                    yPos += printFont.GetHeight() * lineHeight; 
        //                    currentLine++;

        //                    // If there is more content, set HasMorePages to true
        //                    if (currentLine < receiptContent.Length)
        //                    {
        //                        e.HasMorePages = true;
        //                        return;
        //                    }
        //                }

        //                // If there is no more content, set HasMorePages to false
        //                e.HasMorePages = false;
        //            };

        //            // Use PrintDocument's Print method to start the printing process
        //            printDocument.Print();



        //        }


        //        usuario.Retorno = "Devolucao adicionada com sucesso";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogWhy = ex.Message;
        //        ErrorViewModel erro = new ErrorViewModel();
        //        erro.Tela = log.LogWhere;
        //        erro.Descricao = log.LogWhy;
        //        erro.Mensagem = log.LogWhat;
        //        erro.IdLog = auxiliar.GravaLogRetornoErro(log);
        //        return View();
        //    }

        //}

        // GET: DevolucaoController/Details/5
        //public ActionResult Details(int id)
        //{
        //    (from produtoAlocado in _context.ProdutoAlocado
        //     join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
        //     join ferrOndeProdRetirado in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOndeProdRetirado.Id
        //     join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //     join obra in _context.Obra on produtoAlocado.IdObra equals obra.Id
        //     join controleca in _context.ControleCA on produtoAlocado.IdControleCA equals controleca.Id into controlecaGroup
        //     from controleca in controlecaGroup.DefaultIfEmpty()
        //     where (CombinedDevolucao.UserViewModel.CodColigada == null || produtoAlocado.Solicitante_CodColigada == CombinedDevolucao.UserViewModel.CodColigada)
        //         && (CombinedDevolucao.UserViewModel.Chapa == null || produtoAlocado.Solicitante_Chapa == CombinedDevolucao.UserViewModel.Chapa)
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.Observacao == null || produtoAlocado.Observacao.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Observacao))
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe == null || produtoAlocado.DataEmprestimo >= CombinedDevolucao.SearchDevolucaoViewModel.TransacoesDe)
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte == null || produtoAlocado.DataEmprestimo <= CombinedDevolucao.SearchDevolucaoViewModel.TransacoesAte)
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe == null || produtoAlocado.DataPrevistaDevolucao >= CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoDe)
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte == null || produtoAlocado.DataPrevistaDevolucao <= CombinedDevolucao.SearchDevolucaoViewModel.PrevisaoAte)
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.Codigo == null || catalogo.Codigo.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Codigo))
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.Catalogo == null || catalogo.Nome.Contains(CombinedDevolucao.SearchDevolucaoViewModel.Catalogo))
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.AF == null || produto.AF.Contains(CombinedDevolucao.SearchDevolucaoViewModel.AF))
        //         && (CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade == null || produto.DataVencimento == CombinedDevolucao.SearchDevolucaoViewModel.DataDeValidade)
        //     orderby produtoAlocado.DataEmprestimo descending
        //     select new DevolucaoViewModel
        //     {
        //         IdProdutoAlocado = produtoAlocado.Id,
        //         Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
        //         Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
        //         Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
        //         Balconista_IdLogin = produtoAlocado.Balconista_IdLogin,
        //         Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
        //         Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
        //         Liberador_Chapa = produtoAlocado.Liberador_Chapa,
        //         Observacao = produtoAlocado.Observacao,
        //         DataEmprestimo = produtoAlocado.DataEmprestimo,
        //         DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
        //         Quantidade = produtoAlocado.Quantidade,
        //         IdFerramentaria = ferrOndeProdRetirado.Id,
        //         NomeFerramentaria = ferrOndeProdRetirado.Nome,
        //         IdObra = obra.Id,
        //         NomeObra = obra.Nome,
        //         IdProduto = produto.Id,
        //         AFProduto = produto.AF,
        //         PATProduto = produto.PAT,
        //         DataVencimento = produto.DataVencimento,
        //         DC_DataAquisicao = produto.DC_DataAquisicao,
        //         DC_Valor = produto.DC_Valor,
        //         CodigoCatalogo = catalogo.Codigo,
        //         NomeCatalogo = catalogo.Nome,
        //         CatalogoPorAferido = catalogo.PorAferido,
        //         CatalogoPorSerial = catalogo.PorSerial,
        //         ImpedirDescarte = catalogo.ImpedirDescarte,
        //         HabilitarDescarteEPI = catalogo.HabilitarDescarteEPI,
        //         IdCategoria = categoria.Id,
        //         ClasseCategoria = categoria.Classe,
        //         NomeCategoria = categoria.Nome,
        //         ProdutoAtivo = produto.Ativo,
        //         IdControleCA = produtoAlocado.IdControleCA,
        //         NumeroControleCA = controleca.NumeroCA,
        //         ValidadeControlCA = controleca.Validade,
        //     }).ToList();

        //    return View();
        //}

        #endregion

        //Not Used
        public ActionResult SearchDev(CombinedDevolucao? CombinedDevolucao)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
            UserViewModel? UsuarioModel = new UserViewModel();

            VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            if (CombinedDevolucao?.UserViewModel != null && ModelState["UserViewModel.filter"].Errors.Count == 0)
            {
                listTerceiroResult = searches.SearchTercerio(CombinedDevolucao.UserViewModel.filter);
                TotalResult.AddRange(listTerceiroResult);

                listEmployeeResult = searches.SearchEmployeeChapa(CombinedDevolucao.UserViewModel.filter);
                TotalResult.AddRange(listEmployeeResult);

                if (GlobalValues.DevolucaoViewModel.Count > 0)
                {
                    GlobalValues.DevolucaoViewModel.Clear();
                }

                if (TotalResult.Count > 1)
                {
                    ViewBag.ListOfEmployees = TotalResult;
                    return View("Index");
                }
                else if (TotalResult.Count == 1)
                {
                    httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                    httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, TotalResult[0].Chapa);

                    UsuarioModel = searches.SearchEmployeeOnLoad();
                    List<MensagemSolicitanteViewModel> messages = new List<MensagemSolicitanteViewModel>();

                    messages = searches.SearchMensagem(UsuarioModel.Chapa, usuariofer.Id);
                    ViewBag.Messages = messages.Count > 0 ? messages : null;

                    var combinedViewModel = new CombinedDevolucao();
                    SearchDevolucaoViewModel? searchfilter = new SearchDevolucaoViewModel();
                    combinedViewModel.UserViewModel = UsuarioModel;
                    combinedViewModel.SearchDevolucaoViewModel = searchfilter;

                    return View("Index", combinedViewModel);
                    //return RedirectToAction(nameof(Index));
                }
                else if (listEmployeeResult.Count == 0)
                {
                    ViewBag.Error = "No Searched has been found.";
                    return View("Index");
                }

            }
            else
            {
                ModelState.AddModelError("UserViewModel.filter", "Matricula/Nome is Required");
                return View("Index");
            }

            return View("Index");
        }

        public ActionResult TestPage(int? page, string searchCriteria, string UserModel)
        {
            var searcDevolucaoModel = JsonConvert.DeserializeObject<SearchDevolucaoViewModel>(searchCriteria);
            var userModel = JsonConvert.DeserializeObject<UserViewModel>(UserModel);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            IPagedList<DevolucaoViewModel> devoPagedList = GlobalValues.DevolucaoViewModel.ToPagedList(pageNumber, pageSize);

            CombinedDevolucao combined = new CombinedDevolucao
            {
                SearchDevolucaoViewModel = searcDevolucaoModel,
                UserViewModel = userModel,
                DevolucaoViewModel = devoPagedList
            };

            return View("Index", combined);
        }







    }
}
