using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using X.PagedList;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using NuGet.Packaging;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class VW_Reativacao_Item_ExtraviadoController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thProdutoReinclusaoExtraviado.aspx";
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyPageNumberVWItemExtraviado = "PageNumberVWItemExtraviado";
        private const string SessionKeyPaginationVWItemExtraviado = "PaginationVWItemExtraviado";
        //private static int? GlobalPagination;
        //private static int? UserEditPermission;

        private const string SessionKeyReactivacaoItemExtraviado = "ReactivacaoItemExtraviado";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";
        //private static List<VW_Reativacao_Item_ExtraviadoViewModel> _ListVW_Reativacao_Item_Extraviado = new List<VW_Reativacao_Item_ExtraviadoViewModel>();

        public VW_Reativacao_Item_ExtraviadoController(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_Reativacao_Item_Extraviado, VW_Reativacao_Item_ExtraviadoViewModel>();
                cfg.CreateMap<VW_Reativacao_Item_ExtraviadoViewModel, VW_Reativacao_Item_Extraviado>();
            });
        }

        // GET: VW_Reativacao_Item_Extraviado
        public IActionResult Index(int? page, string? codigo, string? item, string? AF, int? PAT, string? controle, string? estoque, string? justificativa, string? usuariores, DateTime? de, DateTime? ate, string? matricula)
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

                            //if (GlobalValues.VW_Reativacao_Item_ExtraviadoViewModel.Count != 0)
                            //{
                            //    int? testget = GlobalPagination;
                            //    int pageSize = GlobalPagination ?? 10;
                            //    int pageNumber = (page ?? 1);

                            //    List<VW_Reativacao_Item_ExtraviadoViewModel> ListVW_Reativacao_Item_ExtraviadoViewModelResult = GlobalValues.VW_Reativacao_Item_ExtraviadoViewModel;

                            //    IPagedList<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoPagedList = ListVW_Reativacao_Item_ExtraviadoViewModelResult.ToPagedList(pageNumber, pageSize);

                            //    usuario.Retorno = "Acesso Permitido";
                            //    log.LogWhy = usuario.Retorno;
                            //    auxiliar.GravaLogSucesso(log);

                            //    var combinedViewModel = new CombinedExtraviado
                            //    {
                            //        VW_Reativacao_Item_ExtraviadoViewModel = VW_Reativacao_Item_ExtraviadoPagedList
                            //    };

                            //    return View(combinedViewModel);
                            //}

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
                return View(ex);
            }

        }

        public ActionResult GetExtraviadoItems(CombinedExtraviado CombinedExtraviado)
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

                            List<VW_Reativacao_Item_ExtraviadoViewModel>? Result = _context.VW_Reativacao_Item_Extraviado.
                                Where(r => (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Codigo == null || r.Codigo.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Codigo))
                                                && (string.IsNullOrEmpty(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Descricao) || r.Descricao.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Descricao))
                                                && (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.AF == null || r.AF.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.AF))
                                                //&& (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.PAT == null || r.PAT == (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.PAT))
                                                && (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.PAT == null || EF.Functions.Like(r.PAT.ToString(), $"%{CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.PAT}%"))
                                                //&& (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Controle == null || r.Controle.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Controle))
                                                //&& (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.LocalEmEstoque == null || r.LocalEmEstoque.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.LocalEmEstoque))
                                                && (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Justificativa == null || r.Justificativa.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Justificativa))
                                                && (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Usuario == null || r.Usuario.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Usuario))
                                                && (!CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.De.HasValue || r.DataInativacao >= CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.De)
                                                && (!CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Ate.HasValue || r.DataInativacao <= CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Ate)
                                                && (CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.MatriculaFuncionarioEmprestimo == null || r.MatriculaFuncionarioEmprestimo.Contains(CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.MatriculaFuncionarioEmprestimo))
                                                )
                                                .ProjectTo<VW_Reativacao_Item_ExtraviadoViewModel>(mapeamentoClasses)
                                                .ToList();

                            if (Result.Any())
                            {
                                var mapper = mapeamentoClasses.CreateMapper();

                                List<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoViewModelResult = new List<VW_Reativacao_Item_ExtraviadoViewModel>();
                                VW_Reativacao_Item_ExtraviadoViewModelResult = mapper.Map<List<VW_Reativacao_Item_ExtraviadoViewModel>>(Result);

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacaoItemExtraviado);
                                HttpContext.Session.SetObject(SessionKeyReactivacaoItemExtraviado, VW_Reativacao_Item_ExtraviadoViewModelResult);

                                //_ListVW_Reativacao_Item_Extraviado = VW_Reativacao_Item_ExtraviadoViewModelResult;
                                //GlobalValues.VW_Reativacao_Item_ExtraviadoViewModel = VW_Reativacao_Item_ExtraviadoViewModelResult;

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPaginationVWItemExtraviado);
                                httpContextAccessor.HttpContext?.Session.SetInt32(SessionKeyPaginationVWItemExtraviado, (int)CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Pagination);

                                //GlobalPagination = CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Pagination;
                                int pageSize = CombinedExtraviado.VW_Reativacao_Item_ExtraviadoSearchModel.Pagination ?? 10;
                                int pageNumber = 1;
                                IPagedList<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoPagedList = VW_Reativacao_Item_ExtraviadoViewModelResult.ToPagedList(pageNumber, pageSize);

                                //UserEditPermission = usuario.Permissao.Editar;

                                var combinedViewModel = new CombinedExtraviado
                                {
                                    VW_Reativacao_Item_ExtraviadoViewModel = VW_Reativacao_Item_ExtraviadoPagedList,
                                    UserEditPermission = checkPermission.Editar
                                };


                                return View("Index", combinedViewModel);
                            }
                            else
                            {
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
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            int? size = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeyPaginationVWItemExtraviado);
            int pageSize = size ?? 10;
            int pageNumber = (page ?? 1);


            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPageNumberVWItemExtraviado);
            httpContextAccessor.HttpContext?.Session.SetInt32(SessionKeyPageNumberVWItemExtraviado, (int)pageNumber);

            //#region Authenticate User
            //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
            //usuario.Pagina = pagina;
            //usuario = auxiliar.VerificaPermissao(usuario);

            //if (usuario.Permissao == null)
            //{
            //    usuario.Retorno = "Usuário sem permissão na página!";
            //    return RedirectToAction("PreserveActionError", "Home", usuario);
            //}
            //else
            //{
            //    if (usuario.Permissao.Visualizar != 1)
            //    {
            //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
            //        return RedirectToAction("PreserveActionError", "Home", usuario);
            //    }
            //}

            //if (usuario.Permissao.Editar == 1)
            //{
            //    ViewBag.EditarValue = 1;
            //}

            //#endregion

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
            PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);

            var RelatorioListModel = HttpContext.Session.GetObject<List<VW_Reativacao_Item_ExtraviadoViewModel?>>(SessionKeyReactivacaoItemExtraviado) ?? new List<VW_Reativacao_Item_ExtraviadoViewModel?>();

            IPagedList<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoPagedList = RelatorioListModel.ToPagedList(pageNumber, pageSize);

            var combinedViewModel = new CombinedExtraviado
            {
                VW_Reativacao_Item_ExtraviadoViewModel = VW_Reativacao_Item_ExtraviadoPagedList,
                UserEditPermission = checkPermission?.Editar
            };


            return View("Index", combinedViewModel);
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de Reinclusao de Item!";
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
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (id == null || _context.VW_Reativacao_Item_Extraviado == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            var vW_Reativacao_Item_Extraviado = _context.VW_Reativacao_Item_Extraviado.Find(id);
                            if (vW_Reativacao_Item_Extraviado == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                return NotFound();
                            }

                            log.LogWhy = "Reinclusao de Item Extraviado carregada com sucesso!";
                            auxiliar.GravaLogSucesso(log);

                            return View(vW_Reativacao_Item_Extraviado);



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


                //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

              
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
        public IActionResult Edit(int id, string Reactivate, int? IdProdutoAlocado,int? Status)
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
                //        usuario.Retorno = "Usuário sem permissão de Editar a página de Reinclusao de Item!";
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


                            if (Reactivate == null)
                            {
                                log.LogWhy = "ID da ferramentaria não localizada no banco de dados!";
                                ErrorViewModel erro = new ErrorViewModel();
                                erro.Tela = log.LogWhere;
                                erro.Descricao = log.LogWhy;
                                erro.Mensagem = log.LogWhat;
                                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                                TempData["ErrorMessage"] = "Informe a Justificativa para Reativação Obrigatorio";
                                return RedirectToAction("Edit", new { id = id });
                            }

                            if (Status == 1)
                            {
                                var produtoReincluidoExtraviado = _context.ProdutoReincluidoExtraviado.Where(i => i.IdProduto == id && i.Status == 1).OrderByDescending(i => i.Id).FirstOrDefault();
                                if (produtoReincluidoExtraviado != null)
                                {
                                    produtoReincluidoExtraviado.Status = 2;
                                    produtoReincluidoExtraviado.DataRegistro_Aprovacao = DateTime.Now;
                                    produtoReincluidoExtraviado.IdUsuario_Aprovador = loggedUser?.Id;
                                };

                                var EditprodutoExtraviado = _context.ProdutoExtraviado.Where(i => i.IdProdutoAlocado == IdProdutoAlocado).OrderByDescending(i => i.Id).FirstOrDefault();
                                if (EditprodutoExtraviado != null)
                                {
                                    EditprodutoExtraviado.Ativo = 0;
                                }

                                var GetProdutoExtraviado = _context.ProdutoExtraviado.FirstOrDefault(o => o.IdProdutoAlocado == IdProdutoAlocado);
                                var EditProdutoAlocado = _context.ProdutoAlocado.FirstOrDefault(i => i.Id == IdProdutoAlocado);
                                if (EditProdutoAlocado != null)
                                {
                                    int? oldvalue = EditProdutoAlocado.Quantidade;
                                    EditProdutoAlocado.Quantidade = oldvalue + GetProdutoExtraviado.Quantidade;
                                }

                                var EditProduto = _context.Produto.FirstOrDefault(i => i.Id == id);
                                if (EditProduto != null)
                                {
                                    EditProduto.Ativo = 1;
                                    EditProduto.Observacao = EditProduto.Observacao + " ** Reat. Aprov. Por => " + loggedUser?.Nome;
                                }

                                _context.SaveChanges();

                                TempData["ShowSuccessAlert"] = true;
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacaoItemExtraviado);
                                //_ListVW_Reativacao_Item_Extraviado.Clear();
                                //GlobalValues.ClearList(GlobalValues.VW_Reativacao_Item_ExtraviadoViewModel);


                                log.LogWhy = "Reativacao Item com sucesso";
                                auxiliar.GravaLogSucesso(log);

                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                if (Reactivate != null)
                                {
                                    var insertToProdutoReincluidoExtraviado = new ProdutoReincluidoExtraviado
                                    {
                                        IdProduto = id,
                                        Observacao = Reactivate,
                                        IdUsuario_Solicitante = loggedUser?.Id,
                                        IdUsuario_Aprovador = 0,
                                        Status = 1,
                                        DataRegistro = DateTime.Now,
                                    };

                                    _context.ProdutoReincluidoExtraviado.Add(insertToProdutoReincluidoExtraviado);
                                    _context.SaveChanges();


                                    TempData["ShowSuccessAlert"] = true;
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyReactivacaoItemExtraviado);
                                    //_ListVW_Reativacao_Item_Extraviado.Clear();
                                    //GlobalValues.ClearList(GlobalValues.VW_Reativacao_Item_ExtraviadoViewModel);
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

        public ActionResult OpenReactivateActionModal(int? Id)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            //#region Authenticate User
            //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
            //usuario.Pagina = pagina;
            //usuario = auxiliar.VerificaPermissao(usuario);

            //if (usuario.Permissao == null)
            //{
            //    usuario.Retorno = "Usuário sem permissão na página!";
            //    return RedirectToAction("PreserveActionError", "Home", usuario);
            //}
            //else
            //{
            //    if (usuario.Permissao.Visualizar != 1)
            //    {
            //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
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
                    if (checkPermission.Editar == 1)
                    {


                        var RelatorioListModel = HttpContext.Session.GetObject<List<VW_Reativacao_Item_ExtraviadoViewModel?>>(SessionKeyReactivacaoItemExtraviado) ?? new List<VW_Reativacao_Item_ExtraviadoViewModel?>();
                        if (RelatorioListModel.Count > 0)
                        {
                            VW_Reativacao_Item_ExtraviadoViewModel? item = RelatorioListModel.FirstOrDefault(i => i.Id == Id);
                            ViewBag.ProductToReactivateFromExtraviado = item;
                        }

                        int? size = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeyPaginationVWItemExtraviado);
                        int pageSize = size ?? 10;
                        int pageNumber = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeyPageNumberVWItemExtraviado) ?? 1;

                        IPagedList<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoPagedList = RelatorioListModel.ToPagedList(pageNumber, pageSize);

                        var combinedViewModel = new CombinedExtraviado
                        {
                            VW_Reativacao_Item_ExtraviadoViewModel = VW_Reativacao_Item_ExtraviadoPagedList,
                            UserEditPermission = checkPermission.Editar
                        };

                        return View(nameof(Index), combinedViewModel);



                    }
                    else
                    {
                        return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });
                    }
                }
                else
                {
                    return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });
                }
            }
            else
            {
                return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });
            }

           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmReactivation(int? Id, string? Justificativa, int? IdProdutoAlocado)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            //#region Authenticate User
            //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
            //usuario.Pagina = pagina;
            //usuario = auxiliar.VerificaPermissao(usuario);

            //if (usuario.Permissao == null)
            //{
            //    usuario.Retorno = "Usuário sem permissão na página!";
            //    return RedirectToAction("PreserveActionError", "Home", usuario);
            //}
            //else
            //{
            //    if (usuario.Permissao.Visualizar != 1)
            //    {
            //        usuario.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
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



                        ProdutoReincluidoExtraviado? FirstQuery = _context.ProdutoReincluidoExtraviado.Where(i => i.IdProduto == Id && i.Status == 1).OrderByDescending(i => i.Id).FirstOrDefault();
                        if (FirstQuery != null)
                        {
                            FirstQuery.Status = 2;
                            FirstQuery.DataRegistro_Aprovacao = DateTime.Now;
                            FirstQuery.IdUsuario_Aprovador = loggedUser?.Id;

                            _context.SaveChanges();
                        }

                        ProdutoExtraviado? SecondQuery = _context.ProdutoExtraviado.Where(i => i.IdProdutoAlocado == IdProdutoAlocado).OrderByDescending(i => i.Id).FirstOrDefault();
                        if (SecondQuery != null)
                        {
                            SecondQuery.Ativo = 0;

                            ProdutoAlocado? ThirdQuery = _context.ProdutoAlocado.FirstOrDefault(i => i.Id == IdProdutoAlocado);
                            if (ThirdQuery != null)
                            {
                                ThirdQuery.Quantidade = ThirdQuery.Quantidade + SecondQuery.Quantidade;
                            }


                            _context.SaveChanges();
                        }

                        Produto? FourthQuery = _context.Produto.FirstOrDefault(i => i.Id == Id);
                        if (FourthQuery != null)
                        {
                            FourthQuery.Ativo = 1;
                            FourthQuery.Observacao = FourthQuery.Observacao + "** Reat. Approv. Por => " + loggedUser?.Nome;


                            _context.SaveChanges();
                        }


                        ViewBag.ShowSuccessAlert = true;


                        return View(nameof(Index));


                    }
                    else
                    {
                        return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });
                    }
                }
                else
                {
                    return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });
                }
            }
            else
            {
                return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });
            }

     
        }


        //// POST: VW_Reativacao_Item_Extraviado/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Descricao,AF,PAT,Saldo,Controle,LocalEmEstoque,Motivo,Justificativa,Usuario,DataInativacao,Status,Justificativa_Reativacao,MatriculaFuncionarioEmprestimo,IdProdutoAlocado")] VW_Reativacao_Item_Extraviado vW_Reativacao_Item_Extraviado)
        //{
        //    if (id != vW_Reativacao_Item_Extraviado.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(vW_Reativacao_Item_Extraviado);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!VW_Reativacao_Item_ExtraviadoExists(vW_Reativacao_Item_Extraviado.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(vW_Reativacao_Item_Extraviado);
        //}

        //// GET: VW_Reativacao_Item_Extraviado/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.VW_Reativacao_Item_Extraviado == null)
        //    {
        //        return NotFound();
        //    }

        //    var vW_Reativacao_Item_Extraviado = await _context.VW_Reativacao_Item_Extraviado
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (vW_Reativacao_Item_Extraviado == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(vW_Reativacao_Item_Extraviado);
        //}

        //// POST: VW_Reativacao_Item_Extraviado/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.VW_Reativacao_Item_Extraviado == null)
        //    {
        //        return Problem("Entity set 'ContextoBanco.VW_Reativacao_Item_Extraviado'  is null.");
        //    }
        //    var vW_Reativacao_Item_Extraviado = await _context.VW_Reativacao_Item_Extraviado.FindAsync(id);
        //    if (vW_Reativacao_Item_Extraviado != null)
        //    {
        //        _context.VW_Reativacao_Item_Extraviado.Remove(vW_Reativacao_Item_Extraviado);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool VW_Reativacao_Item_ExtraviadoExists(int id)
        //{
        //  return (_context.VW_Reativacao_Item_Extraviado?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
