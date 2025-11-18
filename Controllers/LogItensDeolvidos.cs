using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using OfficeOpenXml;
using System.Text;
using System.Linq;
using System.Data;

namespace FerramentariaTest.Controllers
{
    public class LogItensDeolvidos : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thItensDevolvidos.aspx";
        private MapperConfiguration mapeamentoClasses;
        private static int? GlobalPagination;

        private const string SessionKeyLogItensDevolvidos = "LogItensDevolvidos";
        //private static List<RelatorioViewModel> _ListRelatorio = new List<RelatorioViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public LogItensDeolvidos(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_ItensDevolvidosWithoutFuncionarioViewModel, VW_ItensDevolvidosWithoutFuncionario>();
                cfg.CreateMap<VW_ItensDevolvidosWithoutFuncionario, VW_ItensDevolvidosWithoutFuncionarioViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: LogItensDeolvidos
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

        public IActionResult GetLog(int? page, int? Processo, int? Registro, int? Pagination)
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


                            List<RelatorioViewModel>? RelatorioResult = null;

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio
                                    .Where(r => r.RelatorioData == "ITENS DEVOLVIDOS" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
                                    .OrderByDescending(r => r.DataRegistro)
                                    .Select(r => new RelatorioViewModel
                                    {
                                        Id = r.Id,
                                        IdUsuario = r.IdUsuario,
                                        RelatorioData = r.RelatorioData,
                                        Arquivo = r.Arquivo,
                                        Processar = r.Processar,
                                        Query = r.Query,
                                        ProcessoDataInicio = r.ProcessoDataInicio,
                                        ProcessoDataConclusao = r.ProcessoDataConclusao,
                                        SAMAccountName = r.SAMAccountName,
                                        Ativo = r.Ativo,
                                        DataRegistro = r.DataRegistro
                                    })
                                    .ToList();

                                if (RelatorioResult != null)
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

                                    List<RelatorioViewModel>? entrada = (from relatorio in RelatorioResult
                                                                         join user in result on relatorio.IdUsuario equals user.Id
                                                                         select new RelatorioViewModel
                                                                         {
                                                                             Id = relatorio.Id,
                                                                             IdUsuario = relatorio.IdUsuario,
                                                                             RelatorioData = relatorio.RelatorioData,
                                                                             Arquivo = relatorio.Arquivo,
                                                                             Processar = relatorio.Processar,
                                                                             Query = relatorio.Query,
                                                                             ProcessoDataInicio = relatorio.ProcessoDataInicio,
                                                                             ProcessoDataConclusao = relatorio.ProcessoDataConclusao,
                                                                             SAMAccountName = relatorio.SAMAccountName,
                                                                             Ativo = relatorio.Ativo,
                                                                             DataRegistro = relatorio.DataRegistro,
                                                                             Nome = user.Nome
                                                                         }).ToList();

                                    //List<int?> distinctUsuario = new List<int?>();
                                    //List<VW_Usuario_New> ListUsuario = new List<VW_Usuario_New>();
                                    //List<VW_Usuario> ListUsuarioOld = new List<VW_Usuario>();

                                    //distinctUsuario = RelatorioResult.Select(x => x.IdUsuario).Distinct().ToList();
                                    //ListUsuario = searches.GetDistinctUser(distinctUsuario);
                                    //ListUsuarioOld = searches.GetOldDistinctUser(distinctUsuario);

                                    foreach (var item in entrada)
                                    {
                                        //var UserDetails = ListUsuario.FirstOrDefault(i => i.Id == item.IdUsuario);
                                        //if (UserDetails != null)
                                        //{
                                        //    item.Nome = UserDetails.Nome;
                                        //}
                                        //else
                                        //{
                                        //    var UserDetailsOld = ListUsuarioOld.FirstOrDefault(i => i.Id == item.IdUsuario);
                                        //    if (UserDetailsOld != null)
                                        //    {
                                        //        item.Nome = UserDetailsOld.Nome;
                                        //    }
                                        //}

                                        DateTime? inicio = item?.ProcessoDataInicio != null ? (DateTime)item?.ProcessoDataInicio : null;
                                        DateTime? conclusao = item?.ProcessoDataInicio != null ? (DateTime)item?.ProcessoDataConclusao : null;
                                        TimeSpan? diff = inicio != null && conclusao != null ? conclusao - inicio : null;
                                        item.timeDifference = diff != null ? diff.Value.ToString(@"hh\:mm\:ss") : null;

                                        if (item.Processar == 2)
                                        {
                                            string? FilePath = item.Arquivo;
                                            if (FilePath != null)
                                            {
                                                if (System.IO.File.Exists(FilePath))
                                                {
                                                    item.ArquivoStatus = 1;
                                                    item.ArquivoFilename = Path.GetFileName(FilePath);
                                                }
                                            }
                                        }

                                    }

                                    GlobalPagination = Pagination;
                                    int pageSize = GlobalPagination ?? 10;
                                    int pageNumber = 1;

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyLogItensDevolvidos);
                                    HttpContext.Session.SetObject(SessionKeyLogItensDevolvidos, entrada);

