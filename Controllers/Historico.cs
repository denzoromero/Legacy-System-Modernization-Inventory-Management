using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using FerramentariaTest.EntitiesBS;

using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;

namespace FerramentariaTest.Controllers
{
    //public class GlobalDataHistorico
    //{
    //    public static List<HistoricoViewModel> ListHistorico { get; set; }

    //}

    public class Historico : Controller
    {
        private const string SessionKeyHistoricoList = "HistoricoList";

        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thHistorico.aspx";
        //private MapperConfiguration mapeamentoClasses;

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();
        //private static CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

        //private static UserViewModel? StaticEmployee = new UserViewModel();
        //public List<HistoricoViewModel?>? StaticListHistorico = new List<HistoricoViewModel?>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";


        private const string SessionKeyHistoricoSolicitante = "HistoricoSolicitante";

        public Historico(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //mapeamentoClasses = new MapperConfiguration(cfg =>
            //{
            //    // Loop through the years (2010 to 2023)
            //    for (int year = 2013; year <= 2023; year++)
            //    {
            //        // Construct the class names dynamically
            //        string baseClassName = "HistoricoAlocacao";
            //        string fullClassName = $"{baseClassName}_{year}";

            //        // Using reflection to get the types dynamically
            //        Type baseType = Type.GetType($"FerramentariaTest.Entities.{baseClassName}");
            //        Type fullType = Type.GetType($"FerramentariaTest.Entities.{fullClassName}");

            //        if (baseType != null && fullType != null)
            //        {
            //            // Creating a mapping from base class to derived class
            //            cfg.CreateMap(baseType, fullType);

            //            // Creating a reverse mapping from derived class to base class
            //            cfg.CreateMap(fullType, baseType);
            //        }
            //    }
            //});
        }

        // GET: Historico
        public ActionResult Index(DateTime? DateAdmission, DateTime? TrasacoesDe, DateTime? TrasacoesAte)
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


                            //httpContextAccessor.HttpContext?.Session.Remove(Sessao.Solicitante);
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyHistoricoSolicitante);

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ErrorList"))
                            {
                                ViewBag.ErrorList = (string[]?)TempData["ErrorList"];
                                TempData.Remove("ErrorList"); // Remove it from TempData to avoid displaying it again
                            }
                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            //Observacao Modal
                            if (TempData.ContainsKey("OpenModal"))
                            {
                                ViewBag.Observacao = TempData["GetObs"];
                                ViewBag.Codigo = TempData["GetCodigo"];
                                ViewBag.OpenObsModal = TempData["OpenModal"];
                                TempData.Remove("GetObs");
                                TempData.Remove("GetCodigo");
                                TempData.Remove("OpenModal");
                            }

                            //Single Upload Modal
                            if (TempData.ContainsKey("OpenSingleUpload"))
                            {
                                int? IdHistoricoAlocacao = TempData["IdHistoricoAlocacao"] as int?;

                                if (IdHistoricoAlocacao != null)
                                {
                                    ViewBag.IdHolder = IdHistoricoAlocacao;
                                    var GetListImage = from arquivo in _context.Arquivo
                                                       where arquivo.Ativo == 1 &&
                                                             _context.ArquivoVsHistorico
                                                                 .Where(avh => avh.IdHistoricoAlocacao == IdHistoricoAlocacao)
                                                                 .Select(avh => avh.IdArquivo)
                                                                 .Contains(arquivo.Id)
                                                       orderby arquivo.DataRegistro ascending
                                                       select new ArquivoViewModel
                                                       {
                                                           Id = arquivo.Id,
                                                           Ano = arquivo.Ano,
                                                           Tipo = arquivo.Tipo,
                                                           ArquivoNome = arquivo.ArquivoNome,
                                                           DataRegistro = arquivo.DataRegistro,
                                                           Ativo = arquivo.Ativo,
                                                           Solicitante_IdTerceiro = arquivo.Solicitante_IdTerceiro,
                                                           Solicitante_CodColigada = arquivo.Solicitante_CodColigada,
                                                           Solicitante_Chapa = arquivo.Solicitante_Chapa,
                                                           ImageData = arquivo.ImageData != null ? arquivo.ImageData.ToArray() : null
                                                       };

                                    ViewBag.AlocadoImages = GetListImage;
                                    ViewBag.OpenSingle = TempData["OpenSingleUpload"];
                                }

                                TempData.Remove("OpenSingleUpload");
                                TempData.Remove("ProdutoAlocadoId");
                            }


                            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                            //if (FuncionarioValue != null)
                            //{
                            //    UsuarioModel = searches.SearchEmployeeOnLoad();
                            //    historicoViewModel.UserViewModel = UsuarioModel;
                            //}

                            //if (GlobalDataHistorico.ListHistorico != null && GlobalDataHistorico.ListHistorico.Count != 0)
                            //{
                            //    ViewBag.Historico = GlobalDataHistorico.ListHistorico;
                            //}

                            //CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

                            //StaticCombinedHistorico.SearchHistoricoModel.EPI = 2;


                            //usuariofer.Retorno =;
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

        public ActionResult SearchHistoricoNew(SearchHistoricoViewModel? filters)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            StaticCombinedHistorico.SearchHistoricoModel = filters ?? new SearchHistoricoViewModel();


            try
            {
                List<HistoricoViewModel> TotalHistorico = new List<HistoricoViewModel>();
               

                UserViewModel? SolicitanteModel = new UserViewModel();
                //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                    StaticCombinedHistorico.UserViewModel = SolicitanteModel;
                }

