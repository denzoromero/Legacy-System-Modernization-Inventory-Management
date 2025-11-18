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
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerramentariaTest.Controllers
{
    public class LogEmprestimosConsumiveisEPI : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thRelSumEmpConsEPI.aspx";
        //private MapperConfiguration mapeamentoClasses;
        private static int? GlobalPagination;

        private const string SessionKeyLogEmprestimoConsumiveisEPIRelatorio = "LogEmprestimoConsumiveisEPIRelatorio";
        //private static List<RelatorioViewModel> _ListRelatorio = new List<RelatorioViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public LogEmprestimosConsumiveisEPI(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }


        // GET: LogEmprestimosConsumiveisEPI
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
                                    .Where(r => r.RelatorioData == "RELATÓRIO DE CONTAB. EMPRÉSTIMOS DE CONSUMÍVEIS e EPI" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyLogEmprestimoConsumiveisEPIRelatorio);
                                    HttpContext.Session.SetObject(SessionKeyLogEmprestimoConsumiveisEPIRelatorio, entrada);
                                    //_ListRelatorio = new List<RelatorioViewModel?>();
                                    //_ListRelatorio = RelatorioResult;
                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);

                                    return View("Index", RelatorioPagedList);
                                }
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
            var LogRelatorioModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogEmprestimoConsumiveisEPIRelatorio) ?? new List<RelatorioViewModel?>();

            var Relatorio = LogRelatorioModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogRelatorioModel.ToPagedList(pageNumber, pageSize);

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

            var LogRelatorioModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogEmprestimoConsumiveisEPIRelatorio) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogRelatorioModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult OpenModal()
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
            FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            List<ObraViewModel> ObraList = new List<ObraViewModel>();
            ObraList = searches.OnLoadObra();
            ViewBag.ObraList = ObraList;

            return View("Index");
        }

        public IActionResult Salvar(DateTime? Inicial, DateTime? Final, List<string?> Type, List<string?> Setor, List<string?> Obra)
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
                                List<GroupedHistorico> GroupedHistoricoResult = new List<GroupedHistorico>();

                                GroupedHistoricoResult = _context.VW_Historico
                                                          .Where(i =>
                                                              (i.DataEmprestimo >= Inicial.Value)
                                                              && (i.DataEmprestimo <= Final.Value.AddDays(1).AddTicks(-1))
                                                              && (Setor.Count == 0 || Setor.Contains(i.SetorEmprestimo))
                                                              && (Type.Count == 0 || Type.Contains(i.Catalogo))
                                                              && (Obra.Count == 0 || Obra.Contains(i.Obra))
                                                          )
                                                          .GroupBy(i => new
                                                          {
                                                              Catalogo = i.Catalogo,
                                                              Codigo = i.Codigo,
                                                              Produto = i.Produto,
                                                              SetorEmprestimo = i.SetorEmprestimo,
                                                              Obra = i.Obra
                                                          })
                                                          .Select(group => new GroupedHistorico
                                                          {
                                                              Tipo = group.Key.Catalogo,
                                                              Codigo = group.Key.Codigo,
                                                              Item = group.Key.Produto,
                                                              Quantidade = group.Sum(i => i.Quantidade),
                                                              Setor = group.Key.SetorEmprestimo,
                                                              Obra = group.Key.Obra
                                                          })
                                                          .ToList();

                                if (GroupedHistoricoResult.Count > 0)
                                {
                                    string? FilePathName = MakeExcel(GroupedHistoricoResult);
                                    DateTime? End = DateTime.Now;
                                    StringBuilder q = new StringBuilder();
                                    q.AppendLine(" SELECT ");
                                    q.AppendLine(" 	Tipo = Catalogo, ");
                                    q.AppendLine(" 	Codigo = [Código], ");
                                    q.AppendLine(" 	Item = Produto, ");
                                    q.AppendLine(" 	Quantidade = SUM(Quantidade), ");
                                    q.AppendLine(" 	Setor = [Setor do Empréstimo], ");
                                    q.AppendLine(" 	Obra ");
                                    q.AppendLine(" FROM ");
                                    q.AppendLine(" 	VW_Historico AS Historico ");
                                    q.AppendLine(" WHERE ");
                                    q.AppendFormat("[Data do Empréstimo] >= '{0} 00:00:0' ", Inicial.Value.ToString("yyyy-MM-dd"));
                                    q.AppendLine(" AND ");
                                    q.AppendFormat("[Data do Empréstimo] <= '{0} 00:00:0' ", Final.Value.ToString("yyyy-MM-dd"));

                                    if (Setor.Count > 0)
                                    {
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("([Setor do Empréstimo] in ({0}) )", string.Join(",", Setor));
                                    }
                                    if (Type.Count > 0)
                                    {
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("( Catalogo in ({0}) )", string.Join(",", Type));
                                    }
                                    if (Obra.Count > 0)
                                    {
                                        q.AppendLine(" AND ");
                                        q.AppendFormat("(Obra in ({0}) )", string.Join(",", Setor));
                                    }
                                    q.AppendLine(" Group By ");
                                    q.AppendLine(" 	 Catalogo, ");
                                    q.AppendLine(" 	 [Código], ");
                                    q.AppendLine(" 	 Produto, ");
                                    q.AppendLine(" 	 [Setor do Empréstimo], ");
                                    q.AppendLine(" 	 Obra ");

                                    string? objectQuery = q.ToString();
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "RELATÓRIO DE CONTAB. EMPRÉSTIMOS DE CONSUMÍVEIS e EPI";
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
                                    ViewBag.Error = $"No Data Found for Inicial:{Inicial.Value.Date} AND Final:{Final.Value.Date}";
                                    return View("Index");
                                }

                            }
                            else
                            {
                                List<FerramentariaViewModel> FerramentariaList = new List<FerramentariaViewModel>();
                                FerramentariaList = searches.OnLoadFerramentaria();
                                ViewBag.FerramentariaList = FerramentariaList;


                                List<ObraViewModel> ObraList = new List<ObraViewModel>();
                                ObraList = searches.OnLoadObra();
                                ViewBag.ObraList = ObraList;
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

        public string MakeExcel(List<GroupedHistorico> GroupedHistoricoResult)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Tipo");
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Item");
            dataTable.Columns.Add("Quantidade");
            dataTable.Columns.Add("Setor");
            dataTable.Columns.Add("Obra");

            foreach (var item in GroupedHistoricoResult)
            {
                var row = dataTable.NewRow();
                row["Tipo"] = item.Tipo;
                row["Codigo"] = item.Codigo;
                row["Item"] = item.Item;
                row["Quantidade"] = item.Quantidade;
                row["Setor"] = item.Setor;
                row["Obra"] = item.Obra;

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


        // GET: LogEmprestimosConsumiveisEPI/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogEmprestimosConsumiveisEPI/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogEmprestimosConsumiveisEPI/Create
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

        // GET: LogEmprestimosConsumiveisEPI/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogEmprestimosConsumiveisEPI/Edit/5
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

        // GET: LogEmprestimosConsumiveisEPI/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogEmprestimosConsumiveisEPI/Delete/5
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
