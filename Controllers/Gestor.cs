using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using AutoMapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using X.PagedList;
using Microsoft.AspNetCore.Http.HttpResults;
using FerramentariaTest.Helpers;
using System.Net.Http;
using AutoMapper.QueryableExtensions;
using System;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using OfficeOpenXml;
using NuGet.Protocol.Core.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing.Printing;
using FerramentariaTest.EntitySeek;
using FerramentariaTest.EntitiesBS;

using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;

namespace FerramentariaTest.Controllers
{

    public class Gestor : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thEstoque.aspx";
        private MapperConfiguration mapeamentoClasses;
        //private static int? GlobalPagination;
        //private static int? GlobalPageNumber;
        //private static int? GlobalPageNumberForSaidaEstoque;
        //private static int? GlobalFerramentariaValue;
        //private static string? GlobalFerramentariaName;
        //private static List<int> Inactivate;
        //private static List<int> TransferIds;

        private const string SessionKeyListGestor = "ListGestor";
        //private static List<SP_1600012731_EstoqueViewModel> _ListGestor = new List<SP_1600012731_EstoqueViewModel>();

        private const string SessionKeyListSaidaEstoque = "ListSaidaEstoque";
        //private static List<SaidaEstoqueViewModel> _ListSaidaEstoque = new List<SaidaEstoqueViewModel>();

        private const string SessionKeyGestorEditValues = "GestorEditValues";
        //private static GestorEdit GestorEditValues = new GestorEdit();

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeySearchFilters = "SearchFilters";
        //private static SearchGestorModel? StaticSearchFilters = new SearchGestorModel();


        private const string SessionKeyCombinedGestorModel = "CombinedGestorModel";
        //private static CombinedGestor? StaticCombinedGestor = new CombinedGestor();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        private static List<Empresa>? ListEmpressa = new List<Empresa>();
        private static List<Obra>? ListObra = new List<Obra>();

        public Gestor(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SP_1600012731_Estoque, SP_1600012731_EstoqueViewModel>();
                cfg.CreateMap<SP_1600012731_EstoqueViewModel, SP_1600012731_Estoque>();

                cfg.CreateMap<CatalogoLocal, CatalogoLocalViewModel>();
                cfg.CreateMap<CatalogoLocalViewModel, CatalogoLocal>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Gestor Region
        // GET: Gestor
        public ActionResult Index(string? Codigo, string? Item, string? AF, int? PAT)
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
                //usuario.Pagina1 = "thEstoque.aspx";
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

                            //if (LoggedUserDetails.Id == null)
                            //{
                            //    LoggedUserDetails = usuario;
                            //}

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
                                var nome = _context.Ferramentaria
                                            .Where(f => f.Id == FerramentariaValue)
                                            .Select(f => f.Nome)
                                            .FirstOrDefault();

                                ViewData["GlobalFerramentariaValue"] = nome;

                                httpContextAccessor?.HttpContext?.Session.SetString(Sessao.NomeFerramentaria, nome);
                                httpContextAccessor?.HttpContext?.Session.SetInt32(Sessao.IdFerramentaria, (int)FerramentariaValue);

                                //usuario.NomeFerramentaria = nome;
                                //usuario.IdFerramentaria = FerramentariaValue;
                                //GlobalFerramentariaName = nome;
                                ViewBag.FerramentariaName = nome;
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

        public ActionResult TestPage(int? page)
        {
            //Log log = new Log();
            //log.LogWhat = pagina + "/Index";
            //log.LogWhere = pagina;
            //Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

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

            //GlobalPageNumber = pageNumber;

            try
            {
                List<SP_1600012731_EstoqueViewModel>? ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();
                var SearchFilterModel = httpContextAccessor?.HttpContext?.Session.GetObject<SearchGestorModel?>(SessionKeySearchFilters) ?? new SearchGestorModel();

                CombinedGestorList test = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();

                string? FerramentariaNome = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria);

                int pageSize = SearchFilterModel.Pagination ?? 10;
                int pageNumber = (page ?? 1);

                IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = ListGestorModel.ToPagedList(pageNumber, pageSize);

                CombinedGestor combinedGestor = new CombinedGestor
                {
                    SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                    SearchGestorModel = SearchFilterModel,
                    //LoggedUserDetails = usuariofer ?? new VW_Usuario_NewViewModel(),
                    NomeFerramentaria = FerramentariaNome,
                    ResultCount = ListGestorModel.Count()
                };

                CombinedGestorList combinedGestorListSess = new CombinedGestorList
                {
                    SP_1600012731_EstoqueViewModelList = ListGestorModel,
                    SearchGestorModel = SearchFilterModel,
                    NomeFerramentaria = FerramentariaNome,
                    ResultCount = ListGestorModel.Count,
                    PageNumber = pageNumber,
                    Pagination = pageSize
                };

                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyListGestor, ListGestorModel);


                //httpContextAccessor?.HttpContext?.Session.Remove(SessionKeyCombinedGestorModel);
                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyCombinedGestorModel, combinedGestorListSess);

                //StaticCombinedGestor = combinedGestor;

