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
using System.Drawing.Printing;

namespace FerramentariaTest.Controllers
{
    public class FerramentariasController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "Generico.aspx?ToolHouse.Ferramentaria";
        private MapperConfiguration mapeamentoClasses;
        private static int? GlobalPagination;
        private static List<FerramentariaViewModel> _ListFerramentaria = new List<FerramentariaViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public FerramentariasController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Ferramentaria, FerramentariaViewModel>();
                cfg.CreateMap<FerramentariaViewModel, Ferramentaria>();
            });
        }

        // GET: Ferramentarias
        public IActionResult Index(int? page)
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

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
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

        public ActionResult SearchFerramentaria(string? filter, int? status, int? pagination)
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
                //usuario.Pagina1 = "Generico.aspx?ToolHouse.Obra*Codigo=40.10.s";
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

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {



                            //var ferramentarias = _context.Ferramentaria.Where(i =>
                            //                     (status == null || i.Ativo == status)
                            //                     && (filter == null || i.Nome.Contains(filter))
                            //                     ).ToList();

                            List<WithReservationFerramentariaModel>  ferramentariaWithVirtual = (from ferramentaria in _context.Ferramentaria
                                                                                                 join virtualFerramentaria in _context.VirtualFerrmantaria
                                                                                                 on ferramentaria.Id equals virtualFerramentaria.IdFerramentaria into virtualGroup
                                                                                                 from virtualItem in virtualGroup.DefaultIfEmpty()
                                                                                                 where (status == null || ferramentaria.Ativo == status)
                                                                                                   && (filter == null || ferramentaria.Nome.Contains(filter))
                                                                                                 select new WithReservationFerramentariaModel
                                                                                                 {
                                                                                                     Id = ferramentaria.Id,
                                                                                                     Nome = ferramentaria.Nome,
                                                                                                     DataRegistro = ferramentaria.DataRegistro,
                                                                                                     Ativo = ferramentaria.Ativo,
                                                                                                     IdVirtual = virtualItem.Id,
                                                                                                 }).ToList() ?? new List<WithReservationFerramentariaModel>();

                     



                            if (ferramentariaWithVirtual.Any())
                            {
                                //var mapper = mapeamentoClasses.CreateMapper();
                                //List<FerramentariaViewModel> FerramentariaResult = mapper.Map<List<FerramentariaViewModel>>(ferramentarias);

                                //_ListFerramentaria = FerramentariaResult;
                                //GlobalValues.ListFerramentariaViewModel = FerramentariaResult;
                                //GlobalPagination = pagination;
                                //int pageSize = pagination ?? 10;
                                //int pageNumber = 1;
                                //IPagedList<WithReservationFerramentariaModel> FerramentariaPagedList = ferramentariaWithVirtual.ToPagedList(pageNumber, pageSize);
                                //return View("Index", FerramentariaPagedList);

                                NewFerramentariaModel? newModel = new NewFerramentariaModel()
                                {
                                    WithReservationFerramentariaModel = ferramentariaWithVirtual,
                                    FerramentariaFilter = filter,
                                    Ativo = status,
                                    Pagination = pagination,
                                    PageNumber = 1,
                                };

                                return View("Index", newModel);

                            }
                            else
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "No Searched Ferramentaria has been found.";
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
                return View();
            }
        }

        public ActionResult TestPage(int? page,int? pageSize,string filter,int? ativo)
        {
            pageSize = pageSize ?? 10;
            int pageNumber = (page ?? 1);

            List<WithReservationFerramentariaModel> ferramentariaWithVirtual = (from ferramentaria in _context.Ferramentaria
                                                                                join virtualFerramentaria in _context.VirtualFerrmantaria
                                                                                on ferramentaria.Id equals virtualFerramentaria.IdFerramentaria into virtualGroup
                                                                                from virtualItem in virtualGroup.DefaultIfEmpty()
                                                                                where (ativo == null || ferramentaria.Ativo == ativo)
                                                                                                  && (filter == null || ferramentaria.Nome.Contains(filter))
                                                                                select new WithReservationFerramentariaModel
                                                                                {
                                                                                    Id = ferramentaria.Id,
                                                                                    Nome = ferramentaria.Nome,
                                                                                    DataRegistro = ferramentaria.DataRegistro,
                                                                                    Ativo = ferramentaria.Ativo,
                                                                                    IdVirtual = virtualItem.Id,
                                                                                }).ToList();

            NewFerramentariaModel? newModel = new NewFerramentariaModel()
            {
                WithReservationFerramentariaModel = ferramentariaWithVirtual,
                FerramentariaFilter = filter,
                Ativo = ativo,
                Pagination = pageSize,
                PageNumber = pageNumber,
            };

            return View("Index", newModel);



            //IPagedList<WithReservationFerramentariaModel> FerramentariaPagedList = ferramentariaWithVirtual.ToPagedList(pageNumber, pageSize);

            //return View("Index", FerramentariaPagedList);
            //return View();
        }

        public IActionResult AddToVirtual(int id)
        {

            VirtualFerrmantaria? checkduplicate = _context.VirtualFerrmantaria.FirstOrDefault(i => i.IdFerramentaria == id);

            if (checkduplicate == null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {

                    try
                    {
                        VirtualFerrmantaria entity = new VirtualFerrmantaria()
                        {
                            IdFerramentaria = id,
                        };

                        _context.VirtualFerrmantaria.Add(entity);
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
                ViewBag.Error = "Already in Virtual Ferramentaria";
                return View(nameof(Index));
            }
        }

        public IActionResult RemoveToVirtual(int id)
        {

            VirtualFerrmantaria? checkduplicate = _context.VirtualFerrmantaria.FirstOrDefault(i => i.IdFerramentaria == id);

            if (checkduplicate != null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {

                    try
                    {


                        _context.VirtualFerrmantaria.Remove(checkduplicate);
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
                ViewBag.Error = "Not registered in Virtual Ferramentaria";
                return View(nameof(Index));
            }
        }


        // GET: Ferramentarias/Create
        public IActionResult Create()
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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de ferramentaria!";
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
                        if (checkPermission.Inserir == 1)
                        {

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

        // POST: Ferramentarias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome")] FerramentariaViewModel ferramentariaViewModel)
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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de ferramentaria!";
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
                        if (checkPermission.Inserir == 1)
                        {



                            if (ModelState.IsValid)
                            {

                                bool isDuplicate = _context.Ferramentaria.Any(m => m.Nome == ferramentariaViewModel.Nome);

                                if (isDuplicate)
                                {
                                    log.LogWhy = "Duplicate!";
                                    auxiliar.GravaLogAlerta(log);
                                    ViewBag.Error = "Duplicate!";
                                    return View(ferramentariaViewModel);
                                }

                                var mapper = mapeamentoClasses.CreateMapper();
                                Ferramentaria ferramentaria = mapper.Map<Ferramentaria>(ferramentariaViewModel);
                                ferramentaria.DataRegistro = DateTime.Now;
                                ferramentaria.Ativo = 1;



                                _context.Add(ferramentaria);
                                await _context.SaveChangesAsync();

                                log.LogWhy = "Ferramentaria adicionada com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                TempData["ShowSuccessAlert"] = true;
                                _ListFerramentaria.Clear();
                                //GlobalValues.ClearList(GlobalValues.ListFerramentariaViewModel);

                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                log.LogWhy = "Erro na validação do modelo em criaçao ferramentaria!";
                                auxiliar.GravaLogAlerta(log);
                                return View(ferramentariaViewModel);
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
                return View();
            }


        }

        // GET: Ferramentarias/Edit/5
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


                            if (id == null || _context.Ferramentaria == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var ferramentaria =  _context.Ferramentaria.Find(id);

                            if (ferramentaria == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var mapper = mapeamentoClasses.CreateMapper();

                            FerramentariaViewModel ferramentariaViewModel = mapper.Map<FerramentariaViewModel>(ferramentaria);

                            log.LogWhy = "Ferramentaria carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(ferramentariaViewModel);


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

        // POST: Ferramentarias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,DataRegistro,Ativo")] FerramentariaViewModel ferramentariaViewModel)
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
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


                            if (id != ferramentariaViewModel.Id)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            if (ModelState.IsValid)
                            {

                                var ferramentaria = _context.Ferramentaria.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

                                ferramentariaViewModel.Id = ferramentaria.Id;
                                ferramentariaViewModel.DataRegistro = ferramentaria.DataRegistro;
                                ferramentariaViewModel.Ativo = ferramentaria.Ativo;

                                bool isDuplicate = _context.Marca.Any(m => m.Nome == ferramentariaViewModel.Nome);

                                if (isDuplicate)
                                {
                                    log.LogWhy = "Duplicate!";
                                    auxiliar.GravaLogAlerta(log);
                                    ViewBag.Error = "Duplicate!";
                                    return View(ferramentariaViewModel);
                                }

                                var mapper = mapeamentoClasses.CreateMapper();
                                ferramentaria = mapper.Map<Ferramentaria>(ferramentariaViewModel);

                                _context.Entry(ferramentaria).State = EntityState.Modified;
                                await _context.SaveChangesAsync();

                                log.LogWhy = "Ferramentaria editar com sucesso";
                                auxiliar.GravaLogSucesso(log);
                                //_context.Update(ferramentariaViewModel);

                                TempData["ShowSuccessAlert"] = true;
                                _ListFerramentaria.Clear();
                                //GlobalValues.ClearList(GlobalValues.ListFerramentariaViewModel);

                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                log.LogWhy = "Erro na validação do modelo em editar Ferramentaria!";
                                auxiliar.GravaLogAlerta(log);
                                return View(ferramentariaViewModel);
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
                return View(ferramentariaViewModel);
            }       
            
        }

        // GET: Ferramentarias/Delete/5
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
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Excluir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de Excluir a página de ferramentaria!";
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




                            if (id == null || _context.Ferramentaria == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var ferramentaria = await _context.Ferramentaria
                                .FirstOrDefaultAsync(m => m.Id == id);
                            if (ferramentaria == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var mapper = mapeamentoClasses.CreateMapper();

                            FerramentariaViewModel FerramentariaViewModel = mapper.Map<FerramentariaViewModel>(ferramentaria);

                            log.LogWhy = "Ferramentaria carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(FerramentariaViewModel);


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

        // POST: Ferramentarias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
                //        usuario.Retorno = "Usuário sem permissão de Excluir a página de ferramentaria!";
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



                            if (_context.Ferramentaria == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return Problem("Entity set 'ContextoBanco.Ferramentaria'  is null.");
                            }
                            var ferramentaria = await _context.Ferramentaria.FindAsync(id);
                            if (ferramentaria != null)
                            {
                                ferramentaria.Ativo = 0;
                                //_context.Ferramentaria.Remove(ferramentaria);
                            }

                            await _context.SaveChangesAsync();

                            log.LogWhy = "Ferramentaria deativar com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            TempData["ShowSuccessAlert"] = true;
                            _ListFerramentaria.Clear();
                            //GlobalValues.ClearList(GlobalValues.ListFerramentariaViewModel);

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

        private bool FerramentariaExists(int id)
        {
          return (_context.Ferramentaria?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