                string? error = ValidateSearch(filters);
                if (string.IsNullOrEmpty(error))
                {
                    if (filters.TransacoesDe.HasValue && filters.TransacoesAte.HasValue)
                    {
                        DateTime? dateOfAdmission = SolicitanteModel.DataAdmissao.Value;
                        DateTime? TransactionDe = filters.TransacoesDe.Value;
                        DateTime? TransactionAte = filters.TransacoesAte.Value;
                        string? FuncionarioValue = SolicitanteModel.Chapa;
                        int? CodColigadaValue = SolicitanteModel.CodColigada;
                        List<int?>? ClasseList = new List<int?>();

                        if (filters.Ferramenta != null)
                        {
                            ClasseList.Add(filters.Ferramenta);
                        }
                        if (filters.EPI != null)
                        {
                            ClasseList.Add(filters.EPI);
                        }
                        if (filters.Consumiveis != null)
                        {
                            ClasseList.Add(filters.Consumiveis);
                        }

                        for (int year = TransactionDe.Value.Year; year <= TransactionAte.Value.Year; year++)
                        {
                            // Build the table name dynamically
                            string tableName = $"HistoricoAlocacao_{year}";

                            var dbSetProperties = _context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                            // Find the DbSet property that matches the expected table name
                            var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                            if (tableProperty != null)
                            {
                                List<HistoricoViewModel> result = new List<HistoricoViewModel>();
                                // Use the DbSet property value in your query
                                var table = tableProperty.GetValue(_context, null);

                                if (table != null)
                                {
                                    var query = from hist in (IQueryable<HistoricoAlocacao>)table
                                                join prod in _context.Produto on hist.IdProduto equals prod.Id
                                                join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                join ferrOrig in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                join ferrDev in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDev.Id into ferrDevGroup
                                                from ferrDev in ferrDevGroup.DefaultIfEmpty()
                                                where hist.Solicitante_Chapa == FuncionarioValue
                                                      && hist.Solicitante_CodColigada == CodColigadaValue
                                                      && (string.IsNullOrEmpty(filters.Observacao) || hist.Observacao.Contains(filters.Observacao))
                                                      && (!filters.PrevisaoDe.HasValue || hist.DataPrevistaDevolucao >= filters.PrevisaoDe)
                                                      && (!filters.PrevisaoAte.HasValue || hist.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (!TransactionDe.HasValue || hist.DataEmprestimo >= TransactionDe)
                                                      && (!TransactionAte.HasValue || hist.DataDevolucao <= TransactionAte.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (string.IsNullOrEmpty(filters.AF) || prod.AF.Contains(filters.AF))
                                                      && (filters.PAT == null || prod.PAT == filters.PAT)
                                                      && (string.IsNullOrEmpty(filters.Codigo) || cat.Codigo.Contains(filters.Codigo))
                                                      && (string.IsNullOrEmpty(filters.Catalogo) || cat.Nome.Contains(filters.Catalogo))
                                                      && (ClasseList.Count == 0 || ClasseList.Contains(categoria.Classe))
                                                //&& (categoria.Classe == 1 || categoria.Classe == 2)
                                                select new HistoricoViewModel
                                                {
                                                    IdHistoricoAlocacao = hist.Id,
                                                    IdProdutoAlocado = 0,
                                                    IdProduto = hist.IdProduto,
                                                    Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                    Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                    Solicitante_Chapa = hist.Solicitante_Chapa,
                                                    Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                    Liberador_CodColigada = hist.Liberador_CodColigada,
                                                    Liberador_Chapa = hist.Liberador_Chapa,
                                                    Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                    Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                    Observacao = hist.Observacao,
                                                    DataEmprestimo = hist.DataEmprestimo,
                                                    DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                    DataDevolucao = hist.DataDevolucao,
                                                    IdObra = hist.IdObra,
                                                    Quantidade = hist.Quantidade,
                                                    QuantidadeEmprestada = cat.PorAferido == 0 && cat.PorSerial == 0
                                                        ? _context.ProdutoAlocado
                                                                  .Where(ap => ap.IdProduto == hist.IdProduto &&
                                                                               ap.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                               ap.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                               ap.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                  .Select(ap => ap.Quantidade)
                                                                  .FirstOrDefault()
                                                        : 0,
                                                    IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                    IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                    CodigoCatalogo = cat.Codigo,
                                                    NomeCatalogo = cat.Nome,
                                                    FerrOrigem = ferrOrig.Nome,
                                                    FerrDevolucao = ferrDev.Nome ?? "",
                                                    AFProduto = prod.AF,
                                                    Serie = prod.Serie,
                                                    PATProduto = prod.PAT,
                                                    IdControleCA = (int?)null,
                                                    IdReservation = hist.IdReservation
                                                };

                                    result.AddRange(query);
                                    // Proceed with your query using the specific table for the year
                                }

                                TotalHistorico.AddRange(result);
                            }
                        }

                        //Getting the values of Classe from Emprestimo Fields
                        List<int?>? ClasseListForProdutoAlocado = new List<int?>();
                        if (filters.EmprestadoFerramenta != null)
                        {
                            ClasseListForProdutoAlocado.Add(filters.EmprestadoFerramenta);
                        }
                        if (filters.EmprestadoEPI != null)
                        {
                            ClasseListForProdutoAlocado.Add(filters.EmprestadoEPI);
                        }

                        if (ClasseListForProdutoAlocado.Count > 0)
                        {
                            var produtoAlocadoQuery = from produtoAlocado in _context.ProdutoAlocado
                                                      join prod in _context.Produto on produtoAlocado.IdProduto equals prod.Id
                                                      join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                      join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                      join ferrOrig in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                      where produtoAlocado.Solicitante_Chapa == FuncionarioValue
                                                            && produtoAlocado.Solicitante_CodColigada == CodColigadaValue
                                                            && (string.IsNullOrEmpty(filters.Observacao) || produtoAlocado.Observacao.Contains(filters.Observacao))
                                                            && (!TransactionDe.HasValue || produtoAlocado.DataEmprestimo >= TransactionDe)
                                                            && (!TransactionAte.HasValue || produtoAlocado.DataEmprestimo <= TransactionAte.Value.Date.AddDays(1).AddTicks(-1))
                                                            && (!filters.PrevisaoDe.HasValue || produtoAlocado.DataPrevistaDevolucao >= filters.PrevisaoDe)
                                                            && (!filters.PrevisaoAte.HasValue || produtoAlocado.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                                            && (string.IsNullOrEmpty(filters.AF) || prod.AF.Contains(filters.AF))
                                                            && (filters.PAT == null || prod.PAT == filters.PAT)
                                                            && (string.IsNullOrEmpty(filters.Codigo) || cat.Codigo.Contains(filters.Codigo))
                                                            && (string.IsNullOrEmpty(filters.Catalogo) || cat.Nome.Contains(filters.Catalogo))
                                                            && (ClasseListForProdutoAlocado.Count == 0 || ClasseListForProdutoAlocado.Contains(categoria.Classe))
                                                      select new HistoricoViewModel
                                                      {
                                                          IdHistoricoAlocacao = 0,
                                                          IdProdutoAlocado = produtoAlocado.Id,
                                                          IdProduto = produtoAlocado.IdProduto,
                                                          Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
                                                          Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
                                                          Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
                                                          Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
                                                          Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
                                                          Liberador_Chapa = produtoAlocado.Liberador_Chapa,
                                                          Balconista_Emprestimo_IdLogin = produtoAlocado.Balconista_IdLogin,
                                                          Balconista_Devolucao_IdLogin = 0,
                                                          Observacao = produtoAlocado.Observacao,
                                                          DataEmprestimo = produtoAlocado.DataEmprestimo,
                                                          DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
                                                          DataDevolucao = null,
                                                          IdObra = produtoAlocado.IdObra,
                                                          Quantidade = produtoAlocado.Quantidade,
                                                          QuantidadeEmprestada = produtoAlocado.Quantidade,
                                                          IdFerrOndeProdRetirado = produtoAlocado.IdFerrOndeProdRetirado,
                                                          IdFerrOndeProdDevolvido = 0,
                                                          CodigoCatalogo = cat.Codigo,
                                                          NomeCatalogo = cat.Nome,
                                                          FerrOrigem = ferrOrig.Nome,
                                                          FerrDevolucao = "",
                                                          AFProduto = prod.AF,
                                                          Serie = prod.Serie,
                                                          PATProduto = prod.PAT,
                                                          IdControleCA = produtoAlocado.IdControleCA,
                                                          IdReservation = produtoAlocado.IdReservation
                                                      };

                            TotalHistorico = TotalHistorico.Union(produtoAlocadoQuery).OrderByDescending(temp => temp.DataEmprestimo).ToList();

                        }

                    }
                    else
                    {

                        DateTime? TransactionDe = SolicitanteModel.DataAdmissao.Value;
                        DateTime? TransactionAte = DateTime.Now;
                        string? FuncionarioValue = SolicitanteModel.Chapa;
                        int? CodColigadaValue = SolicitanteModel.CodColigada;
                        List<int?>? ClasseList = new List<int?>();

                        if (filters.Ferramenta != null)
                        {
                            ClasseList.Add(filters.Ferramenta);
                        }
                        if (filters.EPI != null)
                        {
                            ClasseList.Add(filters.EPI);
                        }
                        if (filters.Consumiveis != null)
                        {
                            ClasseList.Add(filters.Consumiveis);
                        }

                        for (int year = TransactionDe.Value.Year; year <= TransactionAte.Value.Year; year++)
                        {
                            // Build the table name dynamically
                            string tableName = $"HistoricoAlocacao_{year}";

                            var dbSetProperties = _context.GetType().GetProperties()
                                              .Where(p => p.PropertyType.IsGenericType &&
                                                          p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                            // Find the DbSet property that matches the expected table name
                            var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                            if (tableProperty != null)
                            {
                                List<HistoricoViewModel> result = new List<HistoricoViewModel>();
                                // Use the DbSet property value in your query
                                var table = tableProperty.GetValue(_context, null);

                                //Getting Values from HistoricoAlocacao
                                if (table != null)
                                {
                                    var query = from hist in (IQueryable<HistoricoAlocacao>)table
                                                join prod in _context.Produto on hist.IdProduto equals prod.Id
                                                join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                join ferrOrig in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                join ferrDev in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDev.Id into ferrDevGroup
                                                from ferrDev in ferrDevGroup.DefaultIfEmpty()
                                                where hist.Solicitante_Chapa == FuncionarioValue
                                                      && hist.Solicitante_CodColigada == CodColigadaValue
                                                      && (string.IsNullOrEmpty(filters.Observacao) || hist.Observacao.Contains(filters.Observacao))
                                                      && (!filters.PrevisaoDe.HasValue || hist.DataPrevistaDevolucao >= filters.PrevisaoDe)
                                                      && (!filters.PrevisaoAte.HasValue || hist.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (!TransactionDe.HasValue || hist.DataEmprestimo >= TransactionDe)
                                                      && (!TransactionAte.HasValue || hist.DataDevolucao <= TransactionAte.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (string.IsNullOrEmpty(filters.AF) || prod.AF.Contains(filters.AF))
                                                      && (filters.PAT == null || prod.PAT == filters.PAT)
                                                      && (string.IsNullOrEmpty(filters.Codigo) || cat.Codigo.Contains(filters.Codigo))
                                                      && (string.IsNullOrEmpty(filters.Catalogo) || cat.Nome.Contains(filters.Catalogo))
                                                      && (ClasseList.Count == 0 || ClasseList.Contains(categoria.Classe))
                                                select new HistoricoViewModel
                                                {
                                                    IdHistoricoAlocacao = hist.Id,
                                                    IdProdutoAlocado = 0,
                                                    IdProduto = hist.IdProduto,
                                                    Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                    Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                    Solicitante_Chapa = hist.Solicitante_Chapa,
                                                    Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                    Liberador_CodColigada = hist.Liberador_CodColigada,
                                                    Liberador_Chapa = hist.Liberador_Chapa,
                                                    Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                    Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                    Observacao = hist.Observacao,
                                                    DataEmprestimo = hist.DataEmprestimo,
                                                    DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                    DataDevolucao = hist.DataDevolucao,
                                                    IdObra = hist.IdObra,
                                                    Quantidade = hist.Quantidade,
                                                    QuantidadeEmprestada = cat.PorAferido == 0 && cat.PorSerial == 0
                                                        ? _context.ProdutoAlocado
                                                                  .Where(ap => ap.IdProduto == hist.IdProduto &&
                                                                               ap.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                               ap.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                               ap.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                  .Select(ap => ap.Quantidade)
                                                                  .FirstOrDefault()
                                                        : 0,
                                                    IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                    IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                    CodigoCatalogo = cat.Codigo,
                                                    NomeCatalogo = cat.Nome,
                                                    FerrOrigem = ferrOrig.Nome,
                                                    FerrDevolucao = ferrDev.Nome ?? "",
                                                    AFProduto = prod.AF,
                                                    Serie = prod.Serie,
                                                    PATProduto = prod.PAT,
                                                    IdControleCA = (int?)null,
                                                    IdReservation = hist.IdReservation
                                                };

                                    result.AddRange(query);
                                    // Proceed with your query using the specific table for the year
                                }

                                //Adding all result of HistoricoAlocacao
                                TotalHistorico.AddRange(result);                      
                     
                            }
                        }

                        //Getting the values of Classe from Emprestimo Fields
                        List<int?>? ClasseListForProdutoAlocado = new List<int?>();
                        if (filters.EmprestadoFerramenta != null)
                        {
                            ClasseListForProdutoAlocado.Add(filters.EmprestadoFerramenta);
                        }
                        if (filters.EmprestadoEPI != null)
                        {
                            ClasseListForProdutoAlocado.Add(filters.EmprestadoEPI);
                        }

                        if (ClasseListForProdutoAlocado.Count > 0)
                        {
                                var produtoAlocadoQuery = from produtoAlocado in _context.ProdutoAlocado
                                                          join prod in _context.Produto on produtoAlocado.IdProduto equals prod.Id
                                                          join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                          join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                          join ferrOrig in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                          where produtoAlocado.Solicitante_Chapa == FuncionarioValue
                                                                && produtoAlocado.Solicitante_CodColigada == CodColigadaValue
                                                                && (string.IsNullOrEmpty(filters.Observacao) || produtoAlocado.Observacao.Contains(filters.Observacao))
                                                                && (!TransactionDe.HasValue || produtoAlocado.DataEmprestimo >= TransactionDe)
                                                                && (!TransactionAte.HasValue || produtoAlocado.DataEmprestimo <= TransactionAte.Value.Date.AddDays(1).AddTicks(-1))
                                                                && (!filters.PrevisaoDe.HasValue || produtoAlocado.DataPrevistaDevolucao >= filters.PrevisaoDe)
                                                                && (!filters.PrevisaoAte.HasValue || produtoAlocado.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                                                && (string.IsNullOrEmpty(filters.AF) || prod.AF.Contains(filters.AF))
                                                                && (filters.PAT == null || prod.PAT == filters.PAT)
                                                                && (string.IsNullOrEmpty(filters.Codigo) || cat.Codigo.Contains(filters.Codigo))
                                                                && (string.IsNullOrEmpty(filters.Catalogo) || cat.Nome.Contains(filters.Catalogo))
                                                                && (ClasseListForProdutoAlocado.Count == 0 || ClasseListForProdutoAlocado.Contains(categoria.Classe))
                                                          select new HistoricoViewModel
                                                          {
                                                              IdHistoricoAlocacao = 0,
                                                              IdProdutoAlocado = produtoAlocado.Id,
                                                              IdProduto = produtoAlocado.IdProduto,
                                                              Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
                                                              Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
                                                              Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
                                                              Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
                                                              Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
                                                              Liberador_Chapa = produtoAlocado.Liberador_Chapa,
                                                              Balconista_Emprestimo_IdLogin = produtoAlocado.Balconista_IdLogin,
                                                              Balconista_Devolucao_IdLogin = null,
                                                              Observacao = produtoAlocado.Observacao,
                                                              DataEmprestimo = produtoAlocado.DataEmprestimo,
                                                              DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
                                                              DataDevolucao = null,
                                                              IdObra = produtoAlocado.IdObra,
                                                              Quantidade = produtoAlocado.Quantidade,
                                                              QuantidadeEmprestada = produtoAlocado.Quantidade,
                                                              IdFerrOndeProdRetirado = produtoAlocado.IdFerrOndeProdRetirado,
                                                              IdFerrOndeProdDevolvido = 0,
                                                              CodigoCatalogo = cat.Codigo,
                                                              NomeCatalogo = cat.Nome,
                                                              FerrOrigem = ferrOrig.Nome,
                                                              FerrDevolucao = "",
                                                              AFProduto = prod.AF,
                                                              Serie = prod.Serie,
                                                              PATProduto = prod.PAT,
                                                              IdControleCA = produtoAlocado.IdControleCA,
                                                              IdReservation = produtoAlocado.IdReservation,
                                                          };

                                TotalHistorico = TotalHistorico.Union(produtoAlocadoQuery).OrderByDescending(temp => temp.DataEmprestimo).ToList();

                        }

                    }

                    if (filters.ItensExtraviados == true)
                    {
                        TotalHistorico = TotalHistorico.Where(hist => _context.ProdutoExtraviado.Any(pe => pe.IdProdutoAlocado == hist.IdProdutoAlocado && pe.Ativo == 1)).ToList();
                    }

                    if (TotalHistorico.Count > 0)
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


                        List<HistoricoViewModel>? history = (from hist in TotalHistorico
                                                             join userEmp in result on hist.Balconista_Emprestimo_IdLogin equals userEmp.Id
                                                             join userDev in result on hist.Balconista_Devolucao_IdLogin equals userDev.Id into devJoin
                                                             from userDev in devJoin.DefaultIfEmpty()
                                                             select new HistoricoViewModel
                                                             {
                                                                         IdHistoricoAlocacao = hist.IdHistoricoAlocacao,
                                                                         IdProdutoAlocado = hist.IdProdutoAlocado,
                                                                         IdProduto = hist.IdProduto,
                                                                         Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                                         Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                                         Solicitante_Chapa = hist.Solicitante_Chapa,
                                                                         Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                                         Liberador_CodColigada = hist.Liberador_CodColigada,
                                                                         Liberador_Chapa = hist.Liberador_Chapa,
                                                                         Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                                         Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                                         Observacao = hist.Observacao,
                                                                         DataEmprestimo = hist.DataEmprestimo,
                                                                         DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                                         DataDevolucao = hist.DataDevolucao,
                                                                         IdObra = hist.IdObra,
                                                                         Quantidade = hist.Quantidade,
                                                                         QuantidadeEmprestada = hist.Quantidade,
                                                                         IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                                         IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                                         CodigoCatalogo = hist.CodigoCatalogo,
                                                                         NomeCatalogo = hist.NomeCatalogo,
                                                                         FerrOrigem = hist.FerrOrigem,
                                                                         FerrDevolucao = hist.FerrDevolucao,
                                                                         AFProduto = hist.AFProduto,
                                                                         Serie = hist.Serie,
                                                                         PATProduto = hist.PATProduto,
                                                                         IdControleCA = hist.IdControleCA,
                                                                         Balconista_EmprestimoChapa = userEmp.Chapa,
                                                                        Balconista_DevolucaoChapa = userDev != null ? userDev.Chapa : string.Empty,
                                                                 IdReservation = hist.IdReservation
                                                             }).ToList();



                        foreach (HistoricoViewModel item in history)
                        {
                            //int? balconistaemp = item.Balconista_Emprestimo_IdLogin;
                            //var usuario = await _contextBS.VW_Usuario_New.FirstOrDefaultAsync(u => u.Id == balconistaemp);
                            //if (usuario != null)
                            //{
                            //    item.Balconista_EmprestimoChapa = usuario.Chapa;
                            //}
                            //else
                            //{
                            //    var usuarioOld = await _contextBS.VW_Usuario.FirstOrDefaultAsync(u => u.Id == balconistaemp);
                            //    item.Balconista_EmprestimoChapa = usuarioOld.Chapa;
                            //}

                            //int? GetIdProdutoAlocado = searches.GetProdutoAlocado(item);
                            int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);
                            item.QuantidadeExtraviada = extraviadoQuantity;

                            //int? balconistadev = item.Balconista_Devolucao_IdLogin;
                            //if (balconistadev != null)
                            //{
                            //    var usuariodevolucao = await _contextBS.VW_Usuario_New.FirstOrDefaultAsync(u => u.Id == balconistadev);
                            //    if (usuariodevolucao != null)
                            //    {
                            //        item.Balconista_DevolucaoChapa = usuariodevolucao.Chapa;
                            //    }
                            //    else
                            //    {
                            //        var usuarioOldDevo = await _contextBS.VW_Usuario.FirstOrDefaultAsync(u => u.Id == balconistadev);

                            //        item.Balconista_DevolucaoChapa = usuarioOldDevo != null ? usuarioOldDevo.Chapa : "";
                            //    }
                            //}

                            int? YearDataDevolucao = item.DataDevolucao.HasValue ? item.DataDevolucao.Value.Year : 0;
                            List<ArquivoViewModel?>? ListArquivo = getArquivo(item.IdHistoricoAlocacao, item.IdProdutoAlocado, YearDataDevolucao);

                            item.UploadedFile = ListArquivo.Count > 0 ? 1 : 0;

                            //var usuariodevolucao = await _contextBS.VW_Usuario_New.FirstOrDefaultAsync(u => u.Id == balconistadev);
                            //if (usuariodevolucao != null)
                            //{
                            //    item.Balconista_DevolucaoChapa = usuariodevolucao.Chapa;
                            //}
                            //else
                            //{
                            //    var usuarioOldDevo = await _contextBS.VW_Usuario.FirstOrDefaultAsync(u => u.Id == balconistadev);

                            //    item.Balconista_DevolucaoChapa = usuarioOldDevo.Chapa;
                            //}
                        }


                        HttpContext.Session.SetObject(SessionKeyHistoricoList, history);

                        //StaticListHistorico = TotalHistorico;
                        StaticCombinedHistorico.HistoricoListModel = history;

                        return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());

                    }
                    else
                    {
                        ViewBag.Error = "Nenhum resultado encontrado";
                        return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                    }

                }
                else
                {
                    ViewBag.Error = error;
                    return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                }
            
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
            }          
        }

        public string ValidateSearch(SearchHistoricoViewModel? filters)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
            }

            if (SolicitanteModel.Chapa == null)
            {
                return "No Employee Selected";
            }

            int? countyear = DateTime.Now.Year - SolicitanteModel?.DataAdmissao?.Year;
            if (countyear > 5)
            {
                if (!filters.TransacoesDe.HasValue && !filters.TransacoesAte.HasValue)
                {
                    return $"Funcionário com {countyear} anos de empresa, entre com Período da Transações DE/ATE para melhor performace do sistema de consulta.";
                }
            }

            if (filters.TransacoesDe.HasValue)
            {
                if (filters.TransacoesDe < SolicitanteModel?.DataAdmissao.Value)
                {
                    return $"Campo Período da Transações DE não pode ser menor que {SolicitanteModel?.DataAdmissao.Value.ToString("dd/MM/yyyy")}.";
                }
            }

            if (filters.TransacoesAte.HasValue)
            {
                if (filters.TransacoesDe.Value > filters.TransacoesAte.Value)
                {
                    return "Campo Período da Transações DE não pode ser maior que Período da Transação ATÉ.";
                }
            }

            if (filters.Ferramenta == null && filters.EPI == null && filters.Consumiveis == null)
            {
                return "Campo Filtro Exibição Ferramenta, EPI ou Consumiveis devem ser selecionados.";
            }

            return null;
        }

        public async Task<ActionResult> SearchHistorico(DateTime? DateAdmission, DateTime? TrasacoesDe, DateTime? TrasacoesAte,string? chapaFuncionario,int? coligadaFuncionario,string? AF,int? PAT, DateTime? DataValidade,string? codigo, string? Catalogo,string? Observacao, DateTime? PrevisaoDe, DateTime? PrevisaoAte)
        {
            if (chapaFuncionario != null && coligadaFuncionario != null)
            {
                DateTime? dateOfAdmission = DateAdmission;
                DateTime? TranDe = TrasacoesDe;
                DateTime? TranAte = TrasacoesAte;
                string? FuncionarioValue = chapaFuncionario;
                Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

                StringBuilder validationErrors = new StringBuilder();

                if (ValidateForm(validationErrors, dateOfAdmission, TranDe, TranAte))
                {
                    if (TrasacoesDe != null && TrasacoesDe.Value != DateTime.MinValue && TrasacoesAte != null && TrasacoesAte.Value != DateTime.MinValue)
                    {
                        DateTime TransactionDe = TrasacoesDe.Value;
                        DateTime TransactionAte = TrasacoesAte.Value;

                        List<HistoricoViewModel> resultList = new List<HistoricoViewModel>();

                        for (int year = TransactionDe.Year; year <= TransactionAte.Year; year++)
                        {
                            // Build the table name dynamically
                            string tableName = $"HistoricoAlocacao_{year}";

                            var dbSetProperties = _context.GetType().GetProperties()
                                              .Where(p => p.PropertyType.IsGenericType &&
                                                          p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                            // Find the DbSet property that matches the expected table name
                            var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                            if (tableProperty != null)
                            {
                                List<HistoricoViewModel> result = new List<HistoricoViewModel>();
                                // Use the DbSet property value in your query
                                var table = tableProperty.GetValue(_context, null);

                                if (table != null)
                                {

                                    var query = from hist in (IQueryable<HistoricoAlocacao>)table
                                                join prod in _context.Produto on hist.IdProduto equals prod.Id
                                                join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                join ferrOrig in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                join ferrDev in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDev.Id into ferrDevGroup
                                                from ferrDev in ferrDevGroup.DefaultIfEmpty()
                                                where hist.Solicitante_Chapa == FuncionarioValue
                                                      && hist.Solicitante_CodColigada == 2
                                                      && hist.DataEmprestimo >= TransactionDe
                                                      && hist.DataDevolucao <= TransactionAte
                                                      && (categoria.Classe == 1 || categoria.Classe == 2)
                                                select new HistoricoViewModel
                                                {
                                                    IdHistoricoAlocacao = hist.Id,
                                                    IdProdutoAlocado = 0,
                                                    IdProduto = hist.IdProduto,
                                                    Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                    Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                    Solicitante_Chapa = hist.Solicitante_Chapa,
                                                    Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                    Liberador_CodColigada = hist.Liberador_CodColigada,
                                                    Liberador_Chapa = hist.Liberador_Chapa,
                                                    Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                    Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                    Observacao = hist.Observacao,
                                                    DataEmprestimo = hist.DataEmprestimo,
                                                    DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                    DataDevolucao = hist.DataDevolucao,
                                                    IdObra = hist.IdObra,
                                                    Quantidade = hist.Quantidade,
                                                    QuantidadeEmprestada = cat.PorAferido == 0 && cat.PorSerial == 0
                                                        ? _context.ProdutoAlocado
                                                                  .Where(ap => ap.IdProduto == hist.IdProduto &&
                                                                               ap.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                               ap.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                               ap.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                  .Select(ap => ap.Quantidade)
                                                                  .FirstOrDefault()
                                                        : 0,
                                                    IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                    IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                    CodigoCatalogo = cat.Codigo,
                                                    NomeCatalogo = cat.Nome,
                                                    FerrOrigem = ferrOrig.Nome,
                                                    FerrDevolucao = ferrDev.Nome ?? "",
                                                    AFProduto = prod.AF,
                                                    Serie = prod.Serie,
                                                    PATProduto = prod.PAT,
                                                    IdControleCA = (int?)null
                                                };

                                    result.AddRange(query);
                                    // Proceed with your query using the specific table for the year
                                }

                                resultList.AddRange(result);
                            }
                        }

                        if (resultList != null)
                        {
                            foreach (var item in resultList)
                            {
                                int? balconistaemp = item.Balconista_Emprestimo_IdLogin;

                                // Use await to asynchronously wait for the result
                                var usuario = await _contextBS.VW_Usuario_New
                                    .FirstOrDefaultAsync(u => u.Id == balconistaemp);

                                // Check if a user was found
                                if (usuario != null)
                                {
                                    // Assign the Chapa value to the item
                                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuario.Chapa);
                                }
                                else
                                {
                                    var usuarioOld = await _contextBS.VW_Usuario
                                    .FirstOrDefaultAsync(u => u.Id == balconistaemp);

                                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuarioOld.Chapa);
                                }

                            }

                            foreach (var item in resultList)
                            {
                                int? balconistadev = item.Balconista_Devolucao_IdLogin;

                                // Use await to asynchronously wait for the result
                                var usuario = await _contextBS.VW_Usuario_New
                                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

                                // Check if a user was found
                                if (usuario != null)
                                {
                                    // Assign the Chapa value to the item
                                    item.Balconista_Devolucao_IdLogin = int.Parse(usuario.Chapa);
                                }
                                else
                                {
                                    var usuarioOld = await _contextBS.VW_Usuario
                                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

                                    item.Balconista_Devolucao_IdLogin = int.Parse(usuarioOld.Chapa);
                                }

                                int? GetIdProdutoAlocado = searches.GetProdutoAlocado(item);

                                int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);
                                item.QuantidadeExtraviada = extraviadoQuantity;
                            }
                        }

                        if (resultList != null && resultList.Count != 0)
                        {
                            //GlobalDataHistorico.ListHistorico = resultList;
                        }

                    }
                    else
                    {
                        DateTime TransactionDe = DateAdmission.Value;
                        DateTime TransactionAte = DateTime.Now;

                        List<HistoricoViewModel> resultList = new List<HistoricoViewModel>();

                        for (int year = TransactionDe.Year; year <= TransactionAte.Year; year++)
                        {
                            // Build the table name dynamically
                            string tableName = $"HistoricoAlocacao_{year}";

                            var dbSetProperties = _context.GetType().GetProperties()
                                              .Where(p => p.PropertyType.IsGenericType &&
                                                          p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                            // Find the DbSet property that matches the expected table name
                            var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                            if (tableProperty != null)
                            {
                                List<HistoricoViewModel> result = new List<HistoricoViewModel>();
                                // Use the DbSet property value in your query
                                var table = tableProperty.GetValue(_context, null);

                                if (table != null)
                                {
                                    var query = from hist in (IQueryable<HistoricoAlocacao>)table
                                                join prod in _context.Produto on hist.IdProduto equals prod.Id
                                                join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
                                                join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
                                                join ferrOrig in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrig.Id
                                                join ferrDev in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDev.Id into ferrDevGroup
                                                from ferrDev in ferrDevGroup.DefaultIfEmpty()
                                                where hist.Solicitante_Chapa == FuncionarioValue
                                                      && hist.Solicitante_CodColigada == coligadaFuncionario
                                                      && (AF == null || prod.AF.Contains(AF))
                                                      && (PAT == null || prod.PAT == PAT)
                                                      && (DataValidade == null || prod.DataVencimento >= DataValidade.Value.Date && prod.DataVencimento <= DataValidade.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (codigo == null || cat.Codigo.Contains(codigo))
                                                      && (Catalogo == null || cat.Nome.Contains(Catalogo))
                                                      && (Observacao == null || hist.Observacao.Contains(Observacao))
                                                      && (PrevisaoDe == null || hist.DataPrevistaDevolucao >= PrevisaoDe.Value.Date)
                                                      && (PrevisaoAte == null || hist.DataPrevistaDevolucao <= PrevisaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                                      && (TransactionDe == null || hist.DataEmprestimo >= TransactionDe.Date)
                                                      && (TransactionAte == null || hist.DataDevolucao <= TransactionAte.Date.AddDays(1).AddTicks(-1))
                                                      && (categoria.Classe == 1 || categoria.Classe == 2)
                                                select new HistoricoViewModel
                                                {
                                                    IdHistoricoAlocacao = hist.Id,
                                                    IdProdutoAlocado = 0,
                                                    IdProduto = hist.IdProduto,
                                                    Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                    Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                    Solicitante_Chapa = hist.Solicitante_Chapa,
                                                    Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                    Liberador_CodColigada = hist.Liberador_CodColigada,
                                                    Liberador_Chapa = hist.Liberador_Chapa,
                                                    Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                    Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                    Observacao = hist.Observacao,
                                                    DataEmprestimo = hist.DataEmprestimo,
                                                    DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                    DataDevolucao = hist.DataDevolucao,
                                                    IdObra = hist.IdObra,
                                                    Quantidade = hist.Quantidade,
                                                    QuantidadeEmprestada = cat.PorAferido == 0 && cat.PorSerial == 0
                                                        ? _context.ProdutoAlocado
                                                                  .Where(ap => ap.IdProduto == hist.IdProduto &&
                                                                               ap.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                               ap.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                               ap.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                  .Select(ap => ap.Quantidade)
                                                                  .FirstOrDefault()
                                                        : 0,
                                                    IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                    IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                    CodigoCatalogo = cat.Codigo,
                                                    NomeCatalogo = cat.Nome,
                                                    FerrOrigem = ferrOrig.Nome,
                                                    FerrDevolucao = ferrDev.Nome ?? "",
                                                    AFProduto = prod.AF,
                                                    Serie = prod.Serie,
                                                    PATProduto = prod.PAT,
                                                    IdControleCA = (int?)null
                                                };

                                    result.AddRange(query);
                                    // Proceed with your query using the specific table for the year
                                }

                                resultList.AddRange(result);
                            }
                        }

                        if (resultList != null)
                        {
                            foreach (var item in resultList)
                            {
                                int? balconistaemp = item.Balconista_Emprestimo_IdLogin;

                                // Use await to asynchronously wait for the result
                                var usuario = await _contextBS.VW_Usuario_New.FirstOrDefaultAsync(u => u.Id == balconistaemp);

                                // Check if a user was found
                                if (usuario != null)
                                {
                                    // Assign the Chapa value to the item
                                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuario.Chapa);
                                }
                                else
                                {
                                    var usuarioOld = await _contextBS.VW_Usuario
                                    .FirstOrDefaultAsync(u => u.Id == balconistaemp);

                                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuarioOld.Chapa);
                                }

                                int? GetIdProdutoAlocado = searches.GetProdutoAlocado(item);

                                int? extraviadoQuantity = searches.SearchProdutoExtraviadoQuantity(GetIdProdutoAlocado);
                                item.QuantidadeExtraviada = extraviadoQuantity;

                            }

                            foreach (var item in resultList)
                            {
                                int? balconistadev = item.Balconista_Devolucao_IdLogin;

                                // Use await to asynchronously wait for the result
                                var usuario = await _contextBS.VW_Usuario_New
                                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

                                // Check if a user was found
                                if (usuario != null)
                                {
                                    // Assign the Chapa value to the item
                                    item.Balconista_Devolucao_IdLogin = int.Parse(usuario.Chapa);
                                }
                                else
                                {
                                    var usuarioOld = await _contextBS.VW_Usuario
                                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

                                    item.Balconista_Devolucao_IdLogin = int.Parse(usuarioOld.Chapa);
                                }

                            }
                        }

                        if (resultList != null && resultList.Count != 0)
                        {
                            //GlobalDataHistorico.ListHistorico = resultList;

                        }
                    }
                }
                else
                {
                    TempData["ShowErrorAlert"] = true;
                    TempData["ErrorMessage"] = validationErrors.ToString();

                    //if (GlobalDataHistorico.ListHistorico != null && GlobalDataHistorico.ListHistorico.Count > 0)
                    //{
                    //    GlobalDataHistorico.ListHistorico.Clear();
                    //}

                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ShowErrorAlert"] = true;
                TempData["ErrorMessage"] = "Please search employee first.";
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult SearchDev(string? IdFuncionario)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>(); //Brasfels Employee Result
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>(); //Third Party Employees Result
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>(); //Brasfels and Third Party Employees Result
            UserViewModel? UsuarioModel = new UserViewModel();

            CombinedHistoricoModel? historicoModel = new CombinedHistoricoModel();

            if (IdFuncionario != null)
            {
                //Searching using the Third Party Employees then adding it to the listTerceiroResult then it on the TotalResult
                listTerceiroResult = searches.SearchTercerio(IdFuncionario);
                TotalResult.AddRange(listTerceiroResult);

                //Searching using the Brasfels Employees then adding it to listEmployeeResult then add to TotalResult
                listEmployeeResult = searches.SearchEmployeeChapa(IdFuncionario);
                TotalResult.AddRange(listEmployeeResult);

                if (TotalResult.Count > 1)
                {
                    ViewBag.ListOfEmployees = TotalResult;
                    return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                }
                else if (TotalResult.Count == 1)
                {

                    //httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);
                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyHistoricoSolicitante);
                    //httpContextAccessor.HttpContext.Session.SetString(Sessao.Solicitante, TotalResult[0].Chapa);
                    httpContextAccessor.HttpContext.Session.SetString(SessionKeyHistoricoSolicitante, TotalResult[0].Chapa);

                    UsuarioModel = searches.SearchSolicitanteLoad(TotalResult[0].Chapa);
                    List<MensagemSolicitanteViewModel>? messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                    if (messages.Count > 0)
                    {
                        ViewBag.Messages = messages;
                    }

                    historicoModel.UserViewModel = UsuarioModel;
                    //StaticEmployee = UsuarioModel;
                    StaticCombinedHistorico = historicoModel;
                    return View(nameof(Index), historicoModel);

                }
                else if (TotalResult.Count == 0)
                {
                    ViewBag.Error = "Nenhum funcionário encontrado.";
                    return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                }
            }
            else
            {
                ViewBag.Error = "Matricula/Nome is Required";
                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
            }

            return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
        }

        public ActionResult DeleteMessage(int? id)
        {
            if (id != null)
            {
                var DeleteMessage = _context.MensagemSolicitante.FirstOrDefault(i => i.Id == id);
                if (DeleteMessage != null)
                {
                    DeleteMessage.Ativo = 0;
                    _context.SaveChanges();
                }

                TempData["ShowSuccessAlert"] = true;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Chapa is Empty";
                return RedirectToAction(nameof(Index));
            }

        }

        public ActionResult SelectedUser(string? chapa)
        {
            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();
            CombinedHistoricoModel? historicoViewModel = new CombinedHistoricoModel();

            //VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();

            if (chapa != null)
            {
                //httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);
                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyHistoricoSolicitante);
                //httpContextAccessor.HttpContext.Session.SetString(Sessao.Solicitante, chapa);
                httpContextAccessor.HttpContext.Session.SetString(SessionKeyHistoricoSolicitante, chapa);

                UsuarioModel = searches.SearchSolicitanteLoad(chapa);
                List<MensagemSolicitanteViewModel>? messages = searches.SearchMensagem(UsuarioModel.Chapa, loggedUser.Id);
                ViewBag.Messages = messages.Count > 0 ? messages : null;

                historicoViewModel.UserViewModel = UsuarioModel;
                //StaticEmployee = UsuarioModel;
                StaticCombinedHistorico = historicoViewModel;
                return View(nameof(Index), historicoViewModel);
            }
            else
            {
                ViewBag.Error = "chapa is empty";
                return View(nameof(Index), historicoViewModel);
            }

        }

        public ActionResult PrintTicket(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
            }

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;

       


            List<HistoricoAlocacaoViewModel> historicoAlocacaoList = new List<HistoricoAlocacaoViewModel>();
            historicoAlocacaoList = (from hist in _context.HistoricoAlocacao_2025
                                     join produto in _context.Produto on hist.IdProduto equals produto.Id
                                     join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                     join ferrOrigem in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrigem.Id
                                     join ferrDevolucao in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDevolucao.Id into devolucaoGroup
                                     from devolucao in devolucaoGroup.DefaultIfEmpty()
                                     where hist.Id == id
                                     select new HistoricoAlocacaoViewModel
                                     {
                                         IdHistoricoAlocacao = hist.Id,
                                         IdProdutoAlocado = 0,
                                         IdProduto = hist.IdProduto,
                                         Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                         Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                         Solicitante_Chapa = hist.Solicitante_Chapa,
                                         Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                         Liberador_CodColigada = hist.Liberador_CodColigada,
                                         Liberador_Chapa = hist.Liberador_Chapa,
                                         Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                         Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                         Observacao = hist.Observacao,
                                         DataEmprestimo = hist.DataEmprestimo,
                                         DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                         DataDevolucao = hist.DataDevolucao,
                                         IdObra = hist.IdObra,
                                         Quantidade = hist.Quantidade,
                                         QuantidadeEmprestada = catalogo.PorAferido == 0 && catalogo.PorSerial == 0
                                             ? (_context.ProdutoAlocado
                                                    .Where(pa =>
                                                        pa.IdProduto == hist.IdProduto &&
                                                        pa.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                        pa.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                        pa.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                    .OrderBy(pa => pa.Id)
                                                    .Select(pa => pa.Quantidade)
                                                    .FirstOrDefault() ?? 0)
                                             : 0,
                                         IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                         IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                         CodigoCatalogo = catalogo.Codigo,
                                         NomeCatalogo = catalogo.Nome,
                                         FerrOrigem = ferrOrigem.Nome,
                                         FerrDevolucao = devolucao.Nome,
                                         AFProduto = produto.AF,
                                         Serie = produto.Serie,
                                         PATProduto = produto.PAT,
                                         IdControleCA = hist.IdControleCA
                                     }).ToList();

            var sb = new StringBuilder();

            if (historicoAlocacaoList.Count > 0)
            {
                sb.AppendLine("<html><head><title>Receipt</title>");
                sb.AppendLine("<style type='text/css'>");
                sb.AppendLine("@media print {");
                sb.AppendLine("  body, html {");
                sb.AppendLine("    margin: 0;");
                sb.AppendLine("    padding: 0;");
                sb.AppendLine("    width: 7.9cm;");
                sb.AppendLine("  }");
                sb.AppendLine("  .receipt {");
                sb.AppendLine("    width: 7.9cm;");
                sb.AppendLine("    font-family: 'Courier New', monospace;");
                sb.AppendLine("    font-size: 12px;");
                sb.AppendLine("  }");
                sb.AppendLine("  @page {");
                sb.AppendLine("    margin: 0;");
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine("body {");
                sb.AppendLine("  font-family: Arial, sans-serif;");
                sb.AppendLine("}");
                sb.AppendLine(".receipt {");
                sb.AppendLine("  width: 100%;");
                sb.AppendLine("  font-family: 'Courier New', monospace;");
                sb.AppendLine("  font-size: 12px;");
                sb.AppendLine("}");
                sb.AppendLine("</style>");
                sb.AppendLine("</head><body>");
                sb.AppendLine("<div class='receipt'>");

                sb.AppendLine(" _");
                sb.AppendFormat("<br>");
                sb.AppendLine(" **************************************");
                sb.AppendLine(" COMPROVANTE DE DEVOLUCAO");
                sb.AppendLine("@FERRAMENTARIA");
                sb.AppendLine(" **************************************");
                sb.AppendFormat("<br>");
                sb.AppendLine(QuebraTexto(String.Format(" OPER. REALIZADA: {0}", historicoAlocacaoList[0]?.DataDevolucao.Value.ToString("dd/MM/yyyy HH:mm:ss"))));
                //sb.AppendLine(QuebraTexto($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}"));
                //sb.AppendLine(QuebraTexto($"FUNCIONARIO:{usuario.Chapa.ToString().Trim()}/{usuario.Nome.ToString().Trim()}"));
                sb.AppendFormat("<br>");

                VW_Usuario_New? balconista = _contextBS.VW_Usuario_New.FirstOrDefault(i => i.Id == historicoAlocacaoList[0].Balconista_Devolucao_IdLogin);

                sb.AppendLine(QuebraTexto(String.Format(" POR BALCONISTA : {0}", balconista?.Nome).TrimEnd()));
                sb.AppendFormat("<br>");
                sb.AppendLine(QuebraTexto(String.Format(" P/ FUNCIONARIO : {0} / {1}", SolicitanteModel.Chapa, SolicitanteModel.Nome.TrimEnd())));
                sb.AppendFormat("<br>");
                foreach (var viewModel in historicoAlocacaoList)
                {
                    sb.AppendLine(QuebraTexto(String.Format(" ID TRANSACAO   : {0}", viewModel.IdHistoricoAlocacao).TrimEnd()));
                    sb.AppendFormat("<br>");
                    sb.AppendLine(QuebraTexto(String.Format(" CODIGO         : {0}", viewModel.CodigoCatalogo).TrimEnd()));
                    sb.AppendFormat("<br>");
                    sb.AppendLine(QuebraTexto(String.Format(" QTD. DEVOLVIDA : {0}", viewModel.Quantidade).TrimEnd()));
                    sb.AppendFormat("<br>");
                    sb.AppendLine(QuebraTexto(String.Format(" SALDO RESTANTE : {0}", viewModel.QuantidadeEmprestada).TrimEnd()));
                    sb.AppendFormat("<br>");
                    sb.AppendLine(QuebraTexto(String.Format(" DAT. EMPRESTIMO: {0}", viewModel.DataEmprestimo).TrimEnd()));
                    sb.AppendFormat("<br>");
                    sb.AppendLine(QuebraTexto(String.Format(" DESCRICAO      : {0}", viewModel.NomeCatalogo).TrimEnd()));
                    sb.AppendFormat("<br>");

                    if (!string.IsNullOrEmpty(viewModel.AFProduto))
                    {
                        //sb.AppendLine($"AF: {viewModel.AFProduto?.ToString().Trim()}");
                        sb.AppendLine(QuebraTexto(String.Format(" AF             : {0}", viewModel.AFProduto).TrimEnd()));
                        sb.AppendFormat("<br>");
                    }
                    if (viewModel.PATProduto != 0)
                    {
                        sb.AppendLine(QuebraTexto(String.Format(" PAT            : {0}", viewModel.PATProduto).TrimEnd()));
                        sb.AppendFormat("<br>");
                    }
                    if (!string.IsNullOrEmpty(viewModel.Observacao))
                    {
                        //sb.AppendLine($"Obs: {viewModel.Observacao?.ToString().Trim()}");
                        sb.AppendLine(QuebraTexto(String.Format(" NOTA BALCONISTA: {0}", viewModel.Observacao).TrimEnd()));
                        sb.AppendFormat("<br>");
                    }

                    sb.Replace("@FERRAMENTARIA", QuebraTexto(String.Format(" {0}", viewModel?.FerrOrigem.ToUpper()).TrimEnd()));
                }

                sb.AppendFormat("<br>");
                sb.AppendLine(" **************************************");
                sb.AppendFormat("<br>");
                sb.AppendLine(QuebraTexto((String.Format(" {0}", "O empregado esta ciente de que, na eventualidade de perda, extravio ou dano do Radio, Equipamento / EPI / Kit / Ferramenta, assim como a nao devolucao do mesmo quando requisitada, o valor correspondente a perda sera descontado dos proximos vencimentos do empregado, nos termos do Artigo 462-1/CLT."))));
                sb.AppendLine(" **************************************");
                sb.AppendFormat("<br>");
                sb.AppendFormat("<br>");
                sb.AppendFormat("<br>");
                sb.AppendFormat("<br>");
                sb.AppendLine(" _");

                sb.AppendLine("</div>");
                sb.AppendLine("</body></html>");

                ViewBag.ReceiptHtml = sb.ToString();
            }


                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
        }

        private string QuebraTexto(string t)
        {
            string texto = t.TrimEnd();
            StringBuilder sb = new StringBuilder();

            double stepFor = texto.Length / 40.0;
            int startIndex = 0;

            if (stepFor > 1)
            {
                for (int index = 0; index <= stepFor; index++)
                {
                    int resto = texto.Length - (startIndex + 40);

                    if (resto >= 40)
                    {
                        if (index != 0)
                        {
                            startIndex += 40;
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, 40).TrimStart(), Environment.NewLine);
                        }
                        else
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
                        }
                    }
                    else if (resto >= 0)
                    {
                        if (index != 0)
                        {
                            startIndex += 40;
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
                        }
                        else
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
                        }
                    }
                    else if (resto < 0)
                    {
                        resto = (texto.Length - 1) - startIndex;
                        if (sb.ToString().IndexOf(texto.Substring(startIndex, resto), StringComparison.Ordinal) == -1)
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
                        }
                    }
                }

