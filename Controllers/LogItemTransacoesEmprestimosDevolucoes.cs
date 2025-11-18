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
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerramentariaTest.Controllers
{
    public class LogItemTransacoesEmprestimosDevolucoes : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thHistItensEmpDev.aspx";
        private static int? GlobalPagination;
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyRelatorioTransacoesEmprestimoDevolucoes = "TransacoesEmprestimoDevolucoes";
        //private static List<RelatorioViewModel?> _ListRelatorio = new List<RelatorioViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static List<ProdutoList?> _ListProduto = new List<ProdutoList?>();

        public LogItemTransacoesEmprestimosDevolucoes(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_HistItensEmpDevViewModel, VW_HistItensEmpDev>();
                cfg.CreateMap<VW_HistItensEmpDev, VW_HistItensEmpDevViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: LogItemTransacoesEmprestimosDevolucoes
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

                            List<RelatorioViewModel> RelatorioResult = null;

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio
                                    .Where(r => r.RelatorioData == "HISTÓRICO DO ITEM EM TRANS. EMPRÉSTIMOS E DEVOLUÇÕES" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyRelatorioTransacoesEmprestimoDevolucoes);
                                    HttpContext.Session.SetObject(SessionKeyRelatorioTransacoesEmprestimoDevolucoes, entrada);
                                    //_ListRelatorio = new List<RelatorioViewModel?>();
                                    //_ListRelatorio = RelatorioResult;
                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);
                                    return View("Index", RelatorioPagedList);
                                }
                                else
                                {
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
                return View("Index");
            }

        }

        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var LogTransacoesEmprestimoDevolucoesModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyRelatorioTransacoesEmprestimoDevolucoes) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogTransacoesEmprestimoDevolucoesModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult GetProduto(string? AF, int? PAT)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            if (AF != null || PAT != null)
            {
                List<ProdutoList>? ProdutoList = new List<ProdutoList>();
                ProdutoList = searches.FindProdutoForLog(AF,PAT);

                if (ProdutoList.Count > 0)
                {
                    //_ListProduto = new List<ProdutoList?>();
                    //_ListProduto = ProdutoList;
                    ViewBag.ProdutoList = ProdutoList;
                    return View("Index");
                }
                else
                {
                    ViewBag.Errormodal = "No Result Found";
                    return View("Index");
                }
            }
            else
            {
                ViewBag.Error = "AF or PAT obrigatorio";
                return View("Index");
            }          
        }

        public ActionResult GetObservacao(int? id)
        {

            var LogTransacoesEmprestimoDevolucoesModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyRelatorioTransacoesEmprestimoDevolucoes) ?? new List<RelatorioViewModel?>();

            var Relatorio = LogTransacoesEmprestimoDevolucoesModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogTransacoesEmprestimoDevolucoesModel.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }
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

        [HttpPost]
        public ActionResult Register(List<int?> checkeditems)
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


                            List<VW_HistItensEmpDevViewModel> HistItensList = new List<VW_HistItensEmpDevViewModel>();
                            List<int?> AcceptedIds = new List<int?>();
                            List<int?> ErrorIds = new List<int?>();

                            var mapper = mapeamentoClasses.CreateMapper();
                            //List<ObraViewModel> ObraResult = mapper.Map<List<ObraViewModel>>(obras);

                            if (checkeditems.Count > 0)
                            {
                                DateTime? Start = DateTime.Now;

                                foreach (int item in checkeditems)
                                {
                                    var query = _context.VW_HistItensEmpDev.Where(i => i.IdProduto == item).OrderByDescending(r => r.DataEmprestimo).ToList();
                                    if (query.Count > 0)
                                    {
                                        foreach (var resultItem in query)
                                        {
                                            HistItensList.Add(mapper.Map<VW_HistItensEmpDevViewModel>(resultItem));
                                        }

                                        AcceptedIds.Add(item);
                                    }
                                    else
                                    {
                                        ErrorIds.Add(item);
                                    }
                                }

                                if (HistItensList.Count > 0)
                                {
                                    string? FilePathName = MakeExcel(HistItensList);
                                    DateTime? End = DateTime.Now;
                                    string? objectQuery = $"Select [Código] ,Produto ,[AF/Serial] ,PAT ,[*] = '*' ,[Data do Empréstimo] ,[Ferr. Empréstimo] ,[Status do Solicitante] ,[Mat. Solicitante] ,[Nome Solicitante] ,[Balconista do Empréstimo] ,[*] = '*' ,[Data de Devolução] ,[Ferr. Devolução] ,[Balc. Dev.] From VW_HistItensEmpDev Where IdProduto IN ({string.Join(",", AcceptedIds)}) Order By [Data do Empréstimo]  Desc";
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "HISTÓRICO DO ITEM EM TRANS. EMPRÉSTIMOS E DEVOLUÇÕES";
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
                                    }

                                    return View("Index");

                                }
                                else
                                {
                                    ViewBag.Error = "No Data Found";
                                    //ViewBag.ProdutoList = _ListProduto;
                                    return View("Index");
                                }

                            }
                            else
                            {
                                ViewBag.Errormodal = "Please Select Produto";
                                //ViewBag.ProdutoList = _ListProduto;
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
                ViewBag.Error = ex.Message;
                return View("Index");
            }          
        }

        public string MakeExcel(List<VW_HistItensEmpDevViewModel> HistItensList)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Código"); //1
            dataTable.Columns.Add("Produto"); //2
            dataTable.Columns.Add("AF/Serial"); //3
            dataTable.Columns.Add("PAT"); //4
            dataTable.Columns.Add("*"); //5
            dataTable.Columns.Add("Data do Empréstimo", typeof(DateTime)); //6
            dataTable.Columns.Add("Ferr. Empréstimo"); //7
            dataTable.Columns.Add("Status do Solicitante"); //8
            dataTable.Columns.Add("Mat. Solicitante"); //9
            dataTable.Columns.Add("Nome Solicitante"); //10
            dataTable.Columns.Add("Balconista do Empréstimo"); //11
            dataTable.Columns.Add("*1"); //12 
            dataTable.Columns.Add("Data de Devolução", typeof(DateTime));//13
            dataTable.Columns.Add("Ferr. Devolução"); //14
            dataTable.Columns.Add("Balc. Dev"); //15



            foreach (var item in HistItensList)
            {
                var row = dataTable.NewRow();
                row["Código"] = item.Codigo; // Replace with the actual property name
                row["Produto"] = item.Produto; // Replace with the actual property name
                row["AF/Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["*"] = "*";
                row["Data do Empréstimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value; //6
                row["Ferr. Empréstimo"] = item.FerrEmprestimo;
                row["Status do Solicitante"] = item.StatusSolicitante;
                row["Mat. Solicitante"] = item.ChapaSolicitante;
                row["Nome Solicitante"] = item.NomeSolicitante;
                row["Balconista do Empréstimo"] = item.BalconistaEmprestimo;
                row["*1"] = "*";
                row["Data de Devolução"] = item.DataDevolucao.HasValue == true ? (object)item.DataDevolucao.Value : DBNull.Value; //13
                row["Ferr. Devolução"] = item.FerrDevolucao;
                row["Balc. Dev"] = item.BalcDev;

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Log Itens");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(6).Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Column(13).Style.Numberformat.Format = "dd/MM/yyyy";
                    package.Save();
                }

                memoryStream.Position = 0;
                byte[] content = memoryStream.ToArray();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                FileContentResult fileResult = File(content, contentType, fileName);
                //string? basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
                string? basePath = "D:\\Ferramentaria\\Relatorio\\";

                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string caminhoFisico = basePath + fileName;
                System.IO.File.WriteAllBytes(caminhoFisico, content);

                return caminhoFisico;
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


        // GET: LogItemTransacoesEmprestimosDevolucoes/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogItemTransacoesEmprestimosDevolucoes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogItemTransacoesEmprestimosDevolucoes/Create
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

        // GET: LogItemTransacoesEmprestimosDevolucoes/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogItemTransacoesEmprestimosDevolucoes/Edit/5
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

        // GET: LogItemTransacoesEmprestimosDevolucoes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogItemTransacoesEmprestimosDevolucoes/Delete/5
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
