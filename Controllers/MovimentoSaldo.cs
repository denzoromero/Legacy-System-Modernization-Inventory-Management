using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;
using System.Linq;
using X.PagedList;
using static NuGet.Packaging.PackagingConstants;

namespace FerramentariaTest.Controllers
{
    public class MovimentoSaldo : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLogProdutos.aspx";
        private static int? GlobalPagination;

        private const string SessionKeyListMovimentoSaldo = "ListMovimentoSaldo";

        private const string SessionKeyListCatalog = "CatalogList";

        private const string SessionKeyLoggedUsers = "SelectUsers";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public MovimentoSaldo(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: MovimentoSaldo
        public IActionResult Index(int? page, int? Usuario,DateTime? De, DateTime? Ate)
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


                            //var usuarios = from usuario in _contextBS.VW_Usuario_New
                            //               where usuario.Ativo == 1 &&
                            //                     (
                            //                         (usuario.CodSituacao == "F" &&
                            //                          (from func in _contextBS.Funcionario
                            //                           where func.FimProgFerias1 <= DateTime.Now &&
                            //                                 func.Chapa == usuario.Chapa &&
                            //                                 func.CodColigada == usuario.CodColigada
                            //                           select func.CodSituacao).FirstOrDefault() != null) ||
                            //                         usuario.CodSituacao == "A"
                            //                     ) &&
                            //                     (from acesso in _contextBS.Acesso
                            //                      join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                            //                      where acesso.IdModulo == 6
                            //                      select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
                            //               orderby usuario.Nome
                            //               select usuario;

                            var permittedUserIds = (
                                                    from acesso in _contextBS.Acesso
                                                    join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                                                    where acesso.IdModulo == 6
                                                    select permissao.IdUsuario
                                                ).ToList();

                            List<simpleUserModel>? usuarios = (
                                                   from usuario in _contextBS.Usuario

                                                   join funcionario in _contextBS.Funcionario
                                                       on new { Chapa = (string)usuario.Chapa, CodColigada = (int)usuario.CodColigada }
                                                       equals new { Chapa = (string)funcionario.Chapa, CodColigada = (int)funcionario.CodColigada }
                                                       into funcionarioJoin
                                                   from funcionario in funcionarioJoin.DefaultIfEmpty()

                                                   where usuario.Ativo == 1 &&
                                                         (
                                                             (funcionario.CodSituacao == "F" &&
                                                              funcionario.Chapa != null &&
                                                              funcionario.FimProgFerias1 <= DateTime.Now &&
                                                              funcionario.CodSituacao != null) ||
                                                             funcionario.CodSituacao == "A"
                                                         ) &&
                                                          permittedUserIds.Contains(usuario.Id.Value)
                                                   orderby funcionario.Nome
                                                   select new simpleUserModel
                                                   {
                                                        Id =usuario.Id,
                                                       Chapa = usuario.Chapa,
                                                       CodColigada = usuario.CodColigada,
                                                       Nome = funcionario.Nome
                                                   }
                                               ).GroupBy(u => u.Id)
                                            .Select(g => g.First()).ToList();

                            HttpContext.Session.SetObject(SessionKeyLoggedUsers, usuarios);

                            ViewBag.UsuarioRelatorio = usuarios;

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

        public ActionResult GetProduto(int? CategoriaClasse, int? IdCategoria, int? Tipo, string? Codigo, string? Item, string? AF, int? PAT, string? NumeroSerie)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();

            ViewBag.UsuarioRelatorio = usuarios;

            try
            {
                ProdutoFilter produtoFilter = new ProdutoFilter();
                produtoFilter.Catalogo = CategoriaClasse;
                produtoFilter.Classe = IdCategoria;
                produtoFilter.Tipo = Tipo;
                produtoFilter.Codigo = Codigo;
                produtoFilter.Item = Item;
                produtoFilter.AF = AF;
                produtoFilter.PAT = PAT;
                produtoFilter.NumeroSerie = NumeroSerie;

                List<ProdutoList?> ProdutoList = new List<ProdutoList?>();
                ProdutoList = searches.FindProduto(produtoFilter);

                if (ProdutoList.Count > 0)
                {
                    GlobalValues.ProdutoList = ProdutoList;

                    int pageSize = 10;
                    int pageNumber = 1;

                    IPagedList<ProdutoList> ProdutoListPagedList = ProdutoList.ToPagedList(pageNumber, pageSize);
                    ViewBag.ProdutoList = ProdutoListPagedList;
                }
                else
                {
                    ViewBag.Error = "No Produto Found";
                    return View("Index");
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                return View("Index", ex);
            }
        }

        public ActionResult GetCatalogo(int? CategoriaClasse, int? IdCategoria, int? Tipo, string? Codigo, string? Item)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();

            ViewBag.UsuarioRelatorio = usuarios;

            try
            {
                CatalogoSearchModel? filters = new CatalogoSearchModel()
                {
                    CategoriaClasse = CategoriaClasse,
                    Id = Tipo,
                    IdCategoria = IdCategoria,
                    Codigo = Codigo,
                    Descricao = Item
                };

                List<CatalogoViewModel>? Result = searches.SearchCatalogo(filters) ?? new List<CatalogoViewModel>();
                if (Result.Count > 0)
                {

                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListCatalog);
                    HttpContext.Session.SetObject(SessionKeyListCatalog, Result);

                    ViewBag.CatalogList = Result;
                    return View("Index");
                }
                else
                {
                    ViewBag.Error = "o item que você está procurando não foi encontrado.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return View("Index", ex);
            }
        }

