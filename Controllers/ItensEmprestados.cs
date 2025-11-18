using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using X.PagedList;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;


namespace FerramentariaTest.Controllers
{
    //public class GlobalDataRelatorio
    //{
    //    public static IPagedList<RelatorioViewModel> RelatorioPagedList { get; set; }
    //}

    public class ItensEmprestados : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thItensEmprestados.aspx";
        private MapperConfiguration mapeamentoClasses;
        private const string SessionKeyPaginationItensEmprestado = "PaginationEmprestados";
        //private static int? GlobalPagination;
        //private IPagedList<RelatorioViewModel> RelatorioPagedList;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public ItensEmprestados(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VW_Itens_EmprestadosViewModel, VW_Itens_Emprestados>();
                cfg.CreateMap<VW_Itens_Emprestados, VW_Itens_EmprestadosViewModel>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }


        // GET: ItensEmprestados
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
                //usuariofer.Pagina1 = "thItensEmprestados.aspx";
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


                            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];
                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

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


                            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];
                            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

                            List<RelatorioViewModel> RelatorioResult = new List<RelatorioViewModel>();

                            if (Processo != null && Registro != null)
                            {
                                RelatorioResult = _context.Relatorio
                                    .Where(r => r.RelatorioData == "ITENS EMPRESTADOS" && (Registro == 2 || r.Ativo == Registro) && (Processo == 4 || r.Processar == Processo))
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
                                        //int? IdUsuario = item.IdUsuario;

                                        //var usuarioOld = await _contextBS.VW_Usuario.FirstOrDefaultAsync(u => u.Id == IdUsuario);
                                        //if (usuarioOld != null)
                                        //{
                                        //    item.Nome = usuarioOld.Nome;
                                        //}

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
                                        TimeSpan? diff = null;

                                        if (inicio != null && conclusao != null)
                                        {
                                            diff = conclusao - inicio;
                                            item.timeDifference = diff.Value.ToString(@"hh\:mm\:ss");
                                        }

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

                                    GlobalValues.RelatorioViewModel = entrada;

                                    //GlobalPagination = Pagination;
                                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyPaginationItensEmprestado);
                                    httpContextAccessor.HttpContext?.Session.SetInt32(SessionKeyPaginationItensEmprestado, (int)Pagination);
                                    int pageSize = Pagination ?? 10;
                                    int pageNumber = 1;

                                    IPagedList<RelatorioViewModel> RelatorioPagedList = entrada.ToPagedList(pageNumber, pageSize);

                                    CombinedRelatorio combined = new CombinedRelatorio
                                    {
                                        RelatorioViewModel = RelatorioPagedList
                                    };

                                    return View("Index", combined);
                                }
                                else
                                {
                                    ViewBag.Error = "Result: 0 Found.";
                                    return View("Index");
                                }
                            }

                            ////GlobalDataRelatorio.RelatorioPagedList = RelatorioResult.ToPagedList(pageNumber, pageSize);

                            //ViewBag.usuariofer = usuariofer;
                            ViewBag.Processo = Processo;
                            ViewBag.Registro = Registro;

                            //CombinedRelatorio combined = new CombinedRelatorio
                            //{
                            //    RelatorioViewModel = RelatorioPagedList
                            //};

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
            int pageSize = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeyPaginationItensEmprestado) ?? 10; 
            int pageNumber = (page ?? 1);

            IPagedList<RelatorioViewModel> RelatorioPagedList = GlobalValues.RelatorioViewModel.ToPagedList(pageNumber, pageSize);

            CombinedRelatorio combined = new CombinedRelatorio
            {
                RelatorioViewModel = RelatorioPagedList
            };

            return View("Index", combined);
        }

        public ActionResult GetObservacao(int? id)
        {
            var Relatorio = GlobalValues.RelatorioViewModel.Where(r => r.Id == id).FirstOrDefault(); 

            if (Relatorio != null)
            {
                ViewBag.Query = Relatorio.Query;
                ViewBag.OpenObsModal = true;

                int pageSize = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeyPaginationItensEmprestado) ?? 10;
                int pageNumber = 1;

                IPagedList<RelatorioViewModel> RelatorioPagedList = GlobalValues.RelatorioViewModel.ToPagedList(pageNumber, pageSize);

                CombinedRelatorio combined = new CombinedRelatorio
                {
                    RelatorioViewModel = RelatorioPagedList
                };

                return View("Index", combined);
            }
            else
            {
                ViewBag.Error = "Result: Observacao Not Found.";
                return View("Index");
            }

        }

        public IActionResult Salvar(RelatorioSearch? RelatorioSearch)
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

                            if (RelatorioSearch != null)
                            {

                                List<VW_Itens_EmprestadosViewModel>? ItensEmprestadosResult = new List<VW_Itens_EmprestadosViewModel>();

                                DateTime? Start = DateTime.Now;

                                var query = _context.VW_Itens_Emprestados
                                   .Where(r =>
                                   (RelatorioSearch.Catalogo == null || r.Catalogo.Contains(RelatorioSearch.Catalogo))
                                   && (RelatorioSearch.Classe == null || r.Classe.Contains(RelatorioSearch.Classe))
                                   && (RelatorioSearch.Tipo == null || r.Tipo.Contains(RelatorioSearch.Tipo))
                                   && (RelatorioSearch.Codigo == null || r.Codigo.Contains(RelatorioSearch.Codigo))
                                   && (RelatorioSearch.Produto == null || r.Produto.Contains(RelatorioSearch.Produto))
                                   && (RelatorioSearch.CA == null || r.NumeroCA.Contains(RelatorioSearch.CA))
                                   && (RelatorioSearch.AF == null || r.AF.Contains(RelatorioSearch.AF))
                                   && (RelatorioSearch.PAT == null || r.PAT == RelatorioSearch.PAT)
                                   && (RelatorioSearch.Observacao == null || r.Observacao.Contains(RelatorioSearch.Observacao))
                                   && (RelatorioSearch.SetorOrigem == null || r.SetorOrigem.Contains(RelatorioSearch.SetorOrigem))
                                   && (RelatorioSearch.ChapaSolicitante == null || r.ChapaSolicitante.Contains(RelatorioSearch.ChapaSolicitante))
                                   && (RelatorioSearch.NomeSolicitante == null || r.NomeSolicitante.Contains(RelatorioSearch.NomeSolicitante))
                                   && (RelatorioSearch.ChapaLiberador == null || r.ChapaLiberador.Contains(RelatorioSearch.ChapaLiberador))
                                   && (RelatorioSearch.NomeLiberador == null || r.NomeLiberador.Contains(RelatorioSearch.NomeLiberador))
                                   && (RelatorioSearch.Balconista == null || r.Balconista.Contains(RelatorioSearch.Balconista))
                                   && (RelatorioSearch.Obra == null || r.Obra.Contains(RelatorioSearch.Obra))
                                   && (RelatorioSearch.DataEmprestimoDe == null || r.DataEmprestimo >= RelatorioSearch.DataEmprestimoDe.Value.Date)
                                   && (RelatorioSearch.DataEmprestimoAte == null || r.DataEmprestimo <= RelatorioSearch.DataEmprestimoAte.Value.Date.AddDays(1).AddTicks(-1))
                                   && (RelatorioSearch.DevolucaoDe == null || r.DataPrevistaDevolucao >= RelatorioSearch.DevolucaoDe.Value.Date)
                                   && (RelatorioSearch.DevolucaoAte == null || r.DataPrevistaDevolucao <= RelatorioSearch.DevolucaoAte.Value.Date.AddDays(1).AddTicks(-1))
                                   && (RelatorioSearch.VencimentoDe == null || r.DataVencimentoProduto >= RelatorioSearch.VencimentoDe.Value.Date)
                                   && (RelatorioSearch.VencimentoAte == null || r.DataVencimentoProduto <= RelatorioSearch.VencimentoAte.Value.Date.AddDays(1).AddTicks(-1))
                                   )
                                   .OrderByDescending(r => r.DataEmprestimo)
                                   .Select(r => new VW_Itens_EmprestadosViewModel
                                   {
                                       Catalogo = r.Catalogo,
                                       Classe = r.Classe,
                                       Tipo = r.Tipo,
                                       Codigo = r.Codigo,
                                       Quantidade = r.Quantidade,
                                       Produto = r.Produto,
                                       NumeroCA = r.NumeroCA,
                                       ValidadeCA = r.ValidadeCA,
                                       AF = r.AF,
                                       PAT = r.PAT,
                                       Observacao = r.Observacao,
                                       SetorOrigem = r.SetorOrigem,
                                       StatusSolicitante = r.StatusSolicitante,
                                       ChapaSolicitante = r.ChapaSolicitante,
                                       NomeSolicitante = r.NomeSolicitante,
                                       FuncaoSolicitante = r.FuncaoSolicitante,
                                       SecaoSolicitante = r.SecaoSolicitante,
                                       ChapaLiberador = r.ChapaLiberador,
                                       NomeLiberador = r.NomeLiberador,
                                       Balconista = r.Balconista,
                                       Obra = r.Obra,
                                       DataEmprestimo = r.DataEmprestimo,
                                       DataPrevistaDevolucao = r.DataPrevistaDevolucao,
                                       DataVencimentoProduto = r.DataVencimentoProduto
                                   });

                                ItensEmprestadosResult = query.ToList();

                                if (ItensEmprestadosResult.Count > 0)
                                {
                                    string? FilePathName = MakeExcel(ItensEmprestadosResult);
                                    DateTime? End = DateTime.Now;

                                    string? objectQuery = query.ToQueryString();
                                    int? IdUsuario = loggedUser.Id;
                                    string? SamAccountName = loggedUser.Email;
                                    string? Relatorio = "ITENS EMPRESTADOS";
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
                                        TempData["ShowSuccessAlert"] = true;
                                    }

                                    return RedirectToAction(nameof(Index));
                                }
                                else
                                {
                                    //DateTime? End = DateTime.Now;

                                    //string? objectQuery = query.ToQueryString();
                                    //int? IdUsuario = usuariofer.Id;
                                    //string? SamAccountName = usuariofer.SAMAccountName;
                                    //string? Relatorio = "ITENS EMPRESTADOS";
                                    //var InsertToRelatorio = new Relatorio
                                    //{
                                    //    SAMAccountName = SamAccountName,
                                    //    IdUsuario = IdUsuario,
                                    //    RelatorioData = Relatorio,
                                    //    Arquivo = "Result Not Found",
                                    //    ProcessoDataInicio = Start,
                                    //    ProcessoDataConclusao = End,
                                    //    Processar = 2,
                                    //    Query = objectQuery,
                                    //    Ativo = 1,
                                    //    DataRegistro = DateTime.Now
                                    //};

                                    //_context.Add(InsertToRelatorio);
                                    //await _context.SaveChangesAsync();


                                    TempData["ErrorMessage"] = "No Data Found";

                                    return RedirectToAction(nameof(Index));
                                }


                                //string? Arquivo = $"D:\\Ferramentaria\\Relatorio\\{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
                                //string? Arquivo = "C:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";         

                            }

                            return RedirectToAction(nameof(Index));



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
                return View(nameof(Index));
            }


        }

        public string MakeExcel(List<VW_Itens_EmprestadosViewModel> ItensEmprestadosResult)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Catalogo"); //1
            dataTable.Columns.Add("Classe"); //2
            dataTable.Columns.Add("Tipo"); //3
            dataTable.Columns.Add("Codigo"); //4
            dataTable.Columns.Add("Quantidade"); //5
            dataTable.Columns.Add("Produto"); //6
            dataTable.Columns.Add("NumeroCA"); //7
            dataTable.Columns.Add("ValidadeCA"); //8
            dataTable.Columns.Add("AF"); //9
            dataTable.Columns.Add("PAT"); //10
            dataTable.Columns.Add("Observação"); //11
            dataTable.Columns.Add("Setor de Origem"); //12
            dataTable.Columns.Add("Status do Solicitante"); //13
            dataTable.Columns.Add("Chapa do Solicitante"); //14
            dataTable.Columns.Add("Nome do Solicitante"); //15
            dataTable.Columns.Add("Função do Solicitante"); //16
            dataTable.Columns.Add("Seçãp do Solicitante"); //17
            dataTable.Columns.Add("Chapa do Liberador"); //18
            dataTable.Columns.Add("Nome do Liberador"); //19
            dataTable.Columns.Add("Balconista do Liberador"); //20
            dataTable.Columns.Add("Obra"); //21
            dataTable.Columns.Add("Data do Emprestimo", typeof(DateTime)); //22
            dataTable.Columns.Add("Data Prevista para Devolução", typeof(DateTime)); //23
            dataTable.Columns.Add("Data de Vencimento do Produto", typeof(DateTime)); //24

            foreach (var item in ItensEmprestadosResult)
            {
                var row = dataTable.NewRow();
                row["Catalogo"] = item.Catalogo; // Replace with the actual property name
                row["Classe"] = item.Classe; // Replace with the actual property name
                row["Tipo"] = item.Tipo;
                row["Codigo"] = item.Codigo;
                row["Quantidade"] = item.Quantidade;
                row["Produto"] = item.Produto;
                row["NumeroCA"] = item.NumeroCA;
                row["ValidadeCA"] = item.ValidadeCA;
                row["AF"] = item.AF;
                row["PAT"] = item.PAT;
                row["Observação"] = item.Observacao;
                row["Setor de Origem"] = item.SetorOrigem;
                row["Status do Solicitante"] = item.StatusSolicitante;
                row["Chapa do Solicitante"] = item.ChapaSolicitante;
                row["Nome do Solicitante"] = item.NomeSolicitante;
                row["Função do Solicitante"] = item.FuncaoSolicitante;
                row["Seçãp do Solicitante"] = item.SecaoSolicitante;
                row["Chapa do Liberador"] = item.ChapaLiberador;
                row["Nome do Liberador"] = item.NomeLiberador;
                row["Balconista do Liberador"] = item.Balconista;
                row["Obra"] = item.Obra;
                row["Data do Emprestimo"] = item.DataEmprestimo.HasValue ? (object)item.DataEmprestimo.Value : DBNull.Value;
                row["Data Prevista para Devolução"] = item.DataPrevistaDevolucao.HasValue ? (object)item.DataPrevistaDevolucao.Value : DBNull.Value;
                row["Data de Vencimento do Produto"] = item.DataVencimentoProduto.HasValue ? (object)item.DataVencimentoProduto.Value : DBNull.Value; 

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("ITENS EMPRESTADOS");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(22).Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Column(23).Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Column(24).Style.Numberformat.Format = "dd/MM/yyyy";
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


        // GET: ItensEmprestados/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ItensEmprestados/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ItensEmprestados/Create
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

    }
}