                return sb.ToString();
            }
            else
            {
                return string.Format(" {0}{1}", texto.TrimStart(), Environment.NewLine);
            }
        }


        #region ModalFunctions
        public ActionResult GetLiberador(string? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
            }

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;

            var queryLiberador = _contextBS.Funcionario.Where(e => e.Chapa == id).OrderByDescending(e => e.DataMudanca).FirstOrDefault();

            int? CodPessoa = TempData["LiberadorCodPessoa"] as int?;

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA 
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == queryLiberador.CodPessoa
                                     select gImagem.IMAGEM).FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);

            ViewBag.LiberadorChapa = queryLiberador.Chapa;
            ViewBag.LiberadorNome = queryLiberador.Nome;
            ViewBag.OpenLiberadorModal = true;

            return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
        }

        //public ActionResult GetObservacao(int? id)
        //{

        //    var observacao = GlobalDataHistorico.ListHistorico
        //                     .Where(devolucao => devolucao.IdHistoricoAlocacao == id)
        //                     .FirstOrDefault(); // You might want to use FirstOrDefault to get the first matching item

        //    //.Select(devolucao => devolucao.Observacao)

        //    if (observacao != null)
        //    {
        //        //ViewBag.Observacao = observacao;

        //        //ViewBag.OpenObsModal = true;

        //        TempData["GetCodigo"] = observacao.CodigoCatalogo;
        //        TempData["GetObs"] = observacao.Observacao;

        //        TempData["OpenModal"] = true;
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        public ActionResult GetBalconistaEmp(string? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
            }

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;


            //var queryliberador = _contextBS.Funcionario.Where(e => EF.Functions.Like(e.Chapa, $"%{id}%")).OrderByDescending(e => e.DataMudanca).FirstOrDefault();
            var queryliberador = _contextBS.Funcionario.Where(e => e.Chapa == id).OrderByDescending(e => e.DataMudanca).FirstOrDefault();

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == queryliberador.CodPessoa
                                     select gImagem.IMAGEM)
                                    .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);

            ViewBag.BalconistaEmpChapa = queryliberador.Chapa;
            ViewBag.BalconistaEmpNome = queryliberador.Nome;
            ViewBag.OpenBalconistaEmpModal = true;


            return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
        }

        public ActionResult GetBalconistaDev(string? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
            }

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;

            //var queryliberador = _contextBS.Funcionario.Where(e => EF.Functions.Like(e.Chapa, $"%{id}%")).OrderByDescending(e => e.DataMudanca).FirstOrDefault();
            var queryliberador = _contextBS.Funcionario.Where(e => e.Chapa == id).OrderByDescending(e => e.DataMudanca).FirstOrDefault();

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == queryliberador.CodPessoa
                                     select gImagem.IMAGEM)
                                    .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);

            ViewBag.BalconistaDevChapa = queryliberador.Chapa;
            ViewBag.BalconistaDevNome = queryliberador.Nome;
            ViewBag.OpenBalconistaDevModal = true;



            return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
        }

        public ActionResult OpenSingleUpload(int? idHistorico, int? IdProdutoAlocado,int? YearDevolucao)
        {
            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            UserViewModel? SolicitanteModel = new UserViewModel();
            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);

                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
            }

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;

            if (idHistorico != null && IdProdutoAlocado != null)
            {
                ViewBag.IdHolder = idHistorico;
                ViewBag.IdProdutoAlocado = IdProdutoAlocado;
                ViewBag.YearDevolucao = YearDevolucao != null ? YearDevolucao : 0;

                List<ArquivoViewModel?>? GetListImage = getArquivo(idHistorico, IdProdutoAlocado, YearDevolucao);

                //List<ArquivoViewModel?>? GetListImage = (from arquivo in _context.Arquivo
                //                                           where arquivo.Ativo == 1
                //                                                            && ((idHistorico == 0 && IdProdutoAlocado == 0) || (idHistorico != 0 && _context.ArquivoVsHistorico.Any(avh => avh.IdHistoricoAlocacao == idHistorico && avh.Ano == YearDevolucao && avh.IdArquivo == arquivo.Id))
                //                                                            || (IdProdutoAlocado != 0 && _context.ArquivoVsProdutoAlocado.Any(avh => avh.IdProdutoAlocado == IdProdutoAlocado && avh.IdArquivo == arquivo.Id)))
                //                                         orderby arquivo.DataRegistro ascending
                //                                           select new ArquivoViewModel
                //                                           {
                //                                               Id = arquivo.Id,
                //                                               Ano = arquivo.Ano,
                //                                               Tipo = arquivo.Tipo,
                //                                               ArquivoNome = arquivo.ArquivoNome,
                //                                               DataRegistro = arquivo.DataRegistro,
                //                                               Ativo = arquivo.Ativo,
                //                                               Solicitante_IdTerceiro = arquivo.Solicitante_IdTerceiro,
                //                                               Solicitante_CodColigada = arquivo.Solicitante_CodColigada,
                //                                               Solicitante_Chapa = arquivo.Solicitante_Chapa,
                //                                               ImageData = arquivo.ImageData != null ? arquivo.ImageData.ToArray() : null
                //                                           }).ToList();

                ViewBag.AlocadoImages = GetListImage.Count > 0 ? GetListImage : new List<ArquivoViewModel?>();
                ViewBag.OpenSingle = true;

                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());

            }
            else
            {
                ViewBag.Error = "Id Not Found";
                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
            }
        }

        #endregion

        #region Action

        [HttpPost]
        public IActionResult UploadAction(IFormFile file, int? HistoricoAlocacaoId, int? IdProdutoAlocado, int? YearDevolucao)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

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
                //        usuario.Retorno = "Usuário sem permissão de Inserir a página de devolucao!";
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


                            UserViewModel? SolicitanteModel = new UserViewModel();
                            //string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(SessionKeyHistoricoSolicitante);
                            if (!string.IsNullOrEmpty(SolicitanteChapa))
                            {
                                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                                StaticCombinedHistorico.UserViewModel = SolicitanteModel;
                            }


                            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
                            StaticCombinedHistorico.HistoricoListModel = historicolist;

                            string? error = ValidateFile(file, HistoricoAlocacaoId, IdProdutoAlocado);
                            if (string.IsNullOrEmpty(error))
                            {
                                ArquivoViewModel? ArquivoDataToInsert = new ArquivoViewModel();

                                ArquivoDataToInsert.ArquivoNome = file.FileName;
                                ArquivoDataToInsert.Tipo = IdProdutoAlocado != 0 ? 4 : 5;
                                ArquivoDataToInsert.Solicitante_Chapa = SolicitanteModel.Chapa;
                                ArquivoDataToInsert.Solicitante_CodColigada = SolicitanteModel.CodColigada;
                                ArquivoDataToInsert.Solicitante_IdTerceiro = 0;

                                byte[] imageData;

                                using (var stream = new MemoryStream())
                                {
                                    file.CopyTo(stream);
                                    imageData = stream.ToArray();
                                }

                                var InsertToArquivo = new Arquivo
                                {
                                    IdHistoricoAlocacao = HistoricoAlocacaoId,
                                    IdProdutoAlocado = IdProdutoAlocado,
                                    Ano = YearDevolucao,
                                    Solicitante_IdTerceiro = ArquivoDataToInsert.Solicitante_IdTerceiro,
                                    Solicitante_CodColigada = ArquivoDataToInsert.Solicitante_CodColigada,
                                    Solicitante_Chapa = ArquivoDataToInsert.Solicitante_Chapa,
                                    IdUsuario = loggedUser.Id,
                                    Tipo = ArquivoDataToInsert.Tipo,
                                    ArquivoNome = file.FileName,
                                    DataRegistro = DateTime.Now,
                                    Ativo = 1,
                                    ImageData = imageData,
                                    Responsavel = loggedUser.Nome
                                };

                                _context.Add(InsertToArquivo);
                                _context.SaveChanges();

                                if (IdProdutoAlocado != 0)
                                {
                                    var InsertToArquivoVsProdutoAlocado = new ArquivoVsProdutoAlocado
                                    {
                                        IdArquivo = InsertToArquivo.Id,
                                        IdProdutoAlocado = InsertToArquivo.IdHistoricoAlocacao,
                                        DataRegistro = DateTime.Now
                                    };

                                    _context.Add(InsertToArquivoVsProdutoAlocado);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    var InsertToArquivoVsHistorico = new ArquivoVsHistorico
                                    {
                                        IdArquivo = InsertToArquivo.Id,
                                        IdHistoricoAlocacao = InsertToArquivo.IdHistoricoAlocacao,
                                        Ano = YearDevolucao,
                                        DataRegistro = DateTime.Now
                                    };

                                    _context.Add(InsertToArquivoVsHistorico);
                                    _context.SaveChanges();
                                }

                                if (InsertToArquivo.Id != null)
                                {
                                    ViewBag.ShowSuccessAlert = true;
                                    return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                                }
                                else
                                {
                                    ViewBag.Error = "Error in inserting the data to the database.";
                                    return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
                                }

                            }
                            else
                            {
                                ViewBag.ErrorUpload = error;
                                ViewBag.IdHolder = HistoricoAlocacaoId;
                                ViewBag.IdProdutoAlocado = IdProdutoAlocado;
                                ViewBag.YearDevolucao = YearDevolucao != null ? YearDevolucao : 0;
                                List<ArquivoViewModel?>? GetListImage = getArquivo(HistoricoAlocacaoId, IdProdutoAlocado, YearDevolucao);
                                ViewBag.AlocadoImages = GetListImage.Count > 0 ? GetListImage : new List<ArquivoViewModel?>();
                                ViewBag.OpenSingle = true;
                                return View(nameof(Index), StaticCombinedHistorico ?? new CombinedHistoricoModel());
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

        public List<ArquivoViewModel?>? getArquivo(int? idHistorico, int? IdProdutoAlocado, int? YearDevolucao)
        {
            List<ArquivoViewModel?>? GetListImage = (from arquivo in _context.Arquivo
                                                     where arquivo.Ativo == 1
                                                                      && ((idHistorico == 0 && IdProdutoAlocado == 0) || (idHistorico != 0 && _context.ArquivoVsHistorico.Any(avh => avh.IdHistoricoAlocacao == idHistorico && avh.Ano == YearDevolucao && avh.IdArquivo == arquivo.Id))
                                                                      || (IdProdutoAlocado != 0 && _context.ArquivoVsProdutoAlocado.Any(avh => avh.IdProdutoAlocado == IdProdutoAlocado && avh.IdArquivo == arquivo.Id)))
                                                     orderby arquivo.DataRegistro ascending
                                                     select new ArquivoViewModel
                                                     {
                                                         Id = arquivo.Id,
                                                         Ano = arquivo.Ano,
                                                         Tipo = arquivo.Tipo,
                                                         ArquivoNome = arquivo.ArquivoNome,
                                                         DataRegistro = arquivo.DataRegistro,
                                                         Ativo = arquivo.Ativo,
                                                         Solicitante_IdTerceiro = arquivo.Solicitante_IdTerceiro,
                                                         Solicitante_CodColigada = arquivo.Solicitante_CodColigada,
                                                         Solicitante_Chapa = arquivo.Solicitante_Chapa,
                                                         ImageData = arquivo.ImageData != null ? arquivo.ImageData.ToArray() : null
                                                     }).ToList();

            return GetListImage;
        }

        public string ValidateFile(IFormFile file, int? IdHistoricoAlocacao, int? IdProdutoAlocado)
        {
            if (file == null)
            {
                return "Nenhum arquivo foi selecionado.";
            }


            if (file.Length > 1048576) // 1MB (1024 bytes * 1024)
            {
                return "O tamanho do arquivo não deve exceder 1 MB";
            }

            if (!file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) && !file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return "Formato de arquivo inválido. Somente imagens JPG são permitidas.";
            }

            if (file.FileName.Length > 250)
            {
                return "O nome do arquivo não deve exceder 250 caracteres.";
            }

            if (IdHistoricoAlocacao == null && IdProdutoAlocado == null)
            {
                return "Id Values are null";
            }

            return null;
        }

        #endregion

        private bool ValidateForm(StringBuilder validationErrors,DateTime? dateOfAdmission, DateTime? TranDe, DateTime? TranAte)
        {
           
            DateTime? dateAdmission = dateOfAdmission;
            int? countyear = DateTime.Now.Year - dateOfAdmission?.Year;
            
            if (countyear > 5)
            {
                if (TranDe == null && TranAte == null)
                {
                    validationErrors.AppendLine(String.Format("Funcionário com {0} anos de empresa, entre com Período da Transações DE/ATE para melhor performace do sistema de consulta.", countyear));
                }                   
            }

            if (TranDe != null)
            {
                if (TranDe < dateAdmission)
                {
                    validationErrors.AppendLine(String.Format("Campo Período da Transações DE não pode ser menor que {0}.", dateAdmission?.ToString("dd/MM/yyyy")));
                }
            }

            if (TranAte != null)
            {
                if (TranDe > TranAte)
                {
                    validationErrors.AppendLine("Campo Período da Transações DE não pode ser maior que Período da Transação ATÉ.");
                }
            }

            return validationErrors.Length == 0;
        }

        [HttpPost]
        public IActionResult ExportToExcel()
        {
            CombinedHistoricoModel? StaticCombinedHistorico = new CombinedHistoricoModel();

            var historicolist = HttpContext.Session.GetObject<List<HistoricoViewModel>>(SessionKeyHistoricoList) ?? new List<HistoricoViewModel>();
            StaticCombinedHistorico.HistoricoListModel = historicolist;

            //var listHistorico = StaticCombinedHistorico.HistoricoListModel;

            DataTable dataTable = new DataTable();
            // Add columns to the dataTable
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Catalogo");
            dataTable.Columns.Add("C.A");
            dataTable.Columns.Add("AF/Serial");
            dataTable.Columns.Add("Pat");
            dataTable.Columns.Add("FerrOrigem");
            dataTable.Columns.Add("FerrDevolucao");
            dataTable.Columns.Add("Quantidade");
            dataTable.Columns.Add("DataEmprestimo");
            dataTable.Columns.Add("DataPrevistaDevolução");
            dataTable.Columns.Add("DataDevolucao");
            dataTable.Columns.Add("Observação");
            dataTable.Columns.Add("Liberador");
            dataTable.Columns.Add("BalcEmprestimo");
            dataTable.Columns.Add("BalcDevolucao");
            dataTable.Columns.Add("DataValidade");
            dataTable.Columns.Add("SolicitanteChapa");
            dataTable.Columns.Add("SolicitanteNome");
            dataTable.Columns.Add("Extraviado");

            // Add data rows from listDevolucao
            foreach (var item in historicolist)
            {
                //var balconistaInfo = GetBalconistaInfo(item.Balconista_IdLogin);

                var solicitacaoinfo = GetSolicitacaoInfo(item.Solicitante_Chapa);

                var row = dataTable.NewRow();
                row["Codigo"] = item.CodigoCatalogo; 
                row["Catalogo"] = item.NomeCatalogo;
                row["C.A"] = "";
                    //"Numero CA: " + item.IdControleCA + " Validade: " + item.ValidadeControlCA?.ToString("dd/MM/yyyy") + "";
                row["AF/Serial"] = item.AFProduto;
                row["Pat"] = item.PATProduto;
                row["FerrOrigem"] = item.FerrOrigem;
                row["FerrDevolucao"] = item.FerrDevolucao;
                row["Quantidade"] = item.Quantidade;
                row["DataEmprestimo"] = item.DataEmprestimo?.ToString("dd/MM/yyyy");
                row["DataPrevistaDevolução"] = item.DataPrevistaDevolucao?.ToString("dd/MM/yyyy");
                row["DataDevolucao"] = item.DataDevolucao?.ToString("dd/MM/yyyy");
                row["Observação"] = item.Observacao;
                row["Liberador"] = item.Liberador_Chapa;
                row["BalcEmprestimo"] = item.Balconista_Emprestimo_IdLogin;
                row["BalcDevolucao"] = item.Balconista_Devolucao_IdLogin;
                //balconistaInfo.Chapa;
                row["DataValidade"] = "";
                row["SolicitanteChapa"] = item.Solicitante_Chapa;
                row["SolicitanteNome"] = solicitacaoinfo.Nome; //nome
                row["Extraviado"] = "";

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {

                    var worksheet = package.Workbook.Worksheets.Add("Historico");

                    // Add data from dataTable to the worksheet
                    worksheet.Cells.LoadFromDataTable(dataTable, true);

                    using (var cells = worksheet.Cells[worksheet.Dimension.Address])
                    {
                        cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    package.Save();
                }

                memoryStream.Position = 0;
                byte[] content = memoryStream.ToArray();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "HISTORICO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                return File(content, contentType, fileName);



                //// Save the Excel file to the physical path
                //string fileName = "DEVOLUCAO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                //string caminhoFisico = "D:\\Ferramentaria\\Relatorio\\" + fileName;
                //System.IO.File.WriteAllBytes(caminhoFisico, content);

                //// Define the content type
                //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //// Trigger the download
                //return File(System.IO.File.ReadAllBytes(caminhoFisico), contentType, fileName);

            }


        }

        public UsuarioBS GetBalconistaInfo(int? balconistaId)
        {
            return _contextBS.Usuario.FirstOrDefault(u => u.Id == balconistaId);
        }

        public Funcionario GetSolicitacaoInfo(string? solicitanteChapa)
        {
            return _contextBS.Funcionario.FirstOrDefault(u => u.Chapa == solicitanteChapa);
        }





        //if (DateAdmission != null)
        //{

        //    if (TrasacoesDe != null && TrasacoesDe.Value != DateTime.MinValue && TrasacoesAte != null && TrasacoesAte.Value != DateTime.MinValue)
        //    {
        //        DateTime TransactionDe = TrasacoesDe.Value;
        //        DateTime TransactionAte = TrasacoesAte.Value;

        //        List<HistoricoViewModel> resultList = new List<HistoricoViewModel>();

        //        for (int year = TransactionDe.Year; year <= TransactionAte.Year; year++)
        //        {                      
        //            // Build the table name dynamically
        //            string tableName = $"HistoricoAlocacao_{year}";

        //            var dbSetProperties = _context.GetType().GetProperties()
        //                              .Where(p => p.PropertyType.IsGenericType &&
        //                                          p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        //            // Find the DbSet property that matches the expected table name
        //            var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

        //            if (tableProperty != null)
        //            {
        //                List<HistoricoViewModel> result = new List<HistoricoViewModel>();
        //                // Use the DbSet property value in your query
        //                var table = tableProperty.GetValue(_context, null);

        //                if (table != null)
        //                {


        //                    var query = from hist in (IQueryable<HistoricoAlocacao>)table
        //                                join prod in _context.Produto on hist.IdProduto equals prod.Id
        //                                join cat in _context.Catalogo on prod.IdCatalogo equals cat.Id
        //                                join categoria in _context.Categoria on cat.IdCategoria equals categoria.Id
        //                                join ferrOrig in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrig.Id
        //                                join ferrDev in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDev.Id into ferrDevGroup
        //                                from ferrDev in ferrDevGroup.DefaultIfEmpty()
        //                                where hist.Solicitante_Chapa == FuncionarioValue &&
        //                                      hist.Solicitante_CodColigada == 2 &&
        //                                      hist.DataEmprestimo >= TransactionDe &&
        //                                      hist.DataDevolucao <= TransactionAte &&
        //                                        (categoria.Classe == 1 || categoria.Classe == 2)
        //                                select new HistoricoViewModel
        //                                {
        //                                    IdHistoricoAlocacao = hist.Id,
        //                                    IdProdutoAlocado = 0,
        //                                    IdProduto = hist.IdProduto,
        //                                    Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
        //                                    Solicitante_CodColigada = hist.Solicitante_CodColigada,
        //                                    Solicitante_Chapa = hist.Solicitante_Chapa,
        //                                    Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
        //                                    Liberador_CodColigada = hist.Liberador_CodColigada,
        //                                    Liberador_Chapa = hist.Liberador_Chapa,
        //                                    Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
        //                                    Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
        //                                    Observacao = hist.Observacao,
        //                                    DataEmprestimo = hist.DataEmprestimo,
        //                                    DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
        //                                    DataDevolucao = hist.DataDevolucao,
        //                                    IdObra = hist.IdObra,
        //                                    Quantidade = hist.Quantidade,
        //                                    QuantidadeEmprestada = cat.PorAferido == 0 && cat.PorSerial == 0
        //                                        ? _context.ProdutoAlocado
        //                                                  .Where(ap => ap.IdProduto == hist.IdProduto &&
        //                                                               ap.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
        //                                                               ap.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
        //                                                               ap.Solicitante_Chapa == hist.Solicitante_Chapa)
        //                                                  .Select(ap => ap.Quantidade)
        //                                                  .FirstOrDefault()
        //                                        : 0,
        //                                    IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
        //                                    IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
        //                                    CodigoCatalogo = cat.Codigo,
        //                                    NomeCatalogo = cat.Nome,
        //                                    FerrOrigem = ferrOrig.Nome,
        //                                    FerrDevolucao = ferrDev.Nome ?? "",
        //                                    AFProduto = prod.AF,
        //                                    Serie = prod.Serie,
        //                                    PATProduto = prod.PAT,
        //                                    IdControleCA = (int?)null
        //                                };

        //                    result.AddRange(query);
        //                    // Proceed with your query using the specific table for the year
        //                }

        //                resultList.AddRange(result);
        //            } 
        //        }

        //        if (resultList != null)
        //        {
        //            foreach (var item in resultList)
        //            {
        //                int? balconistaemp = item.Balconista_Emprestimo_IdLogin;

        //                // Use await to asynchronously wait for the result
        //                var usuario = await _contextBS.VW_Usuario_New
        //                    .FirstOrDefaultAsync(u => u.Id == balconistaemp);

        //                // Check if a user was found
        //                if (usuario != null)
        //                {
        //                    // Assign the Chapa value to the item
        //                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuario.Chapa);
        //                }
        //                else
        //                {
        //                    var usuarioOld = await _contextBS.VW_Usuario
        //                    .FirstOrDefaultAsync(u => u.Id == balconistaemp);

        //                    item.Balconista_Emprestimo_IdLogin = int.Parse(usuarioOld.Chapa);
        //                }

        //            }

        //            foreach (var item in resultList)
        //            {
        //                int? balconistadev = item.Balconista_Devolucao_IdLogin;

        //                // Use await to asynchronously wait for the result
        //                var usuario = await _contextBS.VW_Usuario_New
        //                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

        //                // Check if a user was found
        //                if (usuario != null)
        //                {
        //                    // Assign the Chapa value to the item
        //                    item.Balconista_Devolucao_IdLogin = int.Parse(usuario.Chapa);
        //                }
        //                else
        //                {
        //                    var usuarioOld = await _contextBS.VW_Usuario
        //                    .FirstOrDefaultAsync(u => u.Id == balconistadev);

        //                    item.Balconista_Devolucao_IdLogin = int.Parse(usuarioOld.Chapa);
        //                }

        //            }
        //        }

        //        if (resultList != null && resultList.Count != 0)
        //        {
        //            GlobalDataHistorico.ListHistorico = resultList;

        //        }




        //    }
        //    else
        //    {


        //    }


        //}









    }
}
