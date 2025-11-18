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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Data;

namespace FerramentariaTest.Controllers
{
    public class LogEntradaSaida : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLogEntradaSaida.aspx";
        private static int? GlobalPagination;
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyRelatorioEntradaSaida = "RelatorioEntradaSaida";
        //private static List<Relatorio_LogEntradaSaidaViewModel?> _ListRelatorioEntradaSaida = new List<Relatorio_LogEntradaSaidaViewModel>();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public LogEntradaSaida(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_EntradaSaidaViewModel, VW_EntradaSaida>();
                cfg.CreateMap<VW_EntradaSaida, VW_EntradaSaidaViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: LogEntradaSaida
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

        public IActionResult GetLog(int? page, int? Processo, int? Registro,int? Pagination)
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

                            List<Relatorio_LogEntradaSaidaViewModel>? RelatorioResult = null;

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio_LogEntradaSaida
                                    .Where(r => (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
                                    .OrderByDescending(r => r.DataRegistro)
                                    .Select(r => new Relatorio_LogEntradaSaidaViewModel
                                    {
                                        Id = r.Id,
                                        IdUsuario = r.IdUsuario,
                                        Arquivo = r.Arquivo,
                                        Processar = r.Processar,
                                        Query = r.Query,
                                        ProcessoDataInicio = r.ProcessoDataInicio,
                                        ProcessoDataConclusao = r.ProcessoDataConclusao,
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

                                    List<Relatorio_LogEntradaSaidaViewModel>? entrada = (from relatorio in RelatorioResult
                                                                         join user in result on relatorio.IdUsuario equals user.Id
                                                                         select new Relatorio_LogEntradaSaidaViewModel
                                                                         {
                                                                             Id = relatorio.Id,
                                                                             IdUsuario = relatorio.IdUsuario,
                                                                             Arquivo = relatorio.Arquivo,
                                                                             Processar = relatorio.Processar,
                                                                             Query = relatorio.Query,
                                                                             ProcessoDataInicio = relatorio.ProcessoDataInicio,
                                                                             ProcessoDataConclusao = relatorio.ProcessoDataConclusao,
                                                                             Ativo = relatorio.Ativo,
                                                                             DataRegistro = relatorio.DataRegistro,
                                                                             Nome = user.Nome,
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

                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyRelatorioEntradaSaida);
                                    HttpContext.Session.SetObject(SessionKeyRelatorioEntradaSaida, entrada);

                                    //_ListRelatorioEntradaSaida = new List<Relatorio_LogEntradaSaidaViewModel?>();
                                    //_ListRelatorioEntradaSaida = RelatorioResult;
                                    IPagedList<Relatorio_LogEntradaSaidaViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);
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

            var LogRelatorioEntradaSaidaModel = HttpContext.Session.GetObject<List<Relatorio_LogEntradaSaidaViewModel?>>(SessionKeyRelatorioEntradaSaida) ?? new List<Relatorio_LogEntradaSaidaViewModel?>();

            IPagedList<Relatorio_LogEntradaSaidaViewModel> RelatorioPagedList = LogRelatorioEntradaSaidaModel.ToPagedList(pageNumber, pageSize);

            return View("Index", RelatorioPagedList);
        }

        public ActionResult GetObservacao(int? id)
        {
            var LogRelatorioEntradaSaidaModel = HttpContext.Session.GetObject<List<Relatorio_LogEntradaSaidaViewModel?>>(SessionKeyRelatorioEntradaSaida) ?? new List<Relatorio_LogEntradaSaidaViewModel?>();

            var Relatorio = LogRelatorioEntradaSaidaModel.Where(r => r.Id == id).FirstOrDefault();

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = GlobalPagination ?? 10;
                int pageNumber = 1;

                IPagedList<Relatorio_LogEntradaSaidaViewModel> RelatorioPagedList = LogRelatorioEntradaSaidaModel.ToPagedList(pageNumber, pageSize);

                return View("Index", RelatorioPagedList);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }
        }



        public IActionResult Salvar(string? Ferramentaria, string? Codigo, string? Item, string? RFM, string? Observacao, int? Movimento, string? Usuario, DateTime? Inicial, DateTime? Final)
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


                            List<VW_EntradaSaidaViewModel> EntradaSaidaResult = new List<VW_EntradaSaidaViewModel>();
                            DateTime? Start = DateTime.Now;

                            var query = _context.VW_EntradaSaida.Where(r =>
                                        (Ferramentaria == null || r.Ferramentaria.Contains(Ferramentaria))
                                              && (Codigo == null || r.Codigo.Contains(Codigo))
                                              //&& (Item == null || r.Item.Contains(Item))
                                              && (Item == null || EF.Functions.Like(r.Item, $"%{Item}%"))
                                              && (RFM == null || r.RFM.Contains(RFM))
                                              && (Observacao == null || r.Observacao.Contains(Observacao))
                                              && (Movimento == null || r.Movimento == Movimento)
                                              && (Usuario == null || r.Usuario.Contains(Usuario))
                                              && (Inicial == null || r.DataOcorrencia >= Inicial.Value.Date)
                                              && (Final == null || r.DataOcorrencia <= Final.Value.Date.AddDays(1).AddTicks(-1))
                                            ).OrderByDescending(r => r.DataOcorrencia);

                            var result = query.ToList();
                            var mapper = mapeamentoClasses.CreateMapper();
                            EntradaSaidaResult = mapper.Map<List<VW_EntradaSaidaViewModel>>(result);

                            if (EntradaSaidaResult.Count > 0)
                            {
                                string? FilePathName = MakeExcel(EntradaSaidaResult);
                                DateTime? End = DateTime.Now;
                                string? objectQuery = query.ToQueryString();
                                int? IdUsuario = loggedUser.Id;
                                string? SamAccountName = loggedUser.Email;
                                string? Relatorio = "LOG DE EXCLUSAO DE PRODUTOS";

                                var InsertToRelatorioLogEntradaSaida = new Relatorio_LogEntradaSaida
                                {
                                    IdUsuario = IdUsuario,
                                    Arquivo = FilePathName,
                                    ProcessoDataInicio = Start,
                                    ProcessoDataConclusao = End,
                                    Processar = 2,
                                    Query = objectQuery,
                                    Ativo = 1,
                                    DataRegistro = DateTime.Now
                                };
                                _context.Add(InsertToRelatorioLogEntradaSaida);
                                _context.SaveChanges();

                                if (InsertToRelatorioLogEntradaSaida.Id != null)
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

        public string MakeExcel(List<VW_EntradaSaidaViewModel> EntradaSaidaResult)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Ferramentaria");
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Item");
            dataTable.Columns.Add("RFM");
            dataTable.Columns.Add("Observacao");
            dataTable.Columns.Add("Movimento");
            dataTable.Columns.Add("Usuario");
            dataTable.Columns.Add("DataOcorrencia", typeof(DateTime)); //8

            foreach (var item in EntradaSaidaResult)
            {
                var row = dataTable.NewRow();
                row["Ferramentaria"] = item.Ferramentaria;
                row["Codigo"] = item.Codigo;
                row["Item"] = item.Item;
                row["RFM"] = item.RFM;
                row["Observacao"] = item.Observacao;
                row["Movimento"] = item.Movimento;
                row["Usuario"] = item.Usuario;
                row["DataOcorrencia"] = item.DataOcorrencia.HasValue == true ? item.DataOcorrencia.Value : DBNull.Value;

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(8).Style.Numberformat.Format = "dd/MM/yyyy";
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

            var Relatorio = await _context.Relatorio_LogEntradaSaida.Where(t => t.Id == id).FirstOrDefaultAsync();
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


        // GET: LogEntradaSaida/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LogEntradaSaida/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LogEntradaSaida/Create
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

        // GET: LogEntradaSaida/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LogEntradaSaida/Edit/5
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

        // GET: LogEntradaSaida/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LogEntradaSaida/Delete/5
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
