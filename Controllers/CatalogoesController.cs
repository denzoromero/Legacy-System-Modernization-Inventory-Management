using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using X.PagedList;
using AutoMapper;
using FerramentariaTest.Helpers;
using Microsoft.Win32;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class CatalogoesController : Controller
    {

        private const string SessionKeyCombinedModel = "CombinedModel";
        private const string SessionKeyCatalogoSearchModel = "CatalogoSearchModel";
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thItens.aspx";
        private MapperConfiguration mapeamentoClasses;

        //private static int? GlobalPagination;
        //private static int? GlobalPageNumber;

        //private static CombinedCatalogo? catalogoCombinedModel = new CombinedCatalogo();
        //private static CatalogoSearchModel? SearchModel = new CatalogoSearchModel();
        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyListCatalogo = "ListCatalogo";
        private const string SessionKeyPageNumber = "PageNumber";
        //private static List<CatalogoViewModel?>? _ListCatalogo = new List<CatalogoViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public CatalogoesController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Catalogo, ForCatalogoEdit>();
                cfg.CreateMap<ForCatalogoEdit, Catalogo>();
            });
        }

        // GET: Catalogoes
        public IActionResult Index()
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
                //    if (usuario.Permissao.Visualizar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de ferramentaria!";
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


                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
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
                            else
                            {
                                ViewBag.FerramentariaValue = FerramentariaValue;
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
                ViewBag.Error = ex.Message;
                return View();
            }         
        }

        public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome)
        {
            if (Ferramentaria != null)
            {
                httpContextAccessor?.HttpContext?.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
                httpContextAccessor?.HttpContext?.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult RefreshFerramentaria()
        {
            httpContextAccessor?.HttpContext?.Session.Remove(Sessao.Ferramentaria);
            httpContextAccessor?.HttpContext?.Session.Remove(Sessao.FerramentariaNome);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult GetCatalogo(CombinedCatalogo CombinedCatalogo)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/GetCatalogo";
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

                        List<CatalogoViewModel>? Result = searches.SearchCatalogo(CombinedCatalogo.CatalogoSearchModel);
                        if (Result.Any())
                        {
                            if (CombinedCatalogo.CatalogoSearchModel != null)
                            {
                                HttpContext.Session.SetObject(SessionKeyCatalogoSearchModel, CombinedCatalogo.CatalogoSearchModel);
                            }

                            var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();

                            //SearchModel = CombinedCatalogo.CatalogoSearchModel;
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPageNumber);
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListCatalogo);
                            HttpContext.Session.SetObject(SessionKeyListCatalogo, Result);
                            //_ListCatalogo = Result;

                            //GlobalPagination = CombinedCatalogo.CatalogoSearchModel.Pagination;
                            int pageSize = CombinedCatalogo.CatalogoSearchModel.Pagination ?? 10;
                            int pageNumber = 1;
                            //GlobalPageNumber = pageNumber;

                            IPagedList<CatalogoViewModel> CatalogoPagedList = Result.ToPagedList(pageNumber, pageSize);
                            var combinedViewModel = new CombinedCatalogo
                            {
                                CatalogoViewModel = CatalogoPagedList,
                                CatalogoSearchModel = CatalogSearchModel ?? new CatalogoSearchModel(),
                                ResultCount = Result.Count()
                            };

                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyCombinedModel);
                            HttpContext.Session.SetObject(SessionKeyCombinedModel, CombinedCatalogo);

                            //catalogoCombinedModel = combinedViewModel;
                            return View(nameof(Index), combinedViewModel);

                        }
                        else
                        {
                            ViewBag.Error = "Nenhum resultado encontrado.";
                            return View(nameof(Index), CombinedCatalogo);
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
            //    ViewBag.Error = "O usuário não tem permissão para visualizar novos dados.";
            //    return View(nameof(Index), new CombinedCatalogo());
            //}
      
        }

        [HttpPost]
        public IActionResult CreateCatalogo(CatalogoCreateModel? create)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/CreateCatalogo";
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
                    if (checkPermission.Inserir == 1)
                    {


                        string? error = ValidateCreate(create);
                        if (string.IsNullOrEmpty(error))
                        {
                            CatalogoCreateModel? NewCreate = SetPorValues(create);
                            var duplicates = _context.Catalogo.Where(c => c.Ativo == 1
                                            && c.Nome.Contains(NewCreate.Nome)
                                            && c.Codigo.Contains(NewCreate.Codigo)
                                            && c.Descricao.Contains(NewCreate.Descricao)).ToList();

                            if (!duplicates.Any())
                            {
                                var InsertCatalogo = new Catalogo
                                {
                                    IdCategoria = NewCreate.SelectedTipo,
                                    Codigo = NewCreate.Codigo,
                                    Nome = NewCreate.Nome,
                                    Descricao = NewCreate.Descricao,
                                    PorMetro = NewCreate.PorMetro,
                                    PorAferido = NewCreate.PorAferido,
                                    PorSerial = NewCreate.PorSerial,
                                    RestricaoEmprestimo = NewCreate.RestricaoEmprestimo,
                                    ImpedirDescarte = NewCreate.ImpedirDescarte,
                                    HabilitarDescarteEPI = NewCreate.HabilitarDescarteEpi,
                                    DataDeRetornoAutomatico = NewCreate.DataDeRetornoAutomatico,
                                    DataRegistro = DateTime.Now,
                                    Ativo = 1

                                };

                                _context.Add(InsertCatalogo);
                                _context.SaveChanges();

                                ViewBag.ShowSuccessAlert = true;
                                return View(nameof(Index));
                            }
                            else
                            {
                                ViewBag.Error = "Falha ao INSERIR Registro, Item já existente.";
                                return View(nameof(Index));
                            }
                        }
                        else
                        {
                            ViewBag.CreateValue = create;
                            ViewBag.ErrorCreateCatalogo = error;
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

            //if (usuariofer.Permissao.Inserir == 1)
            //{
             
            //}
            //else
            //{
            //    ViewBag.Error = "O usuário não tem permissão para inserir novos dados.";
            //    return View(nameof(Index), new CombinedCatalogo());
            //}           
        }

        public string ValidateCreate(CatalogoCreateModel? create)
        {
            if (create.SelectedCatalogo == null || create.SelectedCatalogo == 0)
            {
                return "Catalogo não foi selecionada.";
            }

            if (create.SelectedClasse == null || create.SelectedClasse == 0)
            {
                return "Classe não foi selecionada.";
            }

            if (create.SelectedTipo == null || create.SelectedTipo == 0)
            {
                return "Tipo não foi selecionada.";
            }

            if (create.SelectedClasse == 9)
            {
                if (create.Controle == null)
                {
                    return "Por favor escolha o controle: PorAferido";
                }
            }
            else
            {
                if (create.SelectedCatalogo == 1)
                {
                    if (create.Controle == null)
                    {
                        return "Por favor escolha o controle: PorAferido ou PorSerial ou PorQuantidade";
                    }
                }
                else if (create.SelectedCatalogo == 2)
                {
                    if (create.Controle == null)
                    {
                        return "Por favor escolha o controle: PorQuantidade";
                    }
                }
                else if(create.SelectedCatalogo == 3)
                {
                    if (create.Controle == null)
                    {
                        return "Por favor escolha o controle: PorMetro ou PorQuantidade";
                    }
                }
            }

            if (string.IsNullOrEmpty(create.Codigo))
            {
                return "Código vazio.";
            }
            else
            {
                Catalogo? catalogo = _context.Catalogo.Where(i => i.Codigo == create.Codigo).FirstOrDefault();
                if (catalogo != null)
                {
                    return $"Código já foi atribuido ao item {catalogo.Codigo}.";
                }
            }

            if (string.IsNullOrEmpty(create.Nome))
            {
                return "Item vazio.";
            }

            if (!string.IsNullOrEmpty(create.Descricao) && create.Descricao.Length > 500)
            {
                return "Descrição acima de 500 caracteres.";
            }



            return null;
        }

        public CatalogoCreateModel SetPorValues(CatalogoCreateModel? create)
        {
            if (create.Controle == 1)
            {
                create.PorMetro = 1;
                create.PorAferido = 0;
                create.PorSerial = 0;
                create.PorQuantidade = 0;
            }
            else if (create.Controle == 2)
            {
                create.PorAferido = 1;
                create.PorMetro = 0;
                create.PorSerial = 0;
                create.PorQuantidade = 0;
            }
            else if (create.Controle == 3)
            {
                create.PorSerial = 1;
                create.PorMetro = 0;
                create.PorAferido = 0;
                create.PorQuantidade = 0;
            }
            else
            {
                create.PorMetro = 0;
                create.PorAferido = 0;
                create.PorSerial = 0;
                create.PorQuantidade = 1;
            }

            if (create.RestricaoEmprestimo == null)
            {
                create.RestricaoEmprestimo = 0;
            }

            if (create.ImpedirDescarte == null)
            {
                create.ImpedirDescarte = 0;
            }

            if (create.HabilitarDescarteEpi == null)
            {
                create.HabilitarDescarteEpi = 0;
            }

            if (create.chkDataDeRetornoAutomatico == null)
            {
                create.DataDeRetornoAutomatico = 0;
            }

            if (string.IsNullOrEmpty(create.Descricao))
            {
                create.Descricao = "";
            }

            return create;
        }

        public ActionResult GetCatalogoToEdit(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();
            var ListCatalogoModel = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListCatalogo) ?? new List<CatalogoViewModel?>();

            int pageSize = CatalogSearchModel.Pagination ?? 10;
            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumber) ?? 1;
            IPagedList<CatalogoViewModel> CatalogoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

            var combinedViewModel = new CombinedCatalogo
            {
                CatalogoViewModel = CatalogoPagedList,
                CatalogoSearchModel = CatalogSearchModel,
                ResultCount = ListCatalogoModel.Count(),
            };

            if (id != null)
            {
                CatalogoViewModel? catalogo = searches.GetCatalogo(id);
                if (catalogo != null)
                {

                    

                    ViewBag.CatalogoValues = catalogo;
                    return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                }
                else
                {
                    ViewBag.Error = "No data found.";
                    return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                }               
            }
            else
            {
                ViewBag.Error = "No Id Selected.";
                return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
            }           
        }

        public ActionResult TestPage(int? page)
        {
            var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();

            int pageSize = CatalogSearchModel.Pagination ?? 10;
            int pageNumber = (page ?? 1);
            //GlobalPageNumber = pageNumber;

            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPageNumber);
            HttpContext.Session.SetInt32(SessionKeyPageNumber, pageNumber);


            var ListCatalogoModel = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListCatalogo) ?? new List<CatalogoViewModel?>();

            IPagedList<CatalogoViewModel> CatalogoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);
            var combinedViewModel = new CombinedCatalogo
            {
                CatalogoViewModel = CatalogoPagedList,
                CatalogoSearchModel = CatalogSearchModel,
                ResultCount = ListCatalogoModel.Count(),
            };
            return View(nameof(Index), combinedViewModel);
        }


        [HttpPost]
        public IActionResult EditCatalogo(CatalogoCreateModel? edit)
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

                        //var CombinedModel = HttpContext.Session.GetObject<CombinedCatalogo?>(SessionKeyCombinedModel) ?? new CombinedCatalogo();
                        var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();
                        var ListCatalogoModel = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListCatalogo) ?? new List<CatalogoViewModel?>();

                        int pageSize = CatalogSearchModel.Pagination ?? 10;
                        int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumber) ?? 1;
                        IPagedList<CatalogoViewModel> CatalogoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

                        var combinedViewModel = new CombinedCatalogo
                        {
                            CatalogoViewModel = CatalogoPagedList,
                            CatalogoSearchModel = CatalogSearchModel,
                            ResultCount = ListCatalogoModel.Count(),
                        };

                        if (edit != null)
                        {

                            if (checkPermission.Editar == 1)
                            {
                                string? errors = ValidateEdit(edit);
                                if (string.IsNullOrEmpty(errors))
                                {
                                    CatalogoCreateModel? NewCreate = SetPorValues(edit);

                                    var updateCatalogo = _context.Catalogo.FirstOrDefault(i => i.Id == edit.Id);
                                    if (updateCatalogo != null)
                                    {
                                        updateCatalogo.Codigo = updateCatalogo.Codigo != edit.Codigo ? edit.Codigo : updateCatalogo.Codigo;
                                        updateCatalogo.Nome = updateCatalogo.Nome != edit.Nome ? edit.Nome : updateCatalogo.Nome;
                                        updateCatalogo.Descricao = updateCatalogo.Descricao != edit.Descricao ? edit.Descricao : updateCatalogo.Descricao;
                                        //updateCatalogo.PorMetro = updateCatalogo.PorMetro != edit.PorMetro ? edit.PorMetro : updateCatalogo.PorMetro;
                                        //updateCatalogo.PorAferido = updateCatalogo.PorAferido != edit.PorAferido ? edit.PorAferido : updateCatalogo.PorAferido;
                                        //updateCatalogo.PorSerial = updateCatalogo.PorSerial != edit.PorSerial ? edit.PorSerial : updateCatalogo.PorSerial;
                                        updateCatalogo.RestricaoEmprestimo = updateCatalogo.RestricaoEmprestimo != edit.RestricaoEmprestimo ? edit.RestricaoEmprestimo : updateCatalogo.RestricaoEmprestimo;
                                        updateCatalogo.ImpedirDescarte = updateCatalogo.ImpedirDescarte != edit.ImpedirDescarte ? edit.ImpedirDescarte : updateCatalogo.ImpedirDescarte;
                                        updateCatalogo.HabilitarDescarteEPI = updateCatalogo.HabilitarDescarteEPI != edit.HabilitarDescarteEpi ? edit.HabilitarDescarteEpi : updateCatalogo.HabilitarDescarteEPI;
                                        updateCatalogo.DataDeRetornoAutomatico = updateCatalogo.DataDeRetornoAutomatico != edit.DataDeRetornoAutomatico ? edit.DataDeRetornoAutomatico : updateCatalogo.DataDeRetornoAutomatico;
                                        _context.SaveChanges();
                                    }



                                    CatalogoSearchModel? refreshModel = CatalogSearchModel;
                                    CombinedCatalogo? catalogoCombinedModel = RefreshSearchValues(refreshModel);
                                    catalogoCombinedModel.CatalogoSearchModel = CatalogSearchModel;
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyCombinedModel);
                                    HttpContext.Session.SetObject(SessionKeyCombinedModel, catalogoCombinedModel);

                                    ViewBag.ShowSuccessAlert = true;
                                    return View(nameof(Index), catalogoCombinedModel ?? new CombinedCatalogo());

                                }
                                else
                                {
                                    ViewBag.Error = errors;
                                    return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                                }
                            }
                            else
                            {
                                ViewBag.Error = "O usuário não tem permissão para editar dados.";
                                return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                            }
                        }
                        else
                        {
                            ViewBag.Error = "";
                            return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
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

        public string ValidateEdit(CatalogoCreateModel? edit)
        {
            if (string.IsNullOrEmpty(edit.Codigo))
            {
                return "Código vazio.";
            }
            else
            {
                Catalogo? catalogo = _context.Catalogo.Where(i => i.Codigo == edit.Codigo && i.Id != edit.Id).FirstOrDefault();
                if (catalogo != null)
                {
                    return $"Código já foi atribuido ao item {catalogo.Codigo}.";
                }
            }

            if (string.IsNullOrEmpty(edit.Nome))
            {
                return "Item vazio.";
            }

            if (!string.IsNullOrEmpty(edit.Descricao) && edit.Descricao.Length > 500)
            {
                return "Descrição acima de 500 caracteres.";
            }

            return null;
        }

        public CombinedCatalogo? RefreshSearchValues(CatalogoSearchModel? searchModel)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<CatalogoViewModel?>? Result = searches.SearchCatalogo(searchModel);
            if (Result.Any())
            {
                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListCatalogo);
                HttpContext.Session.SetObject(SessionKeyListCatalogo, Result);
                //_ListCatalogo = Result;
                int pageSize = searchModel.Pagination ?? 10;

                int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumber) ?? 1;

                IPagedList<CatalogoViewModel> CatalogoPagedList = Result.ToPagedList(pageNumber, pageSize);

                CombinedCatalogo? combinedViewModel = new CombinedCatalogo
                {
                    CatalogoViewModel = CatalogoPagedList,
                    ResultCount = Result.Count()
                };

                return combinedViewModel;
            }
            else
            {
                return new CombinedCatalogo();
            }
        }


        public ActionResult DeleteCatalogo(int? id)
        {
            var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();
            var ListCatalogoModel = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListCatalogo) ?? new List<CatalogoViewModel?>();

            int pageSize = CatalogSearchModel.Pagination ?? 10;
            int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumber) ?? 1;
            IPagedList<CatalogoViewModel> CatalogoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

            var combinedViewModel = new CombinedCatalogo
            {
                CatalogoViewModel = CatalogoPagedList,
                CatalogoSearchModel = CatalogSearchModel,
                ResultCount = ListCatalogoModel.Count(),
            };


            if (id != null)
            {
                Catalogo? catalogoValue = _context.Catalogo.FirstOrDefault(i => i.Id == id);
                if (catalogoValue != null)
                {
                    ViewBag.IdToBeDeleted = catalogoValue;
                    return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                }
                else
                {
                    ViewBag.Error = "No data found.";
                    return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                }           
            }
            else
            {
                ViewBag.Error = "No Id Selected.";
                return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
            }          
        }

        [HttpPost]
        public async Task<IActionResult> InactivateCatalogo(int? Id)
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

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
            if (loggedUser != null)
            {
                PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                if (checkPermission != null)
                {
                    if (checkPermission.Visualizar == 1)
                    {


                        var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();
                        var ListCatalogoModel = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListCatalogo) ?? new List<CatalogoViewModel?>();

                        int pageSize = CatalogSearchModel.Pagination ?? 10;
                        int pageNumber = HttpContext.Session.GetInt32(SessionKeyPageNumber) ?? 1;
                        IPagedList<CatalogoViewModel> CatalogoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

                        var combinedViewModel = new CombinedCatalogo
                        {
                            CatalogoViewModel = CatalogoPagedList,
                            CatalogoSearchModel = CatalogSearchModel,
                            ResultCount = ListCatalogoModel.Count(),
                        };

                        if (Id != null)
                        {
                            if (checkPermission.Excluir == 1)
                            {
                                var catalogo = await _context.Catalogo.FindAsync(Id);
                                if (catalogo != null)
                                {
                                    catalogo.Ativo = 0;
                                }

                                await _context.SaveChangesAsync();

                                //var CatalogSearchModel = HttpContext.Session.GetObject<CatalogoSearchModel?>(SessionKeyCatalogoSearchModel) ?? new CatalogoSearchModel();

                                //CatalogoSearchModel? refreshModel = CatalogSearchModel;
                                //CombinedCatalogo? catalogoCombinedModel = RefreshSearchValues(refreshModel);

                                //HttpContext.Session.SetObject(SessionKeyCombinedModel, catalogoCombinedModel);

                                //CatalogoSearchModel? refreshModel = catalogoCombinedModel.CatalogoSearchModel != null ? catalogoCombinedModel.CatalogoSearchModel : new CatalogoSearchModel();
                                //catalogoCombinedModel = RefreshSearchValues(refreshModel);

                                CatalogoSearchModel? refreshModel = CatalogSearchModel;
                                CombinedCatalogo? catalogoCombinedModel = RefreshSearchValues(refreshModel);
                                catalogoCombinedModel.CatalogoSearchModel = CatalogSearchModel;
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyCombinedModel);
                                HttpContext.Session.SetObject(SessionKeyCombinedModel, catalogoCombinedModel);

                                ViewBag.ShowSuccessAlert = true;
                                return View(nameof(Index), catalogoCombinedModel ?? new CombinedCatalogo());
                            }
                            else
                            {
                                ViewBag.Error = "O usuário não tem permissão para excluir dados.";
                                return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
                            }
                        }
                        else
                        {
                            ViewBag.Error = "No Id selected";
                            return View(nameof(Index), combinedViewModel ?? new CombinedCatalogo());
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

        // GET: Catalogoes/Edit/5
        public IActionResult Edit(int? id)
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de Catalogo!";
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



                            if (id == null || _context.Catalogo == null)
                            {
                                log.LogWhy = "ID da Catalogo não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

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

                            ViewBag.FerramentariaItems = ferramentariaItems;



                            log.LogWhy = "Catalogo carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(catalogoViewModel);


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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, string? codigo, string? item, string? descricao, int? ferramentaria, int? restricao, int? impedir, int? epi, int? DataAutomatico,
        //   string? txtPos1Prateleira, string? txtPo2Prateleira, string? txtPos3Prateleira, string? txtPos4Prateleira, string? txtPos5Prateleira,
        //   string? txtPos1Coluna, string? txtPos2Coluna, string? txtPos3Coluna, string? txtPos4Coluna, string? txtPos5Coluna,
        //   string? txtPos1Linha, string? txtPos2Linha, string? txtPos3Linha, string? txtPos4Linha, string? txtPos5Linha,
        //   string? txtPos6Prateleira, string? txtPos7Prateleira, string? txtPos8Prateleira, string? txtPos9Prateleira, string? txtPos10Prateleira,
        //   string? txtPos6Coluna, string? txtPos7Coluna, string? txtPos8Coluna, string? txtPos9Coluna, string? txtPos10Coluna,
        //   string? txtPos6Linha, string? txtPos7Linha, string? txtPos8Linha, string? txtPos9Linha, string? txtPos10Linha)
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
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Editar != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de Editar a página de catalogo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        txtPos1Prateleira ??= "";
        //    txtPo2Prateleira ??= "";
        //    txtPos3Prateleira ??= "";
        //    txtPos4Prateleira ??= "";
        //    txtPos5Prateleira ??= "";
        //    txtPos1Coluna ??= "";
        //    txtPos2Coluna ??= "";
        //    txtPos3Coluna ??= "";
        //    txtPos4Coluna ??= "";
        //    txtPos5Coluna ??= "";
        //    txtPos1Linha ??= "";
        //    txtPos2Linha ??= "";
        //    txtPos3Linha ??= "";
        //    txtPos4Linha ??= "";
        //    txtPos5Linha ??= "";
        //    txtPos6Prateleira ??= "";
        //    txtPos7Prateleira ??= "";
        //    txtPos8Prateleira ??= "";
        //    txtPos9Prateleira ??= "";
        //    txtPos10Prateleira ??= "";
        //    txtPos6Coluna ??= "";
        //    txtPos7Coluna ??= "";
        //    txtPos8Coluna ??= "";
        //    txtPos9Coluna ??= "";
        //    txtPos10Coluna ??= "";
        //    txtPos6Linha ??= "";
        //    txtPos7Linha ??= "";
        //    txtPos8Linha ??= "";
        //    txtPos9Linha ??= "";
        //    txtPos10Linha ??= "";


        //    var catalogo = _context.Catalogo.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

        //    if (id != catalogo.Id)
        //    {
        //            usuario.Retorno = "ID da catalogo não localizada no banco de dados!";
        //            log.LogWhy = usuario.Retorno;
        //            ErrorViewModel erro = new ErrorViewModel();
        //            erro.Tela = log.LogWhere;
        //            erro.Descricao = log.LogWhy;
        //            erro.Mensagem = log.LogWhat;
        //            erro.IdLog = auxiliar.GravaLogRetornoErro(log);
        //            return NotFound();
        //    }

        //    ForCatalogoEdit CatalogoEdit = new ForCatalogoEdit();

        //    if (descricao == null)
        //    {
        //        descricao = "";
        //    }

        //    if (ferramentaria == null)
        //    {

        //    }

        //    if (restricao == null)
        //    {
        //        restricao = 0;
        //    }

        //    if (impedir == null)
        //    {
        //        impedir = 0;
        //    }

        //    if (epi == null)
        //    {
        //        epi = 0;
        //    }

        //    if (DataAutomatico == null)
        //    {
        //        DataAutomatico = 0;
        //    }


        //    CatalogoEdit.Id = catalogo.Id;
        //    CatalogoEdit.IdCategoria = catalogo.IdCategoria;
        //    CatalogoEdit.Codigo = codigo;
        //    CatalogoEdit.Nome = item;
        //    CatalogoEdit.Descricao = descricao;
        //    CatalogoEdit.PorMetro = catalogo.PorMetro;
        //    CatalogoEdit.PorAferido = catalogo.PorAferido;
        //    CatalogoEdit.PorSerial = catalogo.PorSerial;
        //    CatalogoEdit.RestricaoEmprestimo = restricao;
        //    CatalogoEdit.ImpedirDescarte = impedir;
        //    CatalogoEdit.HabilitarDescarteEPI = epi;
        //    CatalogoEdit.DataDeRetornoAutomatico = DataAutomatico;
        //    CatalogoEdit.Ativo = catalogo.Ativo;
        //    CatalogoEdit.DataRegistro = catalogo.DataRegistro;

        //    var mapper = mapeamentoClasses.CreateMapper();
        //    catalogo = mapper.Map<Catalogo>(CatalogoEdit);

        //    _context.Entry(catalogo).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //        usuario.Retorno = "Catalogo com sucesso";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);

        //    if (ferramentaria != 0)
        //    {
        //        // Check if a record with the specified IdCatalogo exists
        //        var existingRecord = _context.CatalogoLocal.FirstOrDefault(item => item.IdCatalogo == id && item.IdFerramentaria == ferramentaria);

        //        if (existingRecord != null)
        //        {
        //            // Update the existing record with the given data
        //            existingRecord.IdFerramentaria = ferramentaria;

        //            existingRecord.Pos1Prateleira = txtPos1Prateleira;
        //            existingRecord.Pos2Prateleira = txtPo2Prateleira;
        //            existingRecord.Pos3Prateleira = txtPos3Prateleira;
        //            existingRecord.Pos4Prateleira = txtPos4Prateleira;
        //            existingRecord.Pos5Prateleira = txtPos5Prateleira;
        //            existingRecord.Pos6Prateleira = txtPos6Prateleira;
        //            existingRecord.Pos7Prateleira = txtPos7Prateleira;
        //            existingRecord.Pos8Prateleira = txtPos8Prateleira;
        //            existingRecord.Pos9Prateleira = txtPos9Prateleira;
        //            existingRecord.Pos10Prateleira = txtPos10Prateleira;

        //            existingRecord.Pos1Coluna = txtPos1Coluna;
        //            existingRecord.Pos2Coluna = txtPos2Coluna;
        //            existingRecord.Pos3Coluna = txtPos3Coluna;
        //            existingRecord.Pos4Coluna = txtPos4Coluna;
        //            existingRecord.Pos5Coluna = txtPos5Coluna;
        //            existingRecord.Pos6Coluna = txtPos6Coluna;
        //            existingRecord.Pos7Coluna = txtPos7Coluna;
        //            existingRecord.Pos8Coluna = txtPos8Coluna;
        //            existingRecord.Pos9Coluna = txtPos9Coluna;
        //            existingRecord.Pos10Coluna = txtPos10Coluna;

        //            existingRecord.Pos1Linha = txtPos1Linha;
        //            existingRecord.Pos2Linha = txtPos2Linha;
        //            existingRecord.Pos3Linha = txtPos3Linha;
        //            existingRecord.Pos4Linha = txtPos4Linha;
        //            existingRecord.Pos5Linha = txtPos5Linha;
        //            existingRecord.Pos6Linha = txtPos6Linha;
        //            existingRecord.Pos7Linha = txtPos7Linha;
        //            existingRecord.Pos8Linha = txtPos8Linha;
        //            existingRecord.Pos9Linha = txtPos9Linha;
        //            existingRecord.Pos10Linha = txtPos10Linha;

        //            // Update the DataRegistro (assuming it's a field you want to update)
        //            existingRecord.DataRegistro = DateTime.Now;

        //            // Save the changes to the database
        //            await _context.SaveChangesAsync();

        //                usuario.Retorno = "Catalogo Coluna com sucesso";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogSucesso(log);
        //            }
        //        else
        //        {
        //            // Insert a new record with the given data
        //            var newRecord = new CatalogoLocal
        //            {
        //                IdCatalogo = id,
        //                IdFerramentaria = ferramentaria,

        //                Pos1Prateleira = txtPos1Prateleira,
        //                Pos2Prateleira = txtPo2Prateleira,
        //                Pos3Prateleira = txtPos3Prateleira,
        //                Pos4Prateleira = txtPos4Prateleira,
        //                Pos5Prateleira = txtPos5Prateleira,
        //                Pos6Prateleira = txtPos6Prateleira,
        //                Pos7Prateleira = txtPos7Prateleira,
        //                Pos8Prateleira = txtPos8Prateleira,
        //                Pos9Prateleira = txtPos9Prateleira,
        //                Pos10Prateleira = txtPos10Prateleira,

        //                Pos1Coluna = txtPos1Coluna,
        //                Pos2Coluna = txtPos2Coluna,
        //                Pos3Coluna = txtPos3Coluna,
        //                Pos4Coluna = txtPos4Coluna,
        //                Pos5Coluna = txtPos5Coluna,
        //                Pos6Coluna = txtPos6Coluna,
        //                Pos7Coluna = txtPos7Coluna,
        //                Pos8Coluna = txtPos8Coluna,
        //                Pos9Coluna = txtPos9Coluna,
        //                Pos10Coluna = txtPos10Coluna,

        //                Pos1Linha = txtPos1Linha,
        //                Pos2Linha = txtPos2Linha,
        //                Pos3Linha = txtPos3Linha,
        //                Pos4Linha = txtPos4Linha,
        //                Pos5Linha = txtPos5Linha,
        //                Pos6Linha = txtPos6Linha,
        //                Pos7Linha = txtPos7Linha,
        //                Pos8Linha = txtPos8Linha,
        //                Pos9Linha = txtPos9Linha,
        //                Pos10Linha = txtPos10Linha,

        //                DataRegistro = DateTime.Now
        //            };

        //            _context.Add(newRecord);
        //            await _context.SaveChangesAsync();

        //                usuario.Retorno = "Catalogo Inserir coluna com sucesso";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogSucesso(log);
        //            }
        //    }



        //    TempData["ShowSuccessAlert"] = true;

        //        usuario.Retorno = "catalogo Inserir com sucesso";
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
        //        return View(ex);
        //    }

        //}




        //public IActionResult Create()
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
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Inserir != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de Inserir a página de Catalogo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

        //        if (TempData.ContainsKey("ErrorMessage"))
        //        {
        //            ViewBag.Error = TempData["ErrorMessage"].ToString();
        //            TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
        //        }

        //        var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
        //                                  where ferramentaria.Ativo == 1 &&
        //                                        !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
        //                                        _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
        //                                  orderby ferramentaria.Nome
        //                                  select new
        //                                  {
        //                                      Id = ferramentaria.Id,
        //                                      Nome = ferramentaria.Nome
        //                                  }).ToList();

        //        ViewBag.FerramentariaItems = ferramentariaItems;

        //        usuario.Retorno = "Catalogo adicionada com sucesso";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);
        //        return View();

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

        //[HttpPost]
        //public async Task<IActionResult> Create(int? selectedCatalogo, int? selectedClasse, int? selectedTipo, int? PorMetro, int? PorAferido, int? PorSerial, int? PorQuantidade, string? codigo, string? item, string? descricao, int? restricaoEmprestimo, int? impedirDescarte, int? EPI, int? DataDeRetornoAutomatico)
        //{

        //    Log log = new Log();
        //    log.LogWhat = pagina + "/Index";
        //    log.LogWhere = pagina;
        //    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

        //    try
        //    {
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
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Inserir != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de Inserir a página de ferramentaria!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }


        //        if (selectedCatalogo == 0)
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "No selected catalogo";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao catalogo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction("Create");
        //        }

        //        if (selectedTipo == 0)
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "No selected Tipo";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao catalogo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction("Create");
        //        }

        //        if (codigo == null)
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Codigo is Empty";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao catalogo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction("Create");
        //        }

        //        if (item == null)
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Item is Empty";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao catalogo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction("Create");
        //        }

        //        //if (descricao == null)
        //        //{
        //        //    TempData["ShowErrorAlert"] = true;
        //        //    TempData["ErrorMessage"] = "Descricao is Empty";
        //        //    return RedirectToAction("Create");
        //        //}

        //        int metro;
        //        int aferido;
        //        int serial;
        //        int quantidade;

        //        int? restricao;
        //        int? impedir;
        //        int? epi;
        //        int? retorno;



        //        if (PorMetro == 1)
        //        {
        //            metro = 1;
        //            aferido = 0;
        //            serial = 0;
        //            quantidade = 0;
        //        }
        //        else if (PorAferido == 1)
        //        {
        //            metro = 0;
        //            aferido = 1;
        //            serial = 0;
        //            quantidade = 0;
        //        }
        //        else if (PorSerial == 1)
        //        {
        //            metro = 0;
        //            aferido = 0;
        //            serial = 1;
        //            quantidade = 0;
        //        }
        //        else
        //        {
        //            metro = 0;
        //            aferido = 0;
        //            serial = 0;
        //            quantidade = 1;
        //        }

        //        if (restricaoEmprestimo == null)
        //        {
        //            restricaoEmprestimo = 0;
        //        }

        //        if (impedirDescarte == null)
        //        {
        //            impedirDescarte = 0;
        //        }

        //        if (EPI == null)
        //        {
        //            EPI = 0;
        //        }

        //        if (DataDeRetornoAutomatico == null)
        //        {
        //            DataDeRetornoAutomatico = 0;
        //        }

        //        if (descricao == null)
        //        {
        //            descricao = "";
        //        }


        //        // Assuming you have a DbContext or DataContext called "dbContext"
        //        var duplicates = _context.Catalogo
        //            .Where(c => c.Ativo == 1 && (c.Nome == item || c.Codigo == codigo))
        //            .ToList();


        //        if (duplicates.Any())
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Duplicate";

        //            usuario.Retorno = "Duplicate Catalogo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction("Create");
        //        }

        //        var InsertCatalogo = new Catalogo
        //        {
        //            IdCategoria = selectedTipo,
        //            Codigo = codigo,
        //            Nome = item,
        //            Descricao = descricao,
        //            PorMetro = metro,
        //            PorAferido = aferido,
        //            PorSerial = serial,
        //            RestricaoEmprestimo = restricaoEmprestimo,
        //            ImpedirDescarte = impedirDescarte,
        //            HabilitarDescarteEPI = EPI,
        //            DataDeRetornoAutomatico = DataDeRetornoAutomatico,
        //            DataRegistro = DateTime.Now,
        //            Ativo = 1

        //        };

        //        _context.Add(InsertCatalogo);
        //        await _context.SaveChangesAsync();

        //        TempData["ShowSuccessAlert"] = true;

        //        usuario.Retorno = "Catalogo adicionada com sucesso";
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

        //[HttpPost]
        //public async Task<IActionResult> DeleteConfirmed(int id)
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
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Excluir != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de Excluir a página de Catalogo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        var catalogo = await _context.Catalogo.FindAsync(id);
        //        if (catalogo != null)
        //        {
        //            catalogo.Ativo = 0;
        //        }

        //        await _context.SaveChangesAsync();

        //        //return View();

        //        //return Json(new { success = true, message = "Item deleted successfully" });

        //        usuario.Retorno = "Catalogo deativar com sucesso!";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);
        //        TempData["ShowSuccessAlert"] = true;
        //        return RedirectToAction(nameof(Index));

        //        //return View(id);
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



        //[HttpPost]
        public ActionResult LoadClasse(int selectedValue)
        {
            //var query = _context.Categoria
            // .Where(entity => entity.IdCategoria == selectedValue)
            // .ToList();

            var query = _context.Categoria
             .Where(entity => entity.Classe == selectedValue && entity.IdCategoria == 0)
             .OrderBy(entity => entity.Nome)
             .ToList();

            return Json(query);
        }

        public ActionResult LoadTipo(int selectedValue)
        {
            var query = _context.Categoria
             .Where(entity => entity.IdCategoria == selectedValue && entity.IdCategoria != 0 )
             .OrderBy(entity => entity.Nome)
             .ToList();

            return Json(query);
        }



    }
}