                return View(nameof(Index), combinedGestor);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(nameof(Index));
            }

          
        }

        public ActionResult ToEdit(int? id)
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
                //usuario.Pagina1 = "thEstoque.aspx";
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

                            string? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.FerramentariaNome);
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

                            var CombinedGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();
                            //CombinedGestor combinedGestor = new CombinedGestor();

                            int pageSize = CombinedGestorModel.Pagination ?? 10;
                            int pageNumber = CombinedGestorModel.PageNumber ?? 1;

                            IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.ToPagedList(pageNumber, pageSize);

                            var combinedGestor = new CombinedGestor
                            {
                                SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                                SearchGestorModel = CombinedGestorModel.SearchGestorModel,
                                NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                ResultCount = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.Count,
                            };


                            //var CombinedGestorModel = HttpContext.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();
                            if (id != null)
                            {
                                var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();
                                var GetData = ListGestorModel.FirstOrDefault(i => i.IdProduto == id);
                                if (GetData != null)
                                {
                                    if (GetData.Ativo == 0)
                                    {
                                        if (!string.IsNullOrEmpty(GetData.Extraviado))
                                        {
                                            //yellowgreen
                                            ComunicadoExtraviadoModel? combinedValuesComunicadoExtraviado = new ComunicadoExtraviadoModel();

                                            //Getting Data for the table
                                            combinedValuesComunicadoExtraviado.ComunicadoExtraviadoValue = GetData;

                                            //Getting Data for Quantidade,Justificativa and DataRegistro from the table of ProdutoExtraviado
                                            ProdutoExtraviado? produtoExtraviadoValue = _context.ProdutoExtraviado.FirstOrDefault(i => i.IdProdutoAlocado == GetData.IdProdutoAlocado);
                                            combinedValuesComunicadoExtraviado.Quantidade = produtoExtraviadoValue?.Quantidade;
                                            combinedValuesComunicadoExtraviado.Justificativa = produtoExtraviadoValue?.Observacao;
                                            combinedValuesComunicadoExtraviado.DataRegistro = produtoExtraviadoValue?.DataRegistro;

                                            //Getting the Data for the IdUsuario
                                            UsuarioBS? UsuarioDetails = _contextBS.Usuario.FirstOrDefault(i => i.Id == produtoExtraviadoValue.IdUsuario);
                                            combinedValuesComunicadoExtraviado.Comunicado = searches.GetEmployeeDetails(UsuarioDetails?.Chapa);

                                            //Getting the Data for the Solicitante
                                            ProdutoAlocado? produtoAlocadoValue = _context.ProdutoAlocado.FirstOrDefault(i => i.Id == GetData.IdProdutoAlocado);
                                            combinedValuesComunicadoExtraviado.Colaborador = searches.GetEmployeeDetails(produtoAlocadoValue?.Solicitante_Chapa);

                                            //Getting the Data for Qtd Emp and DataEmprestimo
                                            combinedValuesComunicadoExtraviado.QuantidadeEmprestimo = produtoAlocadoValue?.Quantidade;
                                            combinedValuesComunicadoExtraviado.DataEmprestimo = produtoAlocadoValue?.DataEmprestimo;

                                            ViewBag.ComunicadoExtraviado = combinedValuesComunicadoExtraviado;

                                            return View(nameof(Index), combinedGestor);

                                        }
                                        else
                                        {
                                            //Tomato
                                            VW_1600013295_ProdutoExcluido? GetProdutoExcluido = _context.VW_1600013295_ProdutoExcluido.FirstOrDefault(i => i.IdProduto == GetData.IdProduto);

                                            if (GetProdutoExcluido != null)
                                            {
                                                SP_1600012731_EstoqueViewModel? EstoqueComunicadoInativo = GetData;

                                                ComunicadoInativoModel? combinedValuesComunicadoInativo = new ComunicadoInativoModel();
                                                combinedValuesComunicadoInativo.ComunicadoInativoValue = EstoqueComunicadoInativo;
                                                combinedValuesComunicadoInativo.Tipo = GetProdutoExcluido.Tipo;
                                                combinedValuesComunicadoInativo.Justificativa = GetProdutoExcluido.Justificativa;
                                                combinedValuesComunicadoInativo.DataOcorrencia = GetProdutoExcluido.DataOcorrencia;

                                                //VW_Usuario_New UsuarioDetails = _contextBS.VW_Usuario_New.FirstOrDefault(i => i.Id == GetProdutoExcluido.IdResponsavel);
                                                UsuarioBS? UsuarioDetails = _contextBS.Usuario.FirstOrDefault(i => i.Id == GetProdutoExcluido.IdResponsavel);
                                                combinedValuesComunicadoInativo.UsuarioModel = searches.GetEmployeeDetails(UsuarioDetails?.Chapa);

                                                ViewBag.ComunicadoInativo = combinedValuesComunicadoInativo;

                                                return View(nameof(Index), combinedGestor);
                                            }
                                            else
                                            {
                                                ViewBag.Error = $"No data found in the VW_1600013295_ProdutoExcluido for this Product {GetData.Codigo}";
                                                return View(nameof(Index), combinedGestor);
                                            }

                                            //SP_1600012731_EstoqueViewModel? EstoqueComunicadoInativo = GetData;

                                            //ComunicadoInativoModel? combinedValuesComunicadoInativo = new ComunicadoInativoModel();
                                            //combinedValuesComunicadoInativo.ComunicadoInativoValue = EstoqueComunicadoInativo;
                                            //combinedValuesComunicadoInativo.Tipo = GetProdutoExcluido.Tipo;
                                            //combinedValuesComunicadoInativo.Justificativa = GetProdutoExcluido.Justificativa;
                                            //combinedValuesComunicadoInativo.DataOcorrencia = GetProdutoExcluido.DataOcorrencia;

                                            //VW_Usuario_New UsuarioDetails = _contextBS.VW_Usuario_New.FirstOrDefault(i => i.Id == GetProdutoExcluido.IdResponsavel);
                                            //combinedValuesComunicadoInativo.UsuarioModel = searches.GetEmployeeDetails(UsuarioDetails.Chapa);

                                            //ViewBag.ComunicadoInativo = combinedValuesComunicadoInativo;

                                            //return View(nameof(Index), CombinedGestorModel);
                                        }
                                    }
                                    else
                                    {
                                        var GetProdutoForEmpresaName = _context.Produto.FirstOrDefault(e => e.Id == id);
                                        int? EmpressaValue = 0;
                                        DateTime? DateAquisicao = null;
                                        decimal? DCValor = null;
                                        string? DCFornecedor = null;
                                        string? DCAssetNumber = null;
                                        string? GCContrato = null;
                                        DateTime? GCDataInicio = null;
                                        int? GCIdObra = null;
                                        string? GCOC = null;
                                        string? GCNFSaida = null;
                                        DateTime? GCDataSaida = null;
                                        string? Descricao = null;

                                        List<FerramentariaViewModel?> LiberadorFerramentaria = (from f in _context.Ferramentaria
                                                                                                where f.Ativo == 1 &&
                                                                                                      _context.FerramentariaVsLiberador.Any(fl => fl.IdLogin == loggedUser.Id && fl.IdFerramentaria == f.Id)
                                                                                                orderby f.Nome
                                                                                                select new FerramentariaViewModel
                                                                                                {
                                                                                                    Id = f.Id,
                                                                                                    Nome = f.Nome,
                                                                                                    DataRegistro = f.DataRegistro,
                                                                                                    Ativo = f.Ativo
                                                                                                }).ToList();

                                        bool SaveButton = LiberadorFerramentaria.Any(f => f.Nome == GetData.Ferramentaria);

                                        if (GetProdutoForEmpresaName != null)
                                        {
                                            EmpressaValue = GetProdutoForEmpresaName.IdEmpresa;
                                            DateAquisicao = GetProdutoForEmpresaName.DC_DataAquisicao.HasValue ? GetProdutoForEmpresaName.DC_DataAquisicao : null;
                                            DCValor = GetProdutoForEmpresaName.DC_Valor != null ? GetProdutoForEmpresaName.DC_Valor : null;
                                            DCFornecedor = GetProdutoForEmpresaName.DC_Fornecedor != null ? GetProdutoForEmpresaName.DC_Fornecedor : null;
                                            DCAssetNumber = GetProdutoForEmpresaName.DC_AssetNumber != null ? GetProdutoForEmpresaName.DC_AssetNumber : null;
                                            GCContrato = GetProdutoForEmpresaName.GC_Contrato != null ? GetProdutoForEmpresaName.GC_Contrato : null;
                                            GCDataInicio = GetProdutoForEmpresaName.GC_DataInicio.HasValue ? GetProdutoForEmpresaName.GC_DataInicio : null;
                                            GCIdObra = GetProdutoForEmpresaName.GC_IdObra != null ? GetProdutoForEmpresaName.GC_IdObra : null;
                                            GCOC = GetProdutoForEmpresaName.GC_OC != null ? GetProdutoForEmpresaName.GC_OC : null;
                                            GCNFSaida = GetProdutoForEmpresaName.GC_NFSaida != null ? GetProdutoForEmpresaName.GC_NFSaida : null;
                                            GCDataSaida = GetProdutoForEmpresaName.GC_DataSaida.HasValue ? GetProdutoForEmpresaName.GC_DataSaida : null;

                                            if (GetProdutoForEmpresaName.IdCatalogo != null)
                                            {
                                                var GetCatalogoDetails = _context.Catalogo.FirstOrDefault(e => e.Id == GetProdutoForEmpresaName.IdCatalogo);

                                                Descricao = GetCatalogoDetails.Descricao != null ? GetCatalogoDetails.Descricao : null;
                                            }
                                        }

                                        GestorEdit FormEstoque = new GestorEdit
                                        {
                                            CatalogoType = GetData.Catalogo,
                                            CategoriaNome = GetData.Tipo,
                                            CatalogoNome = GetData.Item,
                                            ClasseNome = GetData.Classe,
                                            CatalogoCodigo = GetData.Codigo,
                                            CatalogoDescricao = Descricao,
                                            Controle = GetData.Controle,
                                            AfSerial = GetData.AfSerial,
                                            PAT = GetData.PAT,
                                            Serie = GetData.Serie,
                                            Selo = GetData.Selo,
                                            Empresa = EmpressaValue,
                                            DatadeVencimento = GetData.DatadeVencimento,
                                            Certificado = GetData.Certificado,
                                            Saldo = GetData.Saldo,
                                            QuantidadeMinima = GetData.QuantidadeMinima,
                                            Observacao = GetProdutoForEmpresaName.Observacao,
                                            RFM = GetData.RFM,
                                            DataAquisicao = DateAquisicao,
                                            Valor = DCValor,
                                            AssetNumber = DCAssetNumber,
                                            Fornecedor = DCFornecedor,
                                            Contrato = GCContrato,
                                            DataInicio = GCDataInicio,
                                            Obra = GCIdObra,
                                            OC = GCOC,
                                            DataSaida = GCDataSaida,
                                            NFSaida = GCNFSaida,
                                            IdProduto = GetData.IdProduto,
                                            IdProdutoAlocado = GetData.IdProdutoAlocado != null ? GetData.IdProdutoAlocado : 0,
                                            IdCatalogo = GetProdutoForEmpresaName.IdCatalogo,
                                            FerramentariaProductValue = GetData.Ferramentaria,
                                            FerramentariaUserValue = FerramentariaValue,
                                            SaveButton = SaveButton
                                        };

                                        if (FormEstoque.IdProdutoAlocado != 0)
                                        {
                                            ProdutoAlocado? details = _context.ProdutoAlocado.FirstOrDefault(i => i.Id == FormEstoque.IdProdutoAlocado);

                                            if (details != null)
                                            {
                                                UserViewModel? UsuarioModel = searches.SearchSolicitanteLoad(details.Solicitante_Chapa);
                                                EmprestadoModel forviews = new EmprestadoModel
                                                {
                                                    DataEmprestimo = details.DataEmprestimo,
                                                    Matricula = UsuarioModel.Chapa,
                                                    Nome = UsuarioModel.Nome,
                                                    Funcao = UsuarioModel.Funcao,
                                                    Secao = UsuarioModel.Secao,
                                                    Image = UsuarioModel.Image,
                                                };

                                                ViewBag.BorrowerDetails = forviews;
                                            }

                                        }

                                        List<Empresa>? ddlEmpresa = _context.Empresa.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Nome).ToList() ?? new List<Empresa>();
                                        ListEmpressa = ddlEmpresa;
                                        ViewBag.ddlEmpresa = ddlEmpresa;

                                        List<Obra>? ddlObra = _context.Obra.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Nome).ToList() ?? new List<Obra>();
                                        ListObra = ddlObra;
                                        ViewBag.ddlObra = ddlObra;

                                        var mapper = mapeamentoClasses.CreateMapper();

                                        GestorEdit EstoqueViewModel = mapper.Map<GestorEdit>(FormEstoque);

                                        httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyGestorEditValues, EstoqueViewModel);
                                        //GestorEditValues = EstoqueViewModel;
                                        return View(EstoqueViewModel);
                                    }

                                }

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

                //Error handling
                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
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
                return View(nameof(ToEdit));
            }
        }

        public IActionResult SaveEdit(GestorEdit? GestorEditValues)
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
                //usuario.Pagina1 = "thEstoque.aspx";
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

                            List<string> validationErrors = ValidateData(GestorEditValues);

                            if (validationErrors.Any())
                            {
                                string[] errorslist = validationErrors.ToArray();

                                ViewBag.ErrorList = validationErrors;
                                ViewBag.ddlEmpresa = ListEmpressa;
                                ViewBag.ddlObra = ListObra;

                                return View("ToEdit", GestorEditValues);
                            }
                            else
                            {
                                //var validateQuantity = _context.Produto.FirstOrDefault(p => p.Id == GestorEditValues.IdProduto);

                                //Edits Edit = new Edits(_context, _contextBS, httpContextAccessor, _configuration);
                                //var EditedProduto = Edit.EditProduto(GestorEditValues);
                                int? OldQuantidade;
                                string? OldObservacao;

                                var EditedProduto = _context.Produto.Where(t => t.Id == GestorEditValues.IdProduto).FirstOrDefault();
                                if (EditedProduto != null)
                                {
                                    OldQuantidade = EditedProduto.Quantidade;
                                    OldObservacao = EditedProduto.Observacao;

                                    if (!string.IsNullOrEmpty(GestorEditValues.Selo))
                                    {
                                        EditedProduto.Selo = GestorEditValues.Selo;
                                    }
                                    if (GestorEditValues.DataAquisicao.HasValue)
                                    {
                                        EditedProduto.DC_DataAquisicao = GestorEditValues.DataAquisicao;
                                    }
                                    if (GestorEditValues.QuantidadeMinima != null)
                                    {
                                        EditedProduto.QuantidadeMinima = GestorEditValues.QuantidadeMinima;
                                    }
                                    if (GestorEditValues.Valor != null)
                                    {
                                        EditedProduto.DC_Valor = GestorEditValues.Valor;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.AssetNumber))
                                    {
                                        EditedProduto.DC_AssetNumber = GestorEditValues.AssetNumber;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.Fornecedor))
                                    {
                                        EditedProduto.DC_Fornecedor = GestorEditValues.Fornecedor;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.Contrato))
                                    {
                                        EditedProduto.GC_Contrato = GestorEditValues.Contrato;
                                    }
                                    if (GestorEditValues.DataInicio.HasValue)
                                    {
                                        EditedProduto.GC_DataInicio = GestorEditValues.DataInicio.Value;
                                    }
                                    if (GestorEditValues.Obra != null)
                                    {
                                        EditedProduto.GC_IdObra = GestorEditValues.Obra;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.OC))
                                    {
                                        EditedProduto.GC_OC = GestorEditValues.OC;
                                    }
                                    if (GestorEditValues.DataSaida.HasValue)
                                    {
                                        EditedProduto.GC_DataSaida = GestorEditValues.DataSaida.Value;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.NFSaida))
                                    {
                                        EditedProduto.GC_NFSaida = GestorEditValues.NFSaida;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.AfSerial))
                                    {
                                        if (EditedProduto.AF != GestorEditValues.AfSerial)
                                        {
                                            LogProduto LogProduct = new LogProduto()
                                            {
                                                IdProduto = GestorEditValues.IdProduto,
                                                AfDe = EditedProduto.AF,
                                                AfPara = GestorEditValues.AfSerial,
                                                IdUsuario = loggedUser.Id,
                                                Acao = 2,
                                                DataRegistro = DateTime.Now
                                            };

                                            _context.Add(LogProduct);

                                        }

                                        EditedProduto.AF = GestorEditValues.AfSerial;



                                    }
                                    if (GestorEditValues.PAT != null)
                                    {
                                        if (GestorEditValues.PAT != 0)
                                        {
                                            if (EditedProduto.PAT != GestorEditValues.PAT)
                                            {
                                                LogProduto LogProduct = new LogProduto()
                                                {
                                                    IdProduto = GestorEditValues.IdProduto,
                                                    PatDe = EditedProduto.PAT,
                                                    PatPara = GestorEditValues.PAT,
                                                    IdUsuario = loggedUser.Id,
                                                    Acao = 2,
                                                    DataRegistro = DateTime.Now
                                                };

                                                _context.Add(LogProduct);
                                            }
                                        }

                                        EditedProduto.PAT = GestorEditValues.PAT;
                                    }
                                    if (GestorEditValues.Empresa != null)
                                    {
                                        EditedProduto.IdEmpresa = GestorEditValues.Empresa;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.Observacao))
                                    {
                                        EditedProduto.Observacao = GestorEditValues.Observacao;
                                    }
                                    if (GestorEditValues.DatadeVencimento.HasValue)
                                    {
                                        EditedProduto.DataVencimento = GestorEditValues.DatadeVencimento.Value;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.Certificado))
                                    {
                                        EditedProduto.Certificado = GestorEditValues.Certificado;
                                    }
                                    if (!string.IsNullOrEmpty(GestorEditValues.Serie))
                                    {
                                        EditedProduto.Serie = GestorEditValues.Serie;
                                    }
                                    if (GestorEditValues.QuantidadeMinima != null)
                                    {
                                        EditedProduto.QuantidadeMinima = GestorEditValues.QuantidadeMinima;
                                    }

                                    _context.Update(EditedProduto);
                                    _context.SaveChanges();

                                }
                                else
                                {
                                    ViewBag.Error = "No produto found";
                                    ViewBag.ddlEmpresa = ListEmpressa;
                                    ViewBag.ddlObra = ListObra;
                                    return View("ToEdit", GestorEditValues);
                                }


                                var InsertLogProduto = new LogProduto
                                {
                                    IdProduto = EditedProduto.Id,
                                    Acao = 2,
                                    IdUsuario = loggedUser.Id,
                                    QuantidadeMinimaDe = GestorEditValues.QuantidadeMinima != OldQuantidade ? OldQuantidade : null,
                                    QuantidadeMinimaPara = GestorEditValues.QuantidadeMinima != OldQuantidade ? GestorEditValues.QuantidadeMinima : null,
                                    ObservacaoDe = GestorEditValues.Observacao != EditedProduto.Observacao ? OldObservacao : null,
                                    ObservacaoPara = GestorEditValues.Observacao != EditedProduto.Observacao ? GestorEditValues.Observacao : null,
                                    DataRegistro = DateTime.Now
                                };

                                _context.LogProduto.Add(InsertLogProduto);
                                _context.SaveChanges();

                                if (InsertLogProduto.Id != null)
                                {
                                    //TempData["ShowSuccessAlert"] = true;
                                    ViewBag.ShowSuccessAlert = true;
                                    ViewBag.ddlEmpresa = ListEmpressa;
                                    ViewBag.ddlObra = ListObra;

                                    GestorEdit? NewGestorEdit = RefreshData(GestorEditValues.IdProduto);

                                    return View("ToEdit", NewGestorEdit);
                                }
                                else
                                {
                                    ViewBag.Error = "Log Not Inserted";
                                    ViewBag.ddlEmpresa = ListEmpressa;
                                    ViewBag.ddlObra = ListObra;
                                    return View("ToEdit", GestorEditValues);
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

       

                //ViewBag.ddlEmpresa = ListEmpressa;
                //ViewBag.ddlObra = ListObra;
                //return View("ToEdit", GestorEditValues);
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
                return View(nameof(ToEdit));
            }      
        }

        public GestorEdit RefreshData(int? IdProduto)
        {

            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            GestorProduto? produto = searches.SearchProductGestor(IdProduto);
            if (produto != null)
            {
                GestorEdit? Refresh = new GestorEdit();
                switch (produto.CategoriaClasse)
                {
                    case 1:
                        Refresh.CatalogoType = "Ferramentas";
                        break;
                    case 2:
                        Refresh.CatalogoType = "EPI";
                        break;
                    case 3:
                        Refresh.CatalogoType = "Consumiveis";
                        break;
                }

                Refresh.IdProduto = produto.Id;
                Refresh.CategoriaNome = produto.CategoriaNome;
                Refresh.CatalogoNome = produto.CatalogoNome;
                Refresh.ClasseNome = _context.Categoria.Where(i => i.Id == produto.IdCategoriaPai && i.Classe == produto.CategoriaClasse && i.IdCategoria == 0).Select(i => i.Nome).FirstOrDefault();
                Refresh.CatalogoCodigo = produto.CatalogoCodigo;
                Refresh.CatalogoDescricao = produto.CatalogoDescricao;

                if (produto.CatalogoPorAferido == 1 && produto.CatalogoPorMetro == 0 && produto.CatalogoPorSerial == 0)
                {
                    Refresh.Controle = "Aferido";
                }
                else if (produto.CatalogoPorAferido == 0 && produto.CatalogoPorMetro == 0 && produto.CatalogoPorSerial == 1)
                {
                    Refresh.Controle = "Serial";
                }
                else if (produto.CatalogoPorAferido == 0 && produto.CatalogoPorMetro == 1 && produto.CatalogoPorSerial == 0)
                {
                    Refresh.Controle = "Metro";
                }
                else if (produto.CatalogoPorAferido == 0 && produto.CatalogoPorMetro == 0 && produto.CatalogoPorSerial == 0)
                {
                    Refresh.Controle = "Quantidade";
                }

                Refresh.AfSerial = produto.AF;
                Refresh.PAT = produto.PAT;
                Refresh.Serie = produto.Serie;
                Refresh.Selo = produto.Selo;
                Refresh.Empresa = produto.EmpresaId;
                Refresh.DatadeVencimento = produto.DataVencimento;
                Refresh.Certificado = produto.Certificado;
                Refresh.Saldo = produto.Quantidade;
                Refresh.QuantidadeMinima = produto.QuantidadeMinima;
                Refresh.Observacao = produto.Observacao;
                Refresh.RFM = produto.RFM;
                Refresh.DataAquisicao = produto.DC_DataAquisicao;
                Refresh.Valor = produto.DC_Valor;
                Refresh.AssetNumber = produto.DC_AssetNumber;
                Refresh.Fornecedor = produto.DC_Fornecedor;
                Refresh.Contrato = produto.GC_Contrato;
                Refresh.DataInicio = produto.GC_DataInicio;
                Refresh.Obra = produto.GC_IdObra;
                Refresh.OC = produto.GC_OC;
                Refresh.DataSaida = produto.GC_DataSaida;
                Refresh.NFSaida = produto.GC_NFSaida;
                Refresh.IdProduto = produto.Id;
                Refresh.IdCatalogo = produto.CatalogoId;
                Refresh.FerramentariaProductValue = produto.FerramentariaNome;
                Refresh.FerramentariaUserValue = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria);

                List<FerramentariaViewModel?> LiberadorFerramentaria = (from f in _context.Ferramentaria
                                                                        where f.Ativo == 1 &&
                                                                              _context.FerramentariaVsLiberador.Any(fl => fl.IdLogin == loggedUser.Id && fl.IdFerramentaria == f.Id)
                                                                        orderby f.Nome
                                                                        select new FerramentariaViewModel
                                                                        {
                                                                            Id = f.Id,
                                                                            Nome = f.Nome,
                                                                            DataRegistro = f.DataRegistro,
                                                                            Ativo = f.Ativo
                                                                        }).ToList();

                bool SaveButton = LiberadorFerramentaria.Any(f => f.Nome == httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria));

                Refresh.SaveButton = SaveButton;

                return Refresh;
            }
            else
            {
                return new GestorEdit();
            }
        }

        public List<string> ValidateData(GestorEdit? GestorEditValues)
        {
            List<string> errors = new List<string>();

            if (GestorEditValues.QuantidadeMinima == null)
            {
                errors.Add("Quantidade Minima vazio");
            }

            if (GestorEditValues.AfSerial != null)
            {
                if (GestorEditValues.Controle != "Serial")
                {
                    var validateAF = _context.Produto.Where(p => p.AF != "N/C"
                                                     && p.AF == GestorEditValues.AfSerial
                                                     && p.IdCatalogo == GestorEditValues.IdCatalogo
                                                     && p.Id != GestorEditValues.IdProduto).ToList();

                    if (validateAF.Count != 0)
                    {
                        errors.Add("AF/Serial sendo utilizado por outro produto.");
                    }
                }        
            }

            if (GestorEditValues.Controle == "Aferido" || GestorEditValues.Controle == "Serial")
            {
                if (GestorEditValues.PAT != null)
                {
                    if(GestorEditValues.PAT != 0)
                    {
                        var validatePAT = _context.Produto.Where(p => p.PAT == GestorEditValues.PAT && p.Id != GestorEditValues.IdProduto).ToList();

                        if (validatePAT.Count != 0)
                        {
                            errors.Add("PAT já foi atribuido a outro produto.");
                        }
                    }
                }
                else
                {
                    errors.Add("PAT vazio.");
                }

                if (GestorEditValues.ClasseNome == "CALIBRADA" || GestorEditValues.ClasseNome == "PREVENTIVO")
                {
                    if (GestorEditValues.DatadeVencimento == null)
                    {
                        errors.Add("DATA DE VENCIMENTO invalida.");
                    }
                    else
                    {
                        TimeSpan timeDifference = GestorEditValues.DatadeVencimento.Value - DateTime.Now;

                        int daysDifference = timeDifference.Days;
                        if (daysDifference > 1826)
                        {
                            errors.Add("O limite da Data de Vencimento ultrapassou 1826 dias (5 anos).");
                        }
                    }
                }
            }

            if(GestorEditValues.Observacao != null && GestorEditValues.Observacao.Length > 250)
            {
                errors.Add(String.Format("<br> OBSERVAÇÃO com {0} excedido.", 250 - GestorEditValues.Observacao.Length));
            }

            if(GestorEditValues.DataInicio.HasValue && GestorEditValues.DataSaida.HasValue)
            {
                if(GestorEditValues.DataSaida.Value < GestorEditValues.DataInicio.Value)
                {
                    errors.Add("DATA INICIO maior que DATA SAIDA  da Gestão de Contratos.");
                }
            }

            return errors;
        }

        #region Stored Procedure

        public ActionResult SearchGestor(CombinedGestor CombinedGestor)
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


                            string? Catalogo = null;
                            string? Classe = null;
                            string? Tipo = null;
                            string? CodigoGestor = null;
                            string? ItemGestor = null;
                            string? AFGestor = null;
                            int? PATValue = 0;
                            string? NumeroGestor = null;
                            string? DateVencimentoValue = null;
                            string? Ferramentaria = null;
                            int? SerieNull = 0;
                            int? DataVencidasNull = 0;
                            int? StatusGestor = 0;
                            int? SaldoGestor = 0;
                            int? SituacaoGestor = 0;


                            if (CombinedGestor.SearchGestorModel.Catalogo != 0 && CombinedGestor.SearchGestorModel.Catalogo != null)
                            {
                                switch (CombinedGestor.SearchGestorModel.Catalogo)
                                {
                                    case 1:
                                        Catalogo = "Ferramentas";
                                        break;
                                    case 2:
                                        Catalogo = "EPI";
                                        break;
                                    case 3:
                                        Catalogo = "Consumiveis";
                                        break;
                                }

                            }

                            if (CombinedGestor.SearchGestorModel.Classe != 0 && CombinedGestor.SearchGestorModel.Classe != null)
                            {
                                var categoryName = _context.Categoria
                                            .Where(c => c.Id == CombinedGestor.SearchGestorModel.Classe)
                                            .Select(c => c.Nome)
                                            .FirstOrDefault();

                                Classe = categoryName;
                            }

                            if (CombinedGestor.SearchGestorModel.Tipo != 0 && CombinedGestor.SearchGestorModel.Tipo != null)
                            {
                                var tipoName = _context.Categoria
                                            .Where(c => c.Id == CombinedGestor.SearchGestorModel.Tipo && c.IdCategoria == CombinedGestor.SearchGestorModel.Classe)
                                            .Select(c => c.Nome)
                                            .FirstOrDefault();

                                Tipo = tipoName;
                            }

                            if (CombinedGestor.SearchGestorModel.Codigo != null)
                            {
                                CodigoGestor = CombinedGestor.SearchGestorModel.Codigo;
                            }

                            if (CombinedGestor.SearchGestorModel.Item != null)
                            {
                                ItemGestor = CombinedGestor.SearchGestorModel.Item;
                            }

                            if (CombinedGestor.SearchGestorModel.AF != null)
                            {
                                AFGestor = CombinedGestor.SearchGestorModel.AF;
                            }

                            if (CombinedGestor.SearchGestorModel.Numero != null)
                            {
                                NumeroGestor = CombinedGestor.SearchGestorModel.Numero;
                            }

                            if (CombinedGestor.SearchGestorModel.PAT != 0 && CombinedGestor.SearchGestorModel.PAT != null)
                            {
                                PATValue = CombinedGestor.SearchGestorModel.PAT;
                            }

                            if (CombinedGestor.SearchGestorModel.DataVencimento.HasValue)
                            {
                                DateVencimentoValue = CombinedGestor.SearchGestorModel.DataVencimento.Value.ToString("yyyy-MM-dd");
                            }

                            if (CombinedGestor.SearchGestorModel.SerieCheckbox != null)
                            {
                                SerieNull = CombinedGestor.SearchGestorModel.SerieCheckbox;
                            }

                            if (CombinedGestor.SearchGestorModel.VencidasCheck != null)
                            {
                                DataVencidasNull = CombinedGestor.SearchGestorModel.VencidasCheck;
                            }

                            if (CombinedGestor.SearchGestorModel.Status != null && CombinedGestor.SearchGestorModel.Status != 0)
                            {
                                StatusGestor = CombinedGestor.SearchGestorModel.Status;
                            }

                            if (CombinedGestor.SearchGestorModel.Saldo != null && CombinedGestor.SearchGestorModel.Saldo != 0)
                            {
                                SaldoGestor = CombinedGestor.SearchGestorModel.Saldo;
                            }

                            if (CombinedGestor.SearchGestorModel.Situacao != null)
                            {
                                SituacaoGestor = CombinedGestor.SearchGestorModel.Situacao;
                            }

                            if (CombinedGestor.SearchGestorModel.Opcional == false)
                            {
                                Ferramentaria = httpContextAccessor.HttpContext.Session.GetString(Sessao.FerramentariaNome);
                            }

                            var result = ExecuteYourStoredProcedure(Catalogo, Classe, Tipo, CodigoGestor, ItemGestor,
                                AFGestor, PATValue, NumeroGestor, DateVencimentoValue, Ferramentaria,
                                SerieNull, DataVencidasNull, StatusGestor, SaldoGestor, SituacaoGestor, out var bdTempoInicial, out var bdTempoFinal);

                            if (result.Count > 0)
                            {
                                var mapper = mapeamentoClasses.CreateMapper();
                                List<SP_1600012731_EstoqueViewModel> SP_1600012731Result = mapper.Map<List<SP_1600012731_EstoqueViewModel>>(result);


                                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyListGestor, SP_1600012731Result);
                                //_ListGestor = SP_1600012731Result;

                                //GlobalPagination = CombinedGestor.SearchGestorModel.Pagination;

                                //ViewData["GlobalFerramentariaValue"] = GlobalFerramentariaName;
                                //ViewBag.FerramentariaName = GlobalFerramentariaName;

                                //int? testget = GlobalPagination;
                                int pageSize = CombinedGestor.SearchGestorModel.Pagination ?? 10;
                                int pageNumber = 1;
                                //GlobalPageNumber = pageNumber;

                                ViewBag.ResultCount = SP_1600012731Result.Count;

                                IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = SP_1600012731Result.ToPagedList(pageNumber, pageSize);

                                SearchGestorModel? SearchFilter = CombinedGestor.SearchGestorModel ?? new SearchGestorModel();

                                httpContextAccessor?.HttpContext?.Session.Remove(SessionKeySearchFilters);
                                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeySearchFilters, SearchFilter);
                                //StaticSearchFilters = SearchFilter;

                                var combinedGestor = new CombinedGestor
                                {
                                    SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                                    SearchGestorModel = SearchFilter,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                    ResultCount = SP_1600012731Result.Count,
                                };

                                CombinedGestorList combinedGestorListSess = new CombinedGestorList
                                {
                                    SP_1600012731_EstoqueViewModelList = SP_1600012731Result,
                                    SearchGestorModel = SearchFilter,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                    ResultCount = SP_1600012731Result.Count,
                                    PageNumber = pageNumber,
                                    Pagination = pageSize
                                };

                                httpContextAccessor?.HttpContext?.Session.Remove(SessionKeyCombinedGestorModel);
                                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyCombinedGestorModel, combinedGestorListSess);

                                //StaticCombinedGestor = combinedGestor;
                                return View(nameof(Index), combinedGestor);
                            }
                            else
                            {
                                ViewBag.Error = "Nenhum resultado encontrado.";
                                return View(nameof(Index), CombinedGestor);
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
                ViewBag.Error = ex.Message;
                return View(nameof(Index));
            }       
           
		}


        private List<SP_1600012731_EstoqueViewModel> ExecuteYourStoredProcedure(
    string? catalogo,
    string? classe,
    string? tipo,
    string? codigo,
    string? item,
    string? afSerial,
    int? pat,
    string? serie,
    string? dataVencimento,
    string? ferramentaria,
    int? serieNull,
    int? dataVencimentoNull,
    int? status,
    int? saldo,
    int? situacao,
    out int bdTempoInicial,
    out int bdTempoFinal)
        {
            var parameters = new List<SqlParameter>
                {
                    // Create SqlParameter objects for each non-null parameter
                    CreateSqlParameter("@P_Catalogo", catalogo),
                    CreateSqlParameter("@P_Classe", classe),
                    CreateSqlParameter("@P_Tipo", tipo),
                    CreateSqlParameter("@P_Codigo", codigo),
                    CreateSqlParameter("@P_Item", item),
                    CreateSqlParameter("@P_AfSerial", afSerial),
                    CreateSqlParameter("@P_Pat", pat),
                    CreateSqlParameter("@P_Serie", serie),
                    CreateSqlParameter("@P_DataVencimento", dataVencimento),
                    CreateSqlParameter("@P_Ferramentaria", ferramentaria),
                    CreateSqlParameter("@P_SerieNull", serieNull),
                    CreateSqlParameter("@P_DataVencimentoNull", dataVencimentoNull),
                    CreateSqlParameter("@P_Status", status),
                    CreateSqlParameter("@P_Saldo", saldo),
                    CreateSqlParameter("@P_Situacao", situacao),
                };

            // Call the stored procedure using FromSqlRaw
            //var result = _context.SP_1600012731_Estoque
            //    .FromSqlRaw(BuildDynamicSql(parameters), parameters.ToArray())
            //    .ToList();

            var mapper = mapeamentoClasses.CreateMapper();

            var result = _context.SP_1600012731_Estoque
                   .FromSqlRaw(BuildDynamicSql(parameters), parameters.ToArray())
                   .AsEnumerable()
                     .Select(e => new SP_1600012731_EstoqueViewModel
                     {
                         Catalogo = e.Catalogo,
                         Classe = e.Classe,
                         Tipo = e.Tipo,
                         Codigo = e.Codigo,
                         Item = e.Item,
                         Controle = e.Controle,
                         AfSerial = e.AfSerial,
                         Serie = e.Serie,
                         PAT = e.PAT,
                         Saldo = e.Saldo,
                         QuantidadeMinima = e.QuantidadeMinima,
                         IdProduto = e.IdProduto,
                         DatadoRegistro = e.DatadoRegistro,
                         DatadeVencimento = e.DatadeVencimento,
                         NumerodeSerie = e.NumerodeSerie,
                         Selo = e.Selo,
                         Certificado = e.Certificado,
                         Observacao = e.Observacao,
                         RFM = e.RFM,
                         Ativo = e.Ativo,
                         Ferramentaria = e.Ferramentaria,
                         IdProdutoAlocado = e.IdProdutoAlocado,
                         Extraviado = e.Extraviado
                     })
                   .ToList();

            // Retrieve the output parameter values
            bdTempoInicial = (int)parameters[^2].Value;
            bdTempoFinal = (int)parameters[^1].Value;

            return result;
        }

        private string BuildDynamicSql(List<SqlParameter> parameters)
        {
            var sqlBuilder = new StringBuilder("SP_1600012731_Estoque ");

            // Add only non-null parameters to the SQL query
            var nonNullParameters = parameters
                .Where(p => p.Value != null)
                .ToList();

            for (int i = 0; i < nonNullParameters.Count; i++)
            {
                sqlBuilder.Append($"{nonNullParameters[i].ParameterName}");

                if (i < nonNullParameters.Count - 1)
                {
                    sqlBuilder.Append(", ");
                }
            }

            return sqlBuilder.ToString();
        }



        private SqlParameter CreateSqlParameter(string parameterName, object value)
        {
            //if (value is DateTime dateTimeValue)
            //{
            //    // If the value is DateTime, convert it to the appropriate string format
            //    value = dateTimeValue.ToString("yyyy-MM-dd");
            //}

            return new SqlParameter(parameterName, value ?? DBNull.Value);
        }

        #endregion

        #region Dropdown List
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
             .Where(entity => entity.IdCategoria == selectedValue && entity.IdCategoria != 0)
             .OrderBy(entity => entity.Nome)
             .ToList();

            return Json(query);
        }

        #endregion

        #region Buttons Below

        public ActionResult Inativar(List<int>? selectedIds)
        {
            var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();
            var CombinedGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();
            //CombinedGestor combinedGestor = new CombinedGestor();

            int pageSize = CombinedGestorModel .Pagination ?? 10;
            int pageNumber = CombinedGestorModel.PageNumber ?? 1;

            IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.ToPagedList(pageNumber, pageSize);

            var combinedGestor = new CombinedGestor
            {
                SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                SearchGestorModel = CombinedGestorModel.SearchGestorModel,
                NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                ResultCount = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.Count,
            };

            if (selectedIds.Count > 0)
            {
                //Inactivate = selectedIds;
                var query = _context.TipoExclusao.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Id).ToList();
                var ListToInactivate = ListGestorModel.Where(item => selectedIds.Contains(item.IdProduto.GetValueOrDefault())).ToList();

                ViewBag.ListToInactivate = ListToInactivate;
                ViewBag.TipoExclusao = query;
                ViewBag.OpenInativarModal = true;

                return View(nameof(Index), combinedGestor);
            }
            else
            {
                ViewBag.Error = "Nenhum item selecionado";
                return View(nameof(Index), combinedGestor);
            }
        }

        public ActionResult Transferir(List<int>? selectedIds)
        {
            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            var CombinedGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();
            var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();

            int pageSize = CombinedGestorModel.Pagination ?? 10;
            int pageNumber = CombinedGestorModel.PageNumber ?? 1;

            IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.ToPagedList(pageNumber, pageSize);

            var combinedGestor = new CombinedGestor
            {
                SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                SearchGestorModel = CombinedGestorModel.SearchGestorModel,
                NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                ResultCount = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.Count,
            };

            if (selectedIds.Count > 0)
            {
                int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
                int? UsuarioLoginId = loggedUser.Id;

                var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                          where ferramentaria.Ativo == 1 &&
                                                !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                _context.FerramentariaVsLiberador.Any(l => l.IdLogin == UsuarioLoginId && l.IdFerramentaria == ferramentaria.Id)
                                          orderby ferramentaria.Nome
                                          select new
                                          {
                                              Id = ferramentaria.Id,
                                              Nome = ferramentaria.Nome
                                          }).ToList();

                //var query = _context.Ferramentaria.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Nome).ToList();
                ferramentariaItems.RemoveAll(it => it.Id == FerramentariaValue);

                List<SP_1600012731_EstoqueViewModel?>? ListToTransfer = ListGestorModel.Where(item => selectedIds.Contains(item.IdProduto.GetValueOrDefault())).ToList();

                ViewBag.ListToTransfer = ListToTransfer;
                ViewBag.FerramentariaList = ferramentariaItems;
                ViewBag.OpenTransferirModal = true;


                return View(nameof(Index), combinedGestor);
            }
            else
            {
                ViewBag.Error = "Nenhum item selecionado";
                return View(nameof(Index), combinedGestor);
            }       
        }

        public ActionResult Export(int id)
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

                var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();

                //var listGestor = GlobalGestor.ListGestor;
                var listGestor = ListGestorModel;

                DataTable dataTable = new DataTable();  
                dataTable.Columns.Add("Ferramentaria");
                dataTable.Columns.Add("IdProduto");
                dataTable.Columns.Add("Catalogo");
                dataTable.Columns.Add("Classe");
                dataTable.Columns.Add("Tipo");
                dataTable.Columns.Add("Codigo");
                dataTable.Columns.Add("Item");
                dataTable.Columns.Add("AfSerial");
                dataTable.Columns.Add("PAT");
                dataTable.Columns.Add("Saldo");
                dataTable.Columns.Add("QuantidadeMinima");
                dataTable.Columns.Add("Controle");
                dataTable.Columns.Add("DatadoRegistro", typeof(DateTime)); //13
                //dataTable.Columns.Add("DatadoRegistro");
                dataTable.Columns.Add("DatadeVencimento", typeof(DateTime)); //14
                //dataTable.Columns.Add("DatadeVencimento");
                dataTable.Columns.Add("NumerodeSerie");
                dataTable.Columns.Add("Selo");
                dataTable.Columns.Add("Certificado");
                dataTable.Columns.Add("Observacao");
                dataTable.Columns.Add("RFM");
                dataTable.Columns.Add("DCProprietario");
                dataTable.Columns.Add("DCDataAquisicao");
                dataTable.Columns.Add("DCValor");
                dataTable.Columns.Add("DCAssetNumber");
                dataTable.Columns.Add("DCFornecedor");
                dataTable.Columns.Add("GCContrato");
                dataTable.Columns.Add("GCDatadeInicio");
                dataTable.Columns.Add("GCObra");
                dataTable.Columns.Add("GCOC");
                dataTable.Columns.Add("GCDatadeSaida");
                dataTable.Columns.Add("GCNFdeSaida");
                dataTable.Columns.Add("IdProdutoAlocado");
                dataTable.Columns.Add("Extraviado");
                dataTable.Columns.Add("Ativo");


                // Add data rows from listDevolucao
                foreach (var item in listGestor)
                {
                    var row = dataTable.NewRow();
                    row["Ferramentaria"] = item.Ferramentaria; 
                    row["IdProduto"] = item.IdProduto;
                    row["Catalogo"] = item.Catalogo;
                    row["Classe"] = item.Classe;
                    row["Tipo"] = item.Tipo;
                    row["Codigo"] = item.Codigo;
                    row["Item"] = item.Item;
                    row["AfSerial"] = item.AfSerial;
                    row["PAT"] = item.PAT;
                    row["Saldo"] = item.Saldo;
                    row["QuantidadeMinima"] = item.QuantidadeMinima;
                    row["Controle"] = item.Controle;
                    row["DatadoRegistro"] = item.DatadoRegistro.HasValue == true ? item.DatadoRegistro.Value : DBNull.Value;
                    //row["DatadoRegistro"] = item.DatadoRegistro;
                    row["DatadeVencimento"] = item.DatadeVencimento.HasValue == true ? item.DatadeVencimento.Value : DBNull.Value;
                    //row["DatadeVencimento"] = item.DatadeVencimento;
                    row["NumerodeSerie"] = item.NumerodeSerie; 
                    row["Selo"] = item.Selo;
                    row["Certificado"] = item.Certificado;
                    row["Observacao"] = item.Observacao;
                    row["RFM"] = item.RFM;
                    row["DCProprietario"] = "";
                    row["DCDataAquisicao"] = "";
                    row["DCValor"] = "";
                    row["DCAssetNumber"] = "";
                    row["DCFornecedor"] = item.Selo;
                    row["GCContrato"] = "";
                    row["GCDatadeInicio"] = "";
                    row["GCObra"] = "";
                    row["GCOC"] = "";
                    row["GCDatadeSaida"] = "";
                    row["GCNFdeSaida"] = "";
                    row["IdProdutoAlocado"] = item.IdProdutoAlocado;
                    row["Extraviado"] = item.Extraviado;
                    row["Ativo"] = item.Ativo;

                    dataTable.Rows.Add(row);
                }

                //string? Arquivo = String.Format("Estoque_{0}_{1}.xls", DateTime.Now.ToString("yyyyMMddHHmmmss"), usuariofer.Id);
                //string? virtualpath = "http://ferramentaria.kfelsbrasil.com.br/Repositorio/Relatorio/" + Arquivo;
                //string? basePath = "C:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

                using (var memoryStream = new MemoryStream())
                {
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Estoque");
                        worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                        worksheet.Cells.AutoFitColumns();
                        worksheet.Column(13).Style.Numberformat.Format = "dd/MM/yyyy";
                        worksheet.Column(14).Style.Numberformat.Format = "dd/MM/yyyy";
                        package.Save();
                    }


                    memoryStream.Position = 0;
                    byte[] content = memoryStream.ToArray();
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string? Arquivo = String.Format("Estoque_{0}_{1}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmmss"), loggedUser.Id);

                    FileContentResult fileResult = File(content, contentType, Arquivo);
                    return File(content, contentType, Arquivo);
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

        #endregion

        #region Actions Button Below

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InactivateAction(List<int>? ProdutoId,int? TipoExclusao,string? justificativa)
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
                //    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //}
                //else
                //{
                //    if (usuariofer.Permissao.Excluir != 1)
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
                        if (checkPermission.Excluir == 1)
                        {


                            var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();
                            var CombinedGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();
                            string? error = ValidateInativa(ProdutoId, TipoExclusao, justificativa);
                            if (string.IsNullOrEmpty(error))
                            {
                                foreach (var item in ProdutoId)
                                {

                                    var InsertToProdutoExcluido = new ProdutoExcluido
                                    {
                                        IdTipoExclusao = TipoExclusao,
                                        IdProduto = item,
                                        Observacao = justificativa,
                                        IdUsuario = loggedUser.Id,
                                        DataRegistro = DateTime.Now,
                                    };

                                    _context.Add(InsertToProdutoExcluido);
                                    _context.SaveChanges();

                                    var produto = _context.Produto.Where(t => t.Id == item).FirstOrDefault();
                                    if (produto != null)
                                    {
                                        produto.Ativo = 0;
                                    }
                                    _context.SaveChanges();

                                    //Inactivate.Remove(item);
                                    //GlobalGestor.ListGestor.RemoveAll(it => it.IdProduto == ToExcludeId);


                                    ListGestorModel.RemoveAll(it => it.IdProduto == item);

                                    httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyListGestor, ListGestorModel);

                                }

                                ViewBag.ShowSuccessAlert = true;

                                var SearchFilterModel = httpContextAccessor?.HttpContext?.Session.GetObject<SearchGestorModel?>(SessionKeySearchFilters) ?? new SearchGestorModel();

                                CombinedGestor combinedGestor = new CombinedGestor
                                {
                                    SearchGestorModel = SearchFilterModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                };

                                CombinedGestorList combinedGestorListSess = new CombinedGestorList
                                {
                                    SearchGestorModel = SearchFilterModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                };

                                HttpContext.Session.SetObject(SessionKeyCombinedGestorModel, combinedGestorListSess);

                                //StaticCombinedGestor = combinedGestor;
                                return View(nameof(Index), combinedGestor);
                            }
                            else
                            {
                                ViewBag.ModalInativarError = error;
                                var query = _context.TipoExclusao.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Id).ToList();
                                var ListToInactivate = ListGestorModel.Where(item => ProdutoId.Contains(item.IdProduto.GetValueOrDefault())).ToList();
                                ViewBag.ListToInactivate = ListToInactivate;
                                ViewBag.TipoExclusao = query;
                                ViewBag.OpenInativarModal = true;

                                int pageSize = CombinedGestorModel.Pagination ?? 10;
                                int pageNumber = CombinedGestorModel.PageNumber ?? 1;

                                IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.ToPagedList(pageNumber, pageSize);

                                var combinedGestor = new CombinedGestor
                                {
                                    SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                                    SearchGestorModel = CombinedGestorModel.SearchGestorModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                    ResultCount = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.Count,
                                };

                                return View(nameof(Index), combinedGestor);
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

        public string ValidateInativa(List<int>? ProdutoId, int? TipoExclusao, string? justificativa)
        {
            if (TipoExclusao == 0)
            {
                return "Nenhum Tipo de Exclusão Selecionado.";
            }

            if (string.IsNullOrEmpty(justificativa))
            {
                return "Nenhum Justificativa.";
            }

            if (ProdutoId.Count == 0)
            {
                return "Nenhum Produto para Inativa.";
            }

            return null;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TransferirActionNew(List<TransferModel?>? TransferValues, int? Ferrmanetaria,string? Documento)
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
                //    if (usuariofer.Permissao.Inserir != 1)
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
                        if (checkPermission.Inserir == 1)
                        {


                            string? error = ValidateTransfer(TransferValues, Ferrmanetaria, Documento);
                            if (string.IsNullOrEmpty(error))
                            {
                                foreach (TransferModel item in TransferValues)
                                {
                                    GestorProduto? query = searches.SearchProductGestor(item.IdProduto);

                                    if (!(query.CatalogoPorAferido == 1 || query.CatalogoPorSerial == 1))
                                    {
                                        Produto produto = _context.Produto.Where(t => t.Id == item.IdProduto).FirstOrDefault();
                                        if (produto != null)
                                        {
                                            produto.Quantidade = produto.Quantidade - item.SaldoTransferir;
                                        }

                                        _context.SaveChanges();

                                        GestorProduto? InsideQuery = searches.SearchInside(produto, query, Ferrmanetaria);
                                        if (InsideQuery == null)
                                        {
                                            var InsertProduto = new Produto
                                            {
                                                IdCatalogo = query.CatalogoId,
                                                IdFerramentaria = Ferrmanetaria,
                                                Quantidade = item.SaldoTransferir,
                                                QuantidadeMinima = query.QuantidadeMinima,
                                                Localizacao = query.Localizacao,
                                                RFM = "TRANSFERENCIA",
                                                Observacao = query.Observacao,
                                                Ativo = 1,
                                                DataRegistro = DateTime.Now,
                                            };

                                            _context.Add(InsertProduto);
                                            _context.SaveChanges();

                                            var prodDebito = _context.Produto.FirstOrDefault(i => i.Id == query.Id);

                                            if (query.Id != 0)
                                            {
                                                var InsertToLogProduto = new LogProduto
                                                {
                                                    IdProduto = item.IdProduto,
                                                    IdUsuario = loggedUser.Id,
                                                    QuantidadeDe = prodDebito.Quantidade,
                                                    QuantidadePara = prodDebito.Quantidade + item.SaldoTransferir,
                                                    Acao = 2,
                                                    DataRegistro = DateTime.Now,
                                                };
                                                _context.Add(InsertToLogProduto);
                                                _context.SaveChanges();
                                            }

                                            var InsertToLogProduto2 = new LogProduto
                                            {
                                                IdProduto = item.IdProduto,
                                                IdUsuario = loggedUser.Id,
                                                QuantidadePara = item.SaldoTransferir,
                                                Acao = 1,
                                                DataRegistro = DateTime.Now,
                                            };
                                            _context.Add(InsertToLogProduto2);
                                            _context.SaveChanges();

                                            var InsertToHistoricoTransferencia = new HistoricoTransferencia
                                            {
                                                IdProduto = InsertProduto.Id,
                                                IdUsuario = loggedUser.Id,
                                                IdFerramentariaOrigem = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                                IdFerramentariaDestino = Ferrmanetaria,
                                                DataOcorrencia = DateTime.Now,
                                                Quantidade = item.SaldoTransferir,
                                                Documento = Documento
                                            };
                                            _context.Add(InsertToHistoricoTransferencia);
                                            _context.SaveChanges();

                                            var InsertToLogEntradaSaida = new LogEntradaSaidaInsert
                                            {
                                                IdFerramentaria = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                                IdProduto = item.IdProduto,
                                                Quantidade = item.SaldoTransferir * -1,
                                                Rfm = "TRANSFERENCIA",
                                                Observacao = "ORIGEM",
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };
                                            _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida);
                                            _context.SaveChanges();


                                            var InsertToLogEntradaSaida2 = new LogEntradaSaidaInsert
                                            {
                                                IdFerramentaria = Ferrmanetaria,
                                                IdProduto = InsertProduto.Id,
                                                Quantidade = item.SaldoTransferir,
                                                Rfm = "TRANSFERENCIA",
                                                Observacao = "DESTINO",
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };
                                            _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida2);
                                            _context.SaveChanges();


                                        }
                                        else
                                        {
                                            int? QuantityOld = InsideQuery.Quantidade;
                                            int? QuantityMinimaOld = InsideQuery.QuantidadeMinima;
                                            string? ObservacaoOld = InsideQuery.Observacao;

                                            InsideQuery.Quantidade = InsideQuery.Quantidade + item.SaldoTransferir;
                                            InsideQuery.QuantidadeMinima = query.QuantidadeMinima;
                                            InsideQuery.Localizacao = query.Localizacao;

                                            int? id = InsideQuery.Id;

                                            var prodDebito = _context.Produto.FirstOrDefault(i => i.Id == item.IdProduto);
                                            if (query.Id != 0)
                                            {
                                                var InsertToLogProduto = new LogProduto
                                                {
                                                    IdProduto = item.IdProduto,
                                                    IdUsuario = loggedUser.Id,
                                                    QuantidadeDe = prodDebito.Quantidade + item.SaldoTransferir,
                                                    QuantidadePara = prodDebito.Quantidade,
                                                    Acao = 2,
                                                    DataRegistro = DateTime.Now,
                                                };
                                                _context.Add(InsertToLogProduto);
                                            }


                                            var InsertToLogProduto2 = new LogProduto
                                            {
                                                IdProduto = id,
                                                IdUsuario = loggedUser.Id,
                                                QuantidadeDe = QuantityOld,
                                                QuantidadePara = InsideQuery.Quantidade,
                                                RfmDe = InsideQuery.RFM,
                                                RfmPara = "TRANSFERENCIA",
                                                Acao = 2,
                                                DataRegistro = DateTime.Now,
                                            };
                                            _context.Add(InsertToLogProduto2);

                                            var produtoSaldo = _context.Produto.Where(t => t.Id == id).FirstOrDefault();
                                            if (produtoSaldo != null)
                                            {
                                                produtoSaldo.Quantidade = InsideQuery.Quantidade;
                                                produtoSaldo.RFM = "TRANSFERENCIA";
                                            }

                                            _context.SaveChanges();

                                            var InsertToHistoricoTransferencia = new HistoricoTransferencia
                                            {
                                                IdProduto = produtoSaldo.Id,
                                                IdUsuario = loggedUser.Id,
                                                IdFerramentariaOrigem = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                                IdFerramentariaDestino = Ferrmanetaria,
                                                DataOcorrencia = DateTime.Now,
                                                Quantidade = item.SaldoTransferir,
                                                Documento = Documento
                                            };
                                            _context.Add(InsertToHistoricoTransferencia);
                                            _context.SaveChanges();

                                            var InsertToLogEntradaSaida = new LogEntradaSaidaInsert
                                            {
                                                IdFerramentaria = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                                IdProduto = item.IdProduto,
                                                Quantidade = item.SaldoTransferir * -1,
                                                Rfm = "TRANSFERENCIA",
                                                Observacao = "ORIGEM",
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };
                                            _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida);
                                            _context.SaveChanges();

                                            var InsertToLogEntradaSaida2 = new LogEntradaSaidaInsert
                                            {
                                                IdFerramentaria = Ferrmanetaria,
                                                IdProduto = produtoSaldo.Id,
                                                Quantidade = item.SaldoTransferir,
                                                Rfm = "TRANSFERENCIA",
                                                Observacao = "DESTINO",
                                                IdUsuario = loggedUser.Id,
                                                DataRegistro = DateTime.Now
                                            };
                                            _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida2);
                                            _context.SaveChanges();
                                        }

                                    }
                                    else
                                    {
                                        var InsertToLogProduto = new LogProduto
                                        {
                                            IdProduto = item.IdProduto,
                                            IdUsuario = loggedUser.Id,
                                            QuantidadeDe = query.Quantidade,
                                            QuantidadePara = query.Quantidade - item.SaldoTransferir,
                                            RfmDe = query.RFM,
                                            RfmPara = "TRANSFERENCIA",
                                            ObservacaoDe = query.Observacao,
                                            ObservacaoPara = "ORIGEM",
                                            Acao = 2,
                                            DataRegistro = DateTime.Now,
                                        };
                                        _context.Add(InsertToLogProduto);
                                        _context.SaveChanges();

                                        var InsertToLogProduto2 = new LogProduto
                                        {
                                            IdProduto = item.IdProduto,
                                            IdUsuario = loggedUser.Id,
                                            QuantidadeDe = 0,
                                            QuantidadePara = query.Quantidade,
                                            RfmDe = query.RFM,
                                            RfmPara = "TRANSFERENCIA",
                                            ObservacaoDe = query.Observacao,
                                            ObservacaoPara = "DESTINO",
                                            Acao = 1,
                                            DataRegistro = DateTime.Now,
                                        };
                                        _context.Add(InsertToLogProduto2);
                                        _context.SaveChanges();

                                        var InsertToHistoricoTransferencia = new HistoricoTransferencia
                                        {
                                            IdProduto = item.IdProduto,
                                            IdUsuario = loggedUser.Id,
                                            IdFerramentariaOrigem = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                            IdFerramentariaDestino = Ferrmanetaria,
                                            DataOcorrencia = DateTime.Now,
                                            Quantidade = 1,
                                            Documento = Documento
                                        };
                                        _context.Add(InsertToHistoricoTransferencia);
                                        _context.SaveChanges();

                                        var produto = _context.Produto.Where(t => t.Id == item.IdProduto).FirstOrDefault();
                                        if (produto != null)
                                        {
                                            produto.IdFerramentaria = Ferrmanetaria;
                                        }
                                        _context.SaveChanges();

                                        var InsertToLogEntradaSaida = new LogEntradaSaidaInsert
                                        {
                                            IdFerramentaria = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria),
                                            IdProduto = item.IdProduto,
                                            Quantidade = item.SaldoTransferir * -1,
                                            Rfm = "TRANSFERENCIA",
                                            Observacao = "ORIGEM",
                                            IdUsuario = loggedUser.Id,
                                            DataRegistro = DateTime.Now
                                        };
                                        _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida);
                                        _context.SaveChanges();

                                        var InsertToLogEntradaSaida2 = new LogEntradaSaidaInsert
                                        {
                                            IdFerramentaria = Ferrmanetaria,
                                            IdProduto = item.IdProduto,
                                            Quantidade = item.SaldoTransferir,
                                            Rfm = "TRANSFERENCIA",
                                            Observacao = "DESTINO",
                                            IdUsuario = loggedUser.Id,
                                            DataRegistro = DateTime.Now
                                        };
                                        _context.LogEntradaSaidaInsert.Add(InsertToLogEntradaSaida2);
                                        _context.SaveChanges();

                                    }
                                }

                                ViewBag.ShowSuccessAlert = true;

                                var SearchFilterModel = httpContextAccessor?.HttpContext?.Session.GetObject<SearchGestorModel?>(SessionKeySearchFilters) ?? new SearchGestorModel();

                                CombinedGestor combinedGestor = new CombinedGestor
                                {
                                    SearchGestorModel = SearchFilterModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                };

                                CombinedGestorList combinedGestorListSess = new CombinedGestorList
                                {
                                    SearchGestorModel = SearchFilterModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                };

                                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyCombinedGestorModel, combinedGestorListSess);

                                //StaticCombinedGestor = combinedGestor;
                                return View(nameof(Index), combinedGestor);
                            }
                            else
                            {
                                ViewBag.ModalTransferError = error;

                                int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
                                int? UsuarioLoginId = loggedUser.Id;

                                var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                                          where ferramentaria.Ativo == 1 &&
                                                                !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                                _context.FerramentariaVsLiberador.Any(l => l.IdLogin == UsuarioLoginId && l.IdFerramentaria == ferramentaria.Id)
                                                          orderby ferramentaria.Nome
                                                          select new
                                                          {
                                                              Id = ferramentaria.Id,
                                                              Nome = ferramentaria.Nome
                                                          }).ToList();

                                //var query = _context.Ferramentaria.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Nome).ToList();
                                ferramentariaItems.RemoveAll(it => it.Id == FerramentariaValue);

                                List<int?>? selectedIds = TransferValues?.Select(t => t?.IdProduto).ToList();
                                //var query = _context.Ferramentaria.Where(entity => entity.Ativo == 1).OrderBy(entity => entity.Nome).ToList();
                                //query.RemoveAll(it => it.Id == usuariofer.IdFerramentaria);

                                var ListGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();
                                List<SP_1600012731_EstoqueViewModel?>? ListToTransfer = ListGestorModel.Where(item => selectedIds.Contains(item.IdProduto.GetValueOrDefault())).ToList();

                                ViewBag.ListToTransfer = ListToTransfer;
                                ViewBag.FerramentariaList = ferramentariaItems;
                                ViewBag.OpenTransferirModal = true;

                                var CombinedGestorModel = httpContextAccessor?.HttpContext?.Session.GetObject<CombinedGestorList?>(SessionKeyCombinedGestorModel) ?? new CombinedGestorList();

                                int pageSize = CombinedGestorModel.Pagination ?? 10;
                                int pageNumber = CombinedGestorModel.PageNumber ?? 1;

                                IPagedList<SP_1600012731_EstoqueViewModel> SP_1600012731PagedList = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.ToPagedList(pageNumber, pageSize);

                                var combinedGestor = new CombinedGestor
                                {
                                    SP_1600012731_EstoqueViewModel = SP_1600012731PagedList,
                                    SearchGestorModel = CombinedGestorModel.SearchGestorModel,
                                    NomeFerramentaria = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria),
                                    ResultCount = CombinedGestorModel.SP_1600012731_EstoqueViewModelList.Count,
                                };

                                return View(nameof(Index), combinedGestor);
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

        public string ValidateTransfer(List<TransferModel?>? TransferValues, int? Ferrmanetaria, string? Documento)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            if (Ferrmanetaria == 0)
            {
                return "Nenhuma ferramentaria está selecionada.";
            }

            if (string.IsNullOrEmpty(Documento))
            {
                return "Documento está vazio";
            }

            if (TransferValues.Count > 0)
            {
                foreach (var item in TransferValues)
                {
                    if (item.SaldoTransferir != null && item.SaldoTransferir != 0)
                    {
                        if (item.SaldoTransferir > item.Saldo)
                        {
                            return String.Format("Limite de transferencia Excedida ao Limite do Estoque disponivel em {0} do ID {1}.", item.IdProduto, (item.Saldo - item.SaldoTransferir));
                        }
                    }
                    else
                    {
                        return $"{item.Codigo} has 0 quantidade value";
                    }

                    GestorProduto? ProductDetails = searches.SearchProductGestor(item.IdProduto);
                    int? Difference = ProductDetails.Quantidade - item.SaldoTransferir;

                    if (Difference < 0)
                    {                    
                        return "FALHA NA TRANSAÇÃO... POSSIBILIDADE DE SALDO NEGATIVO.";
                    }
                }
            }
            else
            {
                return "Nenhum produto foi selecionado";
            }
         

            return null;
        }

        //[HttpPost]
        //public IActionResult TransferirAction(List<int>? ProdutoId, int? Ferrmanetaria, List<int>? StockQtd, List<int>? SaldoTransferir)
        //{
        //    Log log = new Log();
        //    log.LogWhat = pagina + "/Index";
        //    log.LogWhere = pagina;
        //    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
        //    try
        //    {

        //        //#region Authenticate User
        //        //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
        //        ////usuario.Pagina = "Home/Index";
        //        //usuariofer.Pagina = pagina;
        //        //usuariofer.Pagina1 = log.LogWhat;
        //        //usuariofer.Acesso = log.LogWhat;
        //        //usuariofer = auxiliar.VerificaPermissao(usuariofer);

        //        //if (usuariofer.Permissao == null)
        //        //{
        //        //    usuariofer.Retorno = "Usuário sem permissão na página!";
        //        //    log.LogWhy = usuariofer.Retorno;
        //        //    auxiliar.GravaLogAlerta(log);
        //        //    return RedirectToAction("PreserveActionError", "Home", usuariofer);
        //        //}
        //        //else
        //        //{
        //        //    if (usuariofer.Permissao.Visualizar != 1)
        //        //    {
        //        //        usuariofer.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
        //        //        log.LogWhy = usuariofer.Retorno;
        //        //        auxiliar.GravaLogAlerta(log);
        //        //        return RedirectToAction("PreserveActionError", "Home", usuariofer);
        //        //    }
        //        //}
        //        //#endregion

        //        LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
        //        if (loggedUser != null)
        //        {
        //            PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
        //            if (checkPermission != null)
        //            {
        //                if (checkPermission.Visualizar == 1)
        //                {






        //                }
        //                else
        //                {
        //                    return RedirectToAction("PreserveActionError", "Home", new { Error = $"No Permission for Page:{pagina}" });
        //                }
        //            }
        //            else
        //            {
        //                log.LogWhy = "Permission is Empty";
        //                return RedirectToAction("PreserveActionError", "Home", new { Error = "Permission is Empty" });
        //            }
        //        }
        //        else
        //        {
        //            log.LogWhy = "Session Expired";
        //            return RedirectToAction("PreserveActionError", "Home", new { Error = "Session Expired" });
        //        }


        //        #region Validations
        //        if (Ferrmanetaria == null || Ferrmanetaria == 0)
        //        {
        //            //TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "No Ferramentaria is Selected.";
        //            return RedirectToAction(nameof(Transferir), new { selectedIds = ProdutoId });
        //            //return RedirectToAction(nameof(Index));
        //        }

        //        for (int i = 0; i < StockQtd.Count; i++)
        //        {
        //            int? stockQtdValue = StockQtd[i];
        //            int? saldoTransferirValue = SaldoTransferir[i];
        //            int? IdProduto = ProdutoId[i];

        //            if (saldoTransferirValue != null)
        //            {
        //                if (saldoTransferirValue > 0)
        //                {
        //                    if (stockQtdValue < saldoTransferirValue)
        //                    {
        //                        TempData["ShowErrorAlert"] = true;
        //                        TempData["ErrorMessage"] = String.Format("Limite de transferencia Excedida ao Limite do Estoque disponivel em {0} do ID {1}.", IdProduto, (stockQtdValue - saldoTransferirValue));

        //                        return RedirectToAction(nameof(Index));
        //                    }
        //                }
        //                else
        //                {
        //                    TempData["ShowErrorAlert"] = true;
        //                    TempData["ErrorMessage"] = "Quantity to transfer is null or 0. for Id " + IdProduto;

        //                    return RedirectToAction(nameof(Index));
        //                }
        //            }
        //            else
        //            {
        //                TempData["ShowErrorAlert"] = true;
        //                TempData["ErrorMessage"] = "Quantity to transfer is null or 0. for Id " + IdProduto;

        //                return RedirectToAction(nameof(Index));
        //            }

        //        }

        //        #endregion

        //        int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

        //        //Fix Values
        //        int? IdUsuario = usuariofer.Id;
        //        int? FerramentariaOrigin = FerramentariaValue;
        //        int? FerramentariaDestination = Ferrmanetaria;

        //        for (int i = 0; i < ProdutoId.Count; i++)
        //        {
        //            int? StockQuantity = StockQtd[i];
        //            int? TransferValue = SaldoTransferir[i];
        //            int? IdProduto = ProdutoId[i];

        //            int DebitValue = 1;

        //            //var Produto = _context.Produto.FirstOrDefault(i => i.Id == IdProduto);

        //            var query = (from produto in _context.Produto
        //                        join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                        join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                        join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
        //                        join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaJoin
        //                        from empresa in empresaJoin.DefaultIfEmpty()
        //                        where produto.Id == IdProduto
        //                        select new GestorProduto
        //                        {
        //                            DC_DataAquisicao = produto.DC_DataAquisicao,
        //                            DC_Valor = produto.DC_Valor,
        //                            DC_AssetNumber = produto.DC_AssetNumber,
        //                            DC_Fornecedor = produto.DC_Fornecedor,
        //                            GC_Contrato = produto.GC_Contrato,
        //                            GC_DataInicio = produto.GC_DataInicio,
        //                            GC_IdObra = produto.GC_IdObra,
        //                            GC_OC = produto.GC_OC,
        //                            GC_DataSaida = produto.GC_DataSaida,
        //                            GC_NFSaida = produto.GC_NFSaida,
        //                            Selo = produto.Selo,
        //                            Id = produto.Id,
        //                            AF = produto.AF,
        //                            PAT = produto.PAT,
        //                            Quantidade = produto.Quantidade,
        //                            QuantidadeMinima = produto.QuantidadeMinima,
        //                            Localizacao = produto.Localizacao,
        //                            RFM = produto.RFM,
        //                            Observacao = produto.Observacao,
        //                            DataRegistro = produto.DataRegistro,
        //                            DataVencimento = produto.DataVencimento,
        //                            Certificado = produto.Certificado,
        //                            Serie = produto.Serie,
        //                            Ativo = produto.Ativo,
        //                            CatalogoId = catalogo.Id,
        //                            CatalogoCodigo = catalogo.Codigo,
        //                            CatalogoNome = catalogo.Nome,
        //                            CatalogoDescricao = catalogo.Descricao,
        //                            CatalogoPorMetro = catalogo.PorMetro,
        //                            CatalogoPorAferido = catalogo.PorAferido,
        //                            CatalogoPorSerial = catalogo.PorSerial,
        //                            DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
        //                            CatalogoDataRegistro = catalogo.DataRegistro,
        //                            CatalogoAtivo = catalogo.Ativo,
        //                            CategoriaId = categoria.Id,
        //                            IdCategoriaPai = categoria.IdCategoria,
        //                            CategoriaClasse = categoria.Classe,
        //                            CategoriaNome = categoria.Nome,
        //                            CategoriaDataRegistro = categoria.DataRegistro,
        //                            CategoriaAtivo = categoria.Ativo,
        //                            FerramentariaId = ferramentaria.Id,
        //                            FerramentariaNome = ferramentaria.Nome,
        //                            FerramentariaDataRegistro = ferramentaria.DataRegistro,
        //                            FerramentariaAtivo = ferramentaria.Ativo,
        //                            EmpresaId = empresa.Id,
        //                            EmpresaNome = empresa.Nome,
        //                            EmpresaGerente = empresa.Gerente,
        //                            EmpresaTelefone = empresa.Telefone,
        //                            EmpresaDataRegistro = empresa.DataRegistro,
        //                            EmpresaAtivo = empresa.Ativo,
        //                            Status = ((from pa in _context.ProdutoAlocado
        //                                       where pa.IdProduto == produto.Id
        //                                       select pa).Count() >= 1) ? "Emprestado" :
        //                                     (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque")
        //                        }).FirstOrDefault();


        //            int? Difference = query.Quantidade - DebitValue;

        //            if (Difference < 0)
        //            {
        //                TempData["ShowErrorAlert"] = true;
        //                TempData["ErrorMessage"] = "FALHA NA TRANSAÇÃO... POSSIBILIDADE DE SALDO NEGATIVO.";

        //                return RedirectToAction(nameof(Index));
        //            }
        //            else
        //            {
        //                if (!(query.CatalogoPorAferido == 1 || query.CatalogoPorSerial == 1))
        //                {
        //                    var produto = _context.Produto.Where(t => t.Id == IdProduto).FirstOrDefault();
        //                    if (produto != null)
        //                    {
        //                        produto.Quantidade = produto.Quantidade - TransferValue;
        //                    }

        //                    await _context.SaveChangesAsync();

        //                    var InsideQuery = (from produtoinside in _context.Produto
        //                                join catalogo in _context.Catalogo on produtoinside.IdCatalogo equals catalogo.Id
        //                                join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                                join ferramentaria in _context.Ferramentaria on produtoinside.IdFerramentaria equals ferramentaria.Id
        //                                join empresa in _context.Empresa on produtoinside.IdEmpresa equals empresa.Id into empresaJoin
        //                                from empresa in empresaJoin.DefaultIfEmpty()
        //                                where produto.Ativo == 1 &&
        //                                      catalogo.Id == query.CatalogoId &&
        //                                      ferramentaria.Id == FerramentariaDestination
        //                                       orderby produto.DataRegistro descending
        //                                select new GestorProduto
        //                                {
        //                                    DC_DataAquisicao = produtoinside.DC_DataAquisicao,
        //                                    DC_Valor = produtoinside.DC_Valor,
        //                                    DC_AssetNumber = produtoinside.DC_AssetNumber,
        //                                    DC_Fornecedor = produtoinside.DC_Fornecedor,
        //                                    GC_Contrato = produtoinside.GC_Contrato,
        //                                    GC_DataInicio = produtoinside.GC_DataInicio,
        //                                    GC_IdObra = produtoinside.GC_IdObra,
        //                                    GC_OC = produtoinside.GC_OC,
        //                                    GC_DataSaida = produtoinside.GC_DataSaida,
        //                                    GC_NFSaida = produtoinside.GC_NFSaida,
        //                                    Selo = produtoinside.Selo,
        //                                    Id = produtoinside.Id,
        //                                    AF = produtoinside.AF,
        //                                    PAT = produtoinside.PAT,
        //                                    Quantidade = produtoinside.Quantidade,
        //                                    QuantidadeMinima = produtoinside.QuantidadeMinima,
        //                                    Localizacao = produtoinside.Localizacao,
        //                                    RFM = produtoinside.RFM,
        //                                    Observacao = produtoinside.Observacao,
        //                                    DataRegistro = produtoinside.DataRegistro,
        //                                    DataVencimento = produtoinside.DataVencimento,
        //                                    Certificado = produtoinside.Certificado,
        //                                    Serie = produtoinside.Serie,
        //                                    Ativo = produtoinside.Ativo,
        //                                    CatalogoId = catalogo.Id,
        //                                    CatalogoCodigo = catalogo.Codigo,
        //                                    CatalogoNome = catalogo.Nome,
        //                                    CatalogoDescricao = catalogo.Descricao,
        //                                    CatalogoPorMetro = catalogo.PorMetro,
        //                                    CatalogoPorAferido = catalogo.PorAferido,
        //                                    CatalogoPorSerial = catalogo.PorSerial,
        //                                    DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
        //                                    CatalogoDataRegistro = catalogo.DataRegistro,
        //                                    CatalogoAtivo = catalogo.Ativo,
        //                                    CategoriaId = categoria.Id,
        //                                    IdCategoriaPai = categoria.IdCategoria,
        //                                    CategoriaClasse = categoria.Classe,
        //                                    CategoriaNome = categoria.Nome,
        //                                    CategoriaDataRegistro = categoria.DataRegistro,
        //                                    CategoriaAtivo = categoria.Ativo,
        //                                    FerramentariaId = ferramentaria.Id,
        //                                    FerramentariaNome = ferramentaria.Nome,
        //                                    FerramentariaDataRegistro = ferramentaria.DataRegistro,
        //                                    FerramentariaAtivo = ferramentaria.Ativo,
        //                                    EmpresaId = empresa.Id,
        //                                    EmpresaNome = empresa.Nome,
        //                                    EmpresaGerente = empresa.Gerente,
        //                                    EmpresaTelefone = empresa.Telefone,
        //                                    EmpresaDataRegistro = empresa.DataRegistro,
        //                                    EmpresaAtivo = empresa.Ativo
        //                                }).FirstOrDefault();

        //                    if (InsideQuery == null)
        //                    {
        //                        var InsertProduto = new Produto
        //                        {
        //                            IdCatalogo = query.CatalogoId,
        //                            IdFerramentaria = FerramentariaDestination,
        //                            Quantidade = TransferValue,
        //                            QuantidadeMinima = query.QuantidadeMinima,
        //                            Localizacao = query.Localizacao,
        //                            RFM = "TRANSFERENCIA",
        //                            Observacao = query.Observacao,
        //                            Ativo = 1,
        //                            DataRegistro = DateTime.Now,
        //                        };
        //                        _context.Add(InsertProduto);
        //                        await _context.SaveChangesAsync();

        //                        var prodDebito = _context.Produto.FirstOrDefault(i => i.Id == query.Id);

        //                        if (query.Id != 0)
        //                        {
        //                            var InsertToLogProduto = new LogProduto
        //                            {
        //                                IdProduto = IdProduto,
        //                                IdUsuario = usuariofer.Id,
        //                                QuantidadeDe = prodDebito.Quantidade + TransferValue,
        //                                QuantidadePara = prodDebito.Quantidade,
        //                                Acao = 2,
        //                                DataRegistro = DateTime.Now,
        //                            };
        //                            _context.Add(InsertToLogProduto);
        //                            await _context.SaveChangesAsync();

        //                        }
                             
        //                        var InsertToLogProduto2 = new LogProduto
        //                        {
        //                            IdProduto = IdProduto,
        //                            IdUsuario = usuariofer.Id,
        //                            QuantidadePara = TransferValue,
        //                            Acao = 1,
        //                            DataRegistro = DateTime.Now,
        //                        };
        //                        _context.Add(InsertToLogProduto2);
        //                        await _context.SaveChangesAsync();


        //                    }
        //                    else
        //                    {
        //                        int? QuantityOld = InsideQuery.Quantidade;
        //                        int? QuantityMinimaOld = InsideQuery.QuantidadeMinima;
        //                        string? ObservacaoOld = InsideQuery.Observacao;

        //                        InsideQuery.Quantidade = InsideQuery.Quantidade + TransferValue;
        //                        InsideQuery.QuantidadeMinima = query.QuantidadeMinima;
        //                        InsideQuery.Localizacao = query.Localizacao;

        //                        int? id = InsideQuery.Id;

        //                        var prodDebito = _context.Produto.FirstOrDefault(i => i.Id == IdProduto);

        //                        if (query.Id != 0)
        //                        {
        //                            var InsertToLogProduto = new LogProduto
        //                            {
        //                                IdProduto = IdProduto,
        //                                IdUsuario = usuariofer.Id,
        //                                QuantidadeDe = prodDebito.Quantidade + TransferValue,
        //                                QuantidadePara = prodDebito.Quantidade,
        //                                Acao = 2,
        //                                DataRegistro = DateTime.Now,
        //                            };
        //                            _context.Add(InsertToLogProduto);
        //                        }


        //                        var InsertToLogProduto2 = new LogProduto
        //                        {
        //                            IdProduto = id,
        //                            IdUsuario = usuariofer.Id,
        //                            QuantidadeDe = QuantityOld,
        //                            QuantidadePara = InsideQuery.Quantidade,
        //                            RfmDe = InsideQuery.RFM,
        //                            RfmPara = "TRANSFERENCIA",
        //                            ObservacaoDe = ObservacaoOld.Length > 250 ? ObservacaoOld.PadRight(250) : ObservacaoOld,
        //                            ObservacaoPara = InsideQuery.Observacao.Length > 250 ? InsideQuery.Observacao.PadRight(250): InsideQuery.Observacao,
        //                            Acao = 2,
        //                            DataRegistro = DateTime.Now,
        //                        };
        //                        _context.Add(InsertToLogProduto2);

        //                        var produtoSaldo = _context.Produto.Where(t => t.Id == id).FirstOrDefault();
        //                        if (produtoSaldo != null)
        //                        {
        //                            produtoSaldo.Quantidade = InsideQuery.Quantidade;
        //                            produtoSaldo.RFM = "TRANSFERENCIA";
        //                        }

        //                        await _context.SaveChangesAsync();
        //                    }


        //                }
        //                else
        //                {
        //                    var InsertToLogProduto = new LogProduto
        //                    {
        //                        IdProduto = IdProduto,
        //                        IdUsuario = usuariofer.Id,
        //                        QuantidadeDe = query.Quantidade,
        //                        QuantidadePara = query.Quantidade - TransferValue,
        //                        RfmDe = query.RFM,
        //                        RfmPara = "TRANSFERENCIA",
        //                        ObservacaoDe = query.Observacao,
        //                        ObservacaoPara = "ORIGEM",
        //                        Acao = 2,
        //                        DataRegistro = DateTime.Now,
        //                    };
        //                    _context.Add(InsertToLogProduto);

        //                    var InsertToLogProduto2 = new LogProduto
        //                    {
        //                        IdProduto = IdProduto,
        //                        IdUsuario = usuariofer.Id,
        //                        QuantidadeDe = 0,
        //                        QuantidadePara = query.Quantidade,
        //                        RfmDe = query.RFM,
        //                        RfmPara = "TRANSFERENCIA",
        //                        ObservacaoDe = query.Observacao,
        //                        ObservacaoPara = "DESTINO",
        //                        Acao = 1,
        //                        DataRegistro = DateTime.Now,
        //                    };
        //                    _context.Add(InsertToLogProduto2);

        //                    var InsertToHistoricoTransferencia = new HistoricoTransferencia
        //                    {
        //                        IdProduto = IdProduto,
        //                        IdUsuario = usuariofer.Id,
        //                        IdFerramentariaOrigem = FerramentariaOrigin,
        //                        IdFerramentariaDestino = FerramentariaDestination,
        //                        DataOcorrencia = DateTime.Now,
        //                        Quantidade = 1,        

        //                    };
        //                    _context.Add(InsertToHistoricoTransferencia);

        //                    var produto = _context.Produto.Where(t => t.Id == IdProduto).FirstOrDefault();
        //                    if (produto != null)
        //                    {
        //                        produto.IdFerramentaria = FerramentariaDestination;
        //                    }

        //                    var InsertToLogEntradaSaida = new LogEntradaSaidaInsert
        //                    {
        //                        IdFerramentaria = FerramentariaOrigin,
        //                        IdProduto = IdProduto,
        //                        Quantidade = TransferValue *-1,
        //                        Rfm = "TRANSFERENCIA",
        //                        Observacao = "ORIGEM",
        //                        IdUsuario = usuariofer.Id,
        //                        DataRegistro = DateTime.Now        
        //                    };
        //                    _context.Add(InsertToLogEntradaSaida);

        //                    var InsertToLogEntradaSaida2 = new LogEntradaSaidaInsert
        //                    {
        //                        IdFerramentaria = FerramentariaDestination,
        //                        IdProduto = IdProduto,
        //                        Quantidade = TransferValue,
        //                        Rfm = "TRANSFERENCIA",
        //                        Observacao = "DESTINO",
        //                        IdUsuario = usuariofer.Id,
        //                        DataRegistro = DateTime.Now
        //                    };
        //                    _context.Add(InsertToLogEntradaSaida2);

        //                    await _context.SaveChangesAsync();


        //                }
        //            }

        //        }

        //        TempData["ShowSuccessAlert"] = true;

        //        //TransferIds.Clear();
        //        //GlobalGestor.ListGestor.Clear();

        //        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListGestor);
        //        //_ListGestor.Clear();

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

        #endregion

        #endregion

        #region EstoqueSaida Region
        // GET: Gestor/Details/5
        public ActionResult EstoqueSaida(int? page)
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
                //usuario.Pagina1 = "thEstoque.aspx";
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
                                //GlobalFerramentariaValue = FerramentariaValue;
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

           

            //if (GlobalGestor.ListSaidaEstoque.Count != 0)
            //{
            //    //int? testget = GlobalPagination;
            //    int pageSize = 10;
            //    int pageNumber = (page ?? 1);

            //    List<SaidaEstoqueViewModel> SaidaEstoqueResult = GlobalGestor.ListSaidaEstoque;

            //    IPagedList<SaidaEstoqueViewModel> SaidaEstoquePagedList = SaidaEstoqueResult.ToPagedList(pageNumber, pageSize);
            //    return View(SaidaEstoquePagedList);
            //}
       
        }

        public IActionResult SearchEstoqueSaida(int? selectedCatalogo, int? selectedClasse, int? selectedTipo, string? Codigo, string? Item)
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
                //usuario.Pagina1 = "thEstoque.aspx";
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



                            int? CatalogoValue = selectedCatalogo;
                            int? ClasseValue = selectedClasse;
                            int? TipoValue = selectedTipo;
                            string? CodigoValue = Codigo;
                            string? ItemValue = Item;

                            string? FerramentariaName = httpContextAccessor?.HttpContext?.Session.GetString(Sessao.NomeFerramentaria);
                            int? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

                            int? GlobalFerramentaria = FerramentariaValue;

                            if (GlobalFerramentaria != null)
                            {
                                List<SaidaEstoqueViewModel> SaidaEstoqueViewModelResult = new List<SaidaEstoqueViewModel>();

                                var query = (from catalogo in _context.Catalogo
                                             join categoria in _context.Categoria
                                             on catalogo.IdCategoria equals categoria.Id
                                             where catalogo.Ativo == 1
                                                    && catalogo.PorAferido == 0
                                                    && catalogo.PorSerial == 0
                                                    && (selectedCatalogo == null || categoria.Classe == selectedCatalogo)
                                                    && (selectedClasse == null || categoria.IdCategoria == selectedClasse)
                                                    && (selectedTipo == null || categoria.Id == selectedTipo)
                                                    && (Codigo == null || catalogo.Codigo.Contains(Codigo))
                                                    && (Item == null || catalogo.Nome.Contains(Item))
                                                    && _context.Produto.Any(p => p.Ativo == 1 && p.IdFerramentaria == GlobalFerramentaria && p.IdCatalogo == catalogo.Id)
                                                    && _context.Produto.Any(p => p.Ativo == 1 && p.IdFerramentaria == GlobalFerramentaria && p.IdCatalogo == catalogo.Id && p.Quantidade != 0)
                                             orderby catalogo.Nome
                                             select new SaidaEstoqueViewModel
                                             {
                                                 CatalogoId = catalogo.Id,
                                                 CatalogoCodigo = catalogo.Codigo,
                                                 CatalogoNome = catalogo.Nome,
                                                 CatalogoDescricao = catalogo.Descricao,
                                                 CatalogoPorMetro = catalogo.PorMetro,
                                                 CatalogoPorAferido = catalogo.PorAferido,
                                                 CatalogoPorSerial = catalogo.PorSerial,
                                                 CatalogoRestricaoEmprestimo = catalogo.RestricaoEmprestimo,
                                                 CatalogoImpedirDescarte = catalogo.ImpedirDescarte,
                                                 CatalogoHabilitarDescarteEpi = catalogo.HabilitarDescarteEPI,
                                                 CatalogoDataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                 CatalogoDataRegistro = catalogo.DataRegistro,
                                                 CatalogoAtivo = catalogo.Ativo,
                                                 CategoriaId = categoria.Id,
                                                 IdCategoriaPai = categoria.IdCategoria,
                                                 CategoriaClasse = categoria.Classe,
                                                 CategoriaNome = categoria.Nome,
                                                 CategoriaDataRegistro = categoria.DataRegistro,
                                                 CategoriaAtivo = categoria.Ativo
                                             }).ToList();


                                if (query != null)
                                {
                                    foreach (var item in query)
                                    {
                                        int? IdCategoriaPai = item.IdCategoriaPai;

                                        var Classe =  _context.Categoria
                                                .FirstOrDefault(u => u.Id == IdCategoriaPai);

                                        item.ClasseNome = Classe.Nome;
                                    }

                                }

                                var mapper = mapeamentoClasses.CreateMapper();

                                SaidaEstoqueViewModelResult = mapper.Map<List<SaidaEstoqueViewModel>>(query);

                                //GlobalGestor.ListSaidaEstoque = SaidaEstoqueViewModelResult;

                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListSaidaEstoque);
                                httpContextAccessor?.HttpContext?.Session.SetObject(SessionKeyListSaidaEstoque, SaidaEstoqueViewModelResult);
                                //_ListSaidaEstoque = SaidaEstoqueViewModelResult;

                                int pageSize = 10;
                                int pageNumber = 1;
                                IPagedList<SaidaEstoqueViewModel> SaidaEstoquePagedList = SaidaEstoqueViewModelResult.ToPagedList(pageNumber, pageSize);

                                return View(nameof(EstoqueSaida), SaidaEstoquePagedList);
                                //return RedirectToAction(nameof(EstoqueSaida));
                            }
                            else
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
                                //httpContextAccessor.HttpContext.Response.Redirect("/Emprestimo/Index");              
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

        public ActionResult TestPageEstoque(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var ListEstoqueSaidaModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SaidaEstoqueViewModel>>(SessionKeyListSaidaEstoque) ?? new List<SaidaEstoqueViewModel>();

            IPagedList<SaidaEstoqueViewModel> ListSaidaPagedList = ListEstoqueSaidaModel.ToPagedList(pageNumber, pageSize);

            return View(nameof(EstoqueSaida), ListSaidaPagedList);
        }



        #endregion

        #region FormEstoqueSaida Region
        // GET: Gestor/Create
        public ActionResult FormEstoqueSaida(int? Id)
        {

            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewBag.Error = TempData["ErrorMessage"].ToString();
                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
            }

            if (TempData.ContainsKey("ShowSuccessAlert"))
            {
                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"].ToString();
                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
            }

            //Error handling
            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

            //var GetData = GlobalGestor.ListSaidaEstoque.FirstOrDefault(i => i.CatalogoId == Id);
            int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

            var ListEstoqueSaidaModel = httpContextAccessor?.HttpContext?.Session.GetObject<List<SaidaEstoqueViewModel>>(SessionKeyListSaidaEstoque) ?? new List<SaidaEstoqueViewModel>();
            var GetData = ListEstoqueSaidaModel.FirstOrDefault(i => i.CatalogoId == Id);
            var GetProduto = _context.Produto.FirstOrDefault(p => p.IdCatalogo == GetData.CatalogoId && p.IdFerramentaria == FerramentariaValue);

            string? CatalogoNome = "";
            switch (GetData.CategoriaClasse)
            {
                case 1:
                    CatalogoNome = "Ferramenta";
                    break;
                case 2:
                    CatalogoNome = "EPI";
                    break;
                case 3:
                    CatalogoNome = "Consumíveis";
                    break;
            }

            FormEstoqueSaidaViewModel FormEstoque = new FormEstoqueSaidaViewModel
            {
                 CatalogoId = GetData.CatalogoId,
                 CatalogoType = CatalogoNome,
                 ClasseNome = GetData.ClasseNome,
                 CategoriaNome = GetData.CategoriaNome,
                 CatalogoCodigo = GetData.CatalogoCodigo,
                 CatalogoNome = GetData.CatalogoNome,
                 CatalogoDescricao = GetData.CatalogoDescricao,
                 ProdutoQuantidade = GetProduto.Quantidade,
                 ProdutoId = GetProduto.Id,
                 ProdutoRFM = GetProduto.RFM
            };

            var mapper = mapeamentoClasses.CreateMapper();

            FormEstoqueSaidaViewModel EstoqueViewModel = mapper.Map<FormEstoqueSaidaViewModel>(FormEstoque);

            return View(EstoqueViewModel);
        }

        public async Task<IActionResult> SaveSaida(FormEstoqueSaidaViewModel EstoqueValues)
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

                //Error handling
                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
                //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {


                            #region Validations
                            if (EstoqueValues.CatalogoId == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "No value is Selected.";

                                return RedirectToAction(nameof(FormEstoqueSaida), new { id = EstoqueValues.CatalogoId });
                            }

                            if (EstoqueValues.QuantidadeSaida <= 0)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Quantidade is 0.";

                                return RedirectToAction(nameof(FormEstoqueSaida), new { id = EstoqueValues.CatalogoId });
                            }

                            if (EstoqueValues.RFM == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "RFM is null";

                                return RedirectToAction(nameof(FormEstoqueSaida), new { id = EstoqueValues.CatalogoId });
                            }

                            if (EstoqueValues.Observacao == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Observacao is null";

                                return RedirectToAction(nameof(FormEstoqueSaida), new { id = EstoqueValues.CatalogoId });
                            }
                            #endregion

                            int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

                            int? DebitValue = EstoqueValues.ProdutoQuantidade - EstoqueValues.QuantidadeSaida;
                            if (DebitValue < 0)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "FALHA NA TRANSAÇÃO... POSSIBILIDADE DE SALDO NEGATIVO.";
                                return RedirectToAction(nameof(FormEstoqueSaida), new { id = EstoqueValues.CatalogoId });
                            }
                            else
                            {
                                var EditProduto = _context.Produto.Where(t => t.Id == EstoqueValues.ProdutoId).FirstOrDefault();
                                if (EditProduto != null)
                                {
                                    EditProduto.Quantidade = DebitValue;
                                    EditProduto.RFM = EstoqueValues.RFM;
                                    EditProduto.Observacao = EstoqueValues.Observacao;
                                }

                                var InsertToLogProduto = new LogProduto
                                {
                                    QuantidadeDe = EstoqueValues.ProdutoQuantidade,
                                    QuantidadePara = DebitValue,
                                    RfmDe = EstoqueValues.RFM,
                                    RfmPara = EstoqueValues.RFM,
                                    IdProduto = EstoqueValues.ProdutoId,
                                    IdUsuario = loggedUser.Id,
                                    Acao = 2
                                };
                                _context.Add(InsertToLogProduto);

                                var InsertToLogEntradaSaida = new LogEntradaSaidaInsert
                                {
                                    IdFerramentaria = FerramentariaValue,
                                    IdProduto = EstoqueValues.ProdutoId,
                                    Quantidade = EstoqueValues.QuantidadeSaida * -1,
                                    Rfm = EstoqueValues.RFM,
                                    Observacao = EstoqueValues.Observacao,
                                    IdUsuario = loggedUser.Id,
                                    DataRegistro = DateTime.Now
                                };
                                await _context.SaveChangesAsync();


                                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.ProdutoId });
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
                return View();
            }  
        }

        #endregion

        #region EditarEstoqueSaida
        public IActionResult EditarEstoqueSaida(int id)
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
                //usuariofer.Pagina1 = "thEstoque.aspx";
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



                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"].ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"].ToString();
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
                                //httpContextAccessor.HttpContext.Response.Redirect("/Emprestimo/Index");
                            }

                            CatalogoLocalViewModel catalogoLocalViewModel = new CatalogoLocalViewModel();
                            if (TempData.ContainsKey("OpenGerenciaLocal"))
                            {
                                int? Id = TempData["PassedId"] as int?;

                                var NewMapper = mapeamentoClasses.CreateMapper();

                                catalogoLocalViewModel = NewMapper.Map<CatalogoLocalViewModel>(_context.CatalogoLocal
                                                                                .FirstOrDefault(entity => entity.IdCatalogo == Id));

                                if (catalogoLocalViewModel == null)
                                {
                                    catalogoLocalViewModel = new CatalogoLocalViewModel
                                    {
                                        IdCatalogo = Id,
                                        IdFerramentaria = FerramentariaValue
                                    };
                                }

                                //var result = _context.CatalogoLocal.FirstOrDefault(entity => entity.IdCatalogo == Id);

                                //CatalogoLocalViewModel CatalogoLocalViewModel = _context.CatalogoLocal
                                //           .FirstOrDefault(entity => entity.IdCatalogo == Id);



                                //ViewBag.GerenciaLocalValue = query;               
                                ViewBag.OpenGerenciaModal = TempData["OpenGerenciaLocal"];

                                TempData.Remove("PassedId");
                                TempData.Remove("OpenInativar");
                            }

                            List<VW_Usuario_NewViewModel> ViewUsuarioList = new List<VW_Usuario_NewViewModel>();
                            List<VW_Usuario_NewViewModel> ViewUsuarioListRelacionado = new List<VW_Usuario_NewViewModel>();
                            if (TempData.ContainsKey("OpenAtribuirUsuario"))
                            {
                                int? Id = TempData["PassedAtribuiId"] as int?;

                                var query = (from usuario in _contextBS.VW_Usuario
                                             where usuario.Ativo == 1
                                             select new
                                             {
                                                 usuario.Id,
                                                 usuario.Nome,
                                                 usuario.CodSituacao
                                             })
                                             .Where(tmp => tmp.CodSituacao == "A" &&
                                                           _contextBS.Permissao.Any(permissao => permissao.IdUsuario == tmp.Id && permissao.IdAcesso == 923))
                                             .OrderBy(tmp => tmp.Nome)
                                             .Select(tmp => new VW_Usuario_NewViewModel
                                             {
                                                 Id = tmp.Id,
                                                 Nome = tmp.Nome
                                             })
                                             .ToList();

                                ViewUsuarioList = query.ToList();

                                if (ViewUsuarioList != null)
                                {
                                    var ProdutoVsMonitor = _context.ProdutoVsMonitor.Where(i => i.IdProduto == id).ToList();
                                    if (ProdutoVsMonitor != null)
                                    {
                                        foreach (var item in ProdutoVsMonitor)
                                        {
                                            // Assuming 'IdLogin' is a property in 'ProdutoVsMonitor'
                                            var removedUser = ViewUsuarioList.Find(user => user.Id == item.IdLogin);

                                            if (removedUser != null)
                                            {
                                                ViewUsuarioListRelacionado.Add(removedUser);
                                                ViewUsuarioList.Remove(removedUser);
                                            }
                                        }
                                    }

                                }

                                ViewBag.OpenAtribuiUsuario = TempData["OpenAtribuirUsuario"];

                                TempData.Remove("PassedAtribuiId");
                                TempData.Remove("OpenAtribuirUsuario");
                            }


                            FormEstoqueSaidaViewModel EditarSaida = (from produto in _context.Produto
                                                                     join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                                     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                     where produto.Id == id
                                                                     select new FormEstoqueSaidaViewModel
                                                                     {
                                                                         ProdutoId = produto.Id,
                                                                         ProdutoQuantidade = produto.Quantidade,
                                                                         ProdutoQuantidadeMinima = produto.QuantidadeMinima,
                                                                         ProdutoRFM = produto.RFM,
                                                                         ProdutoObservacao = produto.Observacao,
                                                                         CatalogoId = catalogo.Id,
                                                                         CatalogoCodigo = catalogo.Codigo,
                                                                         CatalogoDescricao = catalogo.Descricao,
                                                                         CatalogoNome = catalogo.Nome,
                                                                         CatalogoPorAferido = catalogo.PorAferido,
                                                                         CatalogoPorMetro = catalogo.PorMetro,
                                                                         CatalogoPorSerial = catalogo.PorSerial,
                                                                         CategoriaId = categoria.Id,
                                                                         IdCategoriaPai = categoria.IdCategoria,
                                                                         CategoriaNome = categoria.Nome,
                                                                         CategoriaClasse = categoria.Classe
                                                                     }).FirstOrDefault();

                            var Classe = _context.Categoria
                                     .FirstOrDefault(u => u.Id == EditarSaida.IdCategoriaPai);

                            EditarSaida.ClasseNome = Classe.Nome;

                            string? CatalogoNome = "";
                            switch (EditarSaida.CategoriaClasse)
                            {
                                case 1:
                                    CatalogoNome = "Ferramenta";
                                    break;
                                case 2:
                                    CatalogoNome = "EPI";
                                    break;
                                case 3:
                                    CatalogoNome = "Consumíveis";
                                    break;
                            }

                            EditarSaida.CatalogoType = CatalogoNome;

                            var mapper = mapeamentoClasses.CreateMapper();

                            FormEstoqueSaidaViewModel EstoqueViewModel = mapper.Map<FormEstoqueSaidaViewModel>(EditarSaida);

                            var combinedViewModel = new CombinedViewModel
                            {
                                FormEstoqueSaidaViewModel = EstoqueViewModel,
                                CatalogoLocalViewModel = catalogoLocalViewModel,
                                VW_Usuario_NewViewModel = ViewUsuarioList,
                                VW_Usuario_NewViewModelRelacionado = ViewUsuarioListRelacionado
                            };

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
                ViewBag.Error = ex.Message;
                return View();
            }
            //Error handling
            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

          

            //return View(EstoqueViewModel);

        }

        public IActionResult EditSaida(CombinedViewModel EstoqueValues)
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


                            if (EstoqueValues.FormEstoqueSaidaViewModel.ProdutoQuantidadeMinima == null)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Quantidade Minima is 0.";

                                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
                            }

                            if (EstoqueValues.FormEstoqueSaidaViewModel.ProdutoObservacao.Length > 250)
                            {
                                //TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Observacao Length Exceeded.";

                                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
                            }

                            var EditProduto = _context.Produto.Where(t => t.Id == EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId).FirstOrDefault();
                            string? OldObservacao = EditProduto.Observacao;
                            if (EditProduto != null)
                            {
                                EditProduto.QuantidadeMinima = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoQuantidadeMinima;
                                EditProduto.Observacao = EstoqueValues.FormEstoqueSaidaViewModel.Observacao;
                            }

                            _context.SaveChanges();

                            var InsertLogProduto = new LogProduto
                            {
                                IdProduto = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId,
                                ObservacaoDe = OldObservacao,
                                ObservacaoPara = EstoqueValues.FormEstoqueSaidaViewModel.Observacao,
                                IdUsuario = loggedUser.Id,
                                Acao = 2
                            };

                            _context.Add(InsertLogProduto);
                            _context.SaveChanges();

                            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });


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

                //Error handling
                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
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
                return View();
            }

        }

        public ActionResult GetGerenciadorLocal(int? CatalogoId, int? ProdutoId)
        {
            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

            if (CatalogoId == null)
            {
                //TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "No Id selected for Editar Estoque.";

                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = ProdutoId });
            }

            TempData["OpenGerenciaLocal"] = true;
            TempData["PassedId"] = CatalogoId;

            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = ProdutoId });
        }

        public ActionResult GetAtribuirUsuarioToEdit(int? id)
        {
            if (id != null)
            {

                List<VW_Usuario_NewViewModel> ViewUsuarioList = new List<VW_Usuario_NewViewModel>();
                List<VW_Usuario_NewViewModel> ViewUsuarioListRelacionado = new List<VW_Usuario_NewViewModel>();

                var query = (from usuario in _contextBS.VW_Usuario
                             where usuario.Ativo == 1
                             select new
                             {
                                 usuario.Id,
                                 usuario.Nome,
                                 usuario.CodSituacao
                             })
                        .Where(tmp => tmp.CodSituacao == "A" &&
                                      _contextBS.Permissao.Any(permissao => permissao.IdUsuario == tmp.Id && permissao.IdAcesso == 923))
                        .OrderBy(tmp => tmp.Nome)
                        .Select(tmp => new VW_Usuario_NewViewModel
                        {
                            Id = tmp.Id,
                            Nome = tmp.Nome
                        })
                        .ToList();

                ViewUsuarioList = query.ToList();

                if (ViewUsuarioList != null)
                {
                    var ProdutoVsMonitor = _context.ProdutoVsMonitor.Where(i => i.IdProduto == id).ToList();
                    if (ProdutoVsMonitor != null)
                    {
                        foreach (var item in ProdutoVsMonitor)
                        {
                            // Assuming 'IdLogin' is a property in 'ProdutoVsMonitor'
                            var removedUser = ViewUsuarioList.Find(user => user.Id == item.IdLogin);

                            if (removedUser != null)
                            {
                                ViewUsuarioListRelacionado.Add(removedUser);
                                ViewUsuarioList.Remove(removedUser);
                            }
                        }
                    }

                }


                ViewBag.VW_Usuario_NewViewModel = ViewUsuarioList;
                ViewBag.VW_Usuario_NewViewModelRelacionado = ViewUsuarioListRelacionado;
                ViewBag.IdProduto = id;

                ViewBag.ddlEmpresa = ListEmpressa;
                ViewBag.ddlObra = ListObra;

                var GestorEditModel = HttpContext.Session.GetObject<GestorEdit?>(SessionKeyGestorEditValues) ?? new GestorEdit();
                return View(nameof(ToEdit), GestorEditModel);

            }
            else
            {
                ViewBag.Error = "No Id selected for Editar Estoque.";
                return View(nameof(ToEdit));
            }

        }

        public IActionResult AddToDatabaseToEdit(List<int?>? selectedIds, int? IdProduto)
        {
            var GestorEditModel = HttpContext.Session.GetObject<GestorEdit?>(SessionKeyGestorEditValues) ?? new GestorEdit();
            if (selectedIds.Count != 0)
            {
                foreach (var item in selectedIds)
                {
                    var InsertToProdutoVsMonitor = new ProdutoVsMonitor
                    {
                        IdLogin = item,
                        IdProduto = IdProduto
                    };

                    _context.Add(InsertToProdutoVsMonitor);
                    _context.SaveChanges();
                }

                ViewBag.ShowSuccessAlert = true;
                ViewBag.ddlEmpresa = ListEmpressa;
                ViewBag.ddlObra = ListObra;
           
                return View(nameof(ToEdit), GestorEditModel);
            }
            else
            {
                ViewBag.Error = "No users selected for Atribuir ou Remover Usuario.";
                ViewBag.ddlEmpresa = ListEmpressa;
                ViewBag.ddlObra = ListObra;
                return View(nameof(ToEdit), GestorEditModel);
            }
        }

        public IActionResult DeleteToDatabaseToEdit(List<int> deletedIds, int? IdProduto)
        {
            var GestorEditModel = HttpContext.Session.GetObject<GestorEdit?>(SessionKeyGestorEditValues) ?? new GestorEdit();
            if (deletedIds.Count != 0)
            {
                foreach (var idToDelete in deletedIds)
                {
                    var ProdutoVsMonitorToDelete = _context.ProdutoVsMonitor.FirstOrDefault(item => item.IdProduto == IdProduto && item.IdLogin == idToDelete);

                    if (ProdutoVsMonitorToDelete != null)
                    {
                        _context.ProdutoVsMonitor.Remove(ProdutoVsMonitorToDelete);
                        _context.SaveChanges();
                    }
                }

                ViewBag.ShowSuccessAlert = true;
                ViewBag.ddlEmpresa = ListEmpressa;
                ViewBag.ddlObra = ListObra;
                return View(nameof(ToEdit), GestorEditModel);
            }
            else
            {
                ViewBag.Error = "No users selected for Atribuir ou Remover Usuario.";
                ViewBag.ddlEmpresa = ListEmpressa;
                ViewBag.ddlObra = ListObra;
                return View(nameof(ToEdit), GestorEditModel);
            }


        }

        public ActionResult GetAtribuirUsuario(int? id)
        {
            if (id == null)
            {
                //TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "No Id selected for Editar Estoque.";

                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = id });
            }

            TempData["OpenAtribuirUsuario"] = true;
            TempData["PassedAtribuiId"] = id;

            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = id });
        }

        public  IActionResult SaveCatalogoLocal(CombinedViewModel CatalogoLocalValues)
        {

            var CheckCatalogoLocal = _context.CatalogoLocal.FirstOrDefault(i => i.IdCatalogo == CatalogoLocalValues.CatalogoLocalViewModel.IdCatalogo);

            if (CheckCatalogoLocal != null)
            {
                CheckCatalogoLocal.Pos1Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Prateleira;
                CheckCatalogoLocal.Pos1Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Coluna;
                CheckCatalogoLocal.Pos1Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Linha;

                CheckCatalogoLocal.Pos2Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Prateleira;
                CheckCatalogoLocal.Pos2Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Coluna;
                CheckCatalogoLocal.Pos2Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Linha;

                CheckCatalogoLocal.Pos3Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Prateleira;
                CheckCatalogoLocal.Pos3Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Coluna;
                CheckCatalogoLocal.Pos3Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Linha;

                CheckCatalogoLocal.Pos4Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Prateleira;
                CheckCatalogoLocal.Pos4Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Coluna;
                CheckCatalogoLocal.Pos4Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Linha;

                CheckCatalogoLocal.Pos5Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Prateleira;
                CheckCatalogoLocal.Pos5Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Coluna;
                CheckCatalogoLocal.Pos5Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Linha;

                CheckCatalogoLocal.Pos6Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Prateleira;
                CheckCatalogoLocal.Pos6Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Coluna;
                CheckCatalogoLocal.Pos6Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Linha;

                CheckCatalogoLocal.Pos7Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Prateleira;
                CheckCatalogoLocal.Pos7Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Coluna;
                CheckCatalogoLocal.Pos7Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Linha;

                CheckCatalogoLocal.Pos8Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Prateleira;
                CheckCatalogoLocal.Pos8Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Coluna;
                CheckCatalogoLocal.Pos8Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Linha;

                CheckCatalogoLocal.Pos9Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Prateleira;
                CheckCatalogoLocal.Pos9Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Coluna;
                CheckCatalogoLocal.Pos9Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Linha;

                CheckCatalogoLocal.Pos10Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Prateleira;
                CheckCatalogoLocal.Pos10Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Coluna;
                CheckCatalogoLocal.Pos10Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Linha;

                _context.SaveChanges();
            }
            else
            {
                var InsertToCatalogoLocal = new CatalogoLocal
                {
                    IdCatalogo = CatalogoLocalValues.CatalogoLocalViewModel.IdCatalogo,
                    IdFerramentaria = CatalogoLocalValues.CatalogoLocalViewModel.IdFerramentaria,

                    Pos1Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Prateleira,
                    Pos1Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Coluna,
                    Pos1Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos1Linha,

                    Pos2Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Prateleira,
                    Pos2Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Coluna,
                    Pos2Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos2Linha,

                    Pos3Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Prateleira,
                    Pos3Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Coluna,
                    Pos3Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos3Linha,

                    Pos4Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Prateleira,
                    Pos4Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Coluna,
                    Pos4Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos4Linha,

                    Pos5Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Prateleira,
                    Pos5Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Coluna,
                    Pos5Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos5Linha,

                    Pos6Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Prateleira,
                    Pos6Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Coluna,
                    Pos6Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos6Linha,

                    Pos7Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Prateleira,
                    Pos7Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Coluna,
                    Pos7Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos7Linha,

                    Pos8Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Prateleira,
                    Pos8Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Coluna,
                    Pos8Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos8Linha,

                    Pos9Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Prateleira,
                    Pos9Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Coluna,
                    Pos9Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos9Linha,

                    Pos10Prateleira = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Prateleira,
                    Pos10Coluna = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Coluna,
                    Pos10Linha = CatalogoLocalValues.CatalogoLocalViewModel.Pos10Linha,

                    DataRegistro = DateTime.Now,
                };

                _context.Add(InsertToCatalogoLocal);
                _context.SaveChanges();  
            }

            TempData["ShowSuccessAlert"] = true;
            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = CatalogoLocalValues.FormEstoqueSaidaViewModel.ProdutoId });
        }

        public IActionResult AddToDatabase(List<int> selectedIds, CombinedViewModel EstoqueValues)
        {
            if (selectedIds.Count != 0)
            {
                foreach (var item in selectedIds)
                {
                    var InsertToProdutoVsMonitor = new ProdutoVsMonitor
                    {
                        IdLogin = item,
                        IdProduto = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId
                    };

                    _context.Add(InsertToProdutoVsMonitor);
                    _context.SaveChanges();
                }
            }
            else
            {
                TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "No users selected for Atribuir ou Remover Usuario.";

                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
            }

            TempData["ShowSuccessAlert"] = true;

            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
        }

        public IActionResult DeleteToDatabase(List<int> deletedIds, CombinedViewModel EstoqueValues)
        {

            if (deletedIds.Count != 0)
            {
                foreach (var idToDelete in deletedIds)
                {
                    var ProdutoVsMonitorToDelete = _context.ProdutoVsMonitor
                                         .FirstOrDefault(item => item.IdProduto == EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId && item.IdLogin == idToDelete);

                    if (ProdutoVsMonitorToDelete != null)
                    {
                        _context.ProdutoVsMonitor.Remove(ProdutoVsMonitorToDelete);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "No users selected for Atribuir ou Remover Usuario.";

                return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
            }

            TempData["ShowSuccessAlert"] = true;

            return RedirectToAction(nameof(EditarEstoqueSaida), new { id = EstoqueValues.FormEstoqueSaidaViewModel.ProdutoId });
        }



        #endregion


        #region RedirectBack
        public ActionResult BackToGestor()
        {
            var ListGestorModel = HttpContext.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();

            if (ListGestorModel.Count != 0)
            {
                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListGestor);
            }

            //if (_ListGestor.Count != 0)
            //{
            //    _ListGestor.Clear();
            //}
            
            return RedirectToAction(nameof(Index));
        }

        public ActionResult BackToSaidaEstoque()
        {
            var ListGestorModel = HttpContext.Session.GetObject<List<SP_1600012731_EstoqueViewModel>>(SessionKeyListGestor) ?? new List<SP_1600012731_EstoqueViewModel>();

            if (ListGestorModel.Count != 0)
            {
                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListGestor);
            }
            //if (_ListGestor.Count != 0)
            //{
            //    _ListGestor.Clear();
            //}

            //if (_ListGestor.Count != 0)
            //{
            //    _ListGestor.Clear();
            //}

            return RedirectToAction(nameof(EstoqueSaida));
        }

        #endregion

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






        #region not used

        public ActionResult IndexCopy(string? Codigo, string? Item, string? AF, int? PAT)
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
                usuario.Pagina1 = "thEstoque.aspx";
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

                //Error handling
                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
                //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

                if (TempData.ContainsKey("ErrorMessage"))
                {
                    ViewBag.Error = TempData["ErrorMessage"].ToString();
                    TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                }

                if (TempData.ContainsKey("ShowSuccessAlert"))
                {
                    ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"].ToString();
                    TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                }

                int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

                if (FerramentariaValue == null)
                {
                    var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                              where ferramentaria.Ativo == 1 &&
                                                    !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                    _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
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
                    var nome = _context.Ferramentaria
                                .Where(f => f.Id == FerramentariaValue)
                                .Select(f => f.Nome)
                                .FirstOrDefault();

                    ViewData["GlobalFerramentariaValue"] = nome;

                    //GlobalFerramentariaName = nome;
                    //ViewBag.FerramentariaName = GlobalFerramentariaName;
                }

                if (!string.IsNullOrEmpty(Codigo) || !string.IsNullOrEmpty(Item) || !string.IsNullOrEmpty(AF) || PAT != null)
                {
                    var SearchFilter = new SearchGestorModel
                    {
                        Codigo = Codigo,
                        Item = Item,
                        AF = AF,
                        PAT = PAT,
                    };


                    var combinedGestor = new CombinedGestor
                    {
                        SearchGestorModel = SearchFilter
                    };
                    return View(combinedGestor);
                }
                else
                {

                }


                return View();
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


        //  private List<SP_1600012731_Estoque> ExecuteYourStoredProcedure(
        //string? catalogo,
        //string? classe,
        //string? tipo,
        //int? pat,
        //int? serieNull,
        //int? dataVencimentoNull,
        //int? status,
        //int? saldo,
        //int? situacao,
        //out TimeSpan bdTempoInicial,
        //out TimeSpan bdTempoFinal)
        //  {

        //      var parameters = new List<SqlParameter>
        //        {
        //            // Create SqlParameter objects for each parameter
        //            new SqlParameter("@P_Catalogo", catalogo),
        //            new SqlParameter("@P_Classe", classe),
        //            new SqlParameter("@P_Tipo", tipo),
        //            new SqlParameter("@P_Pat", pat),
        //            new SqlParameter("@P_SerieNull", serieNull),
        //            new SqlParameter("@P_DataVencimentoNull", dataVencimentoNull),
        //            new SqlParameter("@P_Status", status),
        //            new SqlParameter("@P_Saldo", saldo),
        //            new SqlParameter("@P_Situacao", situacao),

        //        };

        //      // Call the stored procedure using FromSqlRaw
        //      var result = _context.SP_1600012731_Estoque
        //          .FromSqlRaw("SP_1600012731_Estoque " +
        //              "@P_Catalogo, @P_Classe, @P_Tipo," +
        //              "@P_Pat," +
        //              "@P_SerieNull, @P_DataVencimentoNull, " +
        //              "@P_Status, @P_Saldo, @P_Situacao ", parameters.ToArray())
        //          .ToList();

        //      // Retrieve the output parameter values
        //      bdTempoInicial = (TimeSpan)parameters[^2].Value;
        //      bdTempoFinal = (TimeSpan)parameters[^1].Value;

        //      return result;
        //  }

        // POST: Gestor/Delete/5
        #endregion




    }
}
