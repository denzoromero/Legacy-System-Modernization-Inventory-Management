using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
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
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using OfficeOpenXml;
using Azure.Core;
using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using Microsoft.IdentityModel.Tokens;


namespace FerramentariaTest.Controllers
{
    public class EntradaEmLote_ReqController : Controller
    {
        private const string SessionKeyCombinedLoteValues = "CombinedLoteValues";
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thEntradaEmLote.aspx"; 
        private MapperConfiguration mapeamentoClasses;

        //private static int? GlobalPagination;
        //private static int? GlobalPageNumber;

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyInputtedRFM = "InputtedRFM";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        #region Index variables
        //private static List<EntradaEmLote_ReqViewModel> _ListEntrada = new List<EntradaEmLote_ReqViewModel>();
        //private static List<CatalogoViewModel> _ListCatalogo = new List<CatalogoViewModel>();

        //private static CombinedEmLote? CombinedLoteValues = new CombinedEmLote();
        #endregion

        #region Create Variables

        private static int? PaginationCreate;
        private static int? PageNumberCreate;

        private const string SessionKeyEntradaEmLoteList = "EntradaEmLoteList";

        private const string SessionKeyListCatalogoCreate = "ListCatalogoCreate";
        //private static List<CatalogoViewModel?>? ListCatalogoCreate = new List<CatalogoViewModel>();
        //private static List<CatalogoViewModel?>? CatalogCart = new List<CatalogoViewModel>();

        #endregion

        public EntradaEmLote_ReqController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EntradaEmLote_Req, EntradaEmLote_ReqViewModel>();
                cfg.CreateMap<EntradaEmLote_ReqViewModel, EntradaEmLote_Req>();
                cfg.CreateMap<VW_Usuario_New, VW_Usuario>();
                cfg.CreateMap<VW_Usuario, VW_Usuario_New>();
                cfg.CreateMap<SerialProductModel, EntradaEmLote_Temp>()
                    .ForMember(dest => dest.DC_DataAquisicao, opt => opt.MapFrom(src => src.DataAquisicao))
                    .ForMember(dest => dest.DC_Valor, opt => opt.MapFrom(src => src.Valor))
                    .ForMember(dest => dest.DC_Fornecedor, opt => opt.MapFrom(src => src.Fornecedor))
                    .ForMember(dest => dest.DataRegistro, opt => opt.MapFrom(src => DateTime.Now));
                cfg.CreateMap<EntradaEmLote_Temp, SerialProductModel>();
                cfg.CreateMap<EntradaEmLote_Temp, Produto>();
                cfg.CreateMap<Produto, SerialProductModel>();
                cfg.CreateMap<SerialProductModel, Produto>()
                    .ForMember(dest => dest.DC_DataAquisicao, opt => opt.MapFrom(src => src.DataAquisicao))
                    .ForMember(dest => dest.DC_Valor, opt => opt.MapFrom(src => src.Valor))
                    .ForMember(dest => dest.DC_Fornecedor, opt => opt.MapFrom(src => src.Fornecedor))
                    .ForMember(dest => dest.DataRegistro, opt => opt.MapFrom(src => DateTime.Now));
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Index Region
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
                //    usuariofer.Retorno = $"Usuário sem permissão na página {pagina}!";
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

