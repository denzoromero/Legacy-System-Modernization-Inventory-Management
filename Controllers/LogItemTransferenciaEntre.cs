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

namespace FerramentariaTest.Controllers
{
    public class LogItemTransferenciaEntre : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thHistItensTransEntreFerr.aspx";
        private static int? GlobalPagination;
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyTransferenciaEntre = "TransferenciaEntre";
        //private static List<RelatorioViewModel?> _ListRelatorio = new List<RelatorioViewModel>();
        //private static List<ProdutoList?> _ListProduto = new List<ProdutoList?>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";


        public LogItemTransferenciaEntre(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_HistItensTransEntreFerrViewModel, VW_HistItensTransEntreFerr>();
                cfg.CreateMap<VW_HistItensTransEntreFerr, VW_HistItensTransEntreFerrViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: LogItemTransferenciaEntre
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
                                    .Where(r => r.RelatorioData == "HISTÓRICO DO ITEM EM TRANSF. ENTRE FERR." && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyTransferenciaEntre);
                                    HttpContext.Session.SetObject(SessionKeyTransferenciaEntre, entrada);

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
                return View();
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

        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var LogItemTransferenciaEntreModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyTransferenciaEntre) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogItemTransferenciaEntreModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult GetProduto(string? AF, int? PAT)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            if (AF != null || PAT != null)
            {
                List<ProdutoList>? ProdutoList = new List<ProdutoList>();
                ProdutoList = searches.FindProdutoForLog(AF, PAT);

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


                            List<VW_HistItensTransEntreFerrViewModel> HistItensTransEntreList = new List<VW_HistItensTransEntreFerrViewModel>();
                            List<int?> AcceptedIds = new List<int?>();
                            List<int?> ErrorIds = new List<int?>();

                            var mapper = mapeamentoClasses.CreateMapper();

                            if (checkeditems.Count > 0)
                            {
                                DateTime? Start = DateTime.Now;

                                foreach (int item in checkeditems)
                                {
                                    var query = _context.VW_HistItensTransEntreFerr.Where(i => i.IdProduto == item).OrderByDescending(r => r.DataOcorrencia).ToList();
                                    if (query.Count > 0)
                                    {
                                        foreach (var resultItem in query)
                                        {
                                            HistItensTransEntreList.Add(mapper.Map<VW_HistItensTransEntreFerrViewModel>(resultItem));
                                        }

                                        AcceptedIds.Add(item);
                                    }
                                    else
                                    {
                                        ErrorIds.Add(item);
                                    }
                                }

                                if (HistItensTransEntreList.Count > 0)
                                {
                                    string? FilePathName = MakeExcel(HistItensTransEntreList);
                                    DateTime? End = DateTime.Now;
                                    string? objectQuery = $"Select [Código] ,Produto ,[AF/Serial] ,PAT ,[*] = '*' ,[Data do Ocorrência] ,[Ferr. Origem] ,[Ferr. Destino] , Suporte FROM VW_HistItensTransEntreFerr Where IdProduto IN ({string.Join(",", AcceptedIds)}) Order By [Data do Ocorrência]  Desc";
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "HISTÓRICO DO ITEM EM TRANSF. ENTRE FERR.";
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

        public string MakeExcel(List<VW_HistItensTransEntreFerrViewModel> HistItensTransEntreList)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Código");
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("AF/Serial");
            dataTable.Columns.Add("PAT");
            dataTable.Columns.Add("*");
            dataTable.Columns.Add("Data do Ocorrência", typeof(DateTime)); //6
            dataTable.Columns.Add("Ferr. Origem");
            dataTable.Columns.Add("Ferr. Destino");
            dataTable.Columns.Add("Suporte");
            dataTable.Columns.Add("Documento");

            foreach (var item in HistItensTransEntreList)
            {
                var row = dataTable.NewRow();
                row["Código"] = item.Codigo; // Replace with the actual property name
                row["Produto"] = item.Produto; // Replace with the actual property name
                row["AF/Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["*"] = "*";
                row["Data do Ocorrência"] = item.DataOcorrencia.HasValue == true ? (object)item.DataOcorrencia.Value : DBNull.Value;//6
                row["Ferr. Origem"] = item.FerrOrigem;
                row["Ferr. Destino"] = item.FerrDestino;
                row["Suporte"] = item.Suporte;
                row["Documento"] = item.Documento;

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

        public ActionResult GetObservacao(int? id)
        {

            var LogItemTransferenciaEntreModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyTransferenciaEntre) ?? new List<RelatorioViewModel?>();
            var Relatorio = LogItemTransferenciaEntreModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogItemTransferenciaEntreModel.ToPagedList(pageNumber, pageSize);

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



        // GET: LogItemTransferenciaEntre/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogItemTransferenciaEntre/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogItemTransferenciaEntre/Create
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

        // GET: LogItemTransferenciaEntre/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogItemTransferenciaEntre/Edit/5
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

        // GET: LogItemTransferenciaEntre/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogItemTransferenciaEntre/Delete/5
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
