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
using System.Linq;
using System.Data;

namespace FerramentariaTest.Controllers
{
    public class LogExclusaoProduto : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLogExclusaoProduto.aspx";
        private static int? GlobalPagination;
        private MapperConfiguration mapeamentoClasses;
        private const string SessionKeyRelatorioExclusaoProduto = "RelatorioExclusaoProduto";
        //private static List<RelatorioViewModel?> _ListRelatorio = new List<RelatorioViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";


        public LogExclusaoProduto(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_Exclusao_ProdutoViewModel, VW_Exclusao_Produto>();
                cfg.CreateMap<VW_Exclusao_Produto, VW_Exclusao_ProdutoViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }


        // GET: LogExclusaoProduto
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
                                    .Where(r => r.RelatorioData == "LOG DE EXCLUSAO DE PRODUTOS" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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


                                    //ViewBag.usuariofer = usuariofer;
                                    GlobalPagination = Pagination;
                                    int pageSize = GlobalPagination ?? 10;
                                    int pageNumber = 1;

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyRelatorioExclusaoProduto);
                                    HttpContext.Session.SetObject(SessionKeyRelatorioExclusaoProduto, entrada);

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

        public ActionResult TestPage(int? page)
        {
            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            var LogRelatorioExclusaoProduto = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyRelatorioExclusaoProduto) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogRelatorioExclusaoProduto.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }


        public IActionResult Salvar(string? Ferramentaria, string? Catalogo, string? Classe, string? Tipo, string? Codigo, string? Produto, string? RFM, string? AF, int? PAT, string? Suporte, string? Motivo, string? Justificativa, DateTime? De, DateTime? Ate) 
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

                            List<VW_Exclusao_ProdutoViewModel>? ExclusaoResult = new List<VW_Exclusao_ProdutoViewModel>();

                            DateTime? Start = DateTime.Now;

                            var query = _context.VW_Exclusao_Produto.Where(r =>
                                   (Ferramentaria == null || r.Ferramentaria.Contains(Ferramentaria))
                                   && (Catalogo == null || r.Catalogo.Contains(Catalogo))
                                   && (Classe == null || r.Classe.Contains(Classe))
                                   && (Tipo == null || r.Tipo.Contains(Tipo))
                                   && (Codigo == null || r.Codigo.Contains(Codigo))
                                   && (Produto == null || r.Produto.Contains(Produto))
                                   && (RFM == null || r.RFM.Contains(RFM))
                                   && (AF == null || r.AF.Contains(AF))
                                   && (PAT == null || r.PAT == PAT)
                                   && (Suporte == null || r.Suporte.Contains(Suporte))
                                   && (Motivo == null || r.Motivo.Contains(Motivo))
                                   && (Justificativa == null || r.Justificativa.Contains(Justificativa))
                                   && (De == null || r.DataOcorrencia >= De.Value.Date)
                                   && (Ate == null || r.DataOcorrencia <= Ate.Value.Date.AddDays(1).AddTicks(-1))
                                   ).OrderByDescending(r => r.DataOcorrencia);

                            var result = query.ToList();
                            var mapper = mapeamentoClasses.CreateMapper();

                            ExclusaoResult = mapper.Map<List<VW_Exclusao_ProdutoViewModel>>(result);

                            if (ExclusaoResult.Count > 0)
                            {
                                string? FilePathName = MakeExcel(ExclusaoResult);
                                DateTime? End = DateTime.Now;
                                string? objectQuery = query.ToQueryString();
                                int? IdUsuario = loggedUser.Id;
                                string? SamAccountName = loggedUser.Email;
                                string? Relatorio = "LOG DE EXCLUSAO DE PRODUTOS";

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
                                    ViewBag.Error = "Report not Inserted.";
                                    return View("Index");
                                }

                            }
                            else
                            {
                                ViewBag.Error = "No Data Found.";
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
            var LogRelatorioExclusaoProduto = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyRelatorioExclusaoProduto) ?? new List<RelatorioViewModel?>();

            var Relatorio = LogRelatorioExclusaoProduto.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogRelatorioExclusaoProduto.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }
        }

        public string MakeExcel(List<VW_Exclusao_ProdutoViewModel> ExclusaoResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Ferramentaria");
            dataTable.Columns.Add("IdProduto");
            dataTable.Columns.Add("Catalogo");
            dataTable.Columns.Add("Classe");
            dataTable.Columns.Add("Tipo");
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("RFM");
            dataTable.Columns.Add("AF/Serial");
            dataTable.Columns.Add("PAT");
            dataTable.Columns.Add("Suporte");
            dataTable.Columns.Add("Motivo");
            dataTable.Columns.Add("Justificativa");
            dataTable.Columns.Add("Data da Ocorrência", typeof(DateTime)); //14

            foreach (var item in ExclusaoResult)
            {
                var row = dataTable.NewRow();
                row["Ferramentaria"] = item.Ferramentaria;
                row["IdProduto"] = item.IdProduto;
                row["Catalogo"] = item.Catalogo; 
                row["Classe"] = item.Classe; 
                row["Tipo"] = item.Tipo;
                row["Codigo"] = item.Codigo;
                row["Produto"] = item.Produto;
                row["RFM"] = item.RFM;
                row["AF/Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["Suporte"] = item.Suporte;
                row["Motivo"] = item.Motivo;
                row["Justificativa"] = item.Justificativa;
                row["Data da Ocorrência"] = item.DataOcorrencia.HasValue == true ? (object)item.DataOcorrencia.Value : DBNull.Value; //14

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(14).Style.Numberformat.Format = "dd/MM/yyyy";
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

        // GET: LogExclusaoProduto/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogExclusaoProduto/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogExclusaoProduto/Create
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

        // GET: LogExclusaoProduto/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogExclusaoProduto/Edit/5
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

        // GET: LogExclusaoProduto/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogExclusaoProduto/Delete/5
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
