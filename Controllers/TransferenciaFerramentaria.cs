using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data;
using X.PagedList;

namespace FerramentariaTest.Controllers
{
    public class TransferenciaFerramentaria : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLogTransferencia.aspx";
        private static int? GlobalPagination;

        private const string SessionKeyListTransferencia = "ListTransferencia";

        private const string SessionKeyListCatalog = "CatalogList";
        private const string SessionKeyLoggedUsers = "SelectUsers";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public TransferenciaFerramentaria(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        //GET: TransferenciaFerramentaria
        public IActionResult Index(int? page, int? Usuario, DateTime? De, DateTime? Ate)
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
                                                       Id = usuario.Id,
                                                       Chapa = usuario.Chapa,
                                                       CodColigada = usuario.CodColigada,
                                                       Nome = funcionario.Nome
                                                   }
                                               ).GroupBy(u => u.Id)
                                            .Select(g => g.First()).ToList();

                            HttpContext.Session.SetObject(SessionKeyLoggedUsers, usuarios);

                            ViewBag.UsuarioRelatorio = usuarios;


                            //var usuarios = searches.OnLoadUsers();
                            //ViewBag.UsuarioRelatorio = usuarios;

                            var FerramentariaList = searches.OnLoadFerramentaria();
                            ViewBag.FerramentariaList = FerramentariaList;

                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);


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

            

                return View();
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

        public IActionResult GetLog(int? page, int? Usuario, int? Setor, int? Para, DateTime? De, DateTime? Ate, int? IdProdutoSelected, int? Pagination, int? IdCatalogoSelected)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            List<HistoricoTransferenciaViewModel>? HistoricoTransferenciaResult = null;

            try
            {
                if (De != null && De != DateTime.MinValue && Ate != null && Ate != DateTime.MinValue)
                {
                    DateTime TransactionDe = De.Value;
                    DateTime TransactionAte = Ate.Value;

                    HistoricoTransferenciaResult = (from log in _context.HistoricoTransferencia
                                                    join produto in _context.Produto on log.IdProduto equals produto.Id
                                                    join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                    join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                    join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
                                                    join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                    join ferramentariaOrigem in _context.Ferramentaria on log.IdFerramentariaOrigem equals ferramentariaOrigem.Id
                                                    join ferramentariaDestino in _context.Ferramentaria on log.IdFerramentariaDestino equals ferramentariaDestino.Id
                                                    // join func in _context.Func on log.IdUsuario equals func.Id // Uncomment this line if Func is the entity for Usuario
                                                    where (Usuario == null || log.IdUsuario == Usuario)
                                                         && (Setor == null || log.IdFerramentariaOrigem == Setor)
                                                         && (Para == null || log.IdFerramentariaDestino == Para)
                                                         && (IdProdutoSelected == null || log.IdProduto == IdProdutoSelected)
                                                         && (IdCatalogoSelected == null || catalogo.Id == IdCatalogoSelected)
                                                         && log.DataOcorrencia >= TransactionDe.Date
                                                         && log.DataOcorrencia <= TransactionAte.Date.AddDays(1).AddTicks(-1)
                                                              && produto.IdFerramentaria != 17
                                                    orderby log.DataOcorrencia
                                                    select new HistoricoTransferenciaViewModel
                                                    {
                                                        IdProduto = log.IdProduto,
                                                        Ferramentaria = ferramentaria.Nome,
                                                        Catalogo = categoriaPai.Classe == 1 ? "FERRAMENTA" : (categoriaPai.Classe == 2 ? "EPI" : "CONSUMIVEIS"),
                                                        Classe = categoria.Nome,
                                                        Tipo = categoriaPai.Nome,
                                                        Codigo = catalogo.Codigo,
                                                        Item = catalogo.Nome,
                                                        RFM = produto.RFM,
                                                        AF = produto.AF,
                                                        PAT = produto.PAT,
                                                        IdUsuario = log.IdUsuario,
                                                        Quantidade = log.Quantidade,
                                                        FerramentariaOrigem = ferramentariaOrigem.Nome,
                                                        FerramentariaDestino = ferramentariaDestino.Nome,
                                                        DataOcorrencia = log.DataOcorrencia,
                                                        Documento = log.Documento
                                                    }).ToList();

                    if (HistoricoTransferenciaResult.Count > 0)
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

                        List<HistoricoTransferenciaViewModel>? entrada = (from hist in HistoricoTransferenciaResult
                                                                     join user in result on hist.IdUsuario equals user.Id
                                                                     select new HistoricoTransferenciaViewModel
                                                                     {
                                                                         IdProduto = hist.IdProduto,
                                                                         Ferramentaria = hist.Ferramentaria,
                                                                         Catalogo = hist.Catalogo,
                                                                         Classe = hist.Classe,
                                                                         Tipo = hist.Tipo,
                                                                         Codigo = hist.Codigo,
                                                                         Item = hist.Item,
                                                                         RFM = hist.RFM,
                                                                         AF = hist.AF,
                                                                         PAT = hist.PAT,
                                                                         IdUsuario = hist.IdUsuario,
                                                                         Quantidade = hist.Quantidade,
                                                                         FerramentariaOrigem = hist.FerramentariaOrigem,
                                                                         FerramentariaDestino = hist.FerramentariaDestino,
                                                                         DataOcorrencia = hist.DataOcorrencia,
                                                                         Documento = hist.Documento,
                                                                         Usuario = user.Nome
                                                                     }).ToList();

                        //List<int?> distinctUsuario = new List<int?>();
                        //List<VW_Usuario_New> ListUsuario = new List<VW_Usuario_New>();

                        //distinctUsuario = HistoricoTransferenciaResult.Select(x => x.IdUsuario).Distinct().ToList();
                        //ListUsuario = searches.GetDistinctUser(distinctUsuario);

                        //foreach (var produto in HistoricoTransferenciaResult)
                        //{
                        //    var UserDetails = ListUsuario.FirstOrDefault(i => i.Id == produto.IdUsuario);
                        //    produto.Usuario = UserDetails != null ? UserDetails.Nome : "";
                        //}




                        GlobalPagination = Pagination;
                        int pageSize = GlobalPagination ?? 10;
                        int pageNumber = 1;

                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListTransferencia);
                        HttpContext.Session.SetObject(SessionKeyListTransferencia, entrada);

                        //GlobalValues.HistoricoTransferenciaViewModel = HistoricoTransferenciaResult;

                        IPagedList<HistoricoTransferenciaViewModel> HistoricoTransferenciaPagedList = entrada.ToPagedList(pageNumber, pageSize);

                        //return View();
                        return View("Index", HistoricoTransferenciaPagedList);
                    }
                    else
                    {
                        ViewBag.Error = "No Datas Found";
                        return View("Index");
                    }

                }
                else
                {
                    ViewBag.Error = "Select Dates";
                    return View("Index");
                }
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

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

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

        public ActionResult GetProduto(int? CategoriaClasse, int? IdCategoria, int? Tipo, string? Codigo, string? Item, string? AF, int? PAT, string? NumeroSerie)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

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

        public ActionResult SelectCatalog(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

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

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

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

        public ActionResult TestPage(int? page)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            List<HistoricoTransferenciaViewModel>? ListCatalogoModel = HttpContext.Session.GetObject<List<HistoricoTransferenciaViewModel>>(SessionKeyListTransferencia) ?? new List<HistoricoTransferenciaViewModel>();

            IPagedList<HistoricoTransferenciaViewModel> HistoricoTransferenciaPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

            return View("Index", HistoricoTransferenciaPagedList);
        }

        public ActionResult ExportToExcel()
        {
            List<HistoricoTransferenciaViewModel>? itemsToExport = HttpContext.Session.GetObject<List<HistoricoTransferenciaViewModel>>(SessionKeyListTransferencia) ?? new List<HistoricoTransferenciaViewModel>();

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
                dataTable.Columns.Add("RFM"); //8
                dataTable.Columns.Add("AF"); //9
                dataTable.Columns.Add("PAT"); //10
                dataTable.Columns.Add("Quantidade"); //11
                dataTable.Columns.Add("Ferramentaria Origem"); //12
                dataTable.Columns.Add("Ferramentaria Destino"); //13
                dataTable.Columns.Add("Usuario"); //14
                dataTable.Columns.Add("Data Ocorrencia", typeof(DateTime)); //15
                dataTable.Columns.Add("Documento"); //14


                foreach (HistoricoTransferenciaViewModel item in itemsToExport)
                {
                    var row = dataTable.NewRow();
                    row["IdProduto"] = item.IdProduto; // Replace with the actual property name
                    row["Ferramentaria"] = item.Ferramentaria; // Replace with the actual property name
                    row["Catalogo"] = item.Catalogo;
                    row["Classe"] = item.Classe;
                    row["Tipo"] = item.Tipo;
                    row["Codigo"] = item.Codigo;
                    row["Item"] = item.Item;
                    row["RFM"] = item.RFM;
                    row["AF"] = item.AF;
                    row["PAT"] = item.PAT;
                    row["Quantidade"] = item.Quantidade;
                    row["Ferramentaria Origem"] = item.FerramentariaOrigem;
                    row["Ferramentaria Destino"] = item.FerramentariaDestino;
                    row["Usuario"] = item.Usuario;
                    row["Data Ocorrencia"] = item.DataOcorrencia.HasValue ? (object)item.DataOcorrencia.Value : DBNull.Value;
                    row["Documento"] = item.Documento;

                    dataTable.Rows.Add(row);
                }


                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Transferencia Ferramentaria");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Column(15).Style.Numberformat.Format = "dd/MM/yyyy";

                    using (var stream = new MemoryStream())
                    {
                        package.SaveAs(stream);
                        stream.Position = 0;
                        string excelName = $"TransferenciaFerramentaria.xlsx";

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


        public ActionResult ProdutoPager(int? page)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //var usuarios = searches.OnLoadUsers();
            //ViewBag.UsuarioRelatorio = usuarios;

            List<simpleUserModel>? usuarios = HttpContext.Session.GetObject<List<simpleUserModel>>(SessionKeyLoggedUsers) ?? new List<simpleUserModel>();
            ViewBag.UsuarioRelatorio = usuarios;

            var FerramentariaList = searches.OnLoadFerramentaria();
            ViewBag.FerramentariaList = FerramentariaList;

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            IPagedList<ProdutoList> ProdutoListPagedList = GlobalValues.ProdutoList.ToPagedList(pageNumber, pageSize);

            ViewBag.ProdutoList = ProdutoListPagedList;
            return View("Index");
        }



        // GET: TransferenciaFerramentaria/Details/5
        public ActionResult Details(int id)
        {
            //var result = from log in _context.HistoricoTransferencia
            //          join produto in _context.Produto on log.IdProduto equals produto.Id
            //          join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
            //          join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
            //          join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
            //          join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
            //          join ferramentariaOrigem in _context.Ferramentaria on log.IdFerramentariaOrigem equals ferramentariaOrigem.Id
            //          join ferramentariaDestino in _context.Ferramentaria on log.IdFerramentariaDestino equals ferramentariaDestino.Id
            //          // join func in _context.Func on log.IdUsuario equals func.Id // Uncomment this line if Func is the entity for Usuario
            //          where log.IdUsuario == 9
            //                && log.IdFerramentariaOrigem == 19
            //                && log.IdFerramentariaDestino == 11
            //                && log.DataOcorrencia >= new DateTime(2012, 2, 29, 0, 0, 0)
            //                && log.DataOcorrencia <= new DateTime(2012, 2, 29, 23, 59, 59)
            //          orderby log.DataOcorrencia
            //          select new HistoricoTransferenciaViewModel
            //          {
            //              IdProduto = log.IdProduto,
            //              Ferramentaria = ferramentaria.Nome,
            //              Catalogo = categoriaPai.Classe == 1 ? "FERRAMENTA" : (categoriaPai.Classe == 2 ? "EPI" : "CONSUMIVEIS"),
            //              Classe = categoria.Nome,
            //              Tipo = categoriaPai.Nome,
            //              Codigo = catalogo.Codigo,
            //              Item = catalogo.Nome,
            //              RFM = produto.RFM,
            //              AF = produto.AF,
            //              PAT = produto.PAT,
            //              Quantidade = log.Quantidade,
            //              FerramentariaOrigem = ferramentariaOrigem.Nome,
            //              FerramentariaDestino = ferramentariaDestino.Nome,
            //              DataOcorrencia = log.DataOcorrencia
            //          };

            return View();
        }


    }
}