        public ActionResult SelectCatalog(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();

            ViewBag.UsuarioRelatorio = usuarios;

            List<CatalogoViewModel>? itemsToExport = HttpContext.Session.GetObject<List<CatalogoViewModel>>(SessionKeyListCatalog) ?? new List<CatalogoViewModel>();

            ViewBag.SelectedCatalog = itemsToExport.FirstOrDefault(i => i.Id == id);

            return View(nameof(Index));
        }


        public ActionResult SelectProduto(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            try
            {
                ViewBag.SelectedProduto = GlobalValues.ProdutoList.FirstOrDefault(i => i.Id == id);
                GlobalValues.ClearList(GlobalValues.ProdutoList);

                return View("Index");
            }
            catch (Exception ex)
            {
                return View("Index", ex);
            }

        }

        public IActionResult GetLog(int? page, int? Usuario, DateTime? De, DateTime? Ate,string? RFM, int? Pagination, int? IdProdutoSelected, int? IdCatalogoSelected)
        {
            //var usuarios = from usuario in _contextBS.VW_Usuario_New
            //               where usuario.Ativo == 1 &&
            //                     (
            //                         (usuario.CodSituacao == "F" &&
            //                          (from func in _contextBS.Funcionario
            //                           where func.FimProgFerias1 <= DateTime.Now &&
            //                                 func.Chapa == usuario.Chapa &&
            //                                 func.CodColigada == usuario.CodColigada
            //                           select func.CodSituacao).FirstOrDefault() != null) ||
            //                         usuario.CodSituacao == "A"
            //                     ) &&
            //                     (from acesso in _contextBS.Acesso
            //                      join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
            //                      where acesso.IdModulo == 6
            //                      select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
            //               orderby usuario.Nome
            //               select usuario;

            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();

            ViewBag.UsuarioRelatorio = usuarios;

            //IEnumerable<LogProdutoViewModel> logProdutoResult = null;
            List<LogProdutoViewModel> logProdutoResult = new List<LogProdutoViewModel>();
            GlobalPagination = Pagination;

            if (De != null && De != DateTime.MinValue && Ate != null && Ate != DateTime.MinValue)
            {
                DateTime TransactionDe = De.Value;
                DateTime TransactionAte = Ate.Value;

                logProdutoResult = (from logPro in _context.LogProduto
                                   join produto in _context.Produto on logPro.IdProduto equals produto.Id
                                   join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                   join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
                                   join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                   //join usuario in _contextBS.VW_Usuario on logPro.IdUsuario equals usuario.Id
                                   //join func in _contextBS.VW_Funcionario_Registro_Atual on new { usuario.CodColigada, usuario.Chapa } equals new { func.CodColigada, func.Chapa }
                                   where logPro.QuantidadeDe != logPro.QuantidadePara
                                   && (Usuario == null || logPro.IdUsuario == Usuario)
                                   && (RFM == null || logPro.RfmDe == RFM || logPro.RfmPara == RFM || produto.RFM == RFM)
                                   && (IdProdutoSelected == null || logPro.IdProduto == IdProdutoSelected)
                                   && (IdCatalogoSelected == null || catalogo.Id == IdCatalogoSelected)
                                   && logPro.DataRegistro >= TransactionDe 
                                   && logPro.DataRegistro <= TransactionAte.AddDays(1).AddTicks(-1) 
                                   && produto.IdFerramentaria != 17
                                   orderby logPro.DataRegistro, catalogo.Codigo
                                   select new LogProdutoViewModel
                                   {
                                       IdProduto = logPro.IdProduto,
                                       Ferramentaria = ferramentaria.Nome,
                                       Catalogo = categoriaPai.Classe == 1 ? "FERRAMENTA" : (categoriaPai.Classe == 2 ? "EPI" : "CONSUMIVEIS"),
                                       Classe = categoria.Nome,
                                       Tipo = categoriaPai.Nome,
                                       Codigo = catalogo.Codigo,
                                       Item = catalogo.Nome,
                                       RfmPara = !string.IsNullOrEmpty(logPro.RfmPara) ? logPro.RfmPara.Replace(";", "") : string.Empty,
                                       RfmDe = !string.IsNullOrEmpty(logPro.RfmDe) ? logPro.RfmDe.Replace(";", "") : string.Empty,
                                       RfmAtual = !string.IsNullOrEmpty(produto.RFM) ? produto.RFM.Replace(";", "") : string.Empty,
                                       Observacao = ("DE:" + (logPro.ObservacaoDe ?? "") + " PARA: " + (logPro.ObservacaoPara ?? "")).Replace(";", ""),
                                       Af = produto.AF,
                                       Pat = produto.PAT,
                                       SaldoDe = logPro.QuantidadeDe,
                                       SaldoPara = logPro.QuantidadePara,
                                       IdUsuario = logPro.IdUsuario,
                                       //Usuario = func.Nome,
                                       Acao = logPro.Acao == 1 ? "Inseriu" : "Editou",
                                       DataOcorrencia = logPro.DataRegistro
                                   }).ToList();

                if (logProdutoResult.Count > 0)
                {
                    List<int?> distinctUsuario = new List<int?>();
                    List<VW_Usuario_New> ListUsuario = new List<VW_Usuario_New>();
                    Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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

                    List<LogProdutoViewModel>? entrada = (from logprod in logProdutoResult
                                                                 join user in result on logprod.IdUsuario equals user.Id
                                                                 select new LogProdutoViewModel
                                                                 {
                                                                     IdProduto = logprod.IdProduto,
                                                                     Ferramentaria = logprod.Ferramentaria,
                                                                     Catalogo = logprod.Catalogo,
                                                                     Classe = logprod.Classe,
                                                                     Tipo = logprod.Tipo,
                                                                     Codigo = logprod.Codigo,
                                                                     Item = logprod.Item,
                                                                     RfmPara = logprod.RfmPara,
                                                                     RfmDe = logprod.RfmDe,
                                                                     RfmAtual = logprod.RfmAtual,
                                                                     Observacao = logprod.Observacao,
                                                                     Af = logprod.Af,
                                                                     Pat = logprod.Pat,
                                                                     SaldoDe = logprod.SaldoDe,
                                                                     SaldoPara = logprod.SaldoPara,
                                                                     IdUsuario = logprod.IdUsuario,
                                                                     Usuario = user.Nome,
                                                                     //Usuario = func.Nome,
                                                                     Acao = logprod.Acao,
                                                                     DataOcorrencia = logprod.DataOcorrencia
                                                                 }).ToList();


                    //distinctUsuario = logProdutoResult.Select(x => x.IdUsuario).Distinct().ToList();
                    //ListUsuario = searches.GetDistinctUser(distinctUsuario);

                    //foreach (var produto in logProdutoResult)
                    //{
                    //    var UserDetails = ListUsuario.FirstOrDefault(i => i.Id == produto.IdUsuario);
                    //    produto.Usuario = UserDetails.Nome;
                    //}

                    //int pageSize = 20;
                    //int pageNumber = (page ?? 1);

                    int pageSize = GlobalPagination ?? 10;
                    int pageNumber = 1;

                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListMovimentoSaldo);
                    HttpContext.Session.SetObject(SessionKeyListMovimentoSaldo, entrada);

                    //GlobalValues.LogProdutoViewModel = logProdutoResult;

                    IPagedList<LogProdutoViewModel> LogProdutoPagedList = entrada.ToPagedList(pageNumber, pageSize);

                    ViewBag.Ate = Ate;
                    ViewBag.De = De;
                    ViewBag.Usuario = Usuario;

                    return View("Index", LogProdutoPagedList);
                }
                else
                {
                    ViewBag.Error = "No Data Found";
                    return View("Index");
                }

            }
            else
            {
                ViewBag.Error = "Select Dates";
                return View("Index");
            }
   
        }

        public ActionResult TestPage(int? page)
        {
            List<LogProdutoViewModel>? ListCatalogoModel = HttpContext.Session.GetObject<List<LogProdutoViewModel>>(SessionKeyListMovimentoSaldo) ?? new List<LogProdutoViewModel>();

            //var usuarios = from usuario in _contextBS.VW_Usuario_New
            //               where usuario.Ativo == 1 &&
            //                     (
            //                         (usuario.CodSituacao == "F" &&
            //                          (from func in _contextBS.Funcionario
            //                           where func.FimProgFerias1 <= DateTime.Now &&
            //                                 func.Chapa == usuario.Chapa &&
            //                                 func.CodColigada == usuario.CodColigada
            //                           select func.CodSituacao).FirstOrDefault() != null) ||
            //                         usuario.CodSituacao == "A"
            //                     ) &&
            //                     (from acesso in _contextBS.Acesso
            //                      join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
            //                      where acesso.IdModulo == 6
            //                      select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
            //               orderby usuario.Nome
            //               select usuario;

            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();

            ViewBag.UsuarioRelatorio = usuarios;

            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            IPagedList<LogProdutoViewModel> LogProdutoPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

            return View("Index", LogProdutoPagedList);
        }

        public ActionResult ExportToExcel()
        {
            List<LogProdutoViewModel>? itemsToExport = HttpContext.Session.GetObject<List<LogProdutoViewModel>>(SessionKeyListMovimentoSaldo) ?? new List<LogProdutoViewModel>();

            if (itemsToExport.Count > 0)
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("IdProduto"); //1
                dataTable.Columns.Add("Ferramentaria"); //2
                dataTable.Columns.Add("Catalogo"); //3
                dataTable.Columns.Add("Classe"); //4
                dataTable.Columns.Add("Tipo"); //5
                dataTable.Columns.Add("Codigo"); //6
                dataTable.Columns.Add("Item"); //7
                dataTable.Columns.Add("RFM Para"); //8
                dataTable.Columns.Add("RFM De"); //9
                dataTable.Columns.Add("RFM Atual"); //10
                dataTable.Columns.Add("Observacao"); //11
                dataTable.Columns.Add("AF"); //12
                dataTable.Columns.Add("PAT"); //13
                dataTable.Columns.Add("Saldo De"); //14
                dataTable.Columns.Add("Saldo Para"); //15
                dataTable.Columns.Add("Usuario"); //16
                dataTable.Columns.Add("Acao"); //17
                dataTable.Columns.Add("Data Ocorrencia", typeof(DateTime)); //18

                foreach (LogProdutoViewModel item in itemsToExport)
                {
                    var row = dataTable.NewRow();
                    row["IdProduto"] = item.IdProduto; // Replace with the actual property name
                    row["Ferramentaria"] = item.Ferramentaria; // Replace with the actual property name
                    row["Catalogo"] = item.Catalogo;
                    row["Classe"] = item.Classe;
                    row["Tipo"] = item.Tipo;
                    row["Codigo"] = item.Codigo;
                    row["Item"] = item.Item;
                    row["RFM Para"] = item.RfmPara;
                    row["RFM De"] = item.RfmDe;
                    row["RFM Atual"] = item.RfmAtual;
                    row["Observacao"] = item.Observacao;
                    row["AF"] = item.Af;
                    row["PAT"] = item.Pat;
                    row["Saldo De"] = item.SaldoDe;
                    row["Saldo Para"] = item.SaldoPara;
                    row["Usuario"] = item.Usuario;
                    row["Acao"] = item.Acao;
                    row["Data Ocorrencia"] = item.DataOcorrencia.HasValue ? (object)item.DataOcorrencia.Value : DBNull.Value;

                    dataTable.Rows.Add(row);
                }


                using (var package = new ExcelPackage())
                {
                        var worksheet = package.Workbook.Worksheets.Add("Movimento de Saldo");
                        worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                        worksheet.Cells.AutoFitColumns();
                        worksheet.Column(18).Style.Numberformat.Format = "dd/MM/yyyy";

                    using (var stream = new MemoryStream())
                    {
                        package.SaveAs(stream);
                        stream.Position = 0;
                        string excelName = $"MovimentoDeSaldo.xlsx";

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                    }
                }



                //return View(nameof(Index));
            }
            else
            {
                ViewBag.Error = "Nenhum dado para exportar.";
                return View(nameof(Index));
            }
        }



        // GET: MovimentoSaldo/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MovimentoSaldo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MovimentoSaldo/Create
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

        // GET: MovimentoSaldo/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MovimentoSaldo/Edit/5
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

        // GET: MovimentoSaldo/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MovimentoSaldo/Delete/5
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
