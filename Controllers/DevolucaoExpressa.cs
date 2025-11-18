using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Drawing.Printing;
using System.Drawing;
using System.Text;

namespace FerramentariaTest.Controllers
{
    //public class GlobalDataDevolucaoExpressa
    //{
    //    public static List<DevolucaoExpressaViewModel> ListDevolucaoExpressa { get; set; }

    //}

  

    public class DevolucaoExpressa : Controller
    {
        private const string SessionKeyExpressaFilter = "ExpressaFilter";
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thDevolucaoExpressa.aspx";
        private StreamReader _streamToPrint;
        private Font _printFont;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        //private static string? AFfilter = "";
        //private static int? PATfilter = new int();

        //private static int? Printer = new int();

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyListDevolucaoExpressa = "ListDevolucaoExpressa";
        //private static List<DevolucaoExpressaViewModel?>? ListOfDevolucaoExpressaProducts = new List<DevolucaoExpressaViewModel?>();

        public DevolucaoExpressa(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            //mapeamentoClasses = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<Funcionario, FuncionarioViewModel>();
            //    cfg.CreateMap<FuncionarioViewModel, Funcionario>();
            //});
        }

        // GET: DevolucaoExpressa
        public ActionResult Index(string? AFSerialDevolucaoExpressa, int? PATDevolucaoExpressa)
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
                //        usuariofer.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
                //        log.LogWhy = usuariofer.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                //    }
                //}
                //#endregion

