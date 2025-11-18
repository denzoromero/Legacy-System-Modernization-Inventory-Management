using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AutoMapper;
using X.PagedList;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class MarcasController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "Generico.aspx?TOOLHOUSE.Marca";
        private MapperConfiguration mapeamentoClasses;
        //private static string _filtro;
        //private static int _Status;
        //private static int _Page;
        private static int? GlobalPagination;

        private const string SessionKeyMarcaList = "MarcaList";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";
        //private static List<MarcaViewModel> _ListMarca = new List<MarcaViewModel>();

        public MarcasController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Marca, MarcaViewModel>();
                cfg.CreateMap<MarcaViewModel, Marca>();
            });
        }

        public IActionResult Index(string filter, int status, int? page)
        {
          
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = pagina;
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

        public ActionResult SearchMarca(string? filter, int? status, int? pagination)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = pagina;
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


                            var marcas = _context.Marca.Where(i =>
                                  (status == null || i.Ativo == status)
                                  && (filter == null || i.Nome.Contains(filter))
                                  ).ToList();


                            if (marcas.Any())
                            {
                                var mapper = mapeamentoClasses.CreateMapper();
                                List<MarcaViewModel> MarcaResult = mapper.Map<List<MarcaViewModel>>(marcas);


                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyMarcaList);
                                HttpContext.Session.SetObject(SessionKeyMarcaList, MarcaResult);
                                //_ListMarca = MarcaResult;

                                //GlobalValues.ListMarcaViewModel = MarcaResult;

                                GlobalPagination = pagination;
                                int pageSize = GlobalPagination ?? 10;
                                int pageNumber = 1;
                                IPagedList<MarcaViewModel> MarcaPagedList = MarcaResult.ToPagedList(pageNumber, pageSize);
                                return View("Index", MarcaPagedList);
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

        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var MarcaListModel = HttpContext.Session.GetObject<List<MarcaViewModel?>>(SessionKeyMarcaList) ?? new List<MarcaViewModel?>();

            IPagedList<MarcaViewModel> MarcaPagedList = MarcaListModel.ToPagedList(pageNumber, pageSize);

            return View("Index", MarcaPagedList);
        }


        public IActionResult Create()
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {

                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = pagina;
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
                        if (checkPermission.Inserir == 1)
                        {


                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            log.LogWhy = "Tela de criação carregada com sucesso";
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
                return View("Index");
            }
        }


        [HttpPost]
        public IActionResult Create(string Nome)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {

                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = pagina;
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
                        if (checkPermission.Inserir == 1)
                        {

                            string MarcaNome = Nome;

                            if (MarcaNome == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Please Enter Nome.";

                                return RedirectToAction(nameof(Create));
                            }
                            else
                            {
                                bool isDuplicate = _context.Marca.Any(m => m.Nome == MarcaNome);

                                if (isDuplicate)
                                {
                                    TempData["ErrorMessage"] = "Duplicate!";
                                    log.LogWhy = "Duplicate!";
                                    auxiliar.GravaLogAlerta(log);
                                    return RedirectToAction(nameof(Create));
                                }
                                else
                                {
                                    var newMarca = new Marca
                                    {
                                        Nome = MarcaNome,
                                        DataRegistro = DateTime.Now,
                                        Ativo = 1 // Assuming 'Ativo' is a boolean field
                                    };

                                    _context.Marca.Add(newMarca);
                                    _context.SaveChanges();

                                    log.LogWhy = "Marca adicionada com sucesso";
                                    auxiliar.GravaLogSucesso(log);

                                    TempData["ShowSuccessAlert"] = true;
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyMarcaList);
                                    //_ListMarca.Clear();

                                    //GlobalValues.ClearList(GlobalValues.ListMarcaViewModel);

                                    return RedirectToAction(nameof(Index));

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
                ViewBag.Error = ex.Message;
                return View("Index");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Edit";
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de marca!";
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

                            var marca = await _context.Marca.FindAsync(id);
                            var mapper = mapeamentoClasses.CreateMapper();

                            MarcaViewModel marcaViewModel = mapper.Map<MarcaViewModel>(marca);

                            log.LogWhy = "Marcas carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(marcaViewModel);

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id, Nome, DataRegistro, Ativo")] MarcaViewModel viewModel)
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de marca!";
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


                            if (ModelState.IsValid)
                            {
                                if (id != viewModel.Id)
                                {
                                    log.LogWhy = "ID da marca não localizada no banco de dados!";
                                    ErrorViewModel erro = new ErrorViewModel();
                                    erro.Tela = log.LogWhere;
                                    erro.Descricao = log.LogWhy;
                                    erro.Mensagem = log.LogWhat;
                                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                    return NotFound();
                                }

                                var marcas = _context.Marca.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

                                if (marcas == null)
                                {
                                    return NotFound();
                                }

                                if (id != marcas.Id)
                                {
                                    return NotFound(marcas.Id);
                                }

                                viewModel.Id = marcas.Id;
                                viewModel.DataRegistro = marcas.DataRegistro;
                                viewModel.Ativo = marcas.Ativo;

                                bool isDuplicate = _context.Marca.Any(m => m.Nome == viewModel.Nome);

                                if (isDuplicate)
                                {
                                    log.LogWhy = "Duplicado cadastrado!";
                                    ErrorViewModel erro = new ErrorViewModel();
                                    erro.Tela = log.LogWhere;
                                    erro.Descricao = log.LogWhy;
                                    erro.Mensagem = log.LogWhat;
                                    erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                    ViewBag.Error = "Duplicado cadastrado!";
                                }

                                var mapper = mapeamentoClasses.CreateMapper();
                                marcas = mapper.Map<Marca>(viewModel);

                                _context.Entry(marcas).State = EntityState.Modified;
                                _context.SaveChanges();

                                log.LogWhy = "Marca editada com sucesso!";
                                auxiliar.GravaLogSucesso(log);

                                TempData["ShowSuccessAlert"] = true;
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyMarcaList);
                                //_ListMarca.Clear();
                                //GlobalValues.ClearList(GlobalValues.ListMarcaViewModel);


                                return RedirectToAction(nameof(Index));

                            }
                            else
                            {
                                log.LogWhy = "Erro na validação do modelo em editar empresa!";
                                auxiliar.GravaLogAlerta(log);
                                return View(viewModel);
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

                //string EditReciever = viewModel.Nome;
                //int EditId = id;
                //int ActiveReceiver = viewModel.Ativo;

            


                    //return View(viewModel);
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


        public async Task<ActionResult> Delete(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Delete";
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
                //    if (usuario.Permissao.Excluir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de excluir uma empresa!";
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
                        if (checkPermission.Excluir == 1)
                        {

                            if (id == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return RedirectToAction("Erro", "Home", erro);
                                //return NotFound();
                            }

                            var marca = await _context.Marca.FirstOrDefaultAsync(m => m.Id == id);

                            if (marca == null)
                            {

                            }

                            var mapper = mapeamentoClasses.CreateMapper();

                            MarcaViewModel marcaViewModel = mapper.Map<MarcaViewModel>(marca);

                            log.LogWhy = "Marca carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);
                            return View(marcaViewModel);


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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
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
                        if (checkPermission.Excluir == 1)
                        {


                            if (id == null)
                            {
                                log.LogWhy = "ID da pergunta não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return RedirectToAction("Erro", "Home", erro);
                                //return NotFound();
                            }

                            var marca = await _context.Marca.FindAsync(id);
                            if (marca == null)
                            {
                                return View();
                            }

                            marca.Ativo = 0;
                            _context.SaveChanges();
                            log.LogWhy = "Marca deativar com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            TempData["ShowSuccessAlert"] = true;
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyMarcaList);
                            //_ListMarca.Clear();
                            //GlobalValues.ClearList(GlobalValues.ListMarcaViewModel);


                            return RedirectToAction("Index");



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