                                    //_ListRelatorio = new List<RelatorioViewModel?>();
                                    //_ListRelatorio = RelatorioResult;
                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);

                                    return View("Index", RelatorioPagedList);
                                }
                                else
                                {
                                    //List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
                                    //FerramentariaList = searches.OnLoadFerramentaria();

                                    //ViewBag.FerramentariaList = FerramentariaList;
                                    ViewBag.Error = "No Data Found.";
                                    return View("Index");
                                }
                            }


                            return View("Index");



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

        public ActionResult GetObservacao(int? id)
        {
            var LogItensDevolvidosModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogItensDevolvidos) ?? new List<RelatorioViewModel?>();

            var Relatorio = LogItensDevolvidosModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogItensDevolvidosModel.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }
        }

        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var LogItensDevolvidosModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogItensDevolvidos) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogItensDevolvidosModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult OpenModal()
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
            FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            return View("Index");
        }

        public IActionResult Salvar(DateTime? Inicial, DateTime? Final, List<string?> Setor, string? Balconista)
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


                            StringBuilder validationErrors = new StringBuilder();

                            if (ValidateForm(validationErrors, Inicial, Final))
                            {
                                DateTime? Start = DateTime.Now;
                                List<VW_ItensDevolvidos> ItensDevolvidosWithoutResult = new List<VW_ItensDevolvidos>();

                                ItensDevolvidosWithoutResult = _context.VW_ItensDevolvidos.Where(i =>
                                            (i.DataDevolucao >= Inicial.Value)
                                            && (i.DataDevolucao <= Final.Value.AddDays(1).AddTicks(-1))
                                            && (Setor.Count == 0 || Setor.Contains(i.SetorDevolucao))
                                            ).OrderByDescending(i => i.DataEmprestimo).ToList();

                                if (ItensDevolvidosWithoutResult.Count > 0)
                                {
                                    //List<VW_ItensDevolvidosWithoutFuncionarioViewModel> FinalItensDevolvidosResult = new List<VW_ItensDevolvidosWithoutFuncionarioViewModel>();

                                    //FinalItensDevolvidosResult = (from historico in ItensDevolvidosWithoutResult
                                    //                              join solicitante in _contextBS.VW_Funcionario_Registro_Atual on new { Chapa = historico.Solicitante_Chapa, IdTerceiro = historico.Solicitante_IdTerceiro, CodColigada = historico.Solicitante_CodColigada } equals new { solicitante.Chapa, solicitante.IdTerceiro, solicitante.CodColigada }
                                    //                              join devolucaoUser in _contextBS.VW_Usuario on historico.Balconista_Devolucao_IdLogin equals devolucaoUser.Id into devolucaoUserGroup
                                    //                              from devolucaoUser in devolucaoUserGroup.DefaultIfEmpty()
                                    //                              select new VW_ItensDevolvidosWithoutFuncionarioViewModel
                                    //                              {
                                    //                                  Catalogo = historico.Catalogo,
                                    //                                  Classe = historico.Classe,
                                    //                                  Tipo = historico.Tipo,
                                    //                                  Codigo = historico.Codigo,
                                    //                                  Produto = historico.Produto,
                                    //                                  AF = historico.AF,
                                    //                                  PAT = historico.PAT,
                                    //                                  Observacao = historico.Observacao,
                                    //                                  SetorOrigem = historico.SetorOrigem,
                                    //                                  SetorDevolucao = historico.SetorDevolucao,
                                    //                                  Solicitante_Chapa = solicitante.Chapa,
                                    //                                  Solicitante_Nome = solicitante.Nome,
                                    //                                  DataEmprestimo = historico.DataEmprestimo,
                                    //                                  Balconista_Devolucao_Nome = devolucaoUser?.Nome,
                                    //                                  DataDevolucao = historico.DataDevolucao,
                                    //                              }).ToList();

                                    if (Balconista != null)
                                    {
                                        string? balconistaUpperCase = Balconista.ToUpper();
                                        ItensDevolvidosWithoutResult = ItensDevolvidosWithoutResult
                                                                   .Where(item => item.Balconista_Devolucao != null && item.Balconista_Devolucao.Contains(balconistaUpperCase))
                                                                   .ToList();

                                        if (ItensDevolvidosWithoutResult.Count == 0)
                                        {
                                            ViewBag.Error = $"No Data Found for Balconista: {Balconista}";
                                            return View("Index");
                                        }
                                    }

                                    if (ItensDevolvidosWithoutResult.Count > 0)
                                    {
                                        string? FilePathName = MakeExcel(ItensDevolvidosWithoutResult);
                                        DateTime? End = DateTime.Now;
                                        StringBuilder q = new StringBuilder();
                                        q.AppendLine(" SELECT ");
                                        q.AppendLine(" 	Catálogo ");
                                        q.AppendLine(" 	,Classe ");
                                        q.AppendLine(" 	,Tipo ");
                                        q.AppendLine(" 	,[Código] ");
                                        q.AppendLine(" 	,Produto ");
                                        q.AppendLine(" 	,[AF/Serial] ");
                                        q.AppendLine(" 	,PAT ");
                                        q.AppendLine(" 	,[Observação] ");
                                        q.AppendLine(" 	,[Setor Origem] ");
                                        q.AppendLine(" 	,[Solicitante Chapa] ");
                                        q.AppendLine(" 	,[Solicitante Nome] ");
                                        q.AppendLine(" 	,[Data Empréstimo] ");
                                        q.AppendLine(" 	,[Balconista Devolução] ");
                                        q.AppendLine(" 	,[Data Devolução] ");

                                        q.AppendLine(" FROM ");
                                        q.AppendLine(" 	VW_ItensDevolvidos ");
                                        q.AppendLine(" WHERE ");
                                        q.AppendFormat("[Data Devolução] >= '{0} 00:00:0' ", Inicial.Value.ToString("yyyy-MM-dd"));
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("[Data Devolução] <= '{0} 00:00:0' ", Final.Value.ToString("yyyy-MM-dd"));

                                        if (Setor.Count > 0)
                                        {
                                            q.AppendFormat("AND [Setor da Devolução] in ({0}) ) ", string.Join(",", Setor));
                                        }
                                        if (Balconista != null)
                                        {
                                            q.AppendFormat("AND [Balconista Devolução] LIKE '%{0}%'  ", Balconista.ToUpper());
                                        }

                                        q.AppendLine(" ORDER BY [Data Devolução] DESC ");
                                        string? objectQuery = q.ToString();
                                        int? IdUsuario = loggedUser.Id;
                                        string? SamAccountName = loggedUser.Email;
                                        string? Relatorio = "ITENS DEVOLVIDOS";
                                        var InsertToRelatorio = new Relatorio
                                        {
                                            SAMAccountName = SamAccountName,
                                            IdUsuario = IdUsuario,
                                            RelatorioData = Relatorio,
                                            Arquivo = FilePathName,
                                            ProcessoDataInicio = Start,
                                            ProcessoDataConclusao = End,
                                            Processar = 2,
                                            Query = objectQuery,
                                            Ativo = 1,
                                            DataRegistro = DateTime.Now
                                        };
                                        _context.Add(InsertToRelatorio);
                                        _context.SaveChanges();

                                        if (InsertToRelatorio.Id != null)
                                        {
                                            ViewBag.ShowSuccessAlert = true;
                                            return View("Index");
                                        }
                                        else
                                        {
                                            ViewBag.Error = "Log Not Inserted";
                                            return View("Index");
                                        }
                                    }

                                    return View("Index");
                                }
                                else
                                {
                                    ViewBag.Error = $"No Data Found between this dates Inicial:{Inicial.Value.Date} - Final: {Final.Value.Date}";
                                    return View("Index");
                                }
                            }
                            else
                            {
                                List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
                                FerramentariaList = searches.OnLoadFerramentaria();

                                ViewBag.FerramentariaList = FerramentariaList;
                                //ViewBag.Error = "Inicial and Final is obrigatorio.";
                                ViewBag.Error = validationErrors.ToString();
                                return View("Index");
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

        public string MakeExcel(List<VW_ItensDevolvidos> FinalItensDevolvidosResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Catálogo");
            dataTable.Columns.Add("Classe");
            dataTable.Columns.Add("Tipo");
            dataTable.Columns.Add("Código");
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("AF/Serial");
            dataTable.Columns.Add("PAT");
            dataTable.Columns.Add("Observação");
            dataTable.Columns.Add("Setor Origem");
            dataTable.Columns.Add("Setor Devolução");
            dataTable.Columns.Add("Solicitante Chapa");
            dataTable.Columns.Add("Solicitante Nome");
            dataTable.Columns.Add("Data Empréstimo", typeof(DateTime)); //13
            dataTable.Columns.Add("Balconista Devolução");
            dataTable.Columns.Add("Data Devolução", typeof(DateTime)); //15

            foreach (var item in FinalItensDevolvidosResult)
            {
                var row = dataTable.NewRow();
                row["Catálogo"] = item.Catalogo;
                row["Classe"] = item.Classe;
                row["Tipo"] = item.Tipo;
                row["Código"] = item.Codigo;
                row["Produto"] = item.Produto;
                row["AF/Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["Observação"] = item.Observacao;
                row["Setor Origem"] = item.SetorOrigem;
                row["Setor Devolução"] = item.SetorDevolucao;
                row["Solicitante Chapa"] = item.Solicitante_Chapa;
                row["Solicitante Nome"] = item.Solicitante_Nome;
                row["Data Empréstimo"] = item.DataEmprestimo.HasValue == true ? item.DataEmprestimo.Value: DBNull.Value; //13
                row["Balconista Devolução"] = item.Balconista_Devolucao;
                row["Data Devolução"] = item.DataDevolucao.HasValue == true ? item.DataDevolucao.Value : DBNull.Value; //15

                dataTable.Rows.Add(row);
            }

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
            string? basePath = "D:\\Ferramentaria\\Relatorio\\";
            string filePath = Path.Combine(basePath, fileName);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Cells.AutoFitColumns();
                worksheet.Column(13).Style.Numberformat.Format = "dd/MM/yyyy";
                worksheet.Column(15).Style.Numberformat.Format = "dd/MM/yyyy";

                // Save the ExcelPackage directly to the file path
                package.SaveAs(new FileInfo(filePath));
            }

            return filePath;

        }

        public ActionResult ViewFile(string? FilePath)
        {
            //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
            //string filePath = Path.Combine(basePath, filename);
            //string filename = Path.GetFileName(filepath);

            string? filename = Path.GetFileName(FilePath);
            byte[] fileContents = System.IO.File.ReadAllBytes(FilePath);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileContents, contentType, filename);
        }

        private bool ValidateForm(StringBuilder validationErrors, DateTime? Inicial, DateTime? Final)
        {
            if (Inicial != null && Final != null)
            {
                int? InicialYear = Inicial.Value.Year;
                int? FinalYear = Final.Value.Year;
                int? countyear = FinalYear - InicialYear;

                if (countyear > 5)
                {
                    validationErrors.AppendLine("Please Filter in between 5 years and below to avoid slowing down in generating the report.");
                }

                if (Inicial.Value > Final.Value)
                {
                    validationErrors.AppendLine("Inicial shouldnt be greater than Final");
                }

            }
            else
            {
                validationErrors.AppendLine("Inicial and Final obrigatorio.");
            }

            return validationErrors.Length == 0;
        }

        public async Task<ActionResult> DeleteConfirmed(int id)
        {

            var Relatorio = await _context.Relatorio.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (Relatorio != null)
            {
                Relatorio.Ativo = 0;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "No ID selected." });
            }

            //return View();
        }

        // GET: LogItensDeolvidos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogItensDeolvidos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogItensDeolvidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LogItensDeolvidos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogItensDeolvidos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LogItensDeolvidos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogItensDeolvidos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
