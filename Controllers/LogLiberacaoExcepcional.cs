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
    public class LogLiberacaoExcepcional : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLiberacaoExcepcional.aspx";
        private static int? GlobalPagination;
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyLogLiberacaoExcepcional = "LogLiberacaoExcepional";
        //private static List<RelatorioViewModel?> _ListRelatorio = new List<RelatorioViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public LogLiberacaoExcepcional(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Historico_LiberacaoExcepcionalViewModel, Historico_LiberacaoExcepcional>();
                cfg.CreateMap<Historico_LiberacaoExcepcional, Historico_LiberacaoExcepcionalViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }


        // GET: LogLiberacaoExcepcional
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

        public IActionResult GetLog(int? Processo, int? Registro,int? Pagination)
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

                            List<RelatorioViewModel> RelatorioResult = new List<RelatorioViewModel>();

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio
                                    .Where(r => r.RelatorioData == "LIBERACAO_EXCEPCIONAL" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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

                                    foreach (var item in RelatorioResult)
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyLogLiberacaoExcepcional);
                                    HttpContext.Session.SetObject(SessionKeyLogLiberacaoExcepcional, entrada);
                                    //_ListRelatorio = new List<RelatorioViewModel?>();
                                    //_ListRelatorio = RelatorioResult;
                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);
                                    return View("Index", RelatorioPagedList);

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

            var LogLiberacaoExcepionalModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogLiberacaoExcepcional) ?? new List<RelatorioViewModel?>();

            IPagedList<RelatorioViewModel> RelatorioPagedList = LogLiberacaoExcepionalModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult GetObservacao(int? id)
        {
            var LogLiberacaoExcepionalModel = HttpContext.Session.GetObject<List<RelatorioViewModel?>>(SessionKeyLogLiberacaoExcepcional) ?? new List<RelatorioViewModel?>();

            var Relatorio = LogLiberacaoExcepionalModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = LogLiberacaoExcepionalModel.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
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

        public IActionResult Salvar(DateTime? Inicial, DateTime? Final, string? Balconista, string? Responsavel, List<string?> Setor)
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



                            if (Inicial != null && Final != null)
                            {
                                List<Historico_LiberacaoExcepcionalViewModel> HistoricoLiberacaoResult = new List<Historico_LiberacaoExcepcionalViewModel>();
                                DateTime? Start = DateTime.Now;

                                List<string?> setorList = Setor;

                                var query = _context.Historico_LiberacaoExcepcional
                                .Where(i =>
                                (i.Data_Emprestimo >= Inicial.Value.Date)
                                && (i.Data_Emprestimo <= Final.Value.Date.AddDays(1).AddTicks(-1))
                                && (setorList.Count == 0 || setorList.Contains(i.Setor_Origem))
                                && (Balconista == null || i.Balconista_Emprestimo.Contains(Balconista))
                                && (Responsavel == null || i.Liberacao_Excepcional.Contains(Responsavel))
                                ).OrderByDescending(r => r.Data_Emprestimo);

                                var result = query.ToList();
                                var mapper = mapeamentoClasses.CreateMapper();
                                HistoricoLiberacaoResult = mapper.Map<List<Historico_LiberacaoExcepcionalViewModel>>(result);

                                if (HistoricoLiberacaoResult.Count > 0)
                                {
                                    string? FilePathName = MakeExcel(HistoricoLiberacaoResult);
                                    DateTime? End = DateTime.Now;
                                    string? objectQuery = query.ToQueryString();
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "LIBERACAO_EXCEPCIONAL";
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
                                ViewBag.Error = "Inicial and Final is obrigatorio.";
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

        public string MakeExcel(List<Historico_LiberacaoExcepcionalViewModel> HistoricoLiberacaoResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Catálogo");
            dataTable.Columns.Add("Código");
            dataTable.Columns.Add("Produto");
            dataTable.Columns.Add("Quantidade");
            dataTable.Columns.Add("AF");
            dataTable.Columns.Add("PAT");
            dataTable.Columns.Add("Observação");
            dataTable.Columns.Add("Setor Origem");
            dataTable.Columns.Add("Chapa do Solicitante");
            dataTable.Columns.Add("Nome do Solicitante");
            dataTable.Columns.Add("Função do Solicitante");
            dataTable.Columns.Add("Seção do Solicitante");
            dataTable.Columns.Add("Chapa do Liberador");
            dataTable.Columns.Add("Nome do Liberador");
            dataTable.Columns.Add("Função do Liberador");
            dataTable.Columns.Add("Seção do Liberador");
            dataTable.Columns.Add("Balconista do Empréstimo");
            dataTable.Columns.Add("Obra");
            dataTable.Columns.Add("Data do Empréstimo", typeof(DateTime)); //19
            dataTable.Columns.Add("Liberação excepcional");

            foreach (var item in HistoricoLiberacaoResult)
            {
                var row = dataTable.NewRow();
                row["Catálogo"] = item.Catalogo;
                row["Código"] = item.Codigo;
                row["Produto"] = item.Produto;
                row["Quantidade"] = item.Quantidade;
                row["AF"] = item.AF;
                row["PAT"] = item.PAT;
                row["Observação"] = item.Observacao;
                row["Setor Origem"] = item.Setor_Origem;
                row["Chapa do Solicitante"] = item.Chapa_Solicitante;
                row["Nome do Solicitante"] = item.Nome_Solicitante;
                row["Função do Solicitante"] = item.Funcao_Solicitante;
                row["Seção do Solicitante"] = item.Secao_Solicitante;
                row["Chapa do Liberador"] = item.Chapa_Liberador;
                row["Nome do Liberador"] = item.Nome_Liberador;
                row["Função do Liberador"] = item.Funcao_Liberador;
                row["Seção do Liberador"] = item.Secao_Liberador;
                row["Balconista do Empréstimo"] = item.Balconista_Emprestimo;
                row["Obra"] = item.Obra;
                row["Data do Empréstimo"] = item.Data_Emprestimo.HasValue == true ? item.Data_Emprestimo.Value : DBNull.Value;
                row["Liberação excepcional"] = item.Liberacao_Excepcional;

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(19).Style.Numberformat.Format = "dd/MM/yyyy";
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





        // GET: LogLiberacaoExcepcional/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogLiberacaoExcepcional/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogLiberacaoExcepcional/Create
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

        // GET: LogLiberacaoExcepcional/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogLiberacaoExcepcional/Edit/5
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

        // GET: LogLiberacaoExcepcional/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogLiberacaoExcepcional/Delete/5
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