                            int? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            if (FerramentariaValue == null)
                            {
                                Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
                                List<FerramentariaViewModel> ferramentarias = searches.SearchFerramentariaBalconista(loggedUser.Id);
                                if (ferramentarias != null)
                                {
                                    ViewBag.FerramentariaItems = ferramentarias;
                                }
                                return PartialView("_FerramentariaPartialView");
                            }
                            else
                            {
                                httpContextAccessor?.HttpContext?.Session.SetInt32(Sessao.IdFerramentaria, (int)FerramentariaValue);
                            }

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
                return View();
            }

        }

        public IActionResult GetEmprestimo(CombinedEmLote? CombinedEmLoteValues)
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

                            CombinedEmLoteValues.EntradaEmLoteSearch.IdFerramentaria = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            List<EntradaEmLote_ReqViewModel>? EntradaEmLotes = searches.SearchEntradaEmLote(CombinedEmLoteValues.EntradaEmLoteSearch);
                            if (EntradaEmLotes.Count > 0)
                            {
                                List<NewUserInformationModel> result = (
                                                       from u in _contextBS.Usuario

                                                           // Subquery: get most recent Funcionario for each (Chapa, CodColigada)
                                                       let funcionarioMaisRecente = (
                                                           from f in _contextBS.Funcionario
                                                           where f.Chapa == u.Chapa && f.CodColigada == u.CodColigada
                                                           orderby f.DataMudanca descending
                                                           select f
                                                       ).FirstOrDefault()

                                                       select new NewUserInformationModel
                                                       {
                                                           Id = u.Id,
                                                           Chapa = u.Chapa,
                                                           Nome = funcionarioMaisRecente != null ? funcionarioMaisRecente.Nome : null,
                                                       }
                                                   ).ToList();

                                List<EntradaEmLote_ReqViewModel>? entrada = (from ent in EntradaEmLotes
                                                                             join user in result on ent.IdSolicitante equals user.Id
                                                                             select new EntradaEmLote_ReqViewModel
                                                                             {
                                                                                 Id = ent.Id,
                                                                                 IdFerramentaria = ent.IdFerramentaria,
                                                                                 RFM = ent.RFM,
                                                                                 Status = ent.Status,
                                                                                 SolicitanteNome = user.Nome,
                                                                                 DataRegistro = ent.DataRegistro,
                                                                             }).ToList();


                       




                                //var distinctUserIds = EntradaEmLotes.Select(e => e.IdSolicitante).Distinct().ToList();
                                //List<UsuarioBS>? listofusers = new List<UsuarioBS>();
                                //foreach (var item in distinctUserIds)
                                //{
                                //    UsuarioBS? users = _contextBS.Usuario.FirstOrDefault(u => u.Id == item);

                                //    // Check if a user was found
                                //    if (users != null)
                                //    {
                                //        // Assign the Chapa value to the item
                                //        listofusers.Add(users);
                                //    }
                                //    else
                                //    {
                                //        var mapper = mapeamentoClasses.CreateMapper();

                                //        var destinationUser = mapper.Map<UsuarioBS>(_contextBS.Usuario.FirstOrDefaultAsync(u => u.Id == item));

                                //        listofusers.Add(destinationUser);
                                //    }
                                //}

                                //foreach (var item in EntradaEmLotes)
                                //{
                                //    int? IdUsuario = item.IdSolicitante;

                                //    var usuario = listofusers.FirstOrDefault(u => u.Id == IdUsuario);

                                //    item.SolicitanteNome = usuario.Nome;

                                //}

                                //_ListEntrada = EntradaEmLotes;
                                int pageSize = CombinedEmLoteValues.EntradaEmLoteSearch.Pagination ?? 10;
                                int pageNumber = 1;
                                //GlobalPagination = pageSize;
                                //GlobalPageNumber = pageNumber;

                                IPagedList<EntradaEmLote_ReqViewModel> EntradaEmLotePagedList = entrada.ToPagedList(pageNumber, pageSize);

                                CombinedEmLote? combinedEmLote = new CombinedEmLote
                                {
                                    EntradaEmLote_ReqViewModel = EntradaEmLotePagedList,
                                    EntradaEmLoteSearch = CombinedEmLoteValues.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                                    ResultCount = entrada.Count()
                                };

                                CombinedEmLoteUsingList? combinedEmLoteListSession = new CombinedEmLoteUsingList
                                {
                                    EntradaEmLote_ReqViewModelList = entrada,
                                    EntradaEmLoteSearch = CombinedEmLoteValues.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                                    ResultCount = entrada.Count(),
                                    PageNumber = pageNumber,
                                    Pagination = pageSize
                                };

                                HttpContext.Session.SetObject(SessionKeyCombinedLoteValues, combinedEmLoteListSession);

                                //CombinedLoteValues = combinedEmLote;
                                return View(nameof(Index), combinedEmLote);
                            }
                            else
                            {
                                ViewBag.Error = "Nenhum resultado encontrado.";
                                CombinedEmLote? combinedEmLote = new CombinedEmLote
                                {
                                    EntradaEmLoteSearch = CombinedEmLoteValues.EntradaEmLoteSearch ?? new EntradaEmLoteSearch()
                                };

                                return View(nameof(Index), combinedEmLote);
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
                return View(nameof(Index));
            }
 
        }

        public ActionResult TestPage(int? page)
        {
            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();

            int pageSize = CombinedLoteValuesSes.Pagination ?? 10;
            int pageNumber = page ?? 1;
            //GlobalPageNumber = pageNumber;

            IPagedList<EntradaEmLote_ReqViewModel> EntradaEmLotePagedList = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(pageNumber, pageSize);

            CombinedEmLote combinedEmLote = new CombinedEmLote
            {
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch,
                EntradaEmLote_ReqViewModel = EntradaEmLotePagedList,
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            CombinedEmLoteUsingList? combinedEmLoteListSession = new CombinedEmLoteUsingList
            {
                EntradaEmLote_ReqViewModelList = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList,
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch,
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count(),
                PageNumber = pageNumber,
                Pagination = pageSize
            };

            HttpContext.Session.SetObject(SessionKeyCombinedLoteValues, combinedEmLoteListSession);

            return View("Index", combinedEmLote);
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

        public ActionResult GetListOfItems(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();

            int PageNumber = CombinedLoteValuesSes.PageNumber ?? 1;
            int Pagination = CombinedLoteValuesSes.Pagination ?? 10;

            CombinedEmLote? combinedEmLote = new CombinedEmLote
            {
                EntradaEmLote_ReqViewModel = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(PageNumber, Pagination),
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            if (id != null)
            {
                List<EntradaEmLote_TempModel?>? TempList = searches.SearchLoteTemp(id);
                if (TempList?.Count > 0)
                {
                    //foreach (EntradaEmLote_TempModel item in TempList)
                    //{
                    //    if (item.PorAferido == 1 || item.PorSerial == 1)
                    //    {
                    //        //string? FilePath = "C:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    //        //string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    //        string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    //        if (System.IO.File.Exists(FilePath))
                    //        {
                    //            item.FilePath = FilePath;
                    //        }
                    //    }
                    //}

                    EntradaEmLote_Req? LoteReqValues = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
                    var mapper = mapeamentoClasses.CreateMapper();
                    EntradaEmLote_ReqViewModel? LoteViewModel = mapper.Map<EntradaEmLote_ReqViewModel>(LoteReqValues);

                    ViewBag.LoteReqValues = LoteViewModel;
                    ViewBag.ModalRegistrado = TempList;

                    return View(nameof(Index), combinedEmLote);
                }
                else
                {
                    List<CombinedForModal?>? catalogoValues = searches.SearchLoteComp(id);

                    if (catalogoValues.Count > 0)
                    {
                        foreach (CombinedForModal item in catalogoValues)
                        {
                            if (item.PorAferido == 1 || item.PorSerial == 1)
                            {
                                //string? FilePath = "C:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                                string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                                //string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                                if (System.IO.File.Exists(FilePath))
                                {
                                    item.FilePath = FilePath;
                                }
                            }
                        }


                        EntradaEmLote_Req? LoteReqValues = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
                        var mapper = mapeamentoClasses.CreateMapper();
                        EntradaEmLote_ReqViewModel? LoteViewModel = mapper.Map<EntradaEmLote_ReqViewModel>(LoteReqValues);

                        ViewBag.LoteReqValues = LoteViewModel;
                        ViewBag.ModalValues = catalogoValues;
                        return View(nameof(Index), combinedEmLote);
                    }
                    else
                    {
                        EntradaEmLote_Req? LoteReqValues = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
                        var mapper = mapeamentoClasses.CreateMapper();
                        EntradaEmLote_ReqViewModel? LoteViewModel = mapper.Map<EntradaEmLote_ReqViewModel>(LoteReqValues);

                        ViewBag.LoteReqValues = LoteViewModel;
                        ViewBag.ModalValues = catalogoValues;
                        return View(nameof(Index), combinedEmLote);
                    }
                }

            }
            else
            {
                ViewBag.Error = "No Id Selectd";
                return View(nameof(Index), combinedEmLote);
            }

            //List<EntradaEmLote_TempModel?>? TempList = searches.SearchLoteTemp(id);

      
            //if (id != null)
            //{
            //    List<CombinedForModal?>? catalogoValues = searches.SearchLoteComp(id);
               
            //    if (catalogoValues.Count > 0)
            //    {
            //        foreach (CombinedForModal item in catalogoValues)
            //        {
            //            if (item.PorAferido == 1 || item.PorSerial == 1)
            //            {
            //                //string? FilePath = "C:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
            //                //string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
            //                string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
            //                if (System.IO.File.Exists(FilePath))
            //                {
            //                    item.FilePath = FilePath;
            //                }
            //            }
            //        }


            //        EntradaEmLote_Req? LoteReqValues = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
            //        var mapper = mapeamentoClasses.CreateMapper();
            //        EntradaEmLote_ReqViewModel? LoteViewModel = mapper.Map<EntradaEmLote_ReqViewModel>(LoteReqValues);

            //        ViewBag.LoteReqValues = LoteViewModel;
            //        ViewBag.ModalValues = catalogoValues;
            //        return View(nameof(Index), combinedEmLote);
            //    }
            //    else
            //    {
            //        ViewBag.Error = "Erorr in retrieving the data.";
            //        return View(nameof(Index), combinedEmLote);
            //    }             
            //}
            //else
            //{
            //    ViewBag.Error = "No Id Selectd";
            //    return View(nameof(Index), combinedEmLote);
            //}
            
        }

        public ActionResult CancelAction(int? id)
        {
            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();

            int PageNumber = CombinedLoteValuesSes.PageNumber ?? 1;
            int Pagination = CombinedLoteValuesSes.Pagination ?? 10;



            CombinedEmLote? combinedEmLote = new CombinedEmLote
            {
                EntradaEmLote_ReqViewModel = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(PageNumber, Pagination),
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            if (id != null)
            {
                var EditEntradaEmLote_Req = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
                if (EditEntradaEmLote_Req != null)
                {
                    EditEntradaEmLote_Req.Status = 5;
                    _context.SaveChanges();
                }
                else
                {
                    ViewBag.Error = "Error in retrieving data";
                    return View(nameof(Index), combinedEmLote);
                }

                CombinedEmLote? combinedEmLoteNew = RefreshListValues(5);
                ViewBag.ShowSuccessAlert = true;
                return View(nameof(Index), combinedEmLoteNew);
            }
            else
            {
                ViewBag.Error = "No Id Selectd";
                return View(nameof(Index), combinedEmLote);
            }          
        }

        public ActionResult DeferirAction(int? id)
        {
            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();

            int PageNumber = CombinedLoteValuesSes.PageNumber ?? 1;
            int Pagination = CombinedLoteValuesSes.Pagination ?? 10;



            CombinedEmLote? combinedEmLote = new CombinedEmLote
            {
                EntradaEmLote_ReqViewModel = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(PageNumber, Pagination),
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            if (id != null)
            {
                var EditEntradaEmLote_Req = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);
                if (EditEntradaEmLote_Req != null)
                {
                    EditEntradaEmLote_Req.Status = 4;
                    _context.SaveChanges();
                }
                else
                {
                    ViewBag.Error = "Error in retrieving data";
                    return View(nameof(Index), combinedEmLote);
                }

                CombinedEmLote? combinedEmLoteNew = RefreshListValues(4);
                ViewBag.ShowSuccessAlert = true;
                return View(nameof(Index), combinedEmLote);
            }
            else
            {
                ViewBag.Error = "No Id Selectd";
                return View(nameof(Index), combinedEmLote);
            }
        }

        public ActionResult ProcessAction(int? id)
        {

            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;

            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();
            int PageNumber = CombinedLoteValuesSes.PageNumber ?? 1;
            int Pagination = CombinedLoteValuesSes.Pagination ?? 10;
            CombinedEmLote? combinedEmLote = new CombinedEmLote
            {
                EntradaEmLote_ReqViewModel = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(PageNumber, Pagination),
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            try
            {
                if (id != null)
                {
                    List<string?>? ErrorHandlers = new List<string?>();
                    EntradaEmLote_Req? Request = _context.EntradaEmLote_Req.FirstOrDefault(i => i.Id == id);

                    List<EntradaEmLote_Comp>? ListOfComp = _context.EntradaEmLote_Comp.Where(i => i.IdRequisicao == id).ToList();
                    if (ListOfComp.Count > 0)
                    {
                        //string? FileLocationDone = $"C:\\Ferramentaria\\EntradaEmLote\\Done\\{Request.Id}\\";
                        string? FileLocationDone = $"D:\\Ferramentaria\\EntradaEmLote\\Done\\{Request.Id}\\";

                        foreach (EntradaEmLote_Comp? item in ListOfComp)
                        {

                            //Catalogo? catalog = _context.Catalogo.FirstOrDefault(i => i.Id == item.IdCatalogo && i.Ativo == 1);
                            SimpleProductModel? catalog = (from catalogo in _context.Catalogo
                                                           join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                           join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
                                                           where catalogo.Id == item.IdCatalogo
                                                             && catalogo.Ativo == 1
                                                             select new SimpleProductModel
                                                             {
                                                                 IdCatalogo = catalogo.Id,
                                                                 PorAferido = catalogo.PorAferido,
                                                                 PorSerial = catalogo.PorSerial,
                                                                 PorMetro = catalogo.PorMetro,
                                                                 Codigo = catalogo.Codigo,
                                                                 IdCategoriaPai = categoriaPai.Id
                                                             }).FirstOrDefault();

                            if (catalog.PorAferido == 1 || catalog.PorSerial == 1)
                            {
                                string? FileLocation = $"D:\\Ferramentaria\\EntradaEmLote\\Temp\\{catalog.IdCatalogo}.xlsx";
                                //string? FileLocation = $"C:\\Ferramentaria\\EntradaEmLote\\Temp\\{catalog.IdCatalogo}.xlsx";
                                if (System.IO.File.Exists(FileLocation))
                                {
                                    List<SerialProductModel?> productInfoSheet = new List<SerialProductModel?>();
                                    using (var package = new ExcelPackage(new FileInfo(FileLocation)))
                                    {
                                        var worksheet = package.Workbook.Worksheets[0];
                                        int rowCount = worksheet.Dimension.Rows;
                                        int colCount = worksheet.Dimension.Columns;

                                        if (catalog.PorAferido == 1)
                                        {
                                            for (int row = 2; row <= rowCount; row++)
                                            {
                                                SerialProductModel productValue = new SerialProductModel
                                                {
                                                    IdRequisicao = Request.Id,
                                                    PorAferido = catalog.PorAferido,
                                                    PorSerial = catalog.PorSerial,
                                                    IdCatalogo = item.IdCatalogo,
                                                    IdCategoriaPai = catalog.IdCategoriaPai,
                                                    Quantidade = 1,
                                                    AF = worksheet.Cells[row, 1].Text, // Assuming AF is in column 1
                                                    PAT = int.TryParse(worksheet.Cells[row, 2].Text, out int patValue) ? patValue : (int?)null, // Column 2
                                                    Serie = worksheet.Cells[row, 3].Text, // Column 3
                                                    Propriedade = worksheet.Cells[row, 4].Text, // Column 4
                                                    DataVencimento = DateTime.TryParse(worksheet.Cells[row, 5].Text, out DateTime vencimentoDate) ? vencimentoDate : (DateTime?)null, // Column 5
                                                    Certificado = worksheet.Cells[row, 6].Text, // Column 6
                                                    DataAquisicao = DateTime.TryParse(worksheet.Cells[row, 7].Text, out DateTime aquisicaoDate) ? aquisicaoDate : (DateTime?)null, // Column 7
                                                    Valor = decimal.TryParse(worksheet.Cells[row, 8].Text, out decimal valorValue) ? valorValue : (decimal?)null, // Column 8
                                                    Fornecedor = worksheet.Cells[row, 9].Text, // Column 9
                                                };

                                                // Add the populated model to the list
                                                productInfoSheet.Add(productValue);
                                            }
                                        }
                                        else
                                        {
                                            for (int row = 2; row <= rowCount; row++)
                                            {
                                                SerialProductModel productValue = new SerialProductModel
                                                {
                                                    IdRequisicao = Request.Id,
                                                    PorAferido = catalog.PorAferido,
                                                    PorSerial = catalog.PorSerial,
                                                    IdCatalogo = item.IdCatalogo,
                                                    IdCategoriaPai = catalog.IdCategoriaPai,
                                                    Quantidade = 1,
                                                    AF = worksheet.Cells[row, 1].Text,
                                                    PAT = int.TryParse(worksheet.Cells[row, 2].Text, out int patValue) ? patValue : (int?)null, 
                                                    DataAquisicao = DateTime.TryParse(worksheet.Cells[row, 3].Text, out DateTime aquisicaoDate) ? aquisicaoDate : (DateTime?)null,
                                                    Valor = decimal.TryParse(worksheet.Cells[row, 4].Text, out decimal valorValue) ? valorValue : (decimal?)null,
                                                    Fornecedor = worksheet.Cells[row, 5].Text,
                                                    DataVencimento = DateTime.TryParse(worksheet.Cells[row, 6].Text, out DateTime vencimentoDate) ? vencimentoDate : (DateTime?)null,                                                                                                                                         
                                                };

                                                if (string.IsNullOrEmpty(productValue.AF) && productValue.PAT == null)
                                                {
                                                    continue;
                                                }

                                                // Add the populated model to the list
                                                productInfoSheet.Add(productValue);
                                            }
                                        }                                
                                    }

                                    if (productInfoSheet.Count > 0)
                                    {

                                        List<SerialProductModel?> finalProducts = new List<SerialProductModel?>();
                                        foreach (SerialProductModel? itemcheck in productInfoSheet)
                                        {
                                            //List<string?> errorcheckperitem = new List<string?>();
                                            //List<string?> errorcheckperitem = CheckProduct(itemcheck);

                                            bool afExists = finalProducts.Any(p =>
                                                                   p != null &&
                                                                    p.IdCatalogo == itemcheck.IdCatalogo &&
                                                                    (
                                                                        // Case 1: If AF is "N/C", only check PAT duplicates
                                                                        (itemcheck.AF == "N/C" && itemcheck.PAT != 0 && p.PAT == itemcheck.PAT) ||

                                                                        // Case 2: If PAT is 0, only check AF duplicates
                                                                        (itemcheck.PAT == 0 && !string.IsNullOrEmpty(itemcheck.AF) && p.AF == itemcheck.AF) ||

                                                                        // Case 3: If neither special case, both must be unique
                                                                        (itemcheck.AF != "N/C" && itemcheck.PAT != 0 &&
                                                                         (p.AF == itemcheck.AF || p.PAT == itemcheck.PAT))
                                                                    ));
                                            if (!afExists)
                                            {
                                                SerialProductModel errorcheckperitem = CheckProductOtherFormat(itemcheck);
                                                finalProducts.Add(errorcheckperitem);
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                         
                                            //if (errorcheckperitem.Count == 0)
                                            //{
                                            //    itemcheck.Observacao = string.Empty;
                                            //    continue;
                                            //}
                                            //else
                                            //{
                                            //    string joinedErrors = string.Join(", ", errorcheckperitem.Where(e => !string.IsNullOrEmpty(e)));
                                            //    itemcheck.Observacao = $"IdRequisicao = {itemcheck.IdRequisicao}, IdCatalogo = {itemcheck.IdCatalogo} -> {joinedErrors}";
                                            //}
                                        }

                                        if (finalProducts.All(i => string.IsNullOrEmpty(i.Observacao)))
                                        {
                                            //foreach
                                            var mapper = mapeamentoClasses.CreateMapper();

                                            List<EntradaEmLote_Temp?>? ToBeInserted = mapper.Map<List<EntradaEmLote_Temp?>?>(finalProducts);

                                            foreach (EntradaEmLote_Temp? tempvalue in ToBeInserted)
                                            {
                                                tempvalue.DataRegistro = DateTime.Now;
                                                _context.EntradaEmLote_Temp.Add(tempvalue);
                                                //_context.SaveChanges();
                                            }

                                            //_context.EntradaEmLote_Temp.AddRange(ToBeInserted);
                                            //_context.SaveChanges();

                                            foreach (SerialProductModel details in finalProducts)
                                            {
                                                Produto ProductToBeInserted = mapper.Map<Produto>(details);
                                                ProductToBeInserted.IdFerramentaria = Request.IdFerramentaria;
                                                ProductToBeInserted.QuantidadeMinima = 1;
                                                ProductToBeInserted.RFM = Request.RFM;
                                                ProductToBeInserted.Ativo = 1;

                                                _context.Produto.Add(ProductToBeInserted);
                                            }

                                            if (!Directory.Exists(FileLocationDone)) 
                                            {
                                                Directory.CreateDirectory(FileLocationDone);
                                            }

                                            System.IO.File.Move(FileLocation, String.Format("{0}\\{1}.xlsx", FileLocationDone, catalog.IdCatalogo));

                                            item.Status = 2;
                                            _context.SaveChanges();
                                        }
                                        else
                                        {
                                            var mapper = mapeamentoClasses.CreateMapper();

                                            List<EntradaEmLote_Temp?>? ToBeInserted = mapper.Map<List<EntradaEmLote_Temp?>?>(finalProducts);
                                            _context.EntradaEmLote_Temp.AddRange(ToBeInserted);
                                            //_context.SaveChanges();

                                            if (!Directory.Exists(FileLocationDone))
                                            {
                                                Directory.CreateDirectory(FileLocationDone);
                                            }

                                            System.IO.File.Move(FileLocation, String.Format("{0}\\{1}.xlsx", FileLocationDone, catalog.IdCatalogo));

                                            ErrorHandlers.Add($"Please check the observação for the errors.");
                                            Request.Status = 3;
                                            _context.SaveChanges();
                                        }

                                    }
                                    else
                                    {
                                        ErrorHandlers.Add($"No Data Read from excel reader.");
                                        Request.Status = 3;
                                        _context.SaveChanges();

                                        if (!Directory.Exists(FileLocationDone))
                                        {
                                            Directory.CreateDirectory(FileLocationDone);
                                        }

                                        System.IO.File.Move(FileLocation, String.Format("{0}\\{1}.xlsx", FileLocationDone, catalog.IdCatalogo));

                                        break;
                                    }

                                }
                                else
                                {
                                    ErrorHandlers.Add($"File Not found for item {catalog.Codigo}, Please Register a new Entrada Em Lote.");
                                    Request.Status = 3;
                                    _context.SaveChanges();
                                    break;
                                  
                                }
                            }
                            else
                            {

                                using (var transaction = _context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        EntradaEmLote_Temp? temp = new EntradaEmLote_Temp()
                                        {
                                            IdRequisicao = id,
                                            IdCatalogo = catalog.IdCatalogo,
                                            Quantidade = item.Quantidade,
                                            AF = string.Empty,
                                            Serie = string.Empty,
                                            PAT = 0,
                                            Observacao = item.Observacao != null ? item.Observacao : string.Empty,
                                            DataRegistro = DateTime.Now,
                                        };

                                        _context.EntradaEmLote_Temp.Add(temp);

                                        Produto? produto = _context.Produto.Where(i => i.IdCatalogo == catalog.IdCatalogo && i.IdFerramentaria == Request.IdFerramentaria && i.Ativo == 1).FirstOrDefault();
                                        if (produto != null)
                                        {
                                            int? QuantityOld = produto.Quantidade;
                                            int? QuantityNew = produto.Quantidade + item.Quantidade;

                                            produto.Quantidade = QuantityNew;
                                            produto.RFM = produto.RFM != Request.RFM ? Request.RFM : produto.RFM;


                                            LogProduto LogToBeInserted1 = new LogProduto()
                                            {
                                                IdProduto = produto.Id,
                                                IdUsuario = loggedUser.Id,
                                                QuantidadeDe = QuantityOld,
                                                QuantidadePara = QuantityNew,
                                                RfmDe = produto.RFM,
                                                RfmPara = Request.RFM,
                                                DataRegistro = DateTime.Now,
                                            };

                                            LogEntradaSaidaInsert LogEntradaSaidaInsert = new LogEntradaSaidaInsert()
                                            {
                                                IdProduto = produto.Id,
                                                IdFerramentaria = Request.IdFerramentaria,
                                                Quantidade = item.Quantidade,
                                                Rfm = Request.RFM,
                                                Observacao = item.Observacao,
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };

                                            item.Status = 2;
                                            _context.LogEntradaSaidaInsert.Add(LogEntradaSaidaInsert);
                                            _context.LogProduto.Add(LogToBeInserted1);
                                            _context.SaveChanges();
                                            transaction.Commit();
                                        }
                                        else
                                        {
                                            Produto ProdutoToBeInserted = new Produto()
                                            {
                                                IdCatalogo = catalog.IdCatalogo,
                                                IdFerramentaria = Request.IdFerramentaria,
                                                Quantidade = item.Quantidade,
                                                QuantidadeMinima = 1,
                                                RFM = Request.RFM,
                                                Observacao = item.Observacao,
                                                Ativo = 1,
                                                DataRegistro = DateTime.Now
                                            };

                                            _context.Produto.Add(ProdutoToBeInserted);
                                            _context.SaveChanges();

                                            LogProduto LogToBeInserted1 = new LogProduto()
                                            {
                                                IdProduto = ProdutoToBeInserted.Id,
                                                IdUsuario = loggedUser.Id,
                                                QuantidadeDe = 0,
                                                QuantidadePara = item.Quantidade,
                                                RfmDe = string.Empty,
                                                RfmPara = Request.RFM,
                                                DataRegistro = DateTime.Now,
                                            };

                                            LogEntradaSaidaInsert LogEntradaSaidaInsert = new LogEntradaSaidaInsert()
                                            {
                                                IdProduto = ProdutoToBeInserted.Id,
                                                IdFerramentaria = Request.IdFerramentaria,
                                                Quantidade = item.Quantidade,
                                                Rfm = Request.RFM,
                                                Observacao = item.Observacao,
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };

                                            item.Status = 2;
                                            _context.LogEntradaSaidaInsert.Add(LogEntradaSaidaInsert);
                                            _context.LogProduto.Add(LogToBeInserted1);
                                            _context.SaveChanges();
                                            transaction.Commit();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback(); // Rollback the transaction in case of an exception
                                                                // Optionally, log the exception or rethrow it

                                        ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";

                                        log.LogWhy = $"SERVER PROBLEM: {ex.Message}";
                                        ErrorViewModel erro = new ErrorViewModel();
                                        erro.Tela = log.LogWhere;
                                        erro.Descricao = log.LogWhy;
                                        erro.Mensagem = log.LogWhat;
                                        erro.IdLog = auxiliar.GravaLogRetornoErro(log);

                                        return View(nameof(Index));
                                    }
                                }

                                //EntradaEmLote_Temp? temp = new EntradaEmLote_Temp()
                                //{
                                //    IdRequisicao = id,
                                //    IdCatalogo = catalog.IdCatalogo,
                                //    Quantidade = item.Quantidade,
                                //    AF = string.Empty,
                                //    Serie = string.Empty,
                                //    PAT = 0,
                                //    Observacao = item.Observacao != null ? item.Observacao : string.Empty,
                                //    DataRegistro = DateTime.Now,
                                //};

                                //_context.EntradaEmLote_Temp.Add(temp);
                                //_context.SaveChanges();

                                //Produto? produto = _context.Produto.Where(i => i.IdCatalogo == catalog.IdCatalogo && i.IdFerramentaria == Request.IdFerramentaria && i.Ativo == 1).FirstOrDefault();
                                //if (produto != null)
                                //{
                                //    int? QuantityOld = produto.Quantidade;
                                //    int? QuantityNew = produto.Quantidade + item.Quantidade;

                                //    produto.Quantidade = QuantityNew;
                                //    produto.RFM = produto.RFM != Request.RFM ? Request.RFM : produto.RFM;


                                //    LogProduto LogToBeInserted1 = new LogProduto()
                                //    {
                                //        IdProduto = produto.Id,
                                //        IdUsuario = usuariofer.Id,
                                //        QuantidadeDe = QuantityOld,
                                //        QuantidadePara = QuantityNew,
                                //        RfmDe = produto.RFM,
                                //        RfmPara = Request.RFM,
                                //        DataRegistro = DateTime.Now,
                                //    };

                                //    LogEntradaSaidaInsert LogEntradaSaidaInsert = new LogEntradaSaidaInsert()
                                //    {
                                //        IdProduto = produto.Id,
                                //        IdFerramentaria = Request.IdFerramentaria,
                                //        Quantidade = item.Quantidade,
                                //        Rfm = Request.RFM,
                                //        Observacao = item.Observacao,
                                //        IdUsuario = usuariofer.Id,
                                //        DataRegistro = DateTime.Now
                                //    };

                                //    item.Status = 2;
                                //    _context.LogEntradaSaidaInsert.Add(LogEntradaSaidaInsert);
                                //    _context.LogProduto.Add(LogToBeInserted1);
                                //    _context.SaveChanges();

                                //}
                                //else
                                //{
                                //    Produto ProdutoToBeInserted = new Produto()
                                //    {
                                //        IdCatalogo = catalog.IdCatalogo,
                                //        IdFerramentaria = Request.IdFerramentaria,
                                //        Quantidade = item.Quantidade,
                                //        QuantidadeMinima = 1,
                                //        RFM = Request.RFM,
                                //        Observacao = item.Observacao,
                                //    };

                                //    _context.Produto.Add(ProdutoToBeInserted);
                                 

                                //    LogProduto LogToBeInserted1 = new LogProduto()
                                //    {
                                //        IdProduto = ProdutoToBeInserted.Id,
                                //        IdUsuario = usuariofer.Id,
                                //        QuantidadeDe = 0,
                                //        QuantidadePara = item.Quantidade,
                                //        RfmDe = string.Empty,
                                //        RfmPara = Request.RFM,
                                //        DataRegistro = DateTime.Now,
                                //    };

                                //    LogEntradaSaidaInsert LogEntradaSaidaInsert = new LogEntradaSaidaInsert()
                                //    {
                                //        IdProduto = ProdutoToBeInserted.Id,
                                //        IdFerramentaria = Request.IdFerramentaria,
                                //        Quantidade = item.Quantidade,
                                //        Rfm = Request.RFM,
                                //        Observacao = item.Observacao,
                                //        IdUsuario = usuariofer.Id,
                                //        DataRegistro = DateTime.Now
                                //    };

                                //    item.Status = 2;
                                //    _context.LogEntradaSaidaInsert.Add(LogEntradaSaidaInsert);
                                //    _context.LogProduto.Add(LogToBeInserted1);
                                //    _context.SaveChanges();
                                //}
                         
                            }


                        }

                        if (ListOfComp.All(i => i.Status == 2) && ErrorHandlers.Count == 0)
                        {
                            Request.Status = 7;
                            _context.SaveChanges();

                            ViewBag.ShowSuccessAlert = true;
                            return View(nameof(Index));
                        }
                        else
                        {
                            ViewBag.Error = "Alguns itens não são bem-sucedidos. Por favor, reprocesse e verifique os erros.";
                            ViewBag.ErrorList = ErrorHandlers;
                            return View(nameof(Index));
                        }
                      


                        //return View(nameof(Index), combinedEmLote);
                    }
                    else
                    {
                        ViewBag.Error = $"{id} is not available on EntradaEmLote_Comp.";
                        return View(nameof(Index), combinedEmLote);
                    }
                }
                else
                {
                    ViewBag.Error = "No Id Selectd";
                    return View(nameof(Index), combinedEmLote);
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
                return View(nameof(Index), combinedEmLote);
            }
        }

        public List<string?> CheckProduct(SerialProductModel? itemcheck)
        {
            List<string?> errorlisthandler = new List<string?>();

            Produto? produtoAF = _context.Produto.FirstOrDefault(i => i.AF != "N/C" && i.AF == itemcheck.AF);
            if (produtoAF != null)
            {
                errorlisthandler.Add($"ERROR Duplicate AF");
            }
            else
            {
                if (string.IsNullOrEmpty(itemcheck.AF))
                {
                    errorlisthandler.Add($"ERROR AF NullOrEmpty");
                }
            }

            Produto? ProdutoPAT = _context.Produto.FirstOrDefault(i => i.PAT != 0 && i.PAT == itemcheck.PAT);
            if (ProdutoPAT != null)
            {
                errorlisthandler.Add($"ERROR Duplicate PAT");
            }

            var Empresacheck = _context.Empresa.FirstOrDefault(i => EF.Functions.Like(i.Nome, $"%{itemcheck.Propriedade}%"));
            if (Empresacheck == null)
            {
                if (string.IsNullOrEmpty(itemcheck.Propriedade))
                {
                    errorlisthandler.Add($"ERROR Propriedade NullOrEmpty");
                }
            }

            if (itemcheck.DataAquisicao.HasValue == false && itemcheck.DataAquisicao == DateTime.MinValue)
            {
                errorlisthandler.Add($"ERROR DataAquisicao formato invalido");
            }

            if (itemcheck.DataVencimento.HasValue == false && itemcheck.DataVencimento == DateTime.MinValue)
            {
                errorlisthandler.Add($"ERROR DataVencimento formato invalido");
            }

            return errorlisthandler;
        }

        public SerialProductModel CheckProductOtherFormat(SerialProductModel? itemcheck)
        {
            List<string?> errorlisthandler = new List<string?>();
            SerialProductModel? item = new SerialProductModel();
            item = itemcheck;

            Produto? produtoAF = _context.Produto.FirstOrDefault(i => i.AF != "N/C" && i.AF == itemcheck.AF && i.IdCatalogo == itemcheck.IdCatalogo);
            if (produtoAF != null)
            {
                errorlisthandler.Add($"ERROR Duplicate AF");
            }
            else
            {
                if (string.IsNullOrEmpty(itemcheck.AF))
                {
                    errorlisthandler.Add($"ERROR AF NullOrEmpty");
                }
            }

            Produto? ProdutoPAT = _context.Produto.FirstOrDefault(i => i.PAT != 0 && i.PAT != null && i.PAT == itemcheck.PAT);
            if (ProdutoPAT != null)
            {
                errorlisthandler.Add($"ERROR Duplicate PAT");
            }

            if (itemcheck.DataAquisicao.HasValue == false && itemcheck.DataAquisicao == DateTime.MinValue)
            {
                errorlisthandler.Add($"ERROR DataAquisicao formato invalido");
            }

            if (itemcheck.Valor == null)
            {
                errorlisthandler.Add($"ERROR Valor formato invalido");
            }

            if (itemcheck.PorAferido == 1)
            {
                Empresa? Empresacheck = _context.Empresa.FirstOrDefault(i => EF.Functions.Like(i.Nome, $"%{itemcheck.Propriedade}%"));
                if (Empresacheck == null)
                {
                    if (string.IsNullOrEmpty(itemcheck.Propriedade))
                    {
                        errorlisthandler.Add($"ERROR Propriedade NullOrEmpty");
                    }
                }
                else
                {
                    item.IdEmpresa = Empresacheck.Id;
                }

                if (itemcheck.DataVencimento.HasValue == false && itemcheck.DataVencimento == DateTime.MinValue)
                {
                    errorlisthandler.Add($"ERROR DataVencimento formato invalido");
                }

            }
            else
            {
                if (itemcheck.IdCategoriaPai == 1384)
                {
                    if (itemcheck.DataVencimento.HasValue == false && itemcheck.DataVencimento == DateTime.MinValue)
                    {
                        errorlisthandler.Add($"ERROR DataVencimento formato invalido");
                    }
                }
            }

               
            if (errorlisthandler.Count > 0)
            {
                string joinedErrors = string.Join(", ", errorlisthandler.Where(e => !string.IsNullOrEmpty(e)));
                item.Observacao = $"IdRequisicao = {itemcheck.IdRequisicao}, IdCatalogo = {itemcheck.IdCatalogo} -> {joinedErrors}";
                return item;
            }
            else
            {
                item.Observacao = string.Empty;
                return item;
            }

        }

        public CombinedEmLote? RefreshListValues(int? value)
        {
            var CombinedLoteValuesSes = HttpContext.Session.GetObject<CombinedEmLoteUsingList>(SessionKeyCombinedLoteValues) ?? new CombinedEmLoteUsingList();

            int PageNumber = CombinedLoteValuesSes.PageNumber ?? 1;
            int Pagination = CombinedLoteValuesSes.Pagination ?? 10;



            CombinedEmLote? combinedEmLoteFromSess = new CombinedEmLote
            {
                EntradaEmLote_ReqViewModel = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.ToPagedList(PageNumber, Pagination),
                EntradaEmLoteSearch = CombinedLoteValuesSes.EntradaEmLoteSearch ?? new EntradaEmLoteSearch(),
                ResultCount = CombinedLoteValuesSes.EntradaEmLote_ReqViewModelList.Count()
            };

            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            EntradaEmLoteSearch? searchFilter = combinedEmLoteFromSess.EntradaEmLoteSearch ?? new EntradaEmLoteSearch();

            List<EntradaEmLote_ReqViewModel?>? EntradaEmLotes = searches.SearchEntradaEmLote(searchFilter);
            if (EntradaEmLotes.Count > 0)
            {
                //var distinctUserIds = EntradaEmLotes.Select(e => e.IdSolicitante).Distinct().ToList();
                //List<VW_Usuario_New> listofusers = new List<VW_Usuario_New>();
                //foreach (var item in distinctUserIds)
                //{
                //    VW_Usuario_New users = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == item);

                //    // Check if a user was found
                //    if (users != null)
                //    {
                //        // Assign the Chapa value to the item
                //        listofusers.Add(users);
                //    }
                //    else
                //    {
                //        var mapper = mapeamentoClasses.CreateMapper();

                //        var destinationUser = mapper.Map<VW_Usuario_New>(_contextBS.VW_Usuario.FirstOrDefault(u => u.Id == item));

                //        listofusers.Add(destinationUser);
                //    }
                //}

                //foreach (var item in EntradaEmLotes)
                //{
                //    int? IdUsuario = item.IdSolicitante;

                //    var usuario = listofusers.FirstOrDefault(u => u.Id == IdUsuario);

                //    item.SolicitanteNome = usuario.Nome;

                //}

                List<NewUserInformationModel> result = (
                       from u in _contextBS.Usuario

                           // Subquery: get most recent Funcionario for each (Chapa, CodColigada)
                       let funcionarioMaisRecente = (
                           from f in _contextBS.Funcionario
                           where f.Chapa == u.Chapa && f.CodColigada == u.CodColigada
                           orderby f.DataMudanca descending
                           select f
                       ).FirstOrDefault()

                       select new NewUserInformationModel
                       {
                           Id = u.Id,
                           Chapa = u.Chapa,
                           Nome = funcionarioMaisRecente != null ? funcionarioMaisRecente.Nome : null,
                       }
                   ).ToList();

                List<EntradaEmLote_ReqViewModel>? entrada = (from ent in EntradaEmLotes
                                                             join user in result on ent.IdSolicitante equals user.Id
                                                             select new EntradaEmLote_ReqViewModel
                                                             {
                                                                 Id = ent.Id,
                                                                 IdFerramentaria = ent.IdFerramentaria,
                                                                 RFM = ent.RFM,
                                                                 Status = ent.Status,
                                                                 SolicitanteNome = user.Nome,
                                                                 DataRegistro = ent.DataRegistro,
                                                             }).ToList();

                //_ListEntrada = EntradaEmLotes;
                int pageSize = searchFilter.Pagination ?? 10;
                int pageNumber = 1;
                //GlobalPagination = pageSize;
                //GlobalPageNumber = pageNumber;

                IPagedList<EntradaEmLote_ReqViewModel> EntradaEmLotePagedList = entrada.ToPagedList(pageNumber, pageSize);

                CombinedEmLote? combinedEmLote = new CombinedEmLote
                {
                    EntradaEmLote_ReqViewModel = EntradaEmLotePagedList,
                    EntradaEmLoteSearch = searchFilter ?? new EntradaEmLoteSearch(),
                    ResultCount = EntradaEmLotes.Count()
                };

                CombinedEmLoteUsingList? combinedEmLoteListSession = new CombinedEmLoteUsingList
                {
                    EntradaEmLote_ReqViewModelList = entrada,
                    EntradaEmLoteSearch = searchFilter,
                    ResultCount = EntradaEmLotes.Count(),
                    PageNumber = pageNumber,
                    Pagination = pageSize
                };

                HttpContext.Session.SetObject(SessionKeyCombinedLoteValues, combinedEmLoteListSession);



                //CombinedLoteValues = combinedEmLote;
                return combinedEmLote;
            }
            else
            {

                searchFilter.Status = value;
                EntradaEmLotes = searches.SearchEntradaEmLote(searchFilter);
                if (EntradaEmLotes.Count > 0)
                {
                    //var distinctUserIds = EntradaEmLotes.Select(e => e.IdSolicitante).Distinct().ToList();
                    //List<VW_Usuario_New> listofusers = new List<VW_Usuario_New>();
                    //foreach (var item in distinctUserIds)
                    //{
                    //    VW_Usuario_New users = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == item);

                    //    // Check if a user was found
                    //    if (users != null)
                    //    {
                    //        // Assign the Chapa value to the item
                    //        listofusers.Add(users);
                    //    }
                    //    else
                    //    {
                    //        var mapper = mapeamentoClasses.CreateMapper();

                    //        var destinationUser = mapper.Map<VW_Usuario_New>(_contextBS.VW_Usuario.FirstOrDefault(u => u.Id == item));

                    //        listofusers.Add(destinationUser);
                    //    }
                    //}

                    //foreach (var item in EntradaEmLotes)
                    //{
                    //    int? IdUsuario = item.IdSolicitante;

                    //    var usuario = listofusers.FirstOrDefault(u => u.Id == IdUsuario);

                    //    item.SolicitanteNome = usuario.Nome;

                    //}

                    List<NewUserInformationModel> result = (
                    from u in _contextBS.Usuario

                        // Subquery: get most recent Funcionario for each (Chapa, CodColigada)
                    let funcionarioMaisRecente = (
                        from f in _contextBS.Funcionario
                        where f.Chapa == u.Chapa && f.CodColigada == u.CodColigada
                        orderby f.DataMudanca descending
                        select f
                    ).FirstOrDefault()

                    select new NewUserInformationModel
                    {
                        Id = u.Id,
                        Chapa = u.Chapa,
                        Nome = funcionarioMaisRecente != null ? funcionarioMaisRecente.Nome : null,
                    }
                ).ToList();

                    List<EntradaEmLote_ReqViewModel>? entrada = (from ent in EntradaEmLotes
                                                                 join user in result on ent.IdSolicitante equals user.Id
                                                                 select new EntradaEmLote_ReqViewModel
                                                                 {
                                                                     Id = ent.Id,
                                                                     IdFerramentaria = ent.IdFerramentaria,
                                                                     RFM = ent.RFM,
                                                                     Status = ent.Status,
                                                                     SolicitanteNome = user.Nome,
                                                                     DataRegistro = ent.DataRegistro,
                                                                 }).ToList();

                    //_ListEntrada = EntradaEmLotes;
                    int pageSize = searchFilter.Pagination ?? 10;
                    int pageNumber = 1;
                    //GlobalPagination = pageSize;
                    //GlobalPageNumber = pageNumber;

                    IPagedList<EntradaEmLote_ReqViewModel> EntradaEmLotePagedList = entrada.ToPagedList(pageNumber, pageSize);

                    CombinedEmLote? combinedEmLote = new CombinedEmLote
                    {
                        EntradaEmLote_ReqViewModel = EntradaEmLotePagedList,
                        EntradaEmLoteSearch = searchFilter ?? new EntradaEmLoteSearch(),
                        ResultCount = EntradaEmLotes.Count()
                    };

                    CombinedEmLoteUsingList? combinedEmLoteListSession = new CombinedEmLoteUsingList
                    {
                        EntradaEmLote_ReqViewModelList = entrada,
                        EntradaEmLoteSearch = searchFilter,
                        ResultCount = EntradaEmLotes.Count(),
                        PageNumber = pageNumber,
                        Pagination = pageSize
                    };

                    HttpContext.Session.SetObject(SessionKeyCombinedLoteValues, combinedEmLoteListSession);

                    //CombinedLoteValues = combinedEmLote;
                    return combinedEmLote;
                }
                else
                {
                    return new CombinedEmLote();
                }
            }
        }


        #endregion

        #region Create Region
        public IActionResult Create(string InputtedRFM)
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
                //    usuario.Retorno = $"Usuário sem permissão na página! {pagina}";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Visualizar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de EntradaEmLote!";
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
                        if (checkPermission.Visualizar == 1)
                        {



                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyEntradaEmLoteList);
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyInputtedRFM);

                            httpContextAccessor.HttpContext?.Session.SetString(SessionKeyInputtedRFM, InputtedRFM);
                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;

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
                ViewBag.Error = ex.Message;
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View();
            }
        
        }

        public ActionResult SearchCatalogo(string? Codigo, string? Item)
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


                        List<CatalogoViewModel?>? Result = searches.SearchCatalogoForEntradaLote(Codigo, Item);
                        var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();
                        if (Result.Count > 0)
                        {
                            if (model.Count > 0)
                            {
                                var cartItemIds = model.Select(item => item.Id).ToList();
                                Result = Result.Where(item => !cartItemIds.Contains(item.Id)).ToList();
                                if (Result.Count > 1)
                                {
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListCatalogoCreate);
                                    HttpContext.Session.SetObject(SessionKeyListCatalogoCreate, Result);
                                    //ListCatalogoCreate = Result;
                                    int pageSize = 10;
                                    int pageNumber = 1;

                                    IPagedList<CatalogoViewModel> CatalogoPagedList = Result.ToPagedList(pageNumber, pageSize);


                                    ViewBag.CatalogList = CatalogoPagedList;
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create), model);
                                }
                                else if (Result.Count == 1)
                                {
                                    ProdutoCompleteViewModel? productDetails = searches.VerifySaldoForEntradaEmLote(Result[0].Id, httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria));
                                    if (productDetails != null)
                                    {
                                        if (Result[0].PorAferido == 0 && Result[0].PorSerial == 0)
                                        {
                                            Result[0].Saldo = productDetails.Quantidade;
                                        }
                                    }

                                    model.Add(Result[0]);
                                    HttpContext.Session.SetObject(SessionKeyEntradaEmLoteList, model);
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create), model);
                                }
                                else
                                {
                                    ViewBag.Error = "Já está adicionado na lista.";
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create), model);
                                }
                            }
                            else
                            {
                                if (Result.Count == 1)
                                {
                                    ProdutoCompleteViewModel? productDetails = searches.VerifySaldoForEntradaEmLote(Result[0].Id, httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria));
                                    if (productDetails != null)
                                    {
                                        if (Result[0].PorAferido == 0 && Result[0].PorSerial == 0)
                                        {
                                            Result[0].Saldo = productDetails.Quantidade;
                                        }
                                    }

                                    model.Add(Result[0]);
                                    HttpContext.Session.SetObject(SessionKeyEntradaEmLoteList, model);
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create), model);
                                }
                                else
                                {
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListCatalogoCreate);
                                    HttpContext.Session.SetObject(SessionKeyListCatalogoCreate, Result);
                                    //ListCatalogoCreate = Result;
                                    int pageSize = 10;
                                    int pageNumber = 1;

                                    IPagedList<CatalogoViewModel> CatalogoPagedList = Result.ToPagedList(pageNumber, pageSize);

                                    ViewBag.CatalogList = CatalogoPagedList;
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create));
                                }
                            }
                        }
                        else
                        {
                            ViewBag.Error = "Nenhum resultado encontrado.";
                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                            return View(nameof(Create), model);
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
            //    usuariofer.Retorno = $"Usuário sem permissão na página para Visualizar ---- {pagina}!";
            //    return RedirectToAction(nameof(HomeController.PreserveActionError), "Home", new { usuario = usuariofer });
            //}            
        }

        public ActionResult TestPageCatalogo(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            PageNumberCreate = pageNumber;

            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

            var SearchCatalogModel = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyListCatalogoCreate) ?? new List<CatalogoViewModel>();

            IPagedList <CatalogoViewModel> CatalogoPagedList = SearchCatalogModel.ToPagedList(pageNumber, pageSize);
            ViewBag.CatalogList = CatalogoPagedList;
            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
            return View(nameof(Create), model);
        }

        public ActionResult AddToCart(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/AddToCart";
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
            //    if (usuariofer.Permissao.Inserir != 1)
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
                    if (checkPermission.Inserir == 1)
                    {

                        if (id != null)
                        {
                            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

                            var SearchCatalogModel = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyListCatalogoCreate) ?? new List<CatalogoViewModel>();

                            CatalogoViewModel? getValue = SearchCatalogModel.FirstOrDefault(i => i.Id == id);
                            if (getValue != null)
                            {
                                ProdutoCompleteViewModel? productDetails = searches.VerifySaldoForEntradaEmLote(getValue.Id, httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria));
                                if (productDetails != null)
                                {
                                    if (getValue.PorAferido == 0 && getValue.PorSerial == 0)
                                    {
                                        getValue.Saldo = productDetails.Quantidade;
                                    }
                                }

                                model.Add(getValue);
                                HttpContext.Session.SetObject(SessionKeyEntradaEmLoteList, model);

                                SearchCatalogModel.Remove(getValue);
                                HttpContext.Session.SetObject(SessionKeyListCatalogoCreate, SearchCatalogModel);
                                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                return View(nameof(Create), model);
                            }
                            else
                            {
                                ViewBag.Error = "Error in retrieving data.";
                                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                return View(nameof(Create));
                            }
                        }
                        else
                        {
                            ViewBag.Error = "No Id Selected.";
                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                            return View(nameof(Create));
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

        public ActionResult ExcludeToCart(int? id)
        {
            if (id != null)
            {
                var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

                model.RemoveAll(item => item.Id == id);

                HttpContext.Session.SetObject(SessionKeyEntradaEmLoteList, model);

                //return View(nameof(Create), CatalogCart);
                return Json(new { success = true });
            }
            else
            {
                //ViewBag.Error = "No Id Selected.";
                //return View(nameof(Create));
                return Json(new { success = false, error = "No ID selected." });
            }        
        }

        public ActionResult ShowAnexoModal(int? id)
        {
            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();
            if (id != null)
            {
            
                CatalogoViewModel? catalogValue = model.FirstOrDefault(i => i.Id == id);
                if (catalogValue != null)
                {
                    //string? FilePath = "C:\\Ferramentaria\\EntradaEmLote\\Temp\\" + id + ".xlsx";
                    string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + id + ".xlsx";
                    //string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + id + ".xlsx";
                    if (System.IO.File.Exists(FilePath))
                    {
                        catalogValue.Uploaded = 1;
                        catalogValue.FilePath = FilePath;
                    }
                    else
                    {
                        catalogValue.Uploaded = 0;
                        catalogValue.FilePath = "";
                    }

                    ViewBag.AnexoCatalogValue = catalogValue;
                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                    return View(nameof(Create), model);
                }
                else
                {
                    ViewBag.Error = "Error in retrieving data.";
                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                    return View(nameof(Create), model);
                }
               
            }
            else
            {
                ViewBag.Error = "No Id Selected.";
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View(nameof(Create), model);
            }           
        }

        public ActionResult ViewFile(string? FilePath)
        {
            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

            if (System.IO.File.Exists(FilePath))
            {
                string? filename = Path.GetFileName(FilePath);
                byte[] fileContents = System.IO.File.ReadAllBytes(FilePath);
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;

                return File(fileContents, contentType, filename);
            }
            else
            {
                ViewBag.Error = "File Not Found.";
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View(nameof(Create), model); 
            }
        }

        public ActionResult DeleteFile(string? FilePath)
        {
            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
                ViewBag.ShowSuccessAlert = true;

                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View(nameof(Create), model);
            }
            else
            {
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                ViewBag.Error = "File Not Found.";
                return View(nameof(Create), model);
            }
        }

        [HttpPost]
        public IActionResult UploadAction(IFormFile file, int? CatalogoId)
        {
            var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

            string? error = ValidateInputsForSingleUpload(file);
            if (string.IsNullOrEmpty(error))
            {
                //string? FolderPath = "C:\\Ferramentaria\\EntradaEmLote\\Temp";
                string? FolderPath = "D:\\Ferramentaria\\EntradaEmLote\\Temp";
                //string? FolderPath = "D:\\Ferramentaria\\EntradaEmLote\\Temp";
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                string? FileExtension = Path.GetExtension(file.FileName);
                string? FinalFileName = String.Concat(CatalogoId, FileExtension);
                string? FilePath = Path.Combine(FolderPath, FinalFileName);

                using (var stream = new FileStream(FilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                ViewBag.ShowSuccessAlert = true;
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View(nameof(Create), model);
            }
            else
            {
                CatalogoViewModel? catalogValue = model.FirstOrDefault(i => i.Id == CatalogoId);
                if (catalogValue != null)
                {
                    ViewBag.AnexoCatalogValue = catalogValue;
                }
                ViewBag.ErrorUpload = error;
                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                return View(nameof(Create), model);
            }
            
        }

        public string ValidateInputsForSingleUpload(IFormFile file)
        {
            if (file != null)
            {
                if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    return "O arquivo selecionado não esta no formato *.xlsx";
                }               
            }
            else
            {
                return "Nenhum arquivo foi selecionado.";
            }
        
            return null;
        }

        [HttpPost]
        public IActionResult Save(List<SaveModel?>? SaveValues, string? RFM)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Save";
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
            //    if (usuariofer.Permissao.Inserir != 1)
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
                    if (checkPermission.Inserir == 1)
                    {



                        var model = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyEntradaEmLoteList) ?? new List<CatalogoViewModel>();

                        if (!string.IsNullOrEmpty(RFM))
                        {
                            if (SaveValues.Count > 0)
                            {
                                List<string?>? error = ValidateInsertionForRFM(SaveValues);
                                if (error.Count == 0)
                                {
                                    var InsertToEntradaEmLote_Req = new EntradaEmLote_Req
                                    {
                                        IdFerramentaria = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                        RFM = RFM,
                                        Status = 0,
                                        IdSolicitante = loggedUser.Id,
                                        DataRegistro = DateTime.Now
                                    };

                                    _context.Add(InsertToEntradaEmLote_Req);
                                    _context.SaveChanges();

                                    if (InsertToEntradaEmLote_Req.Id != null)
                                    {
                                        List<int?>? successfulInsert = new List<int?>();
                                        foreach (SaveModel item in SaveValues)
                                        {
                                            var InsertToEntradaEmLote_Comp = new EntradaEmLote_Comp
                                            {
                                                IdRequisicao = InsertToEntradaEmLote_Req.Id,
                                                IdCatalogo = item.Id,
                                                Quantidade = item.Quantidade,
                                                Observacao = string.IsNullOrEmpty(item.Observacao) == false ? item.Observacao : string.Empty,
                                                Status = 0,
                                                DataRegistro = DateTime.Now
                                            };

                                            _context.EntradaEmLote_Comp.Add(InsertToEntradaEmLote_Comp);
                                            _context.SaveChanges();

                                            successfulInsert.Add(InsertToEntradaEmLote_Comp.IdCatalogo);
                                        }

                                        if (successfulInsert.Count == SaveValues.Count)
                                        {
                                            ViewBag.ShowSuccessAlert = true;
                                            //model.Clear();
                                            httpContextAccessor?.HttpContext?.Session.Remove(SessionKeyEntradaEmLoteList);
                                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                            return View(nameof(Create));
                                        }
                                        else
                                        {
                                            ViewBag.Error = "Items left on the cart are the datas that are not inserted in the database.";
                                            model = model.Where(item => !successfulInsert.Contains(item.Id)).ToList();
                                            HttpContext.Session.SetObject(SessionKeyEntradaEmLoteList, model);
                                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                            return View(nameof(Create), model);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Error = "Error Inserting to EntradaEmLote_Req";
                                        ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                        return View(nameof(Create), model);
                                    }
                                }
                                else
                                {
                                    ViewBag.ErrorList = error;
                                    ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                    return View(nameof(Create), model);
                                }
                            }
                            else
                            {
                                ViewBag.Error = "List is Empty";
                                ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                                return View(nameof(Create), model);
                            }
                        }
                        else
                        {
                            ViewBag.Error = "RFM / Nota Fiscal não informado.";
                            ViewBag.InputtedRFM = HttpContext.Session.GetString(SessionKeyInputtedRFM) ?? string.Empty;
                            return View(nameof(Create), model);
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

        public List<string>? ValidateInsertionForRFM(List<SaveModel>? SaveValue)
        {
            List<string>? errors = new List<string>();
            foreach (SaveModel item in SaveValue)
            {
                if (item.Quantidade == null)
                {
                    errors.Add($"Codigo: {item.Codigo} Quantidade vazia.");
                }
                else
                {
                    if (item.Quantidade <= 0)
                    {
                        errors.Add($"Codigo: {item.Codigo} Quantidade inferior a 1.");
                    }
                }

                if (item.PorAferido == 1 || item.PorSerial == 1)
                {
                    //string? FilePath = "C:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    //string? FilePath = "D:\\Ferramentaria\\EntradaEmLote\\Temp\\" + item.Id + ".xlsx";
                    if (!System.IO.File.Exists(FilePath))
                    {
                        errors.Add($"Codigo: {item.Codigo} Não possui arquivo relacionado.");
                    }
                }

            }

            return errors;
        }


        #endregion


        [HttpGet]
        public ActionResult CheckRFM(string? selectedValue)
        {

            EntradaEmLote_Req? checkRFM = _context.EntradaEmLote_Req.FirstOrDefault(i => i.RFM == selectedValue && i.Status == 7);

            if (checkRFM != null)
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }   
        }


        #region No Use
        //Details
        // GET: EntradaEmLote_Req/Details/5
        public async Task<IActionResult> Details(int? id)
        {        
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Visualizar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                    var query = from ferramentaria in _context.Ferramentaria
                                where ferramentaria.Ativo == 1 &&
                                      !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                      _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
                                orderby ferramentaria.Nome
                                select new
                                {
                                    ferramentaria.Id,
                                    ferramentaria.Nome,
                                };

                    var ResultTaken = query.ToList();

                    ViewBag.TakenList = ResultTaken;
                        

                var catalogoViewModel = (from catalogo in _context.Catalogo
                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                         join c in _context.Categoria on categoria.IdCategoria equals c.Id into parentCategory
                                         from pc in parentCategory.DefaultIfEmpty()
                                         where catalogo.Ativo == 1 && catalogo.Id == id
                                         orderby catalogo.Nome
                                         select new CatalogoViewModel
                                         {
                                             Id = catalogo.Id,
                                             Codigo = catalogo.Codigo,
                                             Nome = catalogo.Nome,
                                             Descricao = catalogo.Descricao,
                                             PorMetro = catalogo.PorMetro,
                                             PorAferido = catalogo.PorAferido,
                                             PorSerial = catalogo.PorSerial,
                                             RestricaoEmprestimo = catalogo.RestricaoEmprestimo,
                                             ImpedirDescarte = catalogo.ImpedirDescarte,
                                             HabilitarDescarteEpi = catalogo.HabilitarDescarteEPI,
                                             DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                             DataRegistro = catalogo.DataRegistro,
                                             Ativo = catalogo.HabilitarDescarteEPI,
                                             IdCategoria = categoria.Id,
                                             IdCategoriaPai = categoria.IdCategoria,
                                             CategoriaNome = categoria.Nome,
                                             CategoriaClasse = categoria.Classe,
                                             CategoriaNomePai = pc.Nome,
                                             CategoriaDataRegistro = categoria.DataRegistro,
                                             CategoriaAtivo = categoria.Ativo
                                         }).FirstOrDefault();

                //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

                if (TempData.ContainsKey("ErrorMessage"))
                {
                    ViewBag.Error = TempData["ErrorMessage"].ToString();
                    TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                }


                usuario.Retorno = "Acesso Permitido";
                log.LogWhy = usuario.Retorno;
                auxiliar.GravaLogSucesso(log);

                return View(catalogoViewModel);


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

        //Create Loading page
        // GET: EntradaEmLote_Req/Create
        public IActionResult CreateNotUse(int? page, string? Codigo, string? Item)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Visualizar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Inserir a página de EntradaEmLote!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }

                var query = from catalogo in _context.Catalogo
                            join categoria in _context.Categoria
                            on catalogo.IdCategoria equals categoria.Id
                            where catalogo.Ativo == 1
                            orderby catalogo.Nome
                            select new
                            {
                                IdCatalogo = catalogo.Id,
                                CodigoCatalogo = catalogo.Codigo,
                                NomeCatalogo = catalogo.Nome,
                                DescricaoCatalogo = catalogo.Descricao,
                                PorMetroCatalogo = catalogo.PorMetro,
                                PorAferidoCatalogo = catalogo.PorAferido,
                                PorSerialCatalogo = catalogo.PorSerial,
                                RestricaoEmprestimoCatalogo = catalogo.RestricaoEmprestimo,
                                ImpedirDescarteCatalogo = catalogo.ImpedirDescarte,
                                HabilitarDescarteEpiCatalogo = catalogo.HabilitarDescarteEPI,
                                DataDeRetornoAutomaticoCatalogo = catalogo.DataDeRetornoAutomatico,
                                DataRegistroCatalogo = catalogo.DataRegistro,
                                AtivoCatalogo = catalogo.Ativo,
                                IdCategoriaCatalogo = categoria.Id,
                                IdCategoriaPaiCatalogo = categoria.IdCategoria,
                                CategoriaNomeCatalogo = categoria.Nome,
                                CategoriaClasseCatalogo = categoria.Classe,
                                CategoriaNomePaiCatalogo = _context.Categoria.Where(cat => cat.Id == categoria.IdCategoria).Select(cat => cat.Nome).FirstOrDefault(),
                                DataRegistroCategoria = categoria.DataRegistro,
                                AtivoCategoria = categoria.Ativo
                            };

                var result = query.ToList();

                var catalogoViewModels = result.Select(item => new CatalogoViewModel
                {
                    Id = item.IdCatalogo,
                    Codigo = item.CodigoCatalogo,
                    Nome = item.NomeCatalogo,
                    Descricao = item.DescricaoCatalogo,
                    PorMetro = item.PorMetroCatalogo,
                    PorAferido = item.PorAferidoCatalogo,
                    PorSerial = item.PorSerialCatalogo,
                    RestricaoEmprestimo = item.RestricaoEmprestimoCatalogo,
                    ImpedirDescarte = item.ImpedirDescarteCatalogo,
                    HabilitarDescarteEpi = item.HabilitarDescarteEpiCatalogo,
                    DataDeRetornoAutomatico = item.DataDeRetornoAutomaticoCatalogo,
                    DataRegistro = item.DataRegistroCatalogo,
                    Ativo = item.AtivoCatalogo,
                    IdCategoria = item.IdCategoriaCatalogo,
                    IdCategoriaPai = item.IdCategoriaPaiCatalogo,
                    CategoriaNome = item.CategoriaNomeCatalogo,
                    CategoriaNomePai = item.CategoriaNomePaiCatalogo,
                    CategoriaClasse = item.CategoriaClasseCatalogo,
                    CategoriaDataRegistro = item.DataRegistroCategoria,
                    CategoriaAtivo = item.AtivoCategoria
                    // ... Map other properties here
                }).ToList();

                if (!string.IsNullOrEmpty(Codigo))
                {
                    catalogoViewModels = catalogoViewModels.Where(c => c.Codigo.Contains(Codigo)).ToList();
                }

                if (!string.IsNullOrEmpty(Item))
                {
                    catalogoViewModels = catalogoViewModels.Where(c => c.Nome.Contains(Item)).ToList();
                }

                int pageSize = 10; // Set your desired page size
                int pageNumber = 1;

                //_ListCatalogo = catalogoViewModels;
                IPagedList<CatalogoViewModel> CatalogoPagedList = catalogoViewModels.ToPagedList(pageNumber, pageSize);

                usuario.Retorno = "Acesso Permitido";
                log.LogWhy = usuario.Retorno;
                auxiliar.GravaLogSucesso(log);

                return View(CatalogoPagedList);

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

  


        //Create action

        // POST: EntradaEmLote_Req/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, string? RFM, int? Setor, int? Quantidade, string? Obs)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Visualizar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion


                if (id == null)
                {
                    usuario.Retorno = "ID da Entrada Em Lote não localizada no banco de dados!";
                    log.LogWhy = usuario.Retorno;
                    ErrorViewModel erro = new ErrorViewModel();
                    erro.Tela = log.LogWhere;
                    erro.Descricao = log.LogWhy;
                    erro.Mensagem = log.LogWhat;
                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                    return RedirectToAction(nameof(Index));
                }

                if (RFM == null)
                {
                    //TempData["ShowErrorAlert"] = true;
                    TempData["ErrorMessage"] = "RFM is Empty";

                    usuario.Retorno = "Erro na validação do modelo em editar Entrada em Lote!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);

                    return RedirectToAction(nameof(Details), new { id = id });
                }

                if (Quantidade == null)
                {
                    //TempData["ShowErrorAlert"] = true;
                    TempData["ErrorMessage"] = "RFM is Empty";

                    usuario.Retorno = "Erro na validação do modelo em editar Entrada em lote!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);

                    return RedirectToAction(nameof(Details), new { id = id });
                }

                if (Setor == null)
                {
                    usuario.Retorno = "Erro na validação do modelo em editar Entrada em lote!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);

                    return RedirectToAction(nameof(Index));
                }

                var InsertEntradaLote = new EntradaEmLote_Req
                {
                    IdFerramentaria = Setor,
                    RFM = RFM,
                    Status = 0,
                    IdSolicitante = usuario.Id,
                    DataRegistro = DateTime.Now

                };

                _context.Add(InsertEntradaLote);
                await _context.SaveChangesAsync();

                int? insertedId = InsertEntradaLote.Id;

                var InsertComp = new EntradaEmLote_Comp
                {
                    IdRequisicao = insertedId,
                    IdCatalogo = id,
                    Quantidade = Quantidade,
                    Observacao = Obs,
                    Status = 0,
                    DataRegistro = DateTime.Now
                };

                _context.Add(InsertComp);
                await _context.SaveChangesAsync();

                TempData["ShowSuccessAlert"] = true;

                usuario.Retorno = "Entrada em Lote insirir com sucesso";
                log.LogWhy = usuario.Retorno;
                auxiliar.GravaLogSucesso(log);

                return RedirectToAction(nameof(Index));

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

        // GET: EntradaEmLote_Req/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            
            try
            {
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Editar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Editar a página de Entrada em lote!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }


                var query = _context.EntradaEmLote_Comp.Where(e => e.IdRequisicao == id);

                var ResultTaken = query.FirstOrDefault();

                ViewBag.QtdObs = ResultTaken;

                var querystatus = _context.EntradaEmLote_Req.Where(e => e.Id == query.FirstOrDefault().IdRequisicao);

                ViewBag.ReqStatus = querystatus.FirstOrDefault();

                var catalogoViewModel = (from catalogo in _context.Catalogo
                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                         join c in _context.Categoria on categoria.IdCategoria equals c.Id into parentCategory
                                         from pc in parentCategory.DefaultIfEmpty()
                                         where catalogo.Ativo == 1 && catalogo.Id == query.FirstOrDefault().IdCatalogo
                                         orderby catalogo.Nome
                                         select new CatalogoViewModel
                                         {
                                             Id = catalogo.Id,
                                             Codigo = catalogo.Codigo,
                                             Nome = catalogo.Nome,
                                             Descricao = catalogo.Descricao,
                                             PorMetro = catalogo.PorMetro,
                                             PorAferido = catalogo.PorAferido,
                                             PorSerial = catalogo.PorSerial,
                                             RestricaoEmprestimo = catalogo.RestricaoEmprestimo,
                                             ImpedirDescarte = catalogo.ImpedirDescarte,
                                             HabilitarDescarteEpi = catalogo.HabilitarDescarteEPI,
                                             DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                             DataRegistro = catalogo.DataRegistro,
                                             Ativo = catalogo.HabilitarDescarteEPI,
                                             IdCategoria = categoria.Id,
                                             IdCategoriaPai = categoria.IdCategoria,
                                             CategoriaNome = categoria.Nome,
                                             CategoriaClasse = categoria.Classe,
                                             CategoriaNomePai = pc.Nome,
                                             CategoriaDataRegistro = categoria.DataRegistro,
                                             CategoriaAtivo = categoria.Ativo
                                         }).FirstOrDefault();

                usuario.Retorno = "Acesso Permitido";
                log.LogWhy = usuario.Retorno;
                auxiliar.GravaLogSucesso(log);

                return View(catalogoViewModel);

                    //if (id == null || _context.EntradaEmLote_Req == null)
                    //{
                    //    return NotFound();
                    //}

                    //var entradaEmLote_Req = await _context.EntradaEmLote_Req.FindAsync(id);
                    //if (entradaEmLote_Req == null)
                    //{
                    //    return NotFound();
                    //}
                    //return View(entradaEmLote_Req);

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

        // POST: EntradaEmLote_Req/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,IdFerramentaria,RFM,Status,IdSolicitante,DataRegistro")] EntradaEmLote_Req entradaEmLote_Req)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Editar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Editar a página de Entrada em lote!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                if (id != entradaEmLote_Req.Id)
                {
                    usuario.Retorno = "ID da entrada em lote não localizada no banco de dados!";
                    log.LogWhy = usuario.Retorno;
                    ErrorViewModel erro = new ErrorViewModel();
                    erro.Tela = log.LogWhere;
                    erro.Descricao = log.LogWhy;
                    erro.Mensagem = log.LogWhat;
                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(entradaEmLote_Req);
                        await _context.SaveChangesAsync();

                        usuario.Retorno = "Entrada em lote editar com sucesso";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogSucesso(log);

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        //if (!EntradaEmLote_ReqExists(entradaEmLote_Req.Id))
                        //{
                        //    return NotFound();
                        //}
                        //else
                        //{
                        //    throw;
                        //}
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(entradaEmLote_Req);

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

        // GET: EntradaEmLote_Req/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Excluir != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Excluir a página de entrada em lote!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                if (id == null || _context.EntradaEmLote_Req == null)
                {
                    usuario.Retorno = "ID da entrada em lote não localizada no banco de dados!";
                    log.LogWhy = usuario.Retorno;
                    ErrorViewModel erro = new ErrorViewModel();
                    erro.Tela = log.LogWhere;
                    erro.Descricao = log.LogWhy;
                    erro.Mensagem = log.LogWhat;
                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                    return NotFound();
                }

                var entradaEmLote_Req = await _context.EntradaEmLote_Req
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (entradaEmLote_Req == null)
                {
                    usuario.Retorno = "ID da entrada em lote não localizada no banco de dados!";
                    log.LogWhy = usuario.Retorno;
                    ErrorViewModel erro = new ErrorViewModel();
                    erro.Tela = log.LogWhere;
                    erro.Descricao = log.LogWhy;
                    erro.Mensagem = log.LogWhat;
                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                    return NotFound();
                }

                usuario.Retorno = "entrada em lote carregada com sucesso!";
                log.LogWhy = usuario.Retorno;
                auxiliar.GravaLogSucesso(log);
                return View(entradaEmLote_Req);


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

        // POST: EntradaEmLote_Req/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Excluir != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Excluir a página de entrada em lote!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                if (id == null)
                {
                    usuario.Retorno = "ID da pergunta não localizada no banco de dados!";
                    log.LogWhy = usuario.Retorno;
                    ErrorViewModel erro = new ErrorViewModel();
                    erro.Tela = log.LogWhere;
                    erro.Descricao = log.LogWhy;
                    erro.Mensagem = log.LogWhat;
                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                    return RedirectToAction(nameof(Index));
                }

                var EmLote = _context.EntradaEmLote_Req.SingleOrDefault(e => e.Id == id);
 
                if (EmLote != null)
                {
                    EmLote.Status = 5;

                    await _context.SaveChangesAsync();

                    usuario.Retorno = "Entrada em lote deativar com sucesso!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogSucesso(log);

                    return Json(new { success = true });

                }

                return Json(new { success = false, error = "Item not found" });

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

// GET: EntradaEmLote_Req
//public async Task<IActionResult> Index(int? page, int? Status, int? Setor, string? RFM)
//{
//    //return _context.EntradaEmLote_Req != null ? 
//    //            View(await _context.EntradaEmLote_Req.ToListAsync()) :
//    //            Problem("Entity set 'ContextoBanco.EntradaEmLote_Req'  is null.");

//    ViewData["Status"] = Status;
//    ViewData["Setor"] = Setor;

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
//            if (usuario.Permissao.Visualizar != 1)
//            {
//                usuario.Retorno = "Usuário sem permissão de visualizar a página de entrada em lote!";
//                log.LogWhy = usuario.Retorno;
//                auxiliar.GravaLogAlerta(log);
//                return RedirectToAction("Login", "Home", usuario);
//            }
//        }
//        #endregion

//        ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

//        if (Setor == null)
//        {
//            var query = from ferramentaria in _context.Ferramentaria
//                        where ferramentaria.Ativo == 1 &&
//                              !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
//                              _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
//                        orderby ferramentaria.Nome
//                        select new
//                        {
//                            ferramentaria.Id,
//                            ferramentaria.Nome,
//                        };

//            var ResultTaken = query.ToList();

//            ViewBag.TakenList = ResultTaken;
//        }
//        else
//        {
//            var query = from ferramentaria in _context.Ferramentaria
//                        where ferramentaria.Ativo == 1 &&
//                              !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
//                              _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
//                        orderby ferramentaria.Nome
//                        select new
//                        {
//                            ferramentaria.Id,
//                            ferramentaria.Nome,
//                        };

//            var ResultTaken = query.Where(item => item.Id == Setor).ToList();

//            ViewBag.TakenList = ResultTaken;
//        }


//        int pageSize = 20; // Set your desired page size
//        int pageNumber = (page ?? 1);

//        if (Status == null && Setor == null)
//        {
//            var emptyData = new List<EntradaEmLote_Req>(); // Create an empty collection or object

//            IPagedList<EntradaEmLote_Req> empty = emptyData.ToPagedList(pageNumber, pageSize);

//            return View(empty); // Pass the empty collection or object to the view

//        }

//        if (Setor == null)
//        {
//            var emptyData = new List<EntradaEmLote_Req>(); // Create an empty collection or object

//            IPagedList<EntradaEmLote_Req> empty = emptyData.ToPagedList(pageNumber, pageSize);

//            return View(empty); // Pass the empty collection or object to the view

//        }

//        var EmLote = _context.EntradaEmLote_Req.AsQueryable();
//        var filteredResults = new List<EntradaEmLote_Req>(); // Initialize an empty list to collect filtered results

//        if (Status == 8)
//        {
//            var data = EmLote.Where(entry => entry.IdFerramentaria == Setor).ToList();

//            filteredResults.AddRange(data);

//        }

//        if (Status != 8 && Status != null)
//        {
//            var data = EmLote.Where(entry => entry.IdFerramentaria == Setor && entry.Status == Status).ToList();

//            filteredResults.Clear();
//            filteredResults.AddRange(data);
//        }

//        if (RFM != null)
//        {
//            var data = EmLote
//                      .Where(entry => entry.IdFerramentaria == Setor && entry.RFM.Contains(RFM))
//                      .ToList();

//            //// Step 1: Extract IdFerramentaria values from data
//            //var idSolicitanteValues = data.Select(entry => entry.IdSolicitante).ToList();

//            //// Step 2: Use the values to filter VW_usuario
//            //var usuarios = _contextBS.VW_Usuario_New.Where(usuario => idSolicitanteValues.Contains(usuario.Id)).ToList();

//            //ViewBag.NomeList = usuarios;

//            filteredResults.Clear();
//            filteredResults.AddRange(data);
//        }


//        var uniqueFilteredResults = filteredResults.ToList();

//        var paginatedResult = uniqueFilteredResults
//                                .OrderByDescending(e => e.Id)
//                                .ToList();


//        // Assuming AutoMapper maps the entity to the view model
//        IPagedList<EntradaEmLote_Req> EntradaEmLotePagedList = paginatedResult.ToPagedList(pageNumber, pageSize);


//        usuario.Retorno = "Acesso Permitido";
//        log.LogWhy = usuario.Retorno;
//        auxiliar.GravaLogSucesso(log);

//        return View(EntradaEmLotePagedList); 

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

#endregion