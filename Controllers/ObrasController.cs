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
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using X.PagedList;
using FerramentariaTest.EntitiesBS;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FerramentariaTest.Controllers
{
    public class ObrasController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thObra.aspx";
        private MapperConfiguration mapeamentoClasses;
        private static int? GlobalPagination;
        private static int? GlobalPageNumber;

        private const string SessionKeyObraListObra = "ObraListObra";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";


        public ObrasController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Obra, ObraViewModel>();
                cfg.CreateMap<ObraViewModel, Obra>();
            });
        }

        // GET: Obras
        public IActionResult Index(int? page)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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



                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                ////usuario.Pagina1 = "Generico.aspx?ToolHouse.Obra*Codigo=40.10.s";
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
                //#endregion

                //UsuarioData = usuario;

            
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

        public ActionResult SearchObra(string? filter, int? status, int? pagination)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchObra";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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
                            ObraViewModelSearch obraViewModelSearch = new ObraViewModelSearch();

                            var obras = _context.Obra.Where(i =>
                                        (status == null || i.Ativo == status)
                                        && (filter == null || i.Nome.Contains(filter) || i.Codigo.Contains(filter))
                                         ).ToList();


                            List<ReservationObraModel> obraWithReservation = (from obra in _context.Obra
                                                                                                join excludedObra in _context.ExcludedObra
                                                                                                on obra.Id equals excludedObra.IdObra into virtualGroup
                                                                                                from excludedItem in virtualGroup.DefaultIfEmpty()
                                                                                                where (status == null || obra.Ativo == status)
                                                                                                      && (filter == null || obra.Nome.Contains(filter) || obra.Codigo.Contains(filter))
                                                                                                select new ReservationObraModel
                                                                                                {
                                                                                                    Id = obra.Id,
                                                                                                    Nome = obra.Nome,
                                                                                                    Codigo = obra.Codigo,
                                                                                                    DataRegistro = obra.DataRegistro,
                                                                                                    Ativo = obra.Ativo,
                                                                                                    IdExcluded = excludedItem.Id,
                                                                                                }).ToList() ?? new List<ReservationObraModel>();

                            if (obras.Any())
                            {
                                //var mapper = mapeamentoClasses.CreateMapper();
                                //List<ObraViewModel> ObraResult = mapper.Map<List<ObraViewModel>>(obras);

                                //httpContextAccessor.HttpContext?.Session.Remove(SessionKeyObraListObra);
                                //HttpContext.Session.SetObject(SessionKeyObraListObra, ObraResult);

                                ////_ListObra = ObraResult;
                                ////GlobalValues.ListObraViewModel = ObraResult;

                                //GlobalPagination = pagination;
                                //int pageSize = GlobalPagination ?? 10;
                                //int pageNumber = 1;
                                //GlobalPageNumber = pageNumber;
                                //IPagedList<ObraViewModel> ObraPagedList = ObraResult.ToPagedList(pageNumber, pageSize);

                                //obraViewModelSearch.ObraPagedList = ObraPagedList;
                                //obraViewModelSearch.Status = status;
                                //return View("Index", obraViewModelSearch);


                                NewObraModel? newModel = new NewObraModel()
                                {
                                    ReservationObraModel = obraWithReservation,
                                    ObraFilter = filter,
                                    Ativo = status,
                                    Pagination = pagination,
                                    PageNumber = 1,
                                };

                                return View("Index", newModel);


                            }
                            else
                            {
                                ViewBag.Error = "No Searched Obra has been found.";
                                return View("Index");
                            }
                        }
                        else
                        {
                            return RedirectToAction("PreserveActionError", "Home", $"No Permission for Page:{pagina}");
                        }
                    }
                    else
                    {
                        log.LogWhy = "Permission is Empty";
                        return RedirectToAction("PreserveActionError", "Home", "Permission is Empty");
                    }
                }
                else
                {
                    log.LogWhy = "Session Expired";
                    return RedirectToAction("PreserveActionError", "Home", "Session Expired");
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

        public ActionResult TestPage(int? page, int? pageSize, string filter, int? ativo)
        {
            //ObraViewModelSearch obraViewModelSearch = new ObraViewModelSearch();

            //int pageSize = GlobalPagination ?? 10;
            //int pageNumber = (page ?? 1);
            //GlobalPageNumber = pageNumber;

            //var ObraListModel = HttpContext.Session.GetObject<List<ObraViewModel?>>(SessionKeyObraListObra) ?? new List<ObraViewModel?>();
            //IPagedList<ObraViewModel> ObraPagedList = ObraListModel.ToPagedList(pageNumber, pageSize);
            //obraViewModelSearch.ObraPagedList = ObraPagedList;

            pageSize = pageSize ?? 10;
            int pageNumber = (page ?? 1);

            List<ReservationObraModel> obraWithReservation = (from obra in _context.Obra
                                                              join excludedObra in _context.ExcludedObra
                                                              on obra.Id equals excludedObra.IdObra into virtualGroup
                                                              from excludedItem in virtualGroup.DefaultIfEmpty()
                                                              where (ativo == null || obra.Ativo == ativo)
                                                                    && (filter == null || obra.Nome.Contains(filter) || obra.Codigo.Contains(filter))
                                                              select new ReservationObraModel
                                                              {
                                                                  Id = obra.Id,
                                                                  Nome = obra.Nome,
                                                                  Codigo = obra.Codigo,
                                                                  DataRegistro = obra.DataRegistro,
                                                                  Ativo = obra.Ativo,
                                                                  IdExcluded = excludedItem.Id,
                                                              }).ToList() ?? new List<ReservationObraModel>();

            NewObraModel? newModel = new NewObraModel()
            {
                ReservationObraModel = obraWithReservation,
                ObraFilter = filter,
                Ativo = ativo,
                Pagination = pageSize,
                PageNumber = pageNumber,
            };

            return View("Index", newModel);

            //return View(nameof(Index), obraViewModelSearch);
        }

        public IActionResult AddToExcludedReservation(int id)
        {
            ExcludedObra? checkduplicate = _context.ExcludedObra.FirstOrDefault(i => i.IdObra == id);

            if (checkduplicate == null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        ExcludedObra entity = new ExcludedObra()
                        {
                            IdObra = id,
                        };

                        _context.ExcludedObra.Add(entity);
                        _context.SaveChanges();
                        transaction.Commit();

                        TempData["ShowSuccessAlert"] = true;
                        return RedirectToAction(nameof(Index));

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ViewBag.ErrorMessage = ex.Message;
                        return View(nameof(Index));
                    }
                }
            }
            else
            {
                ViewBag.Error = "Already in Excluded Obra";
                return View(nameof(Index));
            }
        }

        public IActionResult RemoveToExcludedReservation(int id)
        {
            ExcludedObra? checkduplicate = _context.ExcludedObra.FirstOrDefault(i => i.IdObra == id);

            if (checkduplicate != null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {

                        _context.ExcludedObra.Remove(checkduplicate);
                        _context.SaveChanges();
                        transaction.Commit();

                        TempData["ShowSuccessAlert"] = true;
                        return RedirectToAction(nameof(Index));

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ViewBag.ErrorMessage = ex.Message;
                        return View(nameof(Index));
                    }
                }
            }
            else
            {
                ViewBag.Error = "Not registered in Excluded Obra";
                return View(nameof(Index));
            }
        }




        // GET: Obras/Create
        public IActionResult Create()
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de ferramentaria!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //}

                //usuario.Retorno = "Acesso Permitido";
                //log.LogWhy = usuario.Retorno;
                //auxiliar.GravaLogSucesso(log);

                //return View();
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

        // POST: Obras/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ObraViewModelSearch obraViewModel)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Create";
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
                        if (checkPermission.Inserir == 1)
                        {
                            List<ObraViewModel>? ObraListModel = HttpContext.Session.GetObject<List<ObraViewModel>>(SessionKeyObraListObra) ?? new List<ObraViewModel>();

                            obraViewModel.ObraPagedList = ObraListModel.Count > 0 ? ObraListModel.ToPagedList(GlobalPageNumber ?? 1, GlobalPagination ?? 10) : obraViewModel.ObraPagedList;

                            if (ModelState.IsValid)
                            {
                                bool isDuplicate = _context.Obra.Any(m => m.Nome == obraViewModel.Nome && m.Codigo == obraViewModel.Codigo);

                                if (!isDuplicate)
                                {
                                    var obra = new Obra
                                    {
                                        Codigo = obraViewModel.Codigo,
                                        Nome = obraViewModel.Nome,
                                        Ativo = 1,
                                        DataRegistro = DateTime.Now,
                                    };

                                    _context.Add(obra);
                                    await _context.SaveChangesAsync();

                                    ViewBag.ShowSuccessAlert = true;

                                    log.LogWhy = $"Obra adicionada com sucesso id: {obra.Id}";
                                    auxiliar.GravaLogSucesso(log);

                                    return View(nameof(Index));
                                }
                                else
                                {
                                    //ViewBag.ShowAlertWarning = true;
                                    ViewBag.Error = "Duplicate";                                    
                                    log.LogWhy = "Duplicate!";
                                    auxiliar.GravaLogAlerta(log);
                                    return View(nameof(Index), obraViewModel);
                                }

                            }
                            else
                            {
                                ViewBag.OpenForError = true;                        
                                log.LogWhy = "Erro na validação do modelo em criaçao Obra!";
                                auxiliar.GravaLogAlerta(log);
                                return View(nameof(Index), obraViewModel);
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

        // GET: Obras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Edit";
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de Obra!";
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
                        if (checkPermission.Editar == 1)
                        {
                            if (id == null || _context.Obra == null)
                            {
                                log.LogWhy = "ID da Obra não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var obra = await _context.Obra.FindAsync(id);
                            if (obra == null)
                            {
                                log.LogWhy = "ID da Obra não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var mapper = mapeamentoClasses.CreateMapper();

                            ObraViewModel obraViewModel = mapper.Map<ObraViewModel>(obra);

                            return View(obraViewModel);
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

        // POST: Obras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nome,DataRegistro,Ativo")] ObraViewModel obraViewModel)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Edit";
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de obra!";
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
                        if (checkPermission.Editar == 1)
                        {
                            if (id != obraViewModel.Id)
                            {                               
                                log.LogWhy = "ID da Obra não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            if (ModelState.IsValid)
                            {

                                var obra = _context.Obra.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

                                obraViewModel.Id = obra.Id;
                                obraViewModel.Ativo = obra.Ativo;
                                obraViewModel.DataRegistro = obra.DataRegistro;

                                bool isDuplicate = _context.Obra.Any(m => m.Nome == obraViewModel.Nome && m.Codigo == obraViewModel.Codigo);

                                if (isDuplicate)
                                {
                                    ViewBag.Error = "Duplicate";
                                    log.LogWhy = "Duplicate!";
                                    auxiliar.GravaLogAlerta(log);
                                    return View(obraViewModel);
                                }

                                var mapper = mapeamentoClasses.CreateMapper();
                                obra = mapper.Map<Obra>(obraViewModel);

                                _context.Entry(obra).State = EntityState.Modified;
                                await _context.SaveChangesAsync();

                                //try
                                //{
                                //    _context.Update(obraViewModel);
                                //    await _context.SaveChangesAsync();
                                //}
                                //catch (DbUpdateConcurrencyException)
                                //{
                                //    if (!ObraExists(obraViewModel.Id))
                                //    {
                                //        return NotFound();
                                //    }
                                //    else
                                //    {
                                //        throw;
                                //    }
                                //}

                                TempData["ShowSuccessAlert"] = true;

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyObraListObra);
                                //_ListObra.Clear();
                                //GlobalValues.ClearList(GlobalValues.ListObraViewModel);

                                log.LogWhy = $"Obra editar com sucesso id{id}";
                                auxiliar.GravaLogSucesso(log);

                                return RedirectToAction(nameof(Index));
                            }
                            return View(obraViewModel);
                        }
                        else
                        {
                            return RedirectToAction("PreserveActionError", "Home", $"No Permission for Page:{pagina}");
                        }
                    }
                    else
                    {
                        log.LogWhy = "Permission is Empty";
                        return RedirectToAction("PreserveActionError", "Home", "Permission is Empty");
                    }
                }
                else
                {
                    log.LogWhy = "Session Expired";
                    return RedirectToAction("PreserveActionError", "Home", "Session Expired");
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
                return View(obraViewModel);
            }


          
        }

        // GET: Obras/Delete/5
        public async Task<IActionResult> Delete(int? id)
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
                //    return RedirectToAction("Login", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Excluir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Excluir a página de obra!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("Login", "Home", usuario);
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


                            if (id == null || _context.Obra == null)
                            {
                                log.LogWhy = "ID da obra não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var obra = await _context.Obra
                                .FirstOrDefaultAsync(m => m.Id == id);
                            if (obra == null)
                            {
                                log.LogWhy = "ID da obra não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var mapper = mapeamentoClasses.CreateMapper();

                            ObraViewModel obraViewModel = mapper.Map<ObraViewModel>(obra);

                            log.LogWhy = "Obra carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(obraViewModel);


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

        // POST: Obras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Delete";
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
                //    return RedirectToAction("Login", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Excluir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Excluir a página de obra!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("Login", "Home", usuario);
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


                            if (_context.Obra == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return Problem("Entity set 'ContextoBanco.Obra'  is null.");
                            }
                            var obra = await _context.Obra.FindAsync(id);
                            if (obra != null)
                            {
                                obra.Ativo = 0;
                            }

                            await _context.SaveChangesAsync();

                            TempData["ShowSuccessAlert"] = true;
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyObraListObra);
                            //_ListObra.Clear();
                            //GlobalValues.ClearList(GlobalValues.ListObraViewModel);

                            log.LogWhy = $"Obra deativar com sucesso! id {id}";
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

        private bool ObraExists(int id)
        {
          return (_context.Obra?.Any(e => e.Id == id)).GetValueOrDefault();
        }


    }
}