                //if (LoggedUserDetails.Id == null)
                //{
                //    LoggedUserDetails = usuariofer;
                //}

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {

                            int? FerramentariaValue = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                            if (FerramentariaValue == null)
                            {
                                var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                                          where ferramentaria.Ativo == 1 &&
                                                                !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                                _context.FerramentariaVsLiberador.Any(l => l.IdLogin == loggedUser.Id && l.IdFerramentaria == ferramentaria.Id)
                                                          orderby ferramentaria.Nome
                                                          select new
                                                          {
                                                              Id = ferramentaria.Id,
                                                              Nome = ferramentaria.Nome
                                                          }).ToList();

                                if (ferramentariaItems != null)
                                {
                                    ViewBag.FerramentariaItems = ferramentariaItems;
                                }

                                return PartialView("_FerramentariaPartialView");

                            }
                            else
                            {
                                httpContextAccessor?.HttpContext?.Session.SetInt32(Sessao.IdFerramentaria, (int)FerramentariaValue);
                            }

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ShowSuccessAlert"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"]?.ToString();
                                TempData.Remove("ShowSuccessAlert"); // Remove it from TempData to avoid displaying it again
                            }


                            HttpContext.Session.Remove(SessionKeyExpressaFilter);

                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

                            ViewBag.PrintTicket = 1;
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

        public ActionResult SearchProductForDevolucaoExpressa(string? AFSerialDevolucaoExpressa, int? PATDevolucaoExpressa,int? PrintCheck)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            //Printer = PrintCheck;
            try
            {

                string? error = ValidateSearchFilter(AFSerialDevolucaoExpressa, PATDevolucaoExpressa);
                if (string.IsNullOrEmpty(error))
                {
                    FilterModel? filterModel = new FilterModel();
                    filterModel.AF = !string.IsNullOrEmpty(AFSerialDevolucaoExpressa) ? AFSerialDevolucaoExpressa : null;
                    filterModel.PAT = PATDevolucaoExpressa != null ? PATDevolucaoExpressa : null;
                    filterModel.Printer = PrintCheck != null ? PrintCheck : new int();

                    HttpContext.Session.SetObject(SessionKeyExpressaFilter, filterModel);

                    List<DevolucaoExpressaViewModel?>? result = searches.SearchDevolucaoExpressa(AFSerialDevolucaoExpressa, PATDevolucaoExpressa);
                    if (result.Count > 0)
                    {
                        List<NewUserInformationModel> resultUser = (
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


                        List<DevolucaoExpressaViewModel>? devresult = (from dev in result
                                                                     join user in resultUser on dev.Balconista_IdLogin equals user.Id
                                                                     select new DevolucaoExpressaViewModel
                                                                     {
                                                                         IdProdutoAlocado = dev.IdProdutoAlocado,
                                                                         Solicitante_IdTerceiro = dev.Solicitante_IdTerceiro,
                                                                         Solicitante_CodColigada = dev.Solicitante_CodColigada,
                                                                         Solicitante_Chapa = dev.Solicitante_Chapa,
                                                                         Balconista_IdLogin = dev.Balconista_IdLogin,
                                                                         Liberador_IdTerceiro = dev.Liberador_IdTerceiro,
                                                                         Liberador_CodColigada = dev.Liberador_CodColigada,
                                                                         Liberador_Chapa = dev.Liberador_Chapa,
                                                                         Observacao = dev.Observacao,
                                                                         DataEmprestimo = dev.DataEmprestimo,
                                                                         DataPrevistaDevolucao = dev.DataPrevistaDevolucao,
                                                                         Quantidade = dev.Quantidade,
                                                                         IdFerramentaria = dev.IdFerramentaria,
                                                                         NomeFerramentaria = dev.NomeFerramentaria,
                                                                         IdObra = dev.IdObra,
                                                                         NomeObra = dev.NomeObra,
                                                                         IdProduto = dev.IdProduto,
                                                                         AFProduto = dev.AFProduto,
                                                                         PATProduto = dev.PATProduto,
                                                                         DataVencimento = dev.DataVencimento,
                                                                         DC_DataAquisicao = dev.DC_DataAquisicao,
                                                                         DC_Valor = dev.DC_Valor,
                                                                         CodigoCatalogo = dev.CodigoCatalogo,
                                                                         NomeCatalogo = dev.NomeCatalogo,
                                                                         ImpedirDescarte = dev.ImpedirDescarte,
                                                                         HabilitarDescarteEPI = dev.HabilitarDescarteEPI,
                                                                         IdCategoria = dev.IdCategoria,
                                                                         ClasseCategoria = dev.ClasseCategoria,
                                                                         NomeCategoria = dev.NomeCategoria,
                                                                         ProdutoAtivo = dev.ProdutoAtivo,
                                                                         IdControleCA = dev.IdControleCA,
                                                                         Balconista_Chapa = user.Chapa,
                                                                         LoggedFerramentariaId = httpContextAccessor?.HttpContext?.Session.GetInt32(Sessao.Ferramentaria)
                                                                     }).ToList();


                        foreach (var item in devresult)
                        {
                         
                            item.QuantidadeExtraviada = searches.SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);

                            //int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
                            //item.LoggedFerramentariaId = FerramentariaValue.HasValue ? FerramentariaValue.Value : 0;
                        }


                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListDevolucaoExpressa);
                        HttpContext.Session.SetObject(SessionKeyListDevolucaoExpressa, devresult);
                        //ListOfDevolucaoExpressaProducts = result;
                        //ViewBag.DevolucaoExpressa = ListOfDevolucaoExpressaProducts;
                        ViewBag.PrintTicket = PrintCheck;
                        return View(nameof(Index), devresult);
                    }
                    else
                    {
                        ProdutoCompleteViewModel? productDetail = searches.SearchProductByAF(AFSerialDevolucaoExpressa, PATDevolucaoExpressa);
                        if (productDetail != null)
                        {
                            ViewBag.Result = productDetail.NomeFerramentaria;
                            ViewBag.PrintTicket = PrintCheck;
                            return View(nameof(Index), new List<DevolucaoExpressaViewModel>());
                        }
                        else
                        {
                            ViewBag.Error = "Nenhum resultado encontrado.";
                            ViewBag.PrintTicket = PrintCheck;
                            return View(nameof(Index), new List<DevolucaoExpressaViewModel>());
                        }       
                    }
              
                }
                else
                {
                    ViewBag.Error = error;
                    ViewBag.PrintTicket = PrintCheck;
                    return View(nameof(Index));
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.PrintTicket = PrintCheck;
                return View(nameof(Index));
            }

        }

        public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome)
        {
            if (Ferramentaria != null)
            {
                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult RefreshFerramentaria()
        {
            httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);
            httpContextAccessor.HttpContext.Session.Remove(Sessao.FerramentariaNome);
            return RedirectToAction(nameof(Index));
        }

        #region ModalFunctions

        public ActionResult GetSolicitante(string? id)
        {
            var SearchFilter = HttpContext.Session.GetObject<FilterModel>(SessionKeyExpressaFilter) ?? new FilterModel();

            var querysolicitante = _contextBS.Funcionario
                          .Where(e => e.Chapa == id)
                          .OrderByDescending(e => e.DataMudanca)
                          .FirstOrDefault();

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == querysolicitante.CodPessoa
                                     select gImagem.IMAGEM)
                                    .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);
            ViewBag.SolicitanteChapa = querysolicitante.Chapa;
            ViewBag.SolicitanteNome = querysolicitante.Nome;
            ViewBag.OpenSolicitanteModal = true;
            ViewBag.PrintTicket = SearchFilter.Printer;

            var ListDevolucaoExpressaModel = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();

            return View(nameof(Index), ListDevolucaoExpressaModel);

        }

        public ActionResult GetLiberador(string? id)
        {
            var SearchFilter = HttpContext.Session.GetObject<FilterModel>(SessionKeyExpressaFilter) ?? new FilterModel();

            var queryLiberador = _contextBS.Funcionario
                          .Where(e => e.Chapa == id)
                          .OrderByDescending(e => e.DataMudanca)
                          .FirstOrDefault();

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == queryLiberador.CodPessoa
                                     select gImagem.IMAGEM)
                                        .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);
            ViewBag.LiberadorChapa = queryLiberador.Chapa;
            ViewBag.LiberadorNome = queryLiberador.Nome;
            ViewBag.OpenLiberadorModal = true;
            ViewBag.PrintTicket = SearchFilter.Printer;

            var ListDevolucaoExpressaModel = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();

            return View(nameof(Index), ListDevolucaoExpressaModel);
        }

        public ActionResult GetBalconista(string? id)
        {
            var SearchFilter = HttpContext.Session.GetObject<FilterModel>(SessionKeyExpressaFilter) ?? new FilterModel();

            var queryliberador = _contextBS.Funcionario
                           .Where(e => EF.Functions.Like(e.Chapa, $"%{id}%"))
                           .OrderByDescending(e => e.DataMudanca)
                           .FirstOrDefault();

            var resultsolicitante = (from pessoa in _contextRM.PPESSOA
                                     join gImagem in _contextRM.GIMAGEM
                                     on pessoa.IDIMAGEM equals gImagem.ID
                                     where pessoa.CODIGO == queryliberador.CodPessoa
                                     select gImagem.IMAGEM)
                                    .FirstOrDefault();

            ViewData["Base64ImageBalc"] = Convert.ToBase64String(resultsolicitante);

            ViewBag.BalconistaChapa = queryliberador.Chapa;
            ViewBag.BalconistaNome = queryliberador.Nome;
            ViewBag.OpenBalconistaModal = true;
            ViewBag.PrintTicket = SearchFilter.Printer;

            var ListDevolucaoExpressaModel = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();

            return View(nameof(Index), ListDevolucaoExpressaModel);
        }


        #endregion


        public ActionResult Limpar()
        {
            var SearchFilter = HttpContext.Session.GetObject<FilterModel>(SessionKeyExpressaFilter) ?? new FilterModel();
            HttpContext.Session.Remove(SessionKeyExpressaFilter);

            //AFfilter = "";
            //PATfilter = null;
            //var ListDevolucaoExpressaModel = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();
            //ListDevolucaoExpressaModel.Clear();
            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListDevolucaoExpressa);

            ViewBag.PrintTicket = SearchFilter.Printer;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExecuteActionDevolver(DevolucaoExpressaViewModel? model, int? PrintTicket)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/ExecuteActionDevolver";
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

                            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
                            var SearchFilter = HttpContext.Session.GetObject<FilterModel>(SessionKeyExpressaFilter) ?? new FilterModel();

                            var ListDevolucaoExpressaModel = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();
                            if (model.selectedItem != null && model.selectedItem != 0)
                            {

                                int? HistoricoAlocacaoIdToPrint = null;


                                DevolucaoExpressaViewModel? ChosenProduct = ListDevolucaoExpressaModel.FirstOrDefault(i => i.IdProdutoAlocado == model.IdProdutoAlocado);
                                var GetProdutoAlocado = _context.ProdutoAlocado.FirstOrDefault(e => e.Id == model.IdProdutoAlocado);
                                if (GetProdutoAlocado.Quantidade >= 1)
                                {
                                    int ano = DateTime.Now.Year;
                                    string tableName = $"HistoricoAlocacao_{ano}";

                                    var tableExists = _context.Database.SqlQueryRaw<int>(
                                                        "IF OBJECT_ID('" + tableName + "') IS NOT NULL " +
                                                        "SELECT 1 " +
                                                        "ELSE " +
                                                        "SELECT 0"
                                                    ).AsEnumerable().FirstOrDefault();

                                    if (tableExists != 0)
                                    {
                                        var InsertHistoricoAlocacao2025 = new HistoricoAlocacao_2025
                                        {
                                            IdProduto = GetProdutoAlocado.IdProduto,
                                            Solicitante_IdTerceiro = GetProdutoAlocado.Solicitante_IdTerceiro,
                                            Solicitante_CodColigada = GetProdutoAlocado.Solicitante_CodColigada,
                                            Solicitante_Chapa = GetProdutoAlocado.Solicitante_Chapa,
                                            Liberador_IdTerceiro = GetProdutoAlocado.Liberador_IdTerceiro,
                                            Liberador_CodColigada = GetProdutoAlocado.Liberador_CodColigada,
                                            Liberador_Chapa = GetProdutoAlocado.Liberador_Chapa,
                                            Balconista_Emprestimo_IdLogin = GetProdutoAlocado.Balconista_IdLogin,
                                            Balconista_Devolucao_IdLogin = loggedUser.Id,
                                            Observacao = GetProdutoAlocado.Observacao,
                                            DataEmprestimo = GetProdutoAlocado.DataEmprestimo,
                                            DataPrevistaDevolucao = GetProdutoAlocado.DataPrevistaDevolucao,
                                            DataDevolucao = DateTime.Now,
                                            IdObra = GetProdutoAlocado.IdObra,
                                            Quantidade = model.QuantidadeInput,
                                            IdFerrOndeProdRetirado = GetProdutoAlocado.IdFerrOndeProdRetirado,
                                            IdFerrOndeProdDevolvido = model.FerramentariaReturnId,
                                            IdControleCA = GetProdutoAlocado.IdControleCA
                                        };

                                        _context.Add(InsertHistoricoAlocacao2025);
                                        _context.SaveChanges();

                                        HistoricoAlocacaoIdToPrint = InsertHistoricoAlocacao2025.Id;

                                        var CheckArquivo = _context.ArquivoVsProdutoAlocado.FirstOrDefault(e => e.IdProdutoAlocado == model.IdProdutoAlocado);
                                        if (CheckArquivo != null)
                                        {
                                            var InsertArquivoHistorico = new ArquivoVsHistorico
                                            {
                                                IdArquivo = CheckArquivo.IdArquivo,
                                                IdHistoricoAlocacao = InsertHistoricoAlocacao2025.Id,
                                                Ano = ano,
                                                DataRegistro = DateTime.Now
                                            };
                                            _context.Add(InsertArquivoHistorico);
                                            _context.SaveChanges();
                                        }


                                        if (model.FerramentariaReturnId != 17)
                                        {
                                            var updateproduct = _context.Produto.Where(i => i.Id == model.IdProduto).FirstOrDefault();
                                            if (updateproduct != null)
                                            {
                                                updateproduct.Quantidade = updateproduct.Quantidade + model.QuantidadeInput;
                                                _context.SaveChanges();
                                            }
                                        }

                                        int? overallquantity = ChosenProduct.Quantidade - model.QuantidadeInput;
                                        if (overallquantity == 0 && ChosenProduct.QuantidadeExtraviada == 0)
                                        {
                                            var arquivoVsProdutoAlocadoToDelete = _context.ArquivoVsProdutoAlocado.FirstOrDefault(a => a.IdProdutoAlocado == model.IdProdutoAlocado);

                                            var produtoAlocadoToDelete = _context.ProdutoAlocado.FirstOrDefault(p => p.Id == model.IdProdutoAlocado);

                                            if (arquivoVsProdutoAlocadoToDelete != null)
                                            {
                                                _context.ArquivoVsProdutoAlocado.Remove(arquivoVsProdutoAlocadoToDelete);
                                            }

                                            if (produtoAlocadoToDelete != null)
                                            {
                                                _context.ProdutoAlocado.Remove(produtoAlocadoToDelete);
                                            }

                                            _context.SaveChanges();
                                        }
                                        else
                                        {
                                            var newedit = _context.ProdutoAlocado.Where(i => i.Id == model.IdProdutoAlocado).FirstOrDefault();
                                            if (newedit != null)
                                            {
                                                newedit.Quantidade = overallquantity;
                                                _context.SaveChanges();
                                            }
                                        }


                                        SearchFilter.Printer = PrintTicket;
                                        if (SearchFilter.Printer == 1)
                                        {
                                            List<HistoricoAlocacaoViewModel?>? historicoAlocacaoList = searches.SearchHistoricoAlocacaoById(HistoricoAlocacaoIdToPrint);
                                            if (historicoAlocacaoList.Count > 0)
                                            {
                                                DateTime dataDevolucao = DateTime.Now;
                                                var sb = new StringBuilder();
                                                sb.AppendLine(" _");
                                                sb.AppendFormat("{0}{0}{0}", Environment.NewLine);
                                                sb.AppendLine(" **************************************");
                                                sb.AppendLine(" COMPROVANTE DE DEVOLUÇÃO");
                                                sb.AppendLine("@FERRAMENTARIA");
                                                sb.AppendLine(" **************************************");
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendLine(QuebraTexto(String.Format(" OPER. REALIZADA: {0}", ((DateTime)dataDevolucao).ToString("dd/MM/yyyy HH:mm:ss"))));
                                                //sb.AppendLine(QuebraTexto($"POR BALCONISTA: {usuario.Nome.ToString().Trim()}"));
                                                //sb.AppendLine(QuebraTexto($"FUNCIONARIO:{usuario.Chapa.ToString().Trim()}/{usuario.Nome.ToString().Trim()}"));

                                                sb.AppendLine(QuebraTexto(String.Format(" POR BALCONISTA : {0}", loggedUser.Nome).TrimEnd()));
                                                sb.AppendLine(QuebraTexto(String.Format(" P/ FUNCIONÁRIO : {0}", GetProdutoAlocado?.Solicitante_Chapa.TrimEnd())));

                                                foreach (var viewModel in historicoAlocacaoList)
                                                {
                                                    sb.AppendLine(QuebraTexto(String.Format(" ID TRANSAÇÃO   : {0}", viewModel.IdHistoricoAlocacao).TrimEnd()));
                                                    sb.AppendLine(QuebraTexto(String.Format(" CÓDIGO         : {0}", viewModel.CodigoCatalogo).TrimEnd()));
                                                    sb.AppendLine(QuebraTexto(String.Format(" QTD. DEVOLVIDA : {0}", viewModel.Quantidade).TrimEnd()));
                                                    sb.AppendLine(QuebraTexto(String.Format(" SALDO RESTANTE : {0}", viewModel.QuantidadeEmprestada).TrimEnd()));
                                                    sb.AppendLine(QuebraTexto(String.Format(" DAT. EMPRÉSTIMO: {0}", viewModel.DataEmprestimo).TrimEnd()));
                                                    sb.AppendLine(QuebraTexto(String.Format(" DESCRIÇÃO      : {0}", viewModel.NomeCatalogo).TrimEnd()));

                                                    if (!string.IsNullOrEmpty(viewModel.AFProduto))
                                                    {
                                                        //sb.AppendLine($"AF: {viewModel.AFProduto?.ToString().Trim()}");
                                                        sb.AppendLine(QuebraTexto(String.Format(" AF             : {0}", viewModel.AFProduto).TrimEnd()));
                                                    }
                                                    if (viewModel.PATProduto != 0)
                                                    {
                                                        sb.AppendLine(QuebraTexto(String.Format(" PAT            : {0}", viewModel.PATProduto).TrimEnd()));
                                                    }
                                                    if (!string.IsNullOrEmpty(viewModel.Observacao))
                                                    {
                                                        //sb.AppendLine($"Obs: {viewModel.Observacao?.ToString().Trim()}");
                                                        sb.AppendLine(QuebraTexto(String.Format(" NOTA BALCONISTA: {0}", viewModel.Observacao).TrimEnd()));
                                                        sb.AppendFormat("{0}", Environment.NewLine);
                                                    }

                                                    sb.Replace("@FERRAMENTARIA", QuebraTexto(String.Format(" {0}", viewModel?.FerrOrigem.ToUpper()).TrimEnd()));
                                                }

                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendLine(" **************************************");
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendLine(QuebraTexto((String.Format(" {0}", "O empregado está ciente de que, na eventualidade de perda, extravio ou dano do Rádio, Equipamento / EPI / Kit / Ferramenta, assim como a não devolução do mesmo quando requisitada, o valor correspondente à perda será descontado dos próximos vencimentos do empregado, nos termos do Artigo 462-1º/CLT."))));
                                                sb.AppendLine(" **************************************");
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendFormat("{0}", Environment.NewLine);
                                                sb.AppendLine(" _");

                                                //string? FolderPath = "C:\\Repositorio\\SIB-Ferramentaria\\\\Receipts";
                                                string remoteAddr = HttpContext.Connection.RemoteIpAddress.ToString();
                                                string? FolderPath = "D:\\Ferramentaria\\Diebold\\" + remoteAddr + "\\";
                                                string? caminho = String.Format("{0}{1}{2}{3}.txt", FolderPath, loggedUser.Nome, loggedUser.Chapa, DateTime.Now.ToString().Replace("-", "").Replace(":", "").Replace(" ", "").Replace("/", ""));

                                                if (!Directory.Exists(FolderPath))
                                                {
                                                    Directory.CreateDirectory(FolderPath);
                                                }

                                                using (StreamWriter outfile = new StreamWriter(caminho))
                                                {
                                                    outfile.Write(sb.ToString());
                                                }


                                                try
                                                {

                                                    _streamToPrint = new StreamReader(caminho);

                                                    _printFont = new Font("Arial", 7);

                                                    PrintDocument pd = new PrintDocument();
                                                    pd.DefaultPageSettings.Margins = new Margins(
                                                            Convert.ToInt32(0.147 * 100),   // Left margin in hundredths of an inch
                                                            Convert.ToInt32(0.157 * 100),   // Right margin in hundredths of an inch
                                                            Convert.ToInt32(0 * 100),       // Top margin in hundredths of an inch
                                                            Convert.ToInt32(0.004 * 100)    // Bottom margin in hundredths of an inch
                                                        );

                                                    pd.PrintPage += ImprimirPagina;

                                                    pd.Print();

                                                    _streamToPrint.Close();
                                                }
                                                catch (IOException ioEx)
                                                {
                                                    Console.WriteLine($"IOException: {ioEx.Message}");
                                                }
                                                catch (UnauthorizedAccessException authEx)
                                                {
                                                    Console.WriteLine($"UnauthorizedAccessException: {authEx.Message}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Unexpected Exception: {ex.Message}");
                                                }
                                            }
                                        }



                                        List<DevolucaoExpressaViewModel?>? RefreshList = searches.SearchDevolucaoExpressa(SearchFilter.AF, SearchFilter.PAT);

                                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListDevolucaoExpressa);
                                        HttpContext.Session.SetObject(SessionKeyListDevolucaoExpressa, RefreshList);
                                        //ListOfDevolucaoExpressaProducts = RefreshList;

                                        ViewBag.ShowSuccessAlert = true;
                                        ViewBag.PrintTicket = SearchFilter.Printer;
                                        return View(nameof(Index), RefreshList ?? new List<DevolucaoExpressaViewModel>());
                                    }
                                    else
                                    {
                                        TempData["ShowErrorAlert"] = true;
                                        TempData["ErrorMessage"] = $"Please Contact IT Department, Because {tableName} doesnt exist ";

                                        return RedirectToAction(nameof(Index));
                                    }
                                }
                                else
                                {
                                    ViewBag.Error = "Saldo INSUFICIENTE para operação de devolução! o SALDO ATUALMENTE EMPRESTADO";
                                    ViewBag.PrintTicket = SearchFilter.Printer;

                                    //var ListDevolucaoExpressaModelError = HttpContext.Session.GetObject<List<DevolucaoExpressaViewModel?>>(SessionKeyListDevolucaoExpressa) ?? new List<DevolucaoExpressaViewModel?>();
                                    return View(nameof(Index), ListDevolucaoExpressaModel ?? new List<DevolucaoExpressaViewModel>());
                                }

                                //return View(nameof(Index), ListOfDevolucaoExpressaProducts);
                            }
                            else
                            {
                                ViewBag.Error = "Não foi possível completar a operação!, Por favor selecione o item";
                                ViewBag.PrintTicket = SearchFilter.Printer;


                                return View(nameof(Index), ListDevolucaoExpressaModel ?? new List<DevolucaoExpressaViewModel>());

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
                return View(nameof(Index));
            }
      
        }

        private void ImprimirPagina(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height / _printFont.GetHeight(ev.Graphics);

            // Print each line of the file.
            while (count < linesPerPage)
            {
                line = _streamToPrint.ReadLine();
                if (line == null)
                {
                    break;
                }
                yPos = topMargin + count * _printFont.GetHeight(ev.Graphics);
                ev.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

        private string QuebraTexto(string t)
        {
            string texto = t.TrimEnd();
            StringBuilder sb = new StringBuilder();

            double stepFor = texto.Length / 40.0;
            int startIndex = 0;

            if (stepFor > 1)
            {
                for (int index = 0; index <= stepFor; index++)
                {
                    int resto = texto.Length - (startIndex + 40);

                    if (resto >= 40)
                    {
                        if (index != 0)
                        {
                            startIndex += 40;
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, 40).TrimStart(), Environment.NewLine);
                        }
                        else
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
                        }
                    }
                    else if (resto >= 0)
                    {
                        if (index != 0)
                        {
                            startIndex += 40;
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
                        }
                        else
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(0, 40).TrimStart(), Environment.NewLine);
                        }
                    }
                    else if (resto < 0)
                    {
                        resto = (texto.Length - 1) - startIndex;
                        if (sb.ToString().IndexOf(texto.Substring(startIndex, resto), StringComparison.Ordinal) == -1)
                        {
                            sb.AppendFormat(" {0}{1}", texto.Substring(startIndex, resto).TrimStart(), Environment.NewLine);
                        }
                    }
                }

                return sb.ToString();
            }
            else
            {
                return string.Format(" {0}{1}", texto.TrimStart(), Environment.NewLine);
            }
        }

        public string ValidateSearchFilter(string? AFSerial, int? PAT)
        {
            if (string.IsNullOrEmpty(AFSerial) && PAT == null)
            {
                return "Nenhum filtro informado.";
            }

            return null;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolver(int? selectedItem, int? IdProduto, string? NomeFerramentaria, int? IdFerramentaria, int? SolicitanteIdTerceiro, int? SolicitanteChapa, int? SolicitanteCodColigada)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuariofer.Pagina = pagina;
                usuariofer.Pagina1 = log.LogWhat;
                usuariofer.Acesso = log.LogWhat;
                usuariofer = auxiliar.VerificaPermissao(usuariofer);

                if (usuariofer.Permissao == null)
                {
                    usuariofer.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuariofer.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                }
                else
                {
                    if (usuariofer.Permissao.Editar != 1)
                    {
                        usuariofer.Retorno = "Usuário sem permissão de Editar a página de ferramentaria!";
                        log.LogWhy = usuariofer.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                    }
                }
                #endregion

                if (selectedItem == null)
                {
                    TempData["ShowErrorAlert"] = true;
                    TempData["ErrorMessage"] = "Não foi possível completar a operação!, please check";

                    return RedirectToAction(nameof(Index));
                }

                string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);

                var queryQuantidadeProdutoalocado = _context.ProdutoAlocado
                            .Where(p => p.Id == selectedItem)
                            .Select(p => p.Quantidade)
                            .FirstOrDefault();

                if (queryQuantidadeProdutoalocado > 0)
                {
          
                    int ano = DateTime.Now.Year;
                    string tableName = $"HistoricoAlocacao_{ano}";

                    var tableExists = _context.Database.SqlQueryRaw<int>(
                                        "IF OBJECT_ID('" + tableName + "') IS NOT NULL " +
                                        "SELECT 1 " +
                                        "ELSE " +
                                        "SELECT 0"
                                    ).AsEnumerable().FirstOrDefault();

                    if (tableExists == 0)
                    {
                        TempData["ShowErrorAlert"] = true;
                        TempData["ErrorMessage"] = $"Please Contact IT Department, Because {tableName} doesnt exist ";

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var GetDatas = _context.ProdutoAlocado
                                      .Where(p => p.Id == selectedItem)
                                      .FirstOrDefault();


                        var InsertHistoricoAlocacao2025 = new HistoricoAlocacao_2025
                        {
                            IdProduto = GetDatas.IdProduto,
                            Solicitante_IdTerceiro = GetDatas.Solicitante_IdTerceiro,
                            Solicitante_CodColigada = GetDatas.Solicitante_CodColigada,
                            Solicitante_Chapa = GetDatas.Solicitante_Chapa,
                            //GetProdutoAlocado.Solicitante_Chapa,
                            Liberador_IdTerceiro = GetDatas.Liberador_IdTerceiro,
                            Liberador_CodColigada = GetDatas.Liberador_CodColigada,
                            Liberador_Chapa = GetDatas.Liberador_Chapa,
                            Balconista_Emprestimo_IdLogin = GetDatas.Balconista_IdLogin,
                            Balconista_Devolucao_IdLogin = usuariofer.Id,
                            Observacao = GetDatas.Observacao,
                            DataEmprestimo = GetDatas.DataEmprestimo,
                            DataPrevistaDevolucao = GetDatas.DataPrevistaDevolucao,
                            DataDevolucao = DateTime.Now,
                            IdObra = GetDatas.IdObra,
                            Quantidade = GetDatas.Quantidade,
                            IdFerrOndeProdRetirado = GetDatas.IdFerrOndeProdRetirado,
                            IdFerrOndeProdDevolvido = IdFerramentaria,
                            IdControleCA = GetDatas.IdControleCA
                        };

                        _context.Add(InsertHistoricoAlocacao2025);
                        await _context.SaveChangesAsync();

                        var IdArquivo = _context.ArquivoVsProdutoAlocado
                                                  .Where(avpa => avpa.IdProdutoAlocado == selectedItem)
                                                  .Select(avpa => avpa.IdArquivo)
                                                  .FirstOrDefault();

                        if (IdArquivo != null)
                        {
                            // Insert record into ArquivoVsHistorico
                            var arquivoVsHistorico = new ArquivoVsHistorico
                            {
                                IdArquivo = IdArquivo,
                                IdHistoricoAlocacao = InsertHistoricoAlocacao2025.Id,
                                Ano = ano
                            };

                            _context.ArquivoVsHistorico.Add(arquivoVsHistorico);

                            // Save changes to the database
                            _context.SaveChanges();
                        }

                        var produtoToUpdate = _context.Produto.FirstOrDefault(p => p.Id == IdProduto);

                        if (produtoToUpdate != null)
                        {
                            produtoToUpdate.Quantidade += 1;
                            _context.SaveChanges();
                        }

                        // Delete records from ArquivoVsProdutoAlocado
                        var arquivosToDelete = _context.ArquivoVsProdutoAlocado.Where(avpa => avpa.IdProdutoAlocado == selectedItem);
                        if (arquivosToDelete != null)
                        {
                            _context.ArquivoVsProdutoAlocado.RemoveRange(arquivosToDelete);
                        }

                        // Delete record from ProdutoAlocado
                        var produtoToDelete = _context.ProdutoAlocado.FirstOrDefault(pa => pa.Id == selectedItem);
                        if (produtoToDelete != null)
                        {
                            _context.ProdutoAlocado.Remove(produtoToDelete);
                        }

                        _context.SaveChanges();


                        TempData["ShowSuccessAlert"] = true;

                        //GlobalDataDevolucaoExpressa.ListDevolucaoExpressa.RemoveAll(item => item.IdProdutoAlocado == selectedItem);

                    }
                }
                else
                {
                    TempData["ShowErrorAlert"] = true;
                    TempData["ErrorMessage"] = "Saldo INSUFICIENTE para operação de devolução!";

                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));


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











    }
}
