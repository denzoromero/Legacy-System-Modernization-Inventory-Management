using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Linq;
using OfficeOpenXml;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerramentariaTest.Controllers
{
    public class TransacoesEmprestimosDevolucoes : Controller
    {

        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thTransacaoEmprestimoDevolucao.aspx";
        private MapperConfiguration mapeamentoClasses;
        private static int? GlobalPagination;

        private const string SessionKeyTransacoesEmprestimoDevolucao = "TransacoesEmprestimoDevolucao";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static List<RelatorioViewModel> _ListRelatorio = new List<RelatorioViewModel>();

        public TransacoesEmprestimosDevolucoes(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_HistoricoViewModel, VW_Historico>();
                cfg.CreateMap<VW_Historico, VW_HistoricoViewModel>();
                cfg.CreateMap<VW_HistoricoViewModel, VW_HistoricoWithoutFuncionario>();
                cfg.CreateMap<VW_HistoricoWithoutFuncionario, VW_HistoricoViewModel>();
                //cfg.CreateMap<VW_HistoricoViewModel, VW_Historico2000ate2005>();
                //cfg.CreateMap<VW_Historico2000ate2005, VW_HistoricoViewModel>();
                //cfg.CreateMap<VW_HistoricoViewModel, VW_Historico2006ate2010>();
                //cfg.CreateMap<VW_Historico2006ate2010, VW_HistoricoViewModel>();
                //cfg.CreateMap<VW_HistoricoViewModel, VW_Historico2011ate2015>();
                //cfg.CreateMap<VW_Historico2011ate2015, VW_HistoricoViewModel>();
                //cfg.CreateMap<VW_HistoricoViewModel, VW_Historico2016ate2020>();
                //cfg.CreateMap<VW_Historico2016ate2020, VW_HistoricoViewModel>();
                //cfg.CreateMap<VW_HistoricoViewModel, VW_Historico2021ate2024>();
                //cfg.CreateMap<VW_Historico2021ate2024, VW_HistoricoViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: TransacoesEmprestimosDevolucoes
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


        public ActionResult OpenModal()
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
            FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            return View("Index");
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



                            List<RelatorioViewModel> RelatorioResult = null;

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio
                                    .Where(r => r.RelatorioData == "TRANSAÇÕES DE EMPRÉSTIMOS E DEVOLUÇÕES" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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

                                if (RelatorioResult.Count > 0)
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyTransacoesEmprestimoDevolucao);
                                    HttpContext.Session.SetObject(SessionKeyTransacoesEmprestimoDevolucao, entrada);

                                    //_ListRelatorio = new List<RelatorioViewModel?>();
                                    //_ListRelatorio = RelatorioResult;
                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);


                                    //ViewBag.usuariofer = usuariofer;
                                    //ViewBag.Processo = Processo;
                                    //ViewBag.Registro = Registro;

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


        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var RelatorioListModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyTransacoesEmprestimoDevolucao) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = RelatorioListModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }



        public IActionResult Salvar(DateTime? Inicial, DateTime? Final, List<string?> Type, List<string?> Setor)
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
                                List<VW_HistoricoViewModel> HistoricoResult = new List<VW_HistoricoViewModel>();
                                List<VW_HistoricoWithoutFuncionario> HistoricWithoutoResult = new List<VW_HistoricoWithoutFuncionario>();
                                var mapper = mapeamentoClasses.CreateMapper();

                                //HistoricWithoutoResult = _context.VW_HistoricoWithoutFuncionario.Where(i =>
                                //            (i.DataEmprestimo >= Inicial.Value)
                                //            && (i.DataEmprestimo <= Final.Value.AddDays(1).AddTicks(-1))
                                //            && ((Setor.Count == 0 || (Setor.Contains(i.SetorOrigem) || Setor.Contains(i.SetorEmprestimo) || Setor.Contains(i.SetorDevolucao) )))
                                //            && (Type.Count == 0 || Type.Contains(i.Catalogo))
                                //            ).OrderByDescending(i => i.DataEmprestimo).ToList();

                                //var result = query.ToList();
                                //HistoricoResult = mapper.Map<List<VW_HistoricoViewModel>>(result);

                                List<VW_Historico> HistoricoItems = _context.VW_Historico.Where(i =>
                                                        (i.DataEmprestimo >= Inicial.Value)
                                                        && (i.DataEmprestimo <= Final.Value.AddDays(1).AddTicks(-1))
                                                        && ((Setor.Count == 0 || (Setor.Contains(i.SetorOrigem) || Setor.Contains(i.SetorEmprestimo) || Setor.Contains(i.SetorDevolucao))))
                                                        && (Type.Count == 0 || Type.Contains(i.Catalogo))
                                                        ).OrderByDescending(i => i.DataEmprestimo).ToList();

                                if (HistoricoItems.Count > 0)
                                {
                                    //List<VW_HistoricoViewModel> FinalHistoricoResult = new List<VW_HistoricoViewModel>();

                                    //FinalHistoricoResult = (from historico in HistoricWithoutoResult
                                    //                        join solicitante in _contextBS.VW_Funcionario_Registro_Atual on new { Chapa = historico.Solicitante_Chapa, IdTerceiro = historico.Solicitante_IdTerceiro, CodColigada = historico.Solicitante_CodColigada } equals new { solicitante.Chapa, solicitante.IdTerceiro, solicitante.CodColigada }
                                    //                        join liberador in _contextBS.VW_Funcionario_Registro_Atual on new { Chapa = historico.Liberador_Chapa, IdTerceiro = historico.Liberador_IdTerceiro, CodColigada = historico.Liberador_CodColigada } equals new { liberador.Chapa, liberador.IdTerceiro, liberador.CodColigada }
                                    //                        join emprestimoUser in _contextBS.VW_Usuario on historico.Balconista_Emprestimo_IdLogin equals emprestimoUser.Id
                                    //                        join devolucaoUser in _contextBS.VW_Usuario on historico.Balconista_Devolucao_IdLogin equals devolucaoUser.Id into devolucaoUserGroup
                                    //                        from devolucaoUser in devolucaoUserGroup.DefaultIfEmpty()
                                    //                        select new VW_HistoricoViewModel
                                    //                        {
                                    //                            Catalogo = historico.Catalogo,
                                    //                            Classe = historico.Classe,
                                    //                            Tipo = historico.Tipo,
                                    //                            Codigo = historico.Codigo,
                                    //                            Quantidade = historico.Quantidade,
                                    //                            Produto = historico.Produto,
                                    //                            CA = historico.CA,
                                    //                            AF = historico.AF,
                                    //                            PAT = historico.PAT,
                                    //                            Observacao = historico.Observacao,
                                    //                            SetorOrigem = historico.SetorOrigem,
                                    //                            StatusSolicitante = solicitante.Status,
                                    //                            ChapaSolicitante = solicitante.Chapa,
                                    //                            NomeSolicitante = solicitante.Nome,
                                    //                            FuncaoSolicitante = solicitante.Funcao,
                                    //                            SecaoSolicitante = solicitante.Secao,
                                    //                            ChapaLiberador = liberador.Chapa,
                                    //                            NomeLiberador = liberador.Nome,
                                    //                            SetorEmprestimo = liberador.Secao,
                                    //                            BalconistaEmprestimo = emprestimoUser.Nome,
                                    //                            Obra = historico.Obra,
                                    //                            DataEmprestimo = historico.DataEmprestimo,
                                    //                            DataPrevistaDevolucao = historico.DataPrevistaDevolucao,
                                    //                            DataVencimentoProduto = historico.DataVencimentoProduto,
                                    //                            SetorDevolucao = historico.SetorDevolucao,
                                    //                            BalconistaDevolucao = devolucaoUser != null ? devolucaoUser?.Nome : string.Empty,
                                    //                            DataDevolucao = historico.DataDevolucao,
                                    //                            StatusAtual = historico.StatusAtual,
                                    //                        }).ToList();

                                    string? FilePathName = MakeExcel(HistoricoItems);
                                    DateTime? End = DateTime.Now;
                                    StringBuilder q = new StringBuilder();
                                    q.AppendLine(" SELECT ");
                                    q.AppendLine(" 	Catalogo ");
                                    q.AppendLine(" 	,Classe ");
                                    q.AppendLine(" 	,Tipo ");
                                    q.AppendLine(" 	,[Código] ");
                                    q.AppendLine(" 	,Quantidade ");
                                    q.AppendLine(" 	,Produto ");
                                    q.AppendLine(" 	,[CA] ");
                                    q.AppendLine(" 	,AF ");
                                    q.AppendLine(" 	,PAT ");
                                    q.AppendLine(" 	,[Observação] ");
                                    q.AppendLine(" 	,[Setor de Origem] ");

                                    q.AppendLine("   ,[Status do Solicitante] ");
                                    q.AppendLine("   ,[Chapa do Solicitante] ");
                                    q.AppendLine("   ,[Nome do Solicitante] ");
                                    q.AppendLine("   ,[Função do Solicitante] ");
                                    q.AppendLine("   ,[Seção do Solicitante] ");

                                    q.AppendLine(" 	,[Chapa do Liberador] ");
                                    q.AppendLine(" 	,[Nome do Liberador] ");

                                    q.AppendLine(" 	,[Setor do Empréstimo] ");
                                    q.AppendLine(" 	,[Balconista do Empréstimo] ");

                                    q.AppendLine(" 	,Obra ");

                                    q.AppendLine(" 	,[Data do Empréstimo] ");
                                    q.AppendLine(" 	,[Data Prevista para Devolução] ");
                                    q.AppendLine(" 	,[Data de Vencimento do Produto] ");
                                    q.AppendLine(" 	,[Setor da Devolução] ");
                                    q.AppendLine(" 	,[Balconista da Devolução] ");
                                    q.AppendLine(" 	,[Data de Devolução] ");
                                    q.AppendLine(" 	,[Status Atual] ");
                                    q.AppendLine(" FROM ");
                                    q.AppendLine(" 	VW_Historico AS Historico ");
                                    q.AppendLine(" WHERE ");
                                    q.AppendFormat("[Data do Empréstimo] >= '{0} 00:00:0' ", Inicial.Value.ToString("yyyy-MM-dd"));
                                    q.AppendLine(" AND ");
                                    q.AppendFormat("[Data do Empréstimo] <= '{0} 00:00:0' ", Final.Value.ToString("yyyy-MM-dd"));

                                    if (Setor.Count > 0)
                                    {
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("( [Setor de Origem] in ({0}) ", string.Join(",", Setor));
                                        q.AppendLine(" OR ");
                                        q.AppendFormat("[Setor do Empréstimo] in ({0}) ", string.Join(",", Setor));
                                        q.AppendLine(" OR ");
                                        q.AppendFormat("[Setor da Devolução] in ({0}) ) ", string.Join(",", Setor));
                                    }

                                    if (Type.Count > 0)
                                    {
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("( Catalogo in ({0}) )", string.Join(",", Type));
                                    }
                                    q.AppendLine(" ORDER BY [Data do Empréstimo] DESC ");

                                    string? objectQuery = q.ToString();
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "TRANSAÇÕES DE EMPRÉSTIMOS E DEVOLUÇÕES";
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
                                else
                                {
                                    ViewBag.Error = "No Data Found";
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

        //public string MakeExcel(List<VW_HistoricoViewModel> FinalHistoricoResult)
        //{
        //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");


        //    DataTable dataTable = new DataTable();
        //    dataTable.Columns.Add("Catalogo"); //1
        //    dataTable.Columns.Add("Classe"); //2
        //    dataTable.Columns.Add("Tipo"); //3
        //    dataTable.Columns.Add("Código"); //4
        //    dataTable.Columns.Add("Quantidade"); //5
        //    dataTable.Columns.Add("Produto"); //6
        //    dataTable.Columns.Add("CA"); //7
        //    dataTable.Columns.Add("AF"); //8
        //    dataTable.Columns.Add("PAT"); //9
        //    dataTable.Columns.Add("Observação"); //10
        //    dataTable.Columns.Add("Setor de Origem"); //11

        //    dataTable.Columns.Add("Status do Solicitante"); //12
        //    dataTable.Columns.Add("Chapa do Solicitante"); //13
        //    dataTable.Columns.Add("Nome do Solicitante"); //14
        //    dataTable.Columns.Add("Função do Solicitante"); //15
        //    dataTable.Columns.Add("Seção do Solicitante"); //16

        //    dataTable.Columns.Add("Chapa do Liberador"); //17
        //    dataTable.Columns.Add("Nome do Liberador"); //18

        //    dataTable.Columns.Add("Setor do Empréstimo"); //19
        //    dataTable.Columns.Add("Balconista do Empréstimo"); //20

        //    dataTable.Columns.Add("Obra"); //21

        //    dataTable.Columns.Add("Data do Empréstimo", typeof(DateTime)); //22
        //    dataTable.Columns.Add("Data Prevista para Devolução", typeof(DateTime)); //23
        //    dataTable.Columns.Add("Data de Vencimento do Produto", typeof(DateTime)); //24
        //    dataTable.Columns.Add("Setor da Devolução"); //25
        //    dataTable.Columns.Add("Balconista da Devolução"); //26
        //    dataTable.Columns.Add("Data de Devolução", typeof(DateTime)); //27
        //    dataTable.Columns.Add("Status Atual"); //28


        //    foreach (var item in FinalHistoricoResult)
        //    {
        //        var row = dataTable.NewRow();
        //        row["Catalogo"] = item.Catalogo;
        //        row["Classe"] = item.Classe;
        //        row["Tipo"] = item.Tipo;
        //        row["Código"] = item.Codigo;
        //        row["Quantidade"] = item.Quantidade;
        //        row["Produto"] = item.Produto;
        //        row["CA"] = item.CA;
        //        row["AF"] = item.AF;
        //        row["PAT"] = item.PAT;
        //        row["Setor de Origem"] = item.SetorOrigem;
        //        row["Status do Solicitante"] = item.StatusSolicitante;
        //        row["Chapa do Solicitante"] = item.ChapaSolicitante;
        //        row["Nome do Solicitante"] = item.NomeSolicitante;
        //        row["Função do Solicitante"] = item.FuncaoSolicitante;
        //        row["Seção do Solicitante"] = item.SecaoSolicitante;
        //        row["Chapa do Liberador"] = item.ChapaLiberador;
        //        row["Nome do Liberador"] = item.NomeLiberador;
        //        row["Setor do Empréstimo"] = item.SetorEmprestimo;
        //        row["Balconista do Empréstimo"] = item.BalconistaEmprestimo;
        //        row["Obra"] = item.Obra;
        //        row["Data do Empréstimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value; //22
        //        row["Data Prevista para Devolução"] = item.DataPrevistaDevolucao.HasValue == true ? (object)item.DataPrevistaDevolucao.Value : DBNull.Value; //23
        //        row["Data de Vencimento do Produto"] = item.DataVencimentoProduto.HasValue == true ? (object)item.DataVencimentoProduto.Value : DBNull.Value; //24
        //        row["Setor da Devolução"] = item.SetorDevolucao;
        //        row["Balconista da Devolução"] = item.BalconistaDevolucao;
        //        row["Data de Devolução"] = item.DataDevolucao.HasValue == true ? (object)item.DataDevolucao.Value : DBNull.Value; //27
        //        row["Status Atual"] = item.StatusAtual;


        //        dataTable.Rows.Add(row);
        //    }

        //    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
        //    //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
        //    string? basePath = "D:\\Ferramentaria\\Relatorio\\";
        //    string filePath = Path.Combine(basePath, fileName);

        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
        //        worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
        //        worksheet.Cells.AutoFitColumns();
        //        worksheet.Column(22).Style.Numberformat.Format = "dd/MM/yyyy";
        //        worksheet.Column(23).Style.Numberformat.Format = "dd/MM/yyyy";
        //        worksheet.Column(24).Style.Numberformat.Format = "dd/MM/yyyy";
        //        worksheet.Column(27).Style.Numberformat.Format = "dd/MM/yyyy";

        //        // Save the ExcelPackage directly to the file path
        //        package.SaveAs(new FileInfo(filePath));
        //    }

        //    return filePath;

        //}

        public string MakeExcel(List<VW_Historico> FinalHistoricoResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");


            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Catalogo"); //1
            dataTable.Columns.Add("Classe"); //2
            dataTable.Columns.Add("Tipo"); //3
            dataTable.Columns.Add("Código"); //4
            dataTable.Columns.Add("Quantidade"); //5
            dataTable.Columns.Add("Produto"); //6
            dataTable.Columns.Add("CA"); //7
            dataTable.Columns.Add("No do CA"); //8
            dataTable.Columns.Add("Validade do CA"); //9
            dataTable.Columns.Add("AF"); //10
            dataTable.Columns.Add("PAT"); //11
            dataTable.Columns.Add("Observação"); //12
            dataTable.Columns.Add("Setor de Origem"); //13

            dataTable.Columns.Add("Status do Solicitante"); //14
            dataTable.Columns.Add("Chapa do Solicitante"); //15
            dataTable.Columns.Add("Nome do Solicitante"); //16
            dataTable.Columns.Add("Função do Solicitante"); //17
            dataTable.Columns.Add("Seção do Solicitante"); //18

            dataTable.Columns.Add("Chapa do Liberador"); //19
            dataTable.Columns.Add("Nome do Liberador"); //20

            dataTable.Columns.Add("Setor do Empréstimo"); //21
            dataTable.Columns.Add("Balconista do Empréstimo"); //22

            dataTable.Columns.Add("Obra"); //23

            dataTable.Columns.Add("Data do Empréstimo", typeof(DateTime)); //24
            dataTable.Columns.Add("Data Prevista para Devolução", typeof(DateTime)); //25
            dataTable.Columns.Add("Data de Vencimento do Produto", typeof(DateTime)); //26
            dataTable.Columns.Add("Setor da Devolução"); //27
            dataTable.Columns.Add("Balconista da Devolução"); //28
            dataTable.Columns.Add("Data de Devolução", typeof(DateTime)); //29
            dataTable.Columns.Add("Status Atual"); //30


            foreach (var item in FinalHistoricoResult)
            {
                var row = dataTable.NewRow();
                row["Catalogo"] = item.Catalogo;
                row["Classe"] = item.Classe;
                row["Tipo"] = item.Tipo;
                row["Código"] = item.Codigo;
                row["Quantidade"] = item.Quantidade;
                row["Produto"] = item.Produto;
                row["CA"] = item.CA;
                row["No do CA"] = item.NumeroCA;
                row["Validade do CA"] = item.ValidadeCA;
                row["AF"] = item.AF;
                row["PAT"] = item.PAT;
                row["Setor de Origem"] = item.SetorOrigem;
                row["Status do Solicitante"] = item.StatusSolicitante;
                row["Chapa do Solicitante"] = item.ChapaSolicitante;
                row["Nome do Solicitante"] = item.NomeSolicitante;
                row["Função do Solicitante"] = item.FuncaoSolicitante;
                row["Seção do Solicitante"] = item.SecaoSolicitante;
                row["Chapa do Liberador"] = item.ChapaLiberador;
                row["Nome do Liberador"] = item.NomeLiberador;
                row["Setor do Empréstimo"] = item.SetorEmprestimo;
                row["Balconista do Empréstimo"] = item.BalconistaEmprestimo;
                row["Obra"] = item.Obra;
                row["Data do Empréstimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value; //22
                row["Data Prevista para Devolução"] = item.DataPrevistaDevolucao.HasValue == true ? (object)item.DataPrevistaDevolucao.Value : DBNull.Value; //23
                row["Data de Vencimento do Produto"] = item.DataVencimentoProduto.HasValue == true ? (object)item.DataVencimentoProduto.Value : DBNull.Value; //24
                row["Setor da Devolução"] = item.SetorDevolucao;
                row["Balconista da Devolução"] = item.BalconistaDevolucao;
                row["Data de Devolução"] = item.DataDevolucao.HasValue == true ? (object)item.DataDevolucao.Value : DBNull.Value; //27
                row["Status Atual"] = item.StatusAtual;


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
                worksheet.Column(24).Style.Numberformat.Format = "dd/MM/yyyy";
                worksheet.Column(25).Style.Numberformat.Format = "dd/MM/yyyy";
                worksheet.Column(26).Style.Numberformat.Format = "dd/MM/yyyy";
                worksheet.Column(29).Style.Numberformat.Format = "dd/MM/yyyy";

                // Save the ExcelPackage directly to the file path
                package.SaveAs(new FileInfo(filePath));
            }

            return filePath;

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


        public ActionResult GetObservacao(int? id)
        {
            var RelatorioListModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyTransacoesEmprestimoDevolucao) ?? new List<RelatorioViewModel?>();
            var Relatorio = RelatorioListModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = RelatorioListModel.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }
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


        // GET: TransacoesEmprestimosDevolucoes/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TransacoesEmprestimosDevolucoes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TransacoesEmprestimosDevolucoes/Create
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

        // GET: TransacoesEmprestimosDevolucoes/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TransacoesEmprestimosDevolucoes/Edit/5
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

        // GET: TransacoesEmprestimosDevolucoes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TransacoesEmprestimosDevolucoes/Delete/5
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
