using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.Entities;
using AutoMapper;
using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class BloqueioEmprestimoVsLiberadorController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thBloqueioEmprestimoVsLiberador.aspx";
        //private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public BloqueioEmprestimoVsLiberadorController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            //mapeamentoClasses = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<Obra, ObraViewModel>();
            //    cfg.CreateMap<ObraViewModel, Obra>();
            //});
        }

        // GET: BloqueioEmprestimoVsLiberador
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
                //    usuariofer.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuariofer.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("Login", "Home", usuariofer);
                //}
                //else
                //{
                //    if (usuariofer.Permissao.Visualizar != 1)
                //    {
                //        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                //        log.LogWhy = usuariofer.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("Login", "Home", usuariofer);
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



                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            //var usuarios = from usuario in _contextBS.VW_Usuario_New
                            //               where usuario.Ativo == 1 &&
                            //                     (
                            //                         usuario.CodSituacao == "F" &&
                            //                          (from func in _contextBS.Funcionario
                            //                           where func.FimProgFerias1 <= DateTime.Now &&
                            //                                 func.Chapa == usuario.Chapa &&
                            //                                 func.CodColigada == usuario.CodColigada
                            //                           select func.CodSituacao).FirstOrDefault() != null ||
                            //                         usuario.CodSituacao == "A"
                            //                     ) &&
                            //                     (from acesso in _contextBS.Acesso
                            //                      join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                            //                      where acesso.IdModulo == 6
                            //                      select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
                            //               orderby usuario.Nome
                            //               select usuario;

                            var permittedUserIds = (
                                         from acesso in _contextBS.Acesso
                                         join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                                         where acesso.IdModulo == 6
                                         select permissao.IdUsuario
                                     ).ToList();

                            List<simpleUserModel>? usuarios = (
                                                   from usuario in _contextBS.Usuario

                                                   join funcionario in _contextBS.Funcionario
                                                       on new { Chapa = (string)usuario.Chapa, CodColigada = (int)usuario.CodColigada }
                                                       equals new { Chapa = (string)funcionario.Chapa, CodColigada = (int)funcionario.CodColigada }
                                                       into funcionarioJoin
                                                   from funcionario in funcionarioJoin.DefaultIfEmpty()

                                                   where usuario.Ativo == 1 &&
                                                         (
                                                             (funcionario.CodSituacao == "F" &&
                                                              funcionario.Chapa != null &&
                                                              funcionario.FimProgFerias1 <= DateTime.Now &&
                                                              funcionario.CodSituacao != null) ||
                                                             funcionario.CodSituacao == "A"
                                                         ) &&
                                                          permittedUserIds.Contains(usuario.Id.Value)
                                                   orderby funcionario.Nome
                                                   select new simpleUserModel
                                                   {
                                                       Id = usuario.Id,
                                                       Chapa = usuario.Chapa,
                                                       CodColigada = usuario.CodColigada,
                                                       Nome = funcionario.Nome
                                                   }
                                               ).GroupBy(u => u.Id)
                                            .Select(g => g.First()).ToList();

                            if (usuarios == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return Problem("Entity set 'ContextoBancoBS.VW_Usuario_New' is null.");
                            }

                            // Create a list to hold UsuarioViewModel items
                            var usuarioViewModels = new List<VW_Usuario_NewViewModel>();

                            foreach (var usuario in usuarios)
                            {
                                // Call the IsChecked function to determine if the checkbox should be checked
                                bool isChecked = IsChecked(usuario.Id ?? 0); // Call your IsChecked function here

                                // Create a UsuarioViewModel item with the user and its checked status
                                var usuarioViewModel = new VW_Usuario_NewViewModel
                                {
                                    Id = usuario.Id,
                                    Chapa = usuario.Chapa,
                                    Nome = usuario.Nome,
                                    IsChecked = isChecked
                                };

                                // Add the usuarioViewModel to the list
                                usuarioViewModels.Add(usuarioViewModel);
                            }

                            log.LogWhy = "BloqueioEmprestimoVsLiberador carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(usuarioViewModels);

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

                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
             

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }           
        }

        private bool IsChecked(int id)
        {
            // Use Entity Framework Core to check if the Id exists in BloqueioEmprestimoVsLiberador
            return _context.BloqueioEmprestimoVsLiberador.Any(bloqueio => bloqueio.IdLogin == id);
        }


        //// GET: BloqueioEmprestimoVsLiberador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult Edit(List<int> selectedIds)
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
                //    if (usuario.Permissao.Inserir != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
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
                        if (checkPermission.Visualizar == 1)
                        {



                            DeleteBloqueioRecords();

                            foreach (int idLogin in selectedIds)
                            {
                                var bloqueioEmprestimo = new BloqueioEmprestimoVsLiberador
                                {
                                    IdLogin = idLogin
                                    // You can set other properties if needed
                                };

                                _context.BloqueioEmprestimoVsLiberador.Add(bloqueioEmprestimo);
                            }

                            // Save changes to the database
                            _context.SaveChanges();

                            TempData["ShowSuccessAlert"] = true;

                            log.LogWhy = "BloqueioEmprestimoVsLiberador atualizar com sucesso!";
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

                //if (selectedIds.Count == 0)
                //{

                //}
          
            }
            catch (Exception ex) 
            {
                return View(ex);
            }
            

         
        }

        public void DeleteBloqueioRecords()
        {
            // Retrieve all records from the BloqueioEmprestimoVsLiberador table
            var bloqueioRecords = _context.BloqueioEmprestimoVsLiberador.ToList();

            // Remove all records from the DbSet
            _context.BloqueioEmprestimoVsLiberador.RemoveRange(bloqueioRecords);

            // Save changes to the database
            _context.SaveChanges();

        }

    }
}
