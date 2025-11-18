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
using NuGet.Protocol.Plugins;
using System.Runtime.CompilerServices;
using FerramentariaTest.Helpers;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class FerramentariaVsBalconistaController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thUsuarioBuscar.aspx";
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();
        private static List<VW_Usuario_NewViewModel>? ListOfFerramentariaUsers = new List<VW_Usuario_NewViewModel>();

        public FerramentariaVsBalconistaController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_Usuario_New, VW_Usuario_NewViewModel>();
                cfg.CreateMap<VW_Usuario_NewViewModel, VW_Usuario_New>();
            });
        }

        // GET: FerramentariaVsBalconista
        public IActionResult Index()
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                // #region Authenticate User
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

                            List<VW_Usuario_NewViewModel>? usuarios = (from usuario in _contextBS.VW_Usuario_New
                                                                       where usuario.Ativo == 1 &&
                                                                             (
                                                                                 (usuario.CodSituacao == "F" &&
                                                                                  (from func in _contextBS.Funcionario
                                                                                   where func.FimProgFerias1 <= DateTime.Now &&
                                                                                         func.Chapa == usuario.Chapa &&
                                                                                         func.CodColigada == usuario.CodColigada
                                                                                   select func.CodSituacao).FirstOrDefault() != null) ||
                                                                                 usuario.CodSituacao == "A"
                                                                             ) &&
                                                                             (from acesso in _contextBS.Acesso
                                                                              join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                                                                              where acesso.IdModulo == 6
                                                                              select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
                                                                       orderby usuario.Nome
                                                                       select new VW_Usuario_NewViewModel
                                                                       {
                                                                           Id = usuario.Id,
                                                                           Chapa = usuario.Chapa,
                                                                           Nome = usuario.Nome,
                                                                       }).ToList();

                            //var mapper = mapeamentoClasses.CreateMapper();
                            //List<VW_Usuario_NewViewModel?>? UserResult = mapper.Map<List<VW_Usuario_NewViewModel?>>(usuarios);

                            ListOfFerramentariaUsers = usuarios;



                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

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

                            return View(nameof(Index), ListOfFerramentariaUsers);

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



           


           
                //return ListOfFerramentariaUsers != null ? View(await ListOfFerramentariaUsers.ToListAsync()) : Problem("Entity set 'ContextoBancoBS.VW_Usuario_New' is null.");

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


  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int id, List<int> selectedIds)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Add";
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


                        if (id != 0)
                        {
                            if (selectedIds.Count > 0)
                            {
                                foreach (var itemId in selectedIds)
                                {
                                    // Create a new FerramentariaVsLiberador object
                                    var ferramentariaVsLiberador = new FerramentariaVsLiberador
                                    {
                                        IdLogin = id,
                                        IdFerramentaria = itemId
                                    };
                                    _context.FerramentariaVsLiberador.Add(ferramentariaVsLiberador);

                                    var InsertToLogAtribuicaoFerramentaria = new LogAtribuicaoFerramentaria
                                    {
                                        IdUsuario = id,
                                        IdFerramentaria = itemId,
                                        IdUsuarioResponsavel = loggedUser?.Id,
                                        Acao = 1,
                                        DataRegistro = DateTime.Now,
                                    };
                                    _context.LogAtribuicaoFerramentaria.Add(InsertToLogAtribuicaoFerramentaria);

                                    _context.SaveChanges();
                                }


                                TempData["ShowSuccessModal"] = true;
                                return RedirectToAction(nameof(GetBalconistaInfo), new { id = id });
                                //return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                List<SimpleFerramentariaViewModel?>? resultAvailable = GetAvailableFerramentarias(id);
                                ViewBag.FerramentariaList = resultAvailable;

                                List<SimpleFerramentariaViewModel?>? ResultTaken = RegisteredFerramentarias(id);
                                ViewBag.TakenList = ResultTaken;

                                ViewBag.OpenModalFerramentaria = true;
                                ViewBag.SelectedId = id;
                                ViewBag.ErrorFerramentariaBalconista = "Por favor selecione ferramentaria para adicionar.";
                                return View(nameof(Index), ListOfFerramentariaUsers);
                            }
                        }
                        else
                        {
                            ViewBag.Error = "No Id Selected";
                            return View(nameof(Index), ListOfFerramentariaUsers);
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
        public IActionResult Remove(int? id, List<int> ToDeleteIds)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Remove";
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


                        if (id != 0)
                        {
                            if (ToDeleteIds.Count > 0)
                            {
                                foreach (var itemId in ToDeleteIds)
                                {
                                    var recordsToDelete = _context.FerramentariaVsLiberador
                                                            .Where(x => x.IdFerramentaria == itemId && x.IdLogin == id)
                                                            .ToList();

                                    if (recordsToDelete.Any())
                                    {
                                        _context.FerramentariaVsLiberador.RemoveRange(recordsToDelete);
                                    }

                                    var InsertToLogAtribuicaoFerramentaria = new LogAtribuicaoFerramentaria
                                    {
                                        IdUsuario = id,
                                        IdFerramentaria = itemId,
                                        IdUsuarioResponsavel = loggedUser?.Id,
                                        Acao = 0,
                                        DataRegistro = DateTime.Now,
                                    };
                                    _context.LogAtribuicaoFerramentaria.Add(InsertToLogAtribuicaoFerramentaria);

                                    _context.SaveChanges();
                                }

                                TempData["ShowSuccessModal"] = true;
                                return RedirectToAction(nameof(GetBalconistaInfo), new { id = id });
                            }
                            {
                                List<SimpleFerramentariaViewModel?>? resultAvailable = GetAvailableFerramentarias(id);
                                ViewBag.FerramentariaList = resultAvailable;

                                List<SimpleFerramentariaViewModel?>? ResultTaken = RegisteredFerramentarias(id);
                                ViewBag.TakenList = ResultTaken;

                                ViewBag.OpenModalFerramentaria = true;
                                ViewBag.SelectedId = id;
                                ViewBag.ErrorFerramentariaBalconista = "Por favor selecione ferramentaria para adicionar.";
                                return View(nameof(Index), ListOfFerramentariaUsers);
                            }

                        }
                        else
                        {
                            ViewBag.Error = "No Id Selected";
                            return View(nameof(Index), ListOfFerramentariaUsers);
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

        public IActionResult GetBalconistaInfo(int? id)
        {
            if (id != null)
            {
                if (TempData.ContainsKey("ShowSuccessModal"))
                {
                    ViewBag.ShowSuccessModal = TempData["ShowSuccessModal"]?.ToString();
                    TempData.Remove("ShowSuccessModal"); // Remove it from TempData to avoid displaying it again
                }

                var vW_Usuario_New = _contextBS.VW_Usuario_New.Find(id);
                if (vW_Usuario_New != null)
                {
                    //Available Ferramentarias of the User
                    List<SimpleFerramentariaViewModel?>? resultAvailable = GetAvailableFerramentarias(id);
                    ViewBag.FerramentariaList = resultAvailable;


                    //Registered Ferramentarias of the User      
                    List<SimpleFerramentariaViewModel?>? ResultTaken = RegisteredFerramentarias(id);
                    ViewBag.TakenList = ResultTaken;
                    ViewBag.OpenModalFerramentaria = true;
                    ViewBag.SelectedId = id;

                    return View(nameof(Index), ListOfFerramentariaUsers);
                }
                else
                {
                    ViewBag.Error = "ID da usuario não localizada no banco de dados!";
                    return View(nameof(Index), ListOfFerramentariaUsers);
                }                   
            }
            else
            {
                ViewBag.Error = "Id is Empty";
                return View(nameof(Index), ListOfFerramentariaUsers);
            }
        }


        public List<SimpleFerramentariaViewModel?>? GetAvailableFerramentarias(int? Id)
        {
            List<SimpleFerramentariaViewModel?>? availableFerramentarias = (from ferramentaria in _context.Ferramentaria
                                 where ferramentaria.Ativo == 1 &&
                                       !(
                                           from assSolda in _context.VW_Ferramentaria_Ass_Solda
                                           select assSolda.Id
                                       ).Contains(ferramentaria.Id) &&
                                       !(
                                           from liberador in _context.FerramentariaVsLiberador
                                           where liberador.IdLogin == Id // Make sure you have the 'id' variable defined
                                           select liberador.IdFerramentaria
                                       ).Contains(ferramentaria.Id)
                                 orderby ferramentaria.Nome
                                 select new SimpleFerramentariaViewModel
                                 {
                                     Id = ferramentaria.Id,
                                     Nome = ferramentaria.Nome,
                                 }).ToList();

            return availableFerramentarias;
        }

        public List<SimpleFerramentariaViewModel?>? RegisteredFerramentarias(int? Id)
        {
            List<SimpleFerramentariaViewModel?>? registeredFerramentarias = (from ferramentaria in _context.Ferramentaria
                                                                            where ferramentaria.Ativo == 1 &&
                                                                                  !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                                                  _context.FerramentariaVsLiberador.Any(l => l.IdLogin == Id && l.IdFerramentaria == ferramentaria.Id)
                                                                            orderby ferramentaria.Nome
                                                                            select new SimpleFerramentariaViewModel
                                                                            {
                                                                                Id = ferramentaria.Id,
                                                                                Nome = ferramentaria.Nome,
                                                                            }).ToList();

            return registeredFerramentarias;
        }


        // GET: FerramentariaVsBalconista/Edit/5
        public  IActionResult Edit(int? id)
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
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de Ferramentaria vs Balconista!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //}
                //#endregion

                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

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
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            if (id == null || _contextBS.VW_Usuario_New == null)
                            {
                                log.LogWhy = "ID da usuario não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var vW_Usuario_New = _contextBS.VW_Usuario_New.Find(id);
                            if (vW_Usuario_New == null)
                            {
                                log.LogWhy = "ID da usuario não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var queryAvailable = from ferramentaria in _context.Ferramentaria
                                                 where ferramentaria.Ativo == 1 &&
                                                       !(
                                                           from assSolda in _context.VW_Ferramentaria_Ass_Solda
                                                           select assSolda.Id
                                                       ).Contains(ferramentaria.Id) &&
                                                       !(
                                                           from liberador in _context.FerramentariaVsLiberador
                                                           where liberador.IdLogin == id // Make sure you have the 'id' variable defined
                                                           select liberador.IdFerramentaria
                                                       ).Contains(ferramentaria.Id)
                                                 orderby ferramentaria.Nome
                                                 select new
                                                 {
                                                     ferramentaria.Id,
                                                     ferramentaria.Nome,
                                                 };

                            // Execute the query to get the results
                            var resultAvailable = queryAvailable.ToList();

                            ViewBag.FerramentariaList = resultAvailable;


                            var query = from ferramentaria in _context.Ferramentaria
                                        where ferramentaria.Ativo == 1 &&
                                              !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                              _context.FerramentariaVsLiberador.Any(l => l.IdLogin == id && l.IdFerramentaria == ferramentaria.Id)
                                        orderby ferramentaria.Nome
                                        select new
                                        {
                                            ferramentaria.Id,
                                            ferramentaria.Nome,
                                        };

                            var ResultTaken = query.ToList();
                            ViewBag.TakenList = ResultTaken;

                            log.LogWhy = "Ferramentaria vs Balconista carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(vW_Usuario_New);



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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, List<int> selectedIds)
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
                //#endregion

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {



                            if (selectedIds.Count == 0)
                            {
                                //TempData["ShowSuccessAlert"] = false; // Set to false for error
                                TempData["ErrorMessage"] = true;
                                return RedirectToAction("Index");
                            }
                            else
                            {


                                foreach (var itemId in selectedIds)
                                {
                                    // Create a new FerramentariaVsLiberador object
                                    var ferramentariaVsLiberador = new FerramentariaVsLiberador
                                    {
                                        IdLogin = id,
                                        IdFerramentaria = itemId
                                        // You may need to set other properties here
                                    };

                                    //var sql = $"INSERT INTO FerramentariaVsLiberador (IdFerramentaria, IdLogin) VALUES ({itemId}, {id})";
                                    //_context.Database.ExecuteSqlRaw(sql);

                                    _context.FerramentariaVsLiberador.Add(ferramentariaVsLiberador);

                                }
                                //// Add the new entity to the context
                                //_context.FerramentariaVsLiberador.Add(ferramentariaVsLiberador);


                                _context.SaveChanges();
                            }

                            TempData["ShowSuccessAlert"] = true; // Set to true for success

                            log.LogWhy = "Ferramentaria vs Balconista addicionar com sucesso";
                            auxiliar.GravaLogSucesso(log);
                            TempData["ShowSuccessAlert"] = true;
                            return RedirectToAction("Edit", new { id = id });


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


        // POST: FerramentariaVsBalconista/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int? id, List<int> ToDeleteIds)
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
                //#endregion

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {



                            if (ToDeleteIds.Count == 0)
                            {
                                log.LogWhy = "0 To Delete!";
                                auxiliar.GravaLogAlerta(log);
                                TempData["ShowErrorAlert"] = true;
                                return RedirectToAction("Index");
                            }

                            foreach (var itemId in ToDeleteIds)
                            {
                                var recordsToDelete = _context.FerramentariaVsLiberador
                               .Where(x => x.IdFerramentaria == itemId && x.IdLogin == id)
                               .ToList();

                                if (recordsToDelete.Any())
                                {
                                    _context.FerramentariaVsLiberador.RemoveRange(recordsToDelete);
                                }

                                //var sql = $"DELETE from FerramentariaVsLiberador WHERE IdFerramentaria = {itemId} AND IdLogin = {id}";
                                //_context.Database.ExecuteSqlRaw(sql);

                            }

                            _context.SaveChanges();

                            TempData["ShowSuccessAlert"] = true; // Set to true for success
                                                                 //await _context.SaveChangesAsync();

                            log.LogWhy = "Ferramentaria vs Balconista remover com sucesso";
                            auxiliar.GravaLogSucesso(log);

                            return RedirectToAction(nameof(Edit), new { id });


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

    }
}
