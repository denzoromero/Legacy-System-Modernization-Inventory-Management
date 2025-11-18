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
using System.Globalization;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class ControleCAController : Controller
    {
        
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thControleCA.aspx";
        private MapperConfiguration mapeamentoClasses; 
        //private static int? GlobalPagination;

        private const string SessionKeyListControleCA = "ListControleCA";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static List<CatalogoViewModel> _ListControleCA = new List<CatalogoViewModel>();
        //private static IPagedList<CatalogoViewModel> StaticCatalogoPagedList = null;

        public ControleCAController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ControleCA, ControleCAViewModel>();
                cfg.CreateMap<ControleCAViewModel, ControleCA>();
            });
        }

        // GET: ControleCA
        public IActionResult Index(int? page, string? CodigoCA, string? ItemCA, string? NumeroCA)
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
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de ControleCA!";
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


                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

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


          

                //var query = from catalogo in _context.Catalogo
                //            join categoria in _context.Categoria
                //            on catalogo.IdCategoria equals categoria.Id
                //            where catalogo.Ativo == 1
                //            orderby catalogo.Nome
                //            select new
                //            {
                //                IdCatalogo = catalogo.Id,
                //                CodigoCatalogo = catalogo.Codigo,
                //                NomeCatalogo = catalogo.Nome,
                //                DescricaoCatalogo = catalogo.Descricao,
                //                PorMetroCatalogo = catalogo.PorMetro,
                //                PorAferidoCatalogo = catalogo.PorAferido,
                //                PorSerialCatalogo = catalogo.PorSerial,
                //                RestricaoEmprestimoCatalogo = catalogo.RestricaoEmprestimo,
                //                ImpedirDescarteCatalogo = catalogo.ImpedirDescarte,
                //                HabilitarDescarteEpiCatalogo = catalogo.HabilitarDescarteEPI,
                //                DataDeRetornoAutomaticoCatalogo = catalogo.DataDeRetornoAutomatico,
                //                DataRegistroCatalogo = catalogo.DataRegistro,
                //                AtivoCatalogo = catalogo.Ativo,
                //                IdCategoriaCatalogo = categoria.Id,
                //                IdCategoriaPaiCatalogo = categoria.IdCategoria,
                //                CategoriaNomeCatalogo = categoria.Nome,
                //                CategoriaClasseCatalogo = categoria.Classe,
                //                CategoriaNomePaiCatalogo = _context.Categoria
                //                    .Where(cat => cat.Id == categoria.IdCategoria)
                //                    .Select(cat => cat.Nome)
                //                    .FirstOrDefault(),
                //                DataRegistroCategoria = categoria.DataRegistro,
                //                AtivoCategoria = categoria.Ativo
                //            };

                //var result = query.ToList();

                //var catalogoViewModels = result.Select(item => new CatalogoViewModel
                //{
                //    Id = item.IdCatalogo,
                //    Codigo = item.CodigoCatalogo,
                //    Nome = item.NomeCatalogo,
                //    Descricao = item.DescricaoCatalogo,
                //    PorMetro = item.PorMetroCatalogo,
                //    PorAferido = item.PorAferidoCatalogo,
                //    PorSerial = item.PorSerialCatalogo,
                //    RestricaoEmprestimo = item.RestricaoEmprestimoCatalogo,
                //    ImpedirDescarte = item.ImpedirDescarteCatalogo,
                //    HabilitarDescarteEpi = item.HabilitarDescarteEpiCatalogo,
                //    DataDeRetornoAutomatico = item.DataDeRetornoAutomaticoCatalogo,
                //    DataRegistro = item.DataRegistroCatalogo,
                //    Ativo = item.AtivoCatalogo,
                //    IdCategoria = item.IdCategoriaCatalogo,
                //    IdCategoriaPai = item.IdCategoriaPaiCatalogo,
                //    CategoriaNome = item.CategoriaNomeCatalogo,
                //    CategoriaNomePai = item.CategoriaNomePaiCatalogo,
                //    CategoriaClasse = item.CategoriaClasseCatalogo,
                //    CategoriaDataRegistro = item.DataRegistroCategoria,
                //    CategoriaAtivo = item.AtivoCategoria
                //    // ... Map other properties here
                //}).ToList();

                //if (!string.IsNullOrEmpty(CodigoCA))
                //{
                //    catalogoViewModels = catalogoViewModels.Where(c => c.Codigo == CodigoCA).ToList();
                //}

                //if (!string.IsNullOrEmpty(ItemCA))
                //{
                //    catalogoViewModels = catalogoViewModels.Where(c => c.Nome.Contains(ItemCA)).ToList();
                //}

                //if (!string.IsNullOrEmpty(NumeroCA))
                //{
                //    catalogoViewModels = catalogoViewModels
                //                        .Where(cvm => cvm.Ativo == 1) // Filter by Catalogo.Ativo = 1
                //                        .Where(cvm => _context.ControleCA.Any(cc => cc.IdCatalogo == cvm.Id && cc.NumeroCA.Contains(NumeroCA))) // Filter by ControleCA condition
                //                        .OrderBy(cvm => cvm.Nome)
                //                        .ToList(); // Execute the query and convert the result to a list
                //}



                //GlobalPagination = 20;
                //int pageSize = 20; // Set your desired page size
                //int pageNumber = 1;

                //var mapper = mapeamentoClasses.CreateMapper();

                //_ListControleCA = mapper.Map<List<CatalogoViewModel>>(catalogoViewModels); ;
                //IPagedList<CatalogoViewModel> CatalogoPagedList = catalogoViewModels.ToPagedList(pageNumber, pageSize);

             

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

        public ActionResult GetControleCA(string? CodigoCA, string? ItemCA, string? NumeroCA)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<CatalogoViewModel>? Result = searches.SearchControleCA(CodigoCA, ItemCA, NumeroCA);
            if (Result.Count > 0)
            {

                //GlobalPagination = 20;
                int pageSize = 20; // Set your desired page size
                int pageNumber = 1;

                var mapper = mapeamentoClasses.CreateMapper();

                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListControleCA);
                HttpContext.Session.SetObject(SessionKeyListControleCA, Result);

                //_ListControleCA = mapper.Map<List<CatalogoViewModel>>(Result); ;
                IPagedList<CatalogoViewModel> CatalogoPagedList = Result.ToPagedList(pageNumber, pageSize);
                //StaticCatalogoPagedList = CatalogoPagedList;
                return View(nameof(Index), CatalogoPagedList);
            }
            else
            {
                ViewBag.Error = "Nenhum resultado encontrado";
                return View(nameof(Index));
            }         
        }

        public ActionResult TestPage(int? page)
        {
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var ListControleCA = HttpContext.Session.GetObject<List<CatalogoViewModel?>>(SessionKeyListControleCA) ?? new List<CatalogoViewModel?>();

            IPagedList<CatalogoViewModel> ControleCAPagedList = ListControleCA.ToPagedList(pageNumber, pageSize);

            return View("Index", ControleCAPagedList);
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

        // GET: ControleCA/Details/5
        public IActionResult Details(int? id)
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


                            var controlCA = _context.ControleCA
                                       .Where(item => item.IdCatalogo == id && item.Ativo == 1)
                                       .ToList();

                            ViewBag.ControlCA = controlCA;

                            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            log.LogWhy = "Acesso Permitido";
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
                return View();
            }
        }


        // POST: ControleCA/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int? id, string? numeroCA, DateTime? validade)
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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de ControleCA!";
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



                            if (numeroCA == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Numero da C.A. is Empty";

                                log.LogWhy = "Erro na validação do modelo em criaçao ControleCA!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Details", new { id = id });
                            }

                            if (validade == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Validade is Empty";

                                log.LogWhy = "Erro na validação do modelo em criaçao ControleCA!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Details", new { id = id });
                            }

                            if (id == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Id is Empty";

                                log.LogWhy = "Erro na validação do modelo em criaçao ControleCA!";
                                auxiliar.GravaLogAlerta(log);

                                return RedirectToAction("Index");
                            }

                            var InsertControleCA = new ControleCA
                            {
                                IdCatalogo = id,
                                NumeroCA = numeroCA,
                                Validade = validade,
                                Responsavel = loggedUser.Email,
                                DataRegistro = DateTime.Now,
                                Ativo = 1
                            };

                            _context.Add(InsertControleCA);
                            _context.SaveChanges();

                            TempData["ShowSuccessAlert"] = true;

                            log.LogWhy = "ControleCA adicionada com sucesso";
                            auxiliar.GravaLogSucesso(log);

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
                ViewBag.Error = ex.Message;
                return View();
            }
         
        }

        // GET: ControleCA/Edit/5
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de ControleCA!";
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


                            var result = _context.ControleCA.FirstOrDefault(item => item.Id == id);

                            var catalogoViewModel = (from c in _context.Catalogo
                                                     join categoria in _context.Categoria on c.IdCategoria equals categoria.Id
                                                     join pc in _context.Categoria on categoria.IdCategoria equals pc.Id into parentCategory
                                                     from pc in parentCategory.DefaultIfEmpty()
                                                     where c.Ativo == 1 && c.Id == result.IdCatalogo
                                                     orderby c.Nome
                                                     select new CatalogoViewModel
                                                     {
                                                         Id = c.Id,
                                                         Codigo = c.Codigo,
                                                         Nome = c.Nome,
                                                         Descricao = c.Descricao,
                                                         PorMetro = c.PorMetro,
                                                         PorAferido = c.PorAferido,
                                                         PorSerial = c.PorSerial,
                                                         RestricaoEmprestimo = c.RestricaoEmprestimo,
                                                         ImpedirDescarte = c.ImpedirDescarte,
                                                         HabilitarDescarteEpi = c.HabilitarDescarteEPI,
                                                         DataDeRetornoAutomatico = c.DataDeRetornoAutomatico,
                                                         DataRegistro = c.DataRegistro,
                                                         Ativo = c.HabilitarDescarteEPI,
                                                         IdCategoria = categoria.Id,
                                                         IdCategoriaPai = categoria.IdCategoria,
                                                         CategoriaNome = categoria.Nome,
                                                         CategoriaClasse = categoria.Classe,
                                                         CategoriaNomePai = pc.Nome,
                                                         CategoriaDataRegistro = categoria.DataRegistro,
                                                         CategoriaAtivo = categoria.Ativo
                                                     }).FirstOrDefault();


                            var controlCA = _context.ControleCA
                                       .Where(item => item.IdCatalogo == catalogoViewModel.Id && item.Ativo == 1)
                                       .ToList();

                            ViewBag.ControlCA = controlCA;

                            ControleCA? valueTobeEdited = controlCA.FirstOrDefault(i => i.Id == id);

                            ViewBag.EditValue = valueTobeEdited;

                            ViewBag.EditId = result.Id;
                            ViewBag.EditNumeroCA = result.NumeroCA;
                            ViewBag.EditValidade = result.Validade;

                            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];
                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

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

                            log.LogWhy = "ControleCA carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(nameof(Details), catalogoViewModel);



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
                return View(nameof(Details));
            }
        }

        // POST: ControleCA/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAction(int Id, string? numeroCA, DateTime? validade)
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de ControleCA!";
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


                            ControleCA? result = _context.ControleCA.FirstOrDefault(item => item.Id == Id);

                            CatalogoViewModel? catalogoViewModel = (from c in _context.Catalogo
                                                                    join categoria in _context.Categoria on c.IdCategoria equals categoria.Id
                                                                    join pc in _context.Categoria on categoria.IdCategoria equals pc.Id into parentCategory
                                                                    from pc in parentCategory.DefaultIfEmpty()
                                                                    where c.Ativo == 1 && c.Id == result.IdCatalogo
                                                                    orderby c.Nome
                                                                    select new CatalogoViewModel
                                                                    {
                                                                        Id = c.Id,
                                                                        Codigo = c.Codigo,
                                                                        Nome = c.Nome,
                                                                        Descricao = c.Descricao,
                                                                        PorMetro = c.PorMetro,
                                                                        PorAferido = c.PorAferido,
                                                                        PorSerial = c.PorSerial,
                                                                        RestricaoEmprestimo = c.RestricaoEmprestimo,
                                                                        ImpedirDescarte = c.ImpedirDescarte,
                                                                        HabilitarDescarteEpi = c.HabilitarDescarteEPI,
                                                                        DataDeRetornoAutomatico = c.DataDeRetornoAutomatico,
                                                                        DataRegistro = c.DataRegistro,
                                                                        Ativo = c.HabilitarDescarteEPI,
                                                                        IdCategoria = categoria.Id,
                                                                        IdCategoriaPai = categoria.IdCategoria,
                                                                        CategoriaNome = categoria.Nome,
                                                                        CategoriaClasse = categoria.Classe,
                                                                        CategoriaNomePai = pc.Nome,
                                                                        CategoriaDataRegistro = categoria.DataRegistro,
                                                                        CategoriaAtivo = categoria.Ativo
                                                                    }).FirstOrDefault();

                            List<ControleCA>? controlCA = _context.ControleCA
                                                         .Where(item => item.IdCatalogo == catalogoViewModel.Id && item.Ativo == 1)
                                                         .ToList();

                            //ControleCA? valueTobeEdited = controlCA.FirstOrDefault(i => i.Id == Id);



                            string? error = validateinputs(numeroCA, validade);
                            if (string.IsNullOrEmpty(error))
                            {
                                if (result != null)
                                {
                                    result.NumeroCA = result.NumeroCA != numeroCA ? numeroCA : result.NumeroCA;
                                    result.Validade = result.Validade != validade ? validade : result.Validade;
                                    result.DataRegistro = DateTime.Now;
                                    result.Responsavel = loggedUser.Email;

                                    _context.SaveChanges();

                                    List<ControleCA>? controlCANew = _context.ControleCA
                                                    .Where(item => item.IdCatalogo == catalogoViewModel.Id && item.Ativo == 1)
                                                    .ToList();

                                    ViewBag.ControlCA = controlCANew;
                                    ViewBag.ShowSuccessAlert = true;
                                    return View(nameof(Details), catalogoViewModel);
                                }
                                else
                                {
                                    //ViewBag.EditValue = result;
                                    ViewBag.ControlCA = controlCA;
                                    //ViewBag.EditId = result.Id;
                                    //ViewBag.EditNumeroCA = result.NumeroCA;
                                    //ViewBag.EditValidade = result.Validade;

                                    ViewBag.Error = "No Data to be edited.";
                                    return View(nameof(Details), catalogoViewModel);
                                }


                            }
                            else
                            {
                                ViewBag.EditValue = result;
                                ViewBag.ControlCA = controlCA;
                                ViewBag.EditId = result.Id;
                                ViewBag.EditNumeroCA = result.NumeroCA;
                                ViewBag.EditValidade = result.Validade;

                                ViewBag.ErrorCA = error;
                                return View(nameof(Details), catalogoViewModel);
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

             



                //var EditControlaCA = _context.ControleCA.AsNoTracking().Where(x => x.Id == EditId).FirstOrDefault();

                //if (EditId != EditControlaCA.Id)
                //{
                //    usuario.Retorno = "ID da pergunta não localizada no banco de dados!";
                //    log.LogWhy = usuario.Retorno;
                //    ErrorViewModel erro = new ErrorViewModel();
                //    erro.Tela = log.LogWhere;
                //    erro.Descricao = log.LogWhy;
                //    erro.Mensagem = log.LogWhat;
                //    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                //    return NotFound();
                //}

                //ControleCAViewModel ControleCAedit = new ControleCAViewModel();

                //ControleCAedit.Id = EditControlaCA.Id;
                //ControleCAedit.IdCatalogo = EditControlaCA.IdCatalogo;
                //ControleCAedit.NumeroCA = EditNumeroCA;
                //ControleCAedit.Validade = EditValidade;
                //ControleCAedit.Responsavel = usuario.SAMAccountName;
                //ControleCAedit.DataRegistro = EditControlaCA.DataRegistro;
                //ControleCAedit.Ativo = EditControlaCA.Ativo;

                //var mapper = mapeamentoClasses.CreateMapper();
                //EditControlaCA = mapper.Map<ControleCA>(ControleCAedit);

                //_context.Entry(EditControlaCA).State = EntityState.Modified;
                //await _context.SaveChangesAsync();

                //TempData["ShowSuccessAlert"] = true;

                //usuario.Retorno = "ControleCA carregada com sucesso!";
                //log.LogWhy = usuario.Retorno;
                //auxiliar.GravaLogSucesso(log);

                //return RedirectToAction(nameof(Edit), new { id = EditId });
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
                return View(ex);
            }      
         
        }

        private string validateinputs(string? NumeroCA, DateTime? Validade)
        {
            if (string.IsNullOrEmpty(NumeroCA))
            {            
                return "NumeroCA esta vazio.";
            }

            if (Validade.HasValue == false)
            {
                return "Validade esta vazio.";
            }

            return string.Empty;
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
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
                //    if (usuario.Permissao.Excluir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Excluir a página de ControleCA!";
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
                        if (checkPermission.Excluir == 1)
                        {


                            var controleCA = _context.ControleCA.SingleOrDefault(item => item.Id == id);

                            if (controleCA != null)
                            {
                                controleCA.Ativo = 0;
                            }

                            _context.SaveChanges();


                            log.LogWhy = "Ferramentaria deativar com sucesso!";
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
                ViewBag.Error = ex.Message;
                return View("Index");
            }
        }

    }
}
