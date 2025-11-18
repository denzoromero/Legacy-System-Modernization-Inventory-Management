using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using AutoMapper;
using FerramentariaTest.DAL;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using FerramentariaTest.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Security.Policy;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Http.HttpResults;
using static NuGet.Packaging.PackagingConstants;
using FerramentariaTest.EntitiesBS;
using System.IO;
using Microsoft.IdentityModel.Tokens;

namespace FerramentariaTest.Controllers
{
    //public class GlobalData
    //{
    //    public static List<EmprestimoViewModel> EmprestimoList { get; set; }

    //    public static List<EmprestimoViewModel> list { get; set; }
    //}

    public class EmprestimoController : Controller
    {
        private const string SessionKeyEmprestimoList = "EmprestimoList";
        private const string SessionKeyProductList = "ProductList";
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private MapperConfiguration mapper;
        private readonly IMapper _mapper;
        private static string pagina = "thEmprestimo.aspx";

        private const string SessionKeyEmprestimoCart = "EmprestimoCart";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        private static List<Obra>? StaticObra = new List<Obra>();


        public EmprestimoController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FuncionarioViewModel, NewUserInformationModel>()
                            .ForMember(dest => dest.IdTerceiro, opt => opt.MapFrom(src => src.IdTerceiro != null ? src.IdTerceiro : 0));
                cfg.CreateMap<NewUserInformationModel, FuncionarioViewModel>();
            });
            _mapper = mapper.CreateMapper();
            //mapeamentoClasses = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<Funcionario, FuncionarioViewModel>();
            //    cfg.CreateMap<FuncionarioViewModel, Funcionario>();
            //});
        }

        public ActionResult Index()
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                ////usuario.Pagina = "Home/Index";
                //usuario.Pagina = pagina;
                //usuario.Pagina1 = "thEmprestimo.aspx";
                //usuario.Acesso = log.LogWhat;
                //usuario = auxiliar.VerificaPermissao(usuario);

                //if (usuario.Permissao == null)
                //{
                //    usuario.Retorno = "Usuário sem permissão na página!";
                //    log.LogWhy = usuario.Retorno;
                //    auxiliar.GravaLogAlerta(log);
                //    return RedirectToAction("PreserveActionError", "Home", usuario);
                //}
                //else
                //{
                //    if (usuario.Permissao.Visualizar != 1)
                //    {
                //        usuario.Retorno = "Usuário sem permissão de visualizar a página de Emprestimo!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
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

                            int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
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

                            if (TempData.ContainsKey("ShowSuccessAlertEmprestimo"))
                            {
                                ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlertEmprestimo"]?.ToString();
                                TempData.Remove("ShowSuccessAlertEmprestimo"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ErrorMessage"))
                            {
                                ViewBag.Error = TempData["ErrorMessage"]?.ToString();
                                TempData.Remove("ErrorMessage"); // Remove it from TempData to avoid displaying it again
                            }

                            if (TempData.ContainsKey("ErrorList"))
                            {
                                ViewBag.ErrorList = (string[]?)TempData["ErrorList"];
                                TempData.Remove("ErrorList"); // Remove it from TempData to avoid displaying it again
                            }

                            List<Obra>? obraquery = _context.Obra.Where(s => s.Ativo == 1).OrderBy(s => s.Codigo).ToList();
                            StaticObra = obraquery;

                            ViewBag.Obra = StaticObra;

                            //SolicitanteStaticModel = null;
                            //LiberadorStaticModel = null;
                            //ProductSearchResult.Clear();

                            var model = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyEmprestimoList);
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                            //RegisteredProductResult.Clear();

                            //if (SolicitanteStaticModel != null && !string.IsNullOrEmpty(SolicitanteStaticModel.Chapa) && LiberadorStaticModel != null && !string.IsNullOrEmpty(LiberadorStaticModel.Chapa))
                            //{
                            //    List<Obra?>? obraquery = _context.Obra.Where(s => s.Ativo == 1).OrderBy(s => s.Codigo).ToList();
                            //    ViewBag.Obra = obraquery;

                            //    List<EmprestimoViewModel?> SearchResult = new List<EmprestimoViewModel?>();
                            //    List<EmprestimoViewModel?> Cart = new List<EmprestimoViewModel?>();
                            //    if (ProductSearchResult.Count > 1)
                            //    {
                            //        //SearchResult = ProductSearchResult;
                            //        ViewBag.Emprestimo = ProductSearchResult;
                            //        ViewBag.Emprestimo = ProductSearchResult.Count > 1 ? ProductSearchResult : new List<EmprestimoViewModel?>; 
                            //    }

                            //    ViewBag.Liberador = LiberadorStaticModel;
                            //    ViewBag.Liberador = LiberadorStaticModel != null ? LiberadorStaticModel : new UserViewModel();

                            //    ViewBag.Solicitante = SolicitanteStaticModel;
                            //    ViewBag.Solicitante = SolicitanteStaticModel != null ? SolicitanteStaticModel : new UserViewModel();


                            //    if (RegisteredProductResult.Count > 0)
                            //    {
                            //        //Cart = RegisteredProductResult;
                            //        //ViewBag.EmprestimoCadastro = RegisteredProductResult;
                            //        return View(RegisteredProductResult);
                            //        return View(nameof(Index), RegisteredProductResult.Count > 0 ? RegisteredProductResult : new List<EmprestimoViewModel?>());
                            //    }
                            //    //ViewBag.Emprestimo = SearchResult;
                            //    //ViewBag.EmprestimoCadastro = Cart;

                            //}

                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyEmprestimoCart);


                            log.LogWhy = "Acesso Permitido";
                            auxiliar.GravaLogSucesso(log);

                            var formToken = Guid.NewGuid().ToString();
                            HttpContext.Session.SetString("FormToken", formToken);
                            ViewBag.FormToken = formToken;

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


                //httpContextAccessor.HttpContext?.Session.Remove(Sessao.Solicitante);
                //httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);


           

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
                return View();
            }
        }



        #region new update 

        #region Javascript

        [HttpPost]
        public IActionResult GetEmployeeInformation(string? givenInfo)
        {
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();


            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);


            List<FuncionarioViewModel>? listTerceiroResult = searches.SearchTercerio(givenInfo);
            TotalResult.AddRange(listTerceiroResult);

            List<FuncionarioViewModel> listEmployeeResult = searches.SearchEmployeeChapa(givenInfo);
            TotalResult.AddRange(listEmployeeResult);

            if (TotalResult.Count == 1)
            {
                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                //int? loggedUserId = httpContextAccessor?.HttpContext?.Session.GetInt32(FerramentariaTest.Helpers.Sessao.ID);
                NewUserInformationModel entity = searches.newSearchEmployee(TotalResult[0].Chapa);
                entity.blockMessage = searches.SearchMensagem(entity.Chapa, loggedUser.Id);
                entity.blockSolicitanteMessage = searches.SearchBloqueioMessage(entity.Chapa);

                return Json(new { success = true, entity });

            }
            else if (TotalResult.Count > 1)
            {
                return Json(new { success = true, TotalResult });
            }
            else if (TotalResult.Count == 0)
            {
                return Json(new { success = false, message = $"Nenhum funcionário encontrado para {givenInfo}." });
            }

            return Json(new { success = false, message = "Unexpected error occurred." });

        }

        [HttpPost]
        public IActionResult selectedEmployeeInformation(string? givenInfo)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            try
            {
                NewUserInformationModel entity = searches.newSearchEmployee(givenInfo);

                if (entity != null)
                {
                    return Json(new { success = true, entity });
                }
                else
                {
                    return Json(new { success = false, message = $"Cannot Find Employee with Chapa:{givenInfo}." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        

        }

        [HttpPost]
        public IActionResult deleteBlockMessage(int id)
        {
            MensagemSolicitante? DeleteMessage = _context.MensagemSolicitante.FirstOrDefault(i => i.Id == id);
            if (DeleteMessage != null)
            {
                DeleteMessage.Ativo = 0;
                _context.SaveChanges();


                return Json(new { success = true, message = "Excluído com sucesso." });
            }

            return Json(new { success = false, message = "Cannot Find item." });
        }

        [HttpPost]
        public IActionResult GetProductInfo(string? codigo, string? item, string? af, int? pat, string? chapa, int? coligada)
        {
            List<EmprestimoViewModel>? productlist = getProductList(codigo, item, af, pat) ?? new List<EmprestimoViewModel>();

            if (productlist.Count == 1)
            {
                productCheck prodcheck = checkproduct(productlist[0], chapa, coligada);

                if (prodcheck.success == true)
                {
                    EmprestimoViewModel finalproduct = getmoreproductinfo(productlist[0]);

                    emprestimoCart? selectedproduct = new emprestimoCart()
                    {
                        IdProduto = finalproduct.IdProduto,
                        Codigo = finalproduct.Codigo
                    };

                    if (finalproduct.ControleCAList.Count > 0)
                    {

                    }
                    else
                    {
                        List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();

                        emprestimoList.Add(selectedproduct);
                        HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);
                    }
                                 
                    return Json(new { success = prodcheck.success, finalproduct });
                }
                else
                {
                    return Json(new { success = prodcheck.success, message = prodcheck.message });
                }
            }
            else if (productlist.Count > 1)
            {
                List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();

                if (emprestimoList.Count > 0)
                {
                    productlist = productlist.Where(p => !emprestimoList.Any(e => e.IdProduto == p.IdProduto)).ToList();
                }

                return Json(new { success = true, productlist });
            }
            else if (productlist.Count == 0)
            {
                return Json(new { success = false, message = "Nenhum registro encontrado." });
            }

            return Json(new { success = false, message = "Unexpected error occurred." });
        }


        [HttpPost]
        public IActionResult checkAndToSessionKeyEmprestimoCart(string? codigo, int? idproduto, int? idCatalogo,int? classe)
        {
            if (classe == 2)
            {
                EmprestimoViewModel? productinfo = getmoreproductinfoIndi(idproduto);
                if (productinfo.ControleCAList.Count > 0 )
                {
                    return Json(new { success = true, productinfo });
                } 
                else
                {
                    List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();
                    if (emprestimoList.Count > 0)
                    {
                        emprestimoList = emprestimoList.Where(i => i.IdProduto == idproduto).ToList();
                        if (emprestimoList.Count > 0)
                        {
                            return Json(new { success = false, message = "O produto já está no carrinho." });
                        }
                        else
                        {
                            emprestimoCart? selectedproduct = new emprestimoCart()
                            {
                                IdProduto = idproduto,
                                Codigo = codigo
                            };

                            emprestimoList.Add(selectedproduct);
                            HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

                            if (productinfo.DataEmprestimoFrontEnd != null)
                            {
                                return Json(new { success = true, emprestimoDate = productinfo.DataEmprestimoFrontEnd });
                            }
                            else
                            {
                                return Json(new { success = true });
                            }
                        
                        }
                    }
                    else
                    {
                        emprestimoCart? selectedproduct = new emprestimoCart()
                        {
                            IdProduto = idproduto,
                            Codigo = codigo
                        };

                        emprestimoList.Add(selectedproduct);
                        HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

                        if (productinfo.DataEmprestimoFrontEnd != null)
                        {
                            return Json(new { success = true, emprestimoDate = productinfo.DataEmprestimoFrontEnd });
                        }
                        else
                        {
                            return Json(new { success = true });
                        }

                        //return Json(new { success = true });
                    }
                }              
            }
            else
            {
                List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();
                if (emprestimoList.Count > 0)
                {
                    emprestimoList = emprestimoList.Where(i => i.IdProduto == idproduto).ToList();
                    if (emprestimoList.Count > 0)
                    {
                        return Json(new { success = false, message = "O produto já está no carrinho." });
                    }
                    else
                    {
                        emprestimoCart? selectedproduct = new emprestimoCart()
                        {
                            IdProduto = idproduto,
                            Codigo = codigo
                        };

                        emprestimoList.Add(selectedproduct);
                        HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

                        return Json(new { success = true });
                    }
                }
                else
                {
                    emprestimoCart? selectedproduct = new emprestimoCart()
                    {
                        IdProduto = idproduto,
                        Codigo = codigo
                    };

                    emprestimoList.Add(selectedproduct);
                    HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

                    return Json(new { success = true });
                }
            }
        }


        [HttpPost]
        public IActionResult addToSessionKeyEmprestimoCart(string? codigo, int? idproduto,int? idCatalogo)
        {

            List<emprestimoCart>? CheckemprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();
            if (CheckemprestimoList.Count > 0)
            {
                List<emprestimoCart>? emprestimoList = CheckemprestimoList.Where(i => i.IdProduto == idproduto).ToList();
                if (emprestimoList.Count > 0)
                {
                    return Json(new { success = false, message = "O produto já está no carrinho." });
                }
                else
                {
                    emprestimoCart? selectedproduct = new emprestimoCart()
                    {
                        IdProduto = idproduto,
                        Codigo = codigo
                    };

                    CheckemprestimoList.Add(selectedproduct);
                    HttpContext.Session.SetObject(SessionKeyEmprestimoCart, CheckemprestimoList);

                    return Json(new { success = true });
                }
            }
            else
            {
                emprestimoCart? selectedproduct = new emprestimoCart()
                {
                    IdProduto = idproduto,
                    Codigo = codigo
                };

                CheckemprestimoList.Add(selectedproduct);
                HttpContext.Session.SetObject(SessionKeyEmprestimoCart, CheckemprestimoList);

                return Json(new { success = true });
            }

            //emprestimoCart? selectedproduct = new emprestimoCart()
            //{
            //    IdProduto = idproduto,
            //    Codigo = codigo
            //};

            //List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();

            //emprestimoList.Add(selectedproduct);
            //HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

            //return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult removeToSessionKeyEmprestimoCart(int? productId)
        {

            List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();

            emprestimoList.RemoveAll(item => item.IdProduto == productId);

            HttpContext.Session.SetObject(SessionKeyEmprestimoCart, emprestimoList);

            return Json(new { success = true });
        }

        //public IActionResult submitTransaction(List<submittedEmprestimo> emprestimoList, string? chapaSolicitante, int? codColigadaSolicitante, int? idSolicitanteTerceiro, 
        //    string? chapaLiberador, int? codcoligadaLiberador, int? idLiberadorTerceiro, int? ObraEmprestimo)
        //{

        //    int? userId = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID);
        //    int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

        //    foreach (submittedEmprestimo item in emprestimoList)
        //    {
        //        productDetails productDetail = getProductDetail(item.IdProduto);
        //        if (productDetail != null)
        //        {
        //            if (productDetail.Classe == 3)
        //            {
        //                var InsertHistoricoAlocacao2025 = new HistoricoAlocacao_2025
        //                {
        //                    IdProduto = item.IdProduto,
        //                    Solicitante_IdTerceiro = idSolicitanteTerceiro,
        //                    Solicitante_CodColigada = codColigadaSolicitante,
        //                    Solicitante_Chapa = chapaSolicitante,                           
        //                    Liberador_IdTerceiro = idLiberadorTerceiro,
        //                    Liberador_CodColigada = codcoligadaLiberador,
        //                    Liberador_Chapa = chapaLiberador,
        //                    Balconista_Emprestimo_IdLogin = userId,
        //                    Balconista_Devolucao_IdLogin = userId,
        //                    Observacao = item.Observacao,
        //                    DataEmprestimo = DateTime.Now,
        //                    DataDevolucao = item.DataPrevistaDevolucao.HasValue ? item.DataPrevistaDevolucao : DateTime.Now,
        //                    IdObra = ObraEmprestimo,
        //                    Quantidade = item.Quantidade,
        //                    IdFerrOndeProdRetirado = FerramentariaValue,
        //                    IdControleCA = item.IdControleCA
        //                };

        //                using (var transaction = _context.Database.BeginTransaction())
        //                {
        //                    try
        //                    {
        //                        _context.Add(InsertHistoricoAlocacao2025);
        //                        Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
        //                        if (productToUpdate != null)
        //                        {
        //                            productToUpdate.Quantidade = productToUpdate.Quantidade - item.Quantidade;
        //                        }

        //                        _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
        //                        transaction.Commit(); // Commit the transaction

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        transaction.Rollback(); // Rollback the transaction in case of an exception
        //                                                // Optionally, log the exception or rethrow it

        //                        ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";
        //                        return View(nameof(Index));
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                string key = $"{item.IdProduto}-{codColigadaSolicitante}-{chapaSolicitante}-{userId}-{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}-{ObraEmprestimo}-{item.Quantidade}-{FerramentariaValue}";
        //                string hash;
        //                using (MD5 md5 = MD5.Create())
        //                {
        //                    byte[] inputBytes = Encoding.UTF8.GetBytes(key);
        //                    byte[] hashBytes = md5.ComputeHash(inputBytes);
        //                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        //                }


        //                if (productDetail.PorAferido == 1 || productDetail.PorSerial == 1)
        //                {
        //                    List<ProdutoAlocado>? checkAlocadoDuplicates = _context.ProdutoAlocado.Where(i => i.IdProduto == item.IdProduto).ToList();
        //                    if (checkAlocadoDuplicates.Count == 0)
        //                    {
        //                        var InsertProdutoAlocado = new ProdutoAlocado
        //                        {
        //                            IdProduto = item.IdProduto,
        //                            IdObra = ObraEmprestimo,
        //                            IdFerrOndeProdRetirado = FerramentariaValue,
        //                            Solicitante_IdTerceiro = idSolicitanteTerceiro,
        //                            Solicitante_CodColigada = codColigadaSolicitante,
        //                            Solicitante_Chapa = chapaSolicitante,
        //                            Balconista_IdLogin = userId,
        //                            Liberador_IdTerceiro = idLiberadorTerceiro,
        //                            Liberador_CodColigada = codcoligadaLiberador,
        //                            Liberador_Chapa = chapaLiberador,
        //                            Observacao = item.Observacao,
        //                            DataPrevistaDevolucao = item.DataPrevistaDevolucao,
        //                            DataEmprestimo = DateTime.Now,
        //                            Quantidade = item.Quantidade,
        //                            Chave = hash,
        //                            IdControleCA = item.IdControleCA

        //                        };

        //                        using (var transaction = _context.Database.BeginTransaction())
        //                        {
        //                            try
        //                            {
        //                                _context.Add(InsertProdutoAlocado);
        //                                Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);

        //                                if (productToUpdate != null )
        //                                {
        //                                    if (productToUpdate.Quantidade > 0)
        //                                    {
        //                                        productToUpdate.Quantidade = productToUpdate.Quantidade - item.Quantidade;
        //                                    }
        //                                    else
        //                                    {
        //                                        productToUpdate.Quantidade = 0;
        //                                    }

        //                                }

        //                                _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
        //                                transaction.Commit(); // Commit the transaction

        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                transaction.Rollback(); // Rollback the transaction in case of an exception
        //                                                        // Optionally, log the exception or rethrow it

        //                                ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";
        //                                return View(nameof(Index));
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ViewBag.Error = $"{item.IdProduto} - this ID is PorAferido/PorSerial that was found on the produtoAlocado.";
        //                        return View(nameof(Index));
        //                    }
        //                }
        //                else
        //                {
        //                    var InsertProdutoAlocado = new ProdutoAlocado
        //                    {
        //                        IdProduto = item.IdProduto,
        //                        IdObra = ObraEmprestimo,
        //                        IdFerrOndeProdRetirado = FerramentariaValue,
        //                        Solicitante_IdTerceiro = idSolicitanteTerceiro,
        //                        Solicitante_CodColigada = codColigadaSolicitante,
        //                        Solicitante_Chapa = chapaSolicitante,
        //                        Balconista_IdLogin = userId,
        //                        Liberador_IdTerceiro = idLiberadorTerceiro,
        //                        Liberador_CodColigada = codcoligadaLiberador,
        //                        Liberador_Chapa = chapaLiberador,
        //                        Observacao = item.Observacao,
        //                        DataPrevistaDevolucao = item.DataPrevistaDevolucao,
        //                        DataEmprestimo = DateTime.Now,
        //                        Quantidade = item.Quantidade,
        //                        Chave = hash,
        //                        IdControleCA = item.IdControleCA
        //                    };

        //                    using (var transaction = _context.Database.BeginTransaction())
        //                    {
        //                        try
        //                        {
        //                            _context.Add(InsertProdutoAlocado);

        //                            Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
        //                            if (productToUpdate != null)
        //                            {
        //                                if (productToUpdate.Quantidade > 0)
        //                                {
        //                                    productToUpdate.Quantidade = productToUpdate.Quantidade - item.Quantidade;
        //                                }
        //                                else
        //                                {
        //                                    productToUpdate.Quantidade = 0;
        //                                }
        //                            }

        //                            _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
        //                            transaction.Commit(); // Commit the transaction

        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            transaction.Rollback(); // Rollback the transaction in case of an exception
        //                                                    // Optionally, log the exception or rethrow it

        //                            ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";
        //                            return View(nameof(Index));
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //        else
        //        {
        //            ViewBag.Error = $"{item.IdProduto} - this Id was not found by the system.";
        //            return View(nameof(Index));
        //        }
        //    }

        //    return View(nameof(Index));
        //}

        public IActionResult submitTransaction(List<submittedEmprestimo> emprestimoList, string? chapaSolicitante, int? codColigadaSolicitante, int? idSolicitanteTerceiro,
                            string? chapaLiberador, int? codcoligadaLiberador, int? idLiberadorTerceiro, int? ObraEmprestimo)
        {

            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
            if (loggedUser != null)
            {
                PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                if (checkPermission != null)
                {
                    if (checkPermission.Visualizar == 1)
                    {
                        int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
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


                        if (emprestimoList == null || !emprestimoList.Any())
                        {
                            ViewBag.Error = "No items were submitted.";
                            return View(nameof(Index));
                        }

                        string? checkQuantitystring = CheckQuantityBackend(emprestimoList);
                        if (checkQuantitystring.IsNullOrEmpty() == false)
                        {
                            ViewBag.Error = checkQuantitystring;
                            return View(nameof(Index));
                        }


                        using (var transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                foreach (submittedEmprestimo item in emprestimoList)
                                {
                                    // Fetch product details
                                    var productDetail = getProductDetail(item.IdProduto);
                                    if (productDetail == null)
                                    {
                                        ViewBag.Error = $"Product with ID {item.IdProduto} not found.";
                                        transaction.Rollback();
                                        return View(nameof(Index));
                                    }

                                    Produto? productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                    if (productToUpdate == null || productToUpdate.Quantidade < item.Quantidade)
                                    {
                                        ViewBag.Error = $"Insufficient stock for Product ID {item.IdProduto}. Available: {productToUpdate?.Quantidade ?? 0}.";
                                        transaction.Rollback();
                                        return View(nameof(Index));
                                    }

                                    // Handle Class 3 products
                                    if (productDetail.Classe == 3)
                                    {
                                        var historico = new HistoricoAlocacao_2025
                                        {
                                            IdProduto = item.IdProduto,
                                            Solicitante_IdTerceiro = idSolicitanteTerceiro,
                                            Solicitante_CodColigada = codColigadaSolicitante,
                                            Solicitante_Chapa = chapaSolicitante,
                                            Liberador_IdTerceiro = idLiberadorTerceiro,
                                            Liberador_CodColigada = codcoligadaLiberador,
                                            Liberador_Chapa = chapaLiberador,
                                            Balconista_Emprestimo_IdLogin = loggedUser?.Id,
                                            Balconista_Devolucao_IdLogin = loggedUser?.Id,
                                            Observacao = item.Observacao,
                                            DataEmprestimo = DateTime.Now,
                                            DataDevolucao = item.DataPrevistaDevolucao ?? DateTime.Now,
                                            IdObra = ObraEmprestimo,
                                            Quantidade = item.Quantidade,
                                            IdFerrOndeProdRetirado = FerramentariaValue,
                                            IdControleCA = item.IdControleCA
                                        };

                                        _context.Add(historico);
                                    }
                                    else
                                    {
                                        // Generate a unique hash key
                                        string key = $"{item.IdProduto}-{codColigadaSolicitante}-{chapaSolicitante}-{loggedUser?.Id}-{DateTime.Now:dd/MM/yyyy HH:mm}-{ObraEmprestimo}-{item.Quantidade}-{FerramentariaValue}";
                                        string hash = GenerateMD5Hash(key);

                                        // Check for duplicates if PorAferido or PorSerial
                                        if (productDetail.PorAferido == 1 || productDetail.PorSerial == 1)
                                        {
                                            var existingAllocation = _context.ProdutoAlocado.Any(i => i.IdProduto == item.IdProduto);
                                            if (existingAllocation)
                                            {
                                                ViewBag.Error = $"Product ID {item.IdProduto} is already allocated (PorAferido/PorSerial).";
                                                transaction.Rollback();
                                                return View(nameof(Index));
                                            }
                                        }

                                        var produtoAlocado = new ProdutoAlocado
                                        {
                                            IdProduto = item.IdProduto,
                                            IdObra = ObraEmprestimo,
                                            IdFerrOndeProdRetirado = FerramentariaValue,
                                            Solicitante_IdTerceiro = idSolicitanteTerceiro,
                                            Solicitante_CodColigada = codColigadaSolicitante,
                                            Solicitante_Chapa = chapaSolicitante,
                                            Balconista_IdLogin = loggedUser?.Id,
                                            Liberador_IdTerceiro = idLiberadorTerceiro,
                                            Liberador_CodColigada = codcoligadaLiberador,
                                            Liberador_Chapa = chapaLiberador,
                                            Observacao = item.Observacao,
                                            DataPrevistaDevolucao = item.DataPrevistaDevolucao,
                                            DataEmprestimo = DateTime.Now,
                                            Quantidade = item.Quantidade,
                                            Chave = hash,
                                            IdControleCA = item.IdControleCA
                                        };

                                        _context.Add(produtoAlocado);
                                    }

                                    // Update product quantity
                                    productToUpdate.Quantidade -= item.Quantidade;
                                    _context.Update(productToUpdate);
                                }

                                _context.SaveChanges();
                                transaction.Commit();

                                TempData["ShowSuccessAlertEmprestimo"] = true;
                                return RedirectToAction(nameof(Index));
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                ViewBag.Error = $"SERVER ERROR: {ex.Message}";
                                return View(nameof(Index));
                            }
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


            //int? userId = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.ID);
            //LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
            //int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);





        }

        private string CheckQuantityBackend(List<submittedEmprestimo> emprestimoList)
        {
            foreach (submittedEmprestimo item in emprestimoList)
            {
                if (item.Quantidade == 0)
                {
                    return $"{item.IdProduto} - quantidade esta vazio.";
                }
            }

            return string.Empty;

        }

        private string GenerateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        #endregion

        #endregion

        public List<EmprestimoViewModel>? getProductList(string? codigo, string? item, string? af, int? pat)
        {
            int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

            var subquery = from pa in _context.ProdutoAlocado
                           join catalogo in _context.Catalogo on pa.IdProduto equals catalogo.Id
                           where catalogo.PorAferido == 1 && catalogo.PorSerial == 1
                           select pa.IdProduto;

            List<EmprestimoViewModel>? ProductList = (from produto in _context.Produto
                                                      join catalogo in _context.Catalogo
                                                      on produto.IdCatalogo equals catalogo.Id
                                                      join categoria in _context.Categoria
                                                      on catalogo.IdCategoria equals categoria.Id
                                                      join ferramentaria in _context.Ferramentaria
                                                      on produto.IdFerramentaria equals ferramentaria.Id
                                                      join empresa in _context.Empresa
                                                      on produto.IdEmpresa equals empresa.Id into empresaJoin
                                                      from empresa in empresaJoin.DefaultIfEmpty()
                                                      where produto.Ativo == 1
                                                         //&& !_context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id)
                                                         && !subquery.Contains(produto.Id)
                                                         && produto.Quantidade > 0
                                                         && (string.IsNullOrEmpty(codigo) || catalogo.Codigo.Contains(codigo))
                                                         && (string.IsNullOrEmpty(item) || EF.Functions.Like(catalogo.Nome, $"%{item}%"))
                                                         && (string.IsNullOrEmpty(af) || EF.Functions.Like(produto.AF, $"%{af}%"))
                                                         && (!pat.HasValue || EF.Functions.Like(produto.PAT.ToString(), $"%{pat}%"))
                                                         && ferramentaria.Id == FerramentariaValue
                                                         && produto.IdFerramentaria == FerramentariaValue
                                                      orderby ferramentaria.Nome, catalogo.Descricao
                                                      select new EmprestimoViewModel
                                                      {
                                                          IdProduto = produto.Id,
                                                          AF = produto.AF,
                                                          PAT = produto.PAT,
                                                          Quantidade = produto.Quantidade,
                                                          QuantidadeMinima = produto.QuantidadeMinima,
                                                          Localizacao = produto.Localizacao,
                                                          RFM = produto.RFM,
                                                          Observacao = produto.Observacao,
                                                          DataRegistroProduto = produto.DataRegistro,
                                                          DataVencimento = produto.DataVencimento,
                                                          Certificado = produto.Certificado,
                                                          Serie = produto.Serie,
                                                          AtivoProduto = produto.Ativo,
                                                          IdCatalogo = catalogo.Id,
                                                          Codigo = catalogo.Codigo,
                                                          NomeCatalogo = catalogo.Nome,
                                                          Descricao = catalogo.Descricao,
                                                          PorMetro = catalogo.PorMetro,
                                                          PorAferido = catalogo.PorAferido,
                                                          PorSerial = catalogo.PorSerial,
                                                          DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                          DataRegistroCatalogo = catalogo.DataRegistro,
                                                          AtivoCatalogo = catalogo.Ativo,
                                                          IdCategoria = categoria.Id,
                                                          IdCategoriaPai = categoria.IdCategoria,
                                                          Classe = categoria.Classe,
                                                          NomeCategoria = categoria.Nome,
                                                          DataRegistroCategoria = categoria.DataRegistro,
                                                          AtivoCategoria = categoria.Ativo,
                                                          IdFerramentaria = ferramentaria.Id,
                                                          NomeFerramentaria = ferramentaria.Nome,
                                                          DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                                          AtivoFerramentaria = ferramentaria.Ativo,
                                                          IdEmpresa = empresa.Id,
                                                          NomeEmpresa = empresa.Nome,
                                                          GerenteEmpresa = empresa.Gerente,
                                                          TelefoneEmpresa = empresa.Telefone,
                                                          DataRegistroEmpresa = empresa.DataRegistro,
                                                          AtivoEmpresa = empresa.Ativo,
                                                          //Status = _context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque")
                                                          Status = (subquery.Contains(produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque"))
                                                      }).ToList();

            return ProductList ?? new List<EmprestimoViewModel>();
        }

        public productDetails getProductDetail(int? id)
        {
            productDetails? productDetail = (from produto in _context.Produto
                                             join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                             join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                             where produto.Id == id
                                             select new productDetails
                                             {
                                                 IdProduto = produto.Id,
                                                 Codigo = catalogo.Codigo,
                                                 PorMetro = catalogo.PorMetro,
                                                 PorAferido = catalogo.PorAferido,
                                                 PorSerial = catalogo.PorSerial,
                                                 Classe = categoria.Classe
                                             }).FirstOrDefault();

            return productDetail ?? new productDetails();
        }


        public productCheck checkproduct(EmprestimoViewModel product, string? chapa, int? coligada)
        {
            //List<EmprestimoViewModel>? emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

            List<emprestimoCart>? emprestimoList = HttpContext.Session.GetObject<List<emprestimoCart>>(SessionKeyEmprestimoCart) ?? new List<emprestimoCart>();

            productCheck prodcheck = new productCheck();

            if (!emprestimoList.Any(item => item.IdProduto == product.IdProduto))
            {
                if (product.Quantidade <= 0)
                {
                    List<ProdutoAlocado>? produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == product.IdProduto).ToList() ?? new List<ProdutoAlocado>();
                    if (produtoAlocadoCheck.Count != 0)
                    {
                        prodcheck.success = false;
                        prodcheck.message = "A quantidade não está disponível para empréstimo porque o estoque do item é negativo!";
                        return prodcheck;
                    }
                    else
                    {
                        prodcheck.success = false;
                        prodcheck.message = "Quantidade indisponivel para emprestimo!";
                        return prodcheck;
                    }
                }
                else
                {
                    if (product.PorAferido == 1)
                    {
                        if (product.DataVencimento.HasValue)
                        {
                            if (product.DataVencimento.Value < DateTime.Now)
                            {
                                prodcheck.success = false;
                                prodcheck.message = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                return prodcheck;
                            }
                        }
                    }

                    if (product.PorAferido == 1 || product.PorSerial == 1)
                    {
                        List<ProdutoAlocado>? produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == product.IdProduto).ToList() ?? new List<ProdutoAlocado>();
                        if (produtoAlocadoCheck.Count > 0)
                        {
                            prodcheck.success = false;
                            prodcheck.message = $"Este item é emprestado pelo Chapa do funcionário:{produtoAlocadoCheck[0].Solicitante_Chapa}";
                            return prodcheck;
                        }
                    }

                    if (product.NomeCatalogo.Contains("INUTILIZAR"))
                    {
                        prodcheck.success = false;
                        prodcheck.message = $"O item informado foi inutilizado e não pode ser emprestado. Contate a Equipe de Suporte para acerto do código abaixo em seu setor. Codigo:{product.Codigo} {product.NomeCatalogo}";
                        return prodcheck;
                    }

                    var result = from catalogo in _context.Catalogo
                                 join produto in _context.Produto on catalogo.Id equals produto.IdCatalogo
                                 join produtoAlocado in _context.ProdutoAlocado on produto.Id equals produtoAlocado.IdProduto
                                 where catalogo.RestricaoEmprestimo == 1
                                    && catalogo.Id == product.IdCatalogo
                                    && produtoAlocado.Solicitante_CodColigada == coligada
                                    && produtoAlocado.Solicitante_Chapa == chapa
                                 select produtoAlocado.Id;

                    if (result.Any())
                    {
                        prodcheck.success = false;
                        prodcheck.message = "Item Marcado como Restrição de Emprestimo.O solicitante já possui um Item marcado como restrição de emprestimo sem devolver em sua ficha!";
                        return prodcheck;
                    }

                    if (product.Classe != 2)
                    {
                        BloqueioEmprestimoAoSolicitante? blockresult = _context.BloqueioEmprestimoAoSolicitante.FirstOrDefault(i => i.Chapa == chapa && i.Ativo == 1);
                        if (blockresult != null)
                        {
                            prodcheck.success = false;
                            prodcheck.message = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis. <br> Retire os itens que não sejão EPI da grid. <br> EPI pode ser liberado. Obs. Equipe de Suporte: {blockresult.Mensagem}";
                            return prodcheck;
                        }
                    }

                    if (product.IdCategoriaPai == 1384)
                    {
                        if (product.DataVencimento.HasValue)
                        {
                            if (product.DataVencimento.Value < DateTime.Now)
                            {
                                prodcheck.success = false;
                                prodcheck.message = $"Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                return prodcheck;
                            }
                        }
                    }

                    prodcheck.success = true;
                    prodcheck.message = string.Empty;
                    return prodcheck;


                }
            }
            else
            {
                prodcheck.success = false;
                prodcheck.message = "O produto já está no carrinho.";
                return prodcheck;
            }
        }

        public EmprestimoViewModel getmoreproductinfoIndi(int? idProduto)
        {
            int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);

            var subquery = from pa in _context.ProdutoAlocado
                           join catalogo in _context.Catalogo on pa.IdProduto equals catalogo.Id
                           where catalogo.PorAferido == 1 && catalogo.PorSerial == 1
                           select pa.IdProduto;

            EmprestimoViewModel? ProductInfo = (from produto in _context.Produto
                                                      join catalogo in _context.Catalogo
                                                      on produto.IdCatalogo equals catalogo.Id
                                                      join categoria in _context.Categoria
                                                      on catalogo.IdCategoria equals categoria.Id
                                                      join ferramentaria in _context.Ferramentaria
                                                      on produto.IdFerramentaria equals ferramentaria.Id
                                                      join empresa in _context.Empresa
                                                      on produto.IdEmpresa equals empresa.Id into empresaJoin
                                                      from empresa in empresaJoin.DefaultIfEmpty()
                                                      where produto.Ativo == 1
                                                         //&& !_context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id)
                                                         && !subquery.Contains(produto.Id)
                                                         && produto.Quantidade > 0
                                                         && produto.Id == idProduto
                                                         && ferramentaria.Id == FerramentariaValue
                                                         && produto.IdFerramentaria == FerramentariaValue
                                                      orderby ferramentaria.Nome, catalogo.Descricao
                                                      select new EmprestimoViewModel
                                                      {
                                                          IdProduto = produto.Id,
                                                          AF = produto.AF,
                                                          PAT = produto.PAT,
                                                          Quantidade = produto.Quantidade,
                                                          QuantidadeMinima = produto.QuantidadeMinima,
                                                          Localizacao = produto.Localizacao,
                                                          RFM = produto.RFM,
                                                          Observacao = produto.Observacao,
                                                          DataRegistroProduto = produto.DataRegistro,
                                                          DataVencimento = produto.DataVencimento,
                                                          Certificado = produto.Certificado,
                                                          Serie = produto.Serie,
                                                          AtivoProduto = produto.Ativo,
                                                          IdCatalogo = catalogo.Id,
                                                          Codigo = catalogo.Codigo,
                                                          NomeCatalogo = catalogo.Nome,
                                                          Descricao = catalogo.Descricao,
                                                          PorMetro = catalogo.PorMetro,
                                                          PorAferido = catalogo.PorAferido,
                                                          PorSerial = catalogo.PorSerial,
                                                          DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                          DataRegistroCatalogo = catalogo.DataRegistro,
                                                          AtivoCatalogo = catalogo.Ativo,
                                                          IdCategoria = categoria.Id,
                                                          IdCategoriaPai = categoria.IdCategoria,
                                                          Classe = categoria.Classe,
                                                          NomeCategoria = categoria.Nome,
                                                          DataRegistroCategoria = categoria.DataRegistro,
                                                          AtivoCategoria = categoria.Ativo,
                                                          IdFerramentaria = ferramentaria.Id,
                                                          NomeFerramentaria = ferramentaria.Nome,
                                                          DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                                          AtivoFerramentaria = ferramentaria.Ativo,
                                                          IdEmpresa = empresa.Id,
                                                          NomeEmpresa = empresa.Nome,
                                                          GerenteEmpresa = empresa.Gerente,
                                                          TelefoneEmpresa = empresa.Telefone,
                                                          DataRegistroEmpresa = empresa.DataRegistro,
                                                          AtivoEmpresa = empresa.Ativo,
                                                          //Status = _context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque")
                                                          Status = (subquery.Contains(produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque"))
                                                      }).FirstOrDefault() ?? new EmprestimoViewModel();


            if (ProductInfo.Classe == 2)
            {
                if (ProductInfo.DataDeRetornoAutomatico != 0)
                {
                    ProductInfo.DataEmprestimoFrontEnd = DateTime.Now.AddDays(ProductInfo.DataDeRetornoAutomatico.Value);
                }



                List<ControleCA>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == ProductInfo.IdCatalogo && i.Ativo == 1 && i.Validade > DateTime.Now).OrderByDescending(i => i.Validade).ToList() ?? new List<ControleCA>();
                if (controleCAData.Count > 0)
                {
                    ProductInfo.ControleCAList = controleCAData;
                }
            }

            if (ProductInfo.IdCategoriaPai == 1384)
            {
                if (ProductInfo.DataVencimento.HasValue)
                {
                    ProductInfo.DataEmprestimoFrontEnd = ProductInfo.DataVencimento.Value;
                }
            }


            return ProductInfo;
        }

        public EmprestimoViewModel getmoreproductinfo(EmprestimoViewModel product)
        {
            if (product.Classe == 2)
            {
                product.QuantityFrontEnd = 1;
                if (product.DataDeRetornoAutomatico != 0)
                {
                    product.DataEmprestimoFrontEnd = DateTime.Now.AddDays(product.DataDeRetornoAutomatico.Value);
                }

                List<ControleCA>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == product.IdCatalogo && i.Ativo == 1 && i.Validade > DateTime.Now).OrderByDescending(i => i.Validade).ToList() ?? new List<ControleCA>();
                if (controleCAData.Count > 0)
                {
                    product.ControleCAList = controleCAData;
                }
            }

            if (product.IdCategoriaPai == 1384)
            {
                if (product.DataVencimento.HasValue)
                {
                    product.DataEmprestimoFrontEnd = product.DataVencimento.Value;
                }
            }

            return product;
        }


        public ActionResult SearchSolicitante(string? IdSolicitante)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchSolicitante";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);


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
                if (usuariofer.Permissao.Visualizar != 1)
                {
                    usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                    log.LogWhy = usuariofer.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                }
            }
            #endregion

            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>(); //Brasfels Employee Result
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>(); //Third Party Employees Result
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>(); //Brasfels and Third Party Employees Result
            UserViewModel? UsuarioModel = new UserViewModel();

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            if (IdSolicitante != null)
            {
                //Searching using the Third Party Employees then adding it to the listTerceiroResult then it on the TotalResult
                listTerceiroResult = searches.SearchTercerio(IdSolicitante);
                TotalResult.AddRange(listTerceiroResult);

                //Searching using the Brasfels Employees then adding it to listEmployeeResult then add to TotalResult
                listEmployeeResult = searches.SearchEmployeeChapa(IdSolicitante);
                TotalResult.AddRange(listEmployeeResult);

                if (TotalResult.Count > 1)
                {
                    //If the Employee result is more than 1 then open the modal for the list of employees
                    ViewBag.ListOfEmployees = TotalResult;
                    string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                    if (!string.IsNullOrEmpty(LiberadorChapa))
                    {
                        UserViewModel? LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                        ViewBag.Liberador = LiberadorMOdel;
                        ViewBag.FormToken = sessionToken;
                    }
                    return View(nameof(Index));
                }
                else if (TotalResult.Count == 1)
                {
                    httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);
                    httpContextAccessor.HttpContext.Session.SetString(Sessao.Solicitante, TotalResult[0].Chapa);

                    //Get Employee Details
                    UsuarioModel = searches.SearchSolicitanteLoad(TotalResult[0].Chapa);
                    List<MensagemSolicitanteViewModel?> messages = new List<MensagemSolicitanteViewModel>();

                    //Get MensagemAoSolicitante
                    messages = searches.SearchMensagem(UsuarioModel.Chapa, usuariofer.Id);
                    if (messages.Count > 0)
                    {
                        ViewBag.Messages = messages;
                    }

                    string? blockMessage = searches.SearchBloqueioMessage(UsuarioModel.Chapa);
                    if (!string.IsNullOrEmpty(blockMessage))
                    {
                        ViewBag.BlockMessage = blockMessage;
                        UsuarioModel.BloqueioEmprestimo = true;
                        //BlockedEmprestimo = true;
                    }

                    //SolicitanteStaticModel = UsuarioModel;
                    ViewBag.Solicitante = UsuarioModel;

                    string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                    if (!string.IsNullOrEmpty(LiberadorChapa))
                    {
                        UserViewModel? LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                        ViewBag.Liberador = LiberadorMOdel;
                    }
                    ViewBag.FormToken = sessionToken;
                    return View(nameof(Index));

                }
                else if (TotalResult.Count == 0)
                {
                    ViewBag.Error = "No Searched has been found.";
                    string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                    if (!string.IsNullOrEmpty(LiberadorChapa))
                    {
                        UserViewModel? LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                        ViewBag.Liberador = LiberadorMOdel;
                    }
                    ViewBag.FormToken = sessionToken;
                    return View(nameof(Index));
                }

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index));
            }
            else
            {
                ViewBag.Error = "Matricula/Nome is Required";
                string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                if (!string.IsNullOrEmpty(LiberadorChapa))
                {
                    UserViewModel? LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                    ViewBag.Liberador = LiberadorMOdel;
                }
                ViewBag.FormToken = sessionToken;
                return View(nameof(Index));
            }

        }

        public ActionResult SearchLiberador(string? IdLiberador)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
            UserViewModel? UsuarioModel = new UserViewModel();

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            if (IdLiberador != null)
            {
                //Searching using the Third Party Employees then adding it to the listTerceiroResult then it on the TotalResult
                listTerceiroResult = searches.SearchTercerio(IdLiberador);
                TotalResult.AddRange(listTerceiroResult);

                //Searching using the Brasfels Employees then adding it to listEmployeeResult then add to TotalResult
                listEmployeeResult = searches.SearchEmployeeChapa(IdLiberador);
                TotalResult.AddRange(listEmployeeResult);

                if (TotalResult.Count > 1)
                {
                    //UserViewModel? SolicitanteModel = new UserViewModel();
                    //SolicitanteModel = searches.SearchSolicitanteLoad();
                    //if (SolicitanteModel != null)
                    //{

                    //}
                    string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                    if (!string.IsNullOrEmpty(SolicitanteChapa))
                    {
                        UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                        ViewBag.Solicitante = SolicitanteModel;
                    }
                    //ViewBag.Solicitante = SolicitanteStaticModel;
                    ViewBag.ListOfLiberador = TotalResult;

                    ViewBag.FormToken = sessionToken;
                    return View(nameof(Index));
                }
                else if (TotalResult.Count == 1)
                {
                    httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);
                    httpContextAccessor.HttpContext?.Session.SetString(Sessao.Liberador, TotalResult[0].Chapa);

                    UsuarioModel = searches.SearchLiberadorLoad(TotalResult[0].Chapa);
                    //LiberadorStaticModel = UsuarioModel;
                    ViewBag.Liberador = UsuarioModel;

                    //UserViewModel? SolicitanteModel = new UserViewModel();
                    //SolicitanteModel = searches.SearchSolicitanteLoad();

                    string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                    if (!string.IsNullOrEmpty(SolicitanteChapa))
                    {
                        UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                        ViewBag.Solicitante = SolicitanteModel;

                        //if (SolicitanteModel?.Secao != UsuarioModel.Secao)
                        //{
                        //    ViewBag.UserTest = true;
                        //}
                    }

                    //if (SolicitanteStaticModel != null)
                    //{
                    //    ViewBag.Solicitante = SolicitanteStaticModel;
                    //}



                    ViewBag.FormToken = sessionToken;

                    return View(nameof(Index));

                }
                else if (listEmployeeResult.Count == 0)
                {
                    //TempData["ShowErrorAlert"] = true;
                    ViewBag.Error = "No Searched has been found.";
                    string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                    if (!string.IsNullOrEmpty(SolicitanteChapa))
                    {
                        UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                        ViewBag.Solicitante = SolicitanteModel;
                    }
                    ViewBag.FormToken = sessionToken;
                    return View(nameof(Index));
                    //ViewBag.Error = "No Searched has been found.";
                    //return View("Index", UsuarioModel);
                    //return RedirectToAction(nameof(Index));
                }
                ViewBag.FormToken = sessionToken;
                return View("Index", UsuarioModel);
            }
            else
            {
                //ViewBag.Error = "Matricula/Nome is Required";

                //TempData["ShowErrorAlert"] = true;
                ViewBag.Error = "Matricula/Nome is Required.";
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    ViewBag.Solicitante = SolicitanteModel;
                }

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index));
                //return View("Index", UsuarioModel);
                //return RedirectToAction(nameof(Index));
            }

        }

        public ActionResult SelectedUser(string? chapa)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();

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
                if (usuariofer.Permissao.Visualizar != 1)
                {
                    usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                    log.LogWhy = usuariofer.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                }
            }
            #endregion

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            if (chapa != null)
            {
                httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.Solicitante, chapa);

                //Get Employee Details
                UsuarioModel = searches.SearchSolicitanteLoad(chapa);
                List<MensagemSolicitanteViewModel?> messages = new List<MensagemSolicitanteViewModel>();

                //Get MensagemAoSolicitante
                messages = searches.SearchMensagem(UsuarioModel.Chapa, usuariofer.Id);
                if (messages.Count > 0)
                {
                    ViewBag.Messages = messages;
                }

                string? blockMessage = searches.SearchBloqueioMessage(UsuarioModel.Chapa);
                if (!string.IsNullOrEmpty(blockMessage))
                {
                    UsuarioModel.BloqueioEmprestimo = true;
                    //BlockedEmprestimo = true;
                    ViewBag.BlockMessage = blockMessage;
                }

                //SolicitanteStaticModel = UsuarioModel;
                ViewBag.Solicitante = UsuarioModel;

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index));
            }

            ViewBag.FormToken = sessionToken;
            return View(nameof(Index));
            //return RedirectToAction(nameof(Index));
        }

        public ActionResult SelectedLiberador(string? chapa)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();
            //UserViewModel? SolicitanteModel = new UserViewModel();

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            if (chapa != null)
            {
                //httpContextAccessor.HttpContext.Session.Remove(Sessao.Liberador);
                //httpContextAccessor.HttpContext.Session.SetString(Sessao.Liberador, chapa);

                httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);
                httpContextAccessor.HttpContext?.Session.SetString(Sessao.Liberador, chapa);

                UsuarioModel = searches.SearchLiberadorLoad(chapa);
                //SolicitanteModel = searches.SearchSolicitanteLoad();

                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    ViewBag.Solicitante = SolicitanteModel;

                    //if (SolicitanteModel.Secao != UsuarioModel.Secao)
                    //{
                    //    ViewBag.UserTest = true;
                    //}
                }




                //LiberadorStaticModel = UsuarioModel;
                ViewBag.Liberador = UsuarioModel;
                //ViewBag.Solicitante = SolicitanteStaticModel;

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult DeleteMessage(int? id)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            var model = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();
            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            if (id != null)
            {
                var DeleteMessage = _context.MensagemSolicitante.FirstOrDefault(i => i.Id == id);
                if (DeleteMessage != null)
                {
                    DeleteMessage.Ativo = 0;
                    _context.SaveChanges();
                }

                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    UserViewModel? SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    ViewBag.Solicitante = SolicitanteModel;
                }

                //ViewBag.Solicitante = SolicitanteStaticModel != null ? SolicitanteStaticModel : new UserViewModel();
                ViewBag.ShowSuccessAlert = true;

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index), model);
            }
            else
            {
                ViewBag.Error = "Chapa is Empty";

                ViewBag.FormToken = sessionToken;
                return View(nameof(Index), model);
            }

        }

        //public ActionResult GetEmprestimo(string? CodigoEmprestimo, string? ItemEmprestimo, string? AFEmprestimo, int? PATEmprestimo)
        //{
        //    if (CodigoEmprestimo != null)
        //    {
        //        int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

        //        #region Query Produto
        //        var subquery = from pa in _context.ProdutoAlocado
        //                       join catalogo in _context.Catalogo on pa.IdProduto equals catalogo.Id
        //                       where catalogo.PorAferido == 1 && catalogo.PorSerial == 1
        //                       select pa.IdProduto;

        //        var queryemprestimo = from produto in _context.Produto
        //                              join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                              join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
        //                              join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
        //                              join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaGroup
        //                              from empresa in empresaGroup.DefaultIfEmpty()
        //                              where produto.Ativo == 1 &&
        //                                    !subquery.Contains(produto.Id) &&
        //                                    produto.Quantidade != 0
        //                                    && (CodigoEmprestimo == null || catalogo.Codigo.Contains(CodigoEmprestimo))
        //                                     //&& (ItemEmprestimo == null || catalogo.Nome.Contains(ItemEmprestimo))
        //                                    && (ItemEmprestimo == null || catalogo.Nome.IndexOf(ItemEmprestimo, StringComparison.OrdinalIgnoreCase) >= 0)
        //                                    && (AFEmprestimo == null || produto.AF.Contains(AFEmprestimo))
        //                                    && (PATEmprestimo == null || produto.PAT == PATEmprestimo)
        //                                    && ferramentaria.Id == FerramentariaValue
        //                              orderby ferramentaria.Nome, catalogo.Descricao
        //                              select new EmprestimoViewModel
        //                              {
        //                                  DC_DataAquisicao = produto.DC_DataAquisicao,
        //                                  DC_Valor = produto.DC_Valor,
        //                                  DC_AssetNumber = produto.DC_AssetNumber,
        //                                  DC_Fornecedor = produto.DC_Fornecedor,
        //                                  GC_Contrato = produto.GC_Contrato,
        //                                  GC_DataInicio = produto.GC_DataInicio,
        //                                  GC_IdObra = produto.GC_IdObra,
        //                                  GC_OC = produto.GC_OC,
        //                                  GC_DataSaida = produto.GC_DataSaida,
        //                                  GC_NFSaida = produto.GC_NFSaida,
        //                                  Selo = produto.Selo,
        //                                  IdProduto = produto.Id,
        //                                  AF = produto.AF,
        //                                  PAT = produto.PAT,
        //                                  Quantidade = produto.Quantidade,
        //                                  QuantidadeMinima = produto.QuantidadeMinima,
        //                                  Localizacao = produto.Localizacao,
        //                                  RFM = produto.RFM,
        //                                  Observacao = produto.Observacao,
        //                                  DataRegistroProduto = produto.DataRegistro,
        //                                  DataVencimento = produto.DataVencimento,
        //                                  Certificado = produto.Certificado,
        //                                  Serie = produto.Serie,
        //                                  AtivoProduto = produto.Ativo,
        //                                  IdCatalogo = catalogo.Id,
        //                                  Codigo = catalogo.Codigo,
        //                                  NomeCatalogo = catalogo.Nome,
        //                                  Descricao = catalogo.Descricao,
        //                                  PorMetro = catalogo.PorMetro,
        //                                  PorAferido = catalogo.PorAferido,
        //                                  PorSerial = catalogo.PorSerial,
        //                                  DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
        //                                  DataRegistroCatalogo = catalogo.DataRegistro,
        //                                  AtivoCatalogo = catalogo.Ativo,
        //                                  IdCategoria = categoria.Id,
        //                                  IdCategoriaPai = categoria.IdCategoria,
        //                                  Classe = categoria.Classe,
        //                                  NomeCategoria = categoria.Nome,
        //                                  DataRegistroCategoria = categoria.DataRegistro,
        //                                  AtivoCategoria = categoria.Ativo,
        //                                  IdFerramentaria = ferramentaria.Id,
        //                                  NomeFerramentaria = ferramentaria.Nome,
        //                                  DataRegistroFerramentaria = ferramentaria.DataRegistro,
        //                                  AtivoFerramentaria = ferramentaria.Ativo,
        //                                  IdEmpresa = empresa.Id,
        //                                  NomeEmpresa = empresa.Nome,
        //                                  GerenteEmpresa = empresa.Gerente,
        //                                  TelefoneEmpresa = empresa.Telefone,
        //                                  DataRegistroEmpresa = empresa.DataRegistro,
        //                                  AtivoEmpresa = empresa.Ativo,
        //                                  Status = (subquery.Contains(produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque"))
        //                              };

        //        var resultemprestimo = queryemprestimo.ToList();

        //        EmprestimoList = resultemprestimo;
        //        //GlobalData.EmprestimoList = resultemprestimo;

        //        if (list != null)
        //        {
        //            var idsToRemove = list.Select(item => item.IdProduto).ToList();

        //            //GlobalData.EmprestimoList = GlobalData.EmprestimoList
        //            //                            .Where(item => !idsToRemove.Contains(item.IdProduto))
        //            //                            .ToList();
        //            EmprestimoList = EmprestimoList
        //                                        .Where(item => !idsToRemove.Contains(item.IdProduto))
        //                                        .ToList();
        //        }
        //        else
        //        {
        //            EmprestimoList = resultemprestimo;
        //        }

        //        ViewBag.Emprestimo = EmprestimoList;

        //        ViewBag.OpenModal = true;

        //        #endregion
        //    }
        //    else
        //    {
        //        TempData["ShowErrorAlert"] = true;
        //        TempData["ErrorMessage"] = "No Usuario found!";

        //        return RedirectToAction(nameof(Index));
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        public ActionResult SetFerramentariaValue(int? Ferramentaria, string? SelectedNome)
        {
            if (Ferramentaria != null)
            {
                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)Ferramentaria);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.FerramentariaNome, SelectedNome);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult SearchProductUsingJavascript(string? CodigoEmprestimo, string? ItemEmprestimo, string? AFEmprestimo, int? PATEmprestimo)
        {
            return null;
        }

        public ActionResult SearchProduct(string? CodigoEmprestimo, string? ItemEmprestimo, string? AFEmprestimo, int? PATEmprestimo)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/SearchProduct";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            UserViewModel? SolicitanteModel = new UserViewModel();
            UserViewModel? LiberadorMOdel = new UserViewModel();

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                ViewBag.Solicitante = SolicitanteModel;
            }

            string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
            if (!string.IsNullOrEmpty(LiberadorChapa))
            {
                LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                ViewBag.Liberador = LiberadorMOdel;
            }



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
                if (usuariofer.Permissao.Visualizar != 1)
                {
                    usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                    log.LogWhy = usuariofer.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuariofer);
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(SolicitanteModel?.Chapa) && !string.IsNullOrEmpty(LiberadorMOdel?.Chapa))
            {

                int? FerramentariaValue = httpContextAccessor.HttpContext?.Session.GetInt32(Sessao.Ferramentaria);
                if (FerramentariaValue == null)
                {
                    List<FerramentariaViewModel> ferramentarias = searches.SearchFerramentariaBalconista(usuariofer.Id);
                    if (ferramentarias != null)
                    {
                        ViewBag.FerramentariaItems = ferramentarias;
                    }
                    return PartialView("_FerramentariaPartialView");
                }

                if (CodigoEmprestimo != null || ItemEmprestimo != null || AFEmprestimo != null || PATEmprestimo != null)
                {

                    var subquery = from pa in _context.ProdutoAlocado
                                   join catalogo in _context.Catalogo on pa.IdProduto equals catalogo.Id
                                   where catalogo.PorAferido == 1 && catalogo.PorSerial == 1
                                   select pa.IdProduto;

                    List<EmprestimoViewModel?> ProductList = (from produto in _context.Produto
                                                              join catalogo in _context.Catalogo
                                                              on produto.IdCatalogo equals catalogo.Id
                                                              join categoria in _context.Categoria
                                                              on catalogo.IdCategoria equals categoria.Id
                                                              join ferramentaria in _context.Ferramentaria
                                                              on produto.IdFerramentaria equals ferramentaria.Id
                                                              join empresa in _context.Empresa
                                                              on produto.IdEmpresa equals empresa.Id into empresaJoin
                                                              from empresa in empresaJoin.DefaultIfEmpty()
                                                              where produto.Ativo == 1
                                                                 //&& !_context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id)
                                                                 && !subquery.Contains(produto.Id)
                                                                 && produto.Quantidade != 0
                                                                 && (string.IsNullOrEmpty(CodigoEmprestimo) || catalogo.Codigo.Contains(CodigoEmprestimo))
                                                                 //&& (string.IsNullOrEmpty(ItemEmprestimo) || catalogo.Nome.Contains(ItemEmprestimo))
                                                                 && (string.IsNullOrEmpty(ItemEmprestimo) || EF.Functions.Like(catalogo.Nome, $"%{ItemEmprestimo}%"))
                                                                 //&& (string.IsNullOrEmpty(AFEmprestimo) || produto.AF.Contains(AFEmprestimo))
                                                                 && (string.IsNullOrEmpty(AFEmprestimo) || EF.Functions.Like(produto.AF, $"%{AFEmprestimo}%"))
                                                                 //&& (!PATEmprestimo.HasValue || produto.PAT == PATEmprestimo.Value)
                                                                 && (!PATEmprestimo.HasValue || EF.Functions.Like(produto.PAT.ToString(), $"%{PATEmprestimo}%"))
                                                                 //&& catalogo.PorAferido == 1
                                                                 //&& catalogo.PorSerial == 1
                                                                 && ferramentaria.Id == FerramentariaValue
                                                              orderby ferramentaria.Nome, catalogo.Descricao
                                                              select new EmprestimoViewModel
                                                              {
                                                                  DC_DataAquisicao = produto.DC_DataAquisicao,
                                                                  DC_Valor = produto.DC_Valor,
                                                                  DC_AssetNumber = produto.DC_AssetNumber,
                                                                  DC_Fornecedor = produto.DC_Fornecedor,
                                                                  GC_Contrato = produto.GC_Contrato,
                                                                  GC_DataInicio = produto.GC_DataInicio,
                                                                  GC_IdObra = produto.GC_IdObra,
                                                                  GC_OC = produto.GC_OC,
                                                                  GC_DataSaida = produto.GC_DataSaida,
                                                                  GC_NFSaida = produto.GC_NFSaida,
                                                                  Selo = produto.Selo,
                                                                  IdProduto = produto.Id,
                                                                  AF = produto.AF,
                                                                  PAT = produto.PAT,
                                                                  Quantidade = produto.Quantidade,
                                                                  QuantidadeMinima = produto.QuantidadeMinima,
                                                                  Localizacao = produto.Localizacao,
                                                                  RFM = produto.RFM,
                                                                  Observacao = produto.Observacao,
                                                                  DataRegistroProduto = produto.DataRegistro,
                                                                  DataVencimento = produto.DataVencimento,
                                                                  Certificado = produto.Certificado,
                                                                  Serie = produto.Serie,
                                                                  AtivoProduto = produto.Ativo,
                                                                  IdCatalogo = catalogo.Id,
                                                                  Codigo = catalogo.Codigo,
                                                                  NomeCatalogo = catalogo.Nome,
                                                                  Descricao = catalogo.Descricao,
                                                                  PorMetro = catalogo.PorMetro,
                                                                  PorAferido = catalogo.PorAferido,
                                                                  PorSerial = catalogo.PorSerial,
                                                                  DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                                  DataRegistroCatalogo = catalogo.DataRegistro,
                                                                  AtivoCatalogo = catalogo.Ativo,
                                                                  IdCategoria = categoria.Id,
                                                                  IdCategoriaPai = categoria.IdCategoria,
                                                                  Classe = categoria.Classe,
                                                                  NomeCategoria = categoria.Nome,
                                                                  DataRegistroCategoria = categoria.DataRegistro,
                                                                  AtivoCategoria = categoria.Ativo,
                                                                  IdFerramentaria = ferramentaria.Id,
                                                                  NomeFerramentaria = ferramentaria.Nome,
                                                                  DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                                                  AtivoFerramentaria = ferramentaria.Ativo,
                                                                  IdEmpresa = empresa.Id,
                                                                  NomeEmpresa = empresa.Nome,
                                                                  GerenteEmpresa = empresa.Gerente,
                                                                  TelefoneEmpresa = empresa.Telefone,
                                                                  DataRegistroEmpresa = empresa.DataRegistro,
                                                                  AtivoEmpresa = empresa.Ativo,
                                                                  //Status = _context.ProdutoAlocado.Any(pa => pa.IdProduto == produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque")
                                                                  Status = (subquery.Contains(produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque"))
                                                              }).ToList();

                    if (ProductList.Count == 1)
                    {
                        var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

                        //if (!RegisteredProductResult.Any(item => item.IdProduto == ProductList[0].IdProduto))
                        if (!emprestimoList.Any(item => item.IdProduto == ProductList[0].IdProduto))
                        {

                            if (ProductList[0].Quantidade <= 0)
                            {
                                List<ProdutoAlocado?> produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == ProductList[0].IdProduto).ToList();
                                if (produtoAlocadoCheck.Count != 0)
                                {
                                    ViewBag.Error = "Quantidade indisponivel para emprestimo!";
                                    ViewBag.Obra = StaticObra;
                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                    ViewBag.FormToken = sessionToken;

                                    return View(nameof(Index), emprestimoList);
                                }
                                else
                                {
                                    ViewBag.Error = "Quantidade indisponivel para emprestimo!";
                                    ViewBag.Obra = StaticObra;
                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                    ViewBag.FormToken = sessionToken;

                                    return View(nameof(Index), emprestimoList);
                                }
                            }
                            else
                            {
                                if (ProductList[0].PorAferido == 1)
                                {
                                    // ProductList[0].IdProduto is not in the RegisteredProductResult list
                                    if (ProductList[0].DataVencimento.HasValue)
                                    {
                                        if (ProductList[0].DataVencimento.Value < DateTime.Now)
                                        {
                                            ViewBag.Error = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                            ViewBag.Obra = StaticObra;
                                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                            ViewBag.FormToken = sessionToken;

                                            return View(nameof(Index), emprestimoList);
                                        }
                                    }
                                }

                                if (ProductList[0].PorAferido == 1 || ProductList[0].PorSerial == 1)
                                {
                                    List<ProdutoAlocado?> produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == ProductList[0].IdProduto).ToList();
                                    if (produtoAlocadoCheck.Count > 0)
                                    {
                                        ViewBag.Error = $"Este item é emprestado pelo Chapa do funcionário:{produtoAlocadoCheck[0].Solicitante_Chapa}";
                                        ViewBag.Obra = StaticObra;
                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                        ViewBag.FormToken = sessionToken;

                                        return View(nameof(Index), emprestimoList);
                                    }
                                }

                                //var result = _context.Catalogo.Where(i => i.Id == ProductList[0].IdCatalogo).FirstOrDefault();
                                //if (result != null)
                                //{
                                //    if (result.RestricaoEmprestimo == 1)
                                //    {
                                //        ViewBag.Error = "Item Marcado como Restrição de Emprestimo.";
                                //        ViewBag.Obra = StaticObra;
                                //        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                //        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                //        return View(nameof(Index), emprestimoList);
                                //    }

                                //}

                                var result = from catalogo in _context.Catalogo
                                             join produto in _context.Produto on catalogo.Id equals produto.IdCatalogo
                                             join produtoAlocado in _context.ProdutoAlocado on produto.Id equals produtoAlocado.IdProduto
                                             where catalogo.RestricaoEmprestimo == 1
                                                && catalogo.Id == ProductList[0].IdCatalogo
                                                && produtoAlocado.Solicitante_CodColigada == SolicitanteModel.CodColigada
                                                && produtoAlocado.Solicitante_Chapa == SolicitanteModel.Chapa
                                             select produtoAlocado.Id;

                                if (result.Any())
                                {
                                    ViewBag.Error = "Item Marcado como Restrição de Emprestimo.O solicitante já possui um Item marcado como restrição de emprestimo sem devolver em sua ficha!";
                                    ViewBag.Obra = StaticObra;
                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                    ViewBag.FormToken = sessionToken;

                                    return View(nameof(Index), emprestimoList);
                                }

                                if (ProductList[0].NomeCatalogo.Contains("INUTILIZAR"))
                                {
                                    ViewBag.Error = $"O item informado foi inutilizado e não pode ser emprestado. Contate a Equipe de Suporte para acerto do código abaixo em seu setor. Codigo{ProductList[0].Codigo} {ProductList[0].NomeCatalogo}";
                                    ViewBag.Obra = StaticObra;
                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                    ViewBag.FormToken = sessionToken;

                                    return View(nameof(Index), emprestimoList);
                                }

                                if (ProductList[0].Classe != 2)
                                {
                                    if (SolicitanteModel.BloqueioEmprestimo == true)
                                    {
                                        string? blockMessage = searches.SearchBloqueioMessage(SolicitanteModel.Chapa);
                                        if (!string.IsNullOrEmpty(blockMessage))
                                        {
                                            //ViewBag.Error = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis.Retire os itens que não sejão EPI da grid.EPI pode ser liberado. Obs. Equipe de Suporte: {blockMessage}";
                                            ViewBag.ErrorForBloqueio = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis. <br> Retire os itens que não sejão EPI da grid. <br> EPI pode ser liberado. Obs. Equipe de Suporte: {blockMessage}";
                                            ViewBag.Obra = StaticObra;
                                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                            ViewBag.FormToken = sessionToken;

                                            return View(nameof(Index), emprestimoList);
                                        }
                                    }
                                }

                                //if (SolicitanteModel.Secao != LiberadorMOdel.Secao)
                                //{
                                //    int? SupervisorAccepted = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorAccepted);

                                //    if (SupervisorAccepted != 1)
                                //    {
                                //        ViewBag.Error = "Transação falhou porque não foi aprovada pelo supervisor, digite novamente o Liberador e insira a senha para continuar esta transação.";
                                //        ViewBag.Obra = StaticObra;
                                //        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                //        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                //        return View(nameof(Index), emprestimoList);
                                //    }
                                //}

                                if (ProductList[0].Classe == 2)
                                {
                                    ProductList[0].QuantityFrontEnd = 1;
                                    if (ProductList[0].DataDeRetornoAutomatico != 0)
                                    {
                                        ProductList[0].DataEmprestimoFrontEnd = DateTime.Now.AddDays(ProductList[0].DataDeRetornoAutomatico.Value);
                                    }
                                    else
                                    {

                                    }


                                    List<ControleCA?>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == ProductList[0].IdCatalogo && i.Ativo == 1).OrderByDescending(i => i.Validade).ToList();
                                    if (controleCAData.Count > 0)
                                    {
                                        //ProductList[0].ControleCA = controleCAData.NumeroCA;
                                        //ProductList[0].ControleCADate = controleCAData.Validade;
                                        //ProductList[0].IdControleCA = controleCAData.Id;
                                        ProductList[0].ControleCAList = controleCAData;
                                    }

                                }


                                if (ProductList[0].IdCategoriaPai == 1384)
                                {
                                    if (ProductList[0].DataVencimento.HasValue)
                                    {
                                        if (ProductList[0].DataVencimento.Value < DateTime.Now)
                                        {
                                            ViewBag.Error = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                            ViewBag.Obra = StaticObra;
                                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                            ViewBag.FormToken = sessionToken;

                                            return View(nameof(Index), emprestimoList);
                                        }
                                        else
                                        {
                                            ProductList[0].DataEmprestimoFrontEnd = ProductList[0].DataVencimento.Value;
                                        }
                                    }
                                }

                                if (ProductList[0].Classe == 3)
                                {
                                    ProductList[0].QuantityFrontEnd = 1;
                                }

                                HttpContext.Session.SetObject(SessionKeyProductList, ProductList);

                                //ProductList[0].QuantityFrontEnd = 1;
                                //RegisteredProductResult.Add(ProductList[0]);
                                ViewBag.ProductDetails = ProductList[0];
                                //ProductSearchResult.Add(ProductList[0]);

                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                ViewBag.Obra = StaticObra;
                                ViewBag.FormToken = sessionToken;
                                return View(nameof(Index), emprestimoList);
                            }

                        }
                        else
                        {
                            ViewBag.Error = "O produto já está no carrinho.";

                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                            ViewBag.Obra = StaticObra;
                            ViewBag.FormToken = sessionToken;
                            return View(nameof(Index), emprestimoList);
                        }


                    }
                    else if (ProductList.Count > 1)
                    {
                        var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

                        if (emprestimoList.Count > 0)
                        {
                            var obraquery = _context.Obra.Where(s => s.Ativo == 1).OrderBy(s => s.Codigo).ToList();
                            ViewBag.Obra = obraquery;
                            var idsToRemove = emprestimoList.Select(item => item.IdProduto);

                            ProductList.RemoveAll(item => idsToRemove.Contains(item.IdProduto));

                            //ProductSearchResult = ProductList;

                            //return RedirectToAction(nameof(Index));
                        }

                        //ProductSearchResult = ProductList;
                        HttpContext.Session.SetObject(SessionKeyProductList, ProductList);

                        ViewBag.Emprestimo = ProductList;
                        ViewBag.Liberador = LiberadorMOdel;
                        ViewBag.Solicitante = SolicitanteModel;
                        ViewBag.Obra = StaticObra;
                        ViewBag.FormToken = sessionToken;
                        return View(nameof(Index), emprestimoList);
                        //return RedirectToAction(nameof(Index));

                        //ViewBag.Emprestimo = ProductSearchResult;
                        //ViewBag.Liberador = LiberadorStaticModel;
                        //ViewBag.Solicitante = SolicitanteStaticModel;

                        //return View(nameof(Index));
                    }
                    else
                    {
                        var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

                        ViewBag.Error = "Nenhum registro encontrado.";
                        ViewBag.Liberador = LiberadorMOdel;
                        ViewBag.Solicitante = SolicitanteModel;
                        ViewBag.Obra = StaticObra;
                        ViewBag.FormToken = sessionToken;
                        if (emprestimoList.Count > 0)
                        {
                            return View(nameof(Index), emprestimoList);
                        }
                        else
                        {
                            return View(nameof(Index));
                        }

                        //TempData["ErrorMessage"] = "Nenhum registro encontrado.";
                        //return RedirectToAction(nameof(Index));

                        //ViewBag.Error = "Nenhum registro encontrado.";
                        //ViewBag.Liberador = LiberadorStaticModel;
                        //ViewBag.Solicitante = SolicitanteStaticModel;
                        //return View(nameof(Index));
                    }

                }
                else
                {
                    var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();
                    ViewBag.Error = "Filtros de produto estão vazios.";
                    ViewBag.Liberador = LiberadorMOdel;
                    ViewBag.Solicitante = SolicitanteModel;
                    ViewBag.Obra = StaticObra;
                    ViewBag.FormToken = sessionToken;
                    if (emprestimoList.Count > 0)
                    {
                        return View(nameof(Index), emprestimoList);
                    }
                    else
                    {
                        return View(nameof(Index));
                    }
                }

            }
            else
            {
                var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

                ViewBag.Error = "No Solicitante e Liberador.";
                ViewBag.Obra = StaticObra;
                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                ViewBag.FormToken = sessionToken;

                return View(nameof(Index), emprestimoList);
            }
        }

        public ActionResult FinalizeCart(int? IdProduto, string? Observacao, int? Quantity, DateTime? DataRetorno, int? IdControleCA)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? SolicitanteModel = new UserViewModel();

            var sessionToken = HttpContext.Session.GetString("FormToken");
            ViewBag.FormToken = sessionToken;

            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                ViewBag.Solicitante = SolicitanteModel;
            }

            UserViewModel? LiberadorMOdel = new UserViewModel();
            string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
            if (!string.IsNullOrEmpty(LiberadorChapa))
            {
                LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                ViewBag.Liberador = LiberadorMOdel;
            }

            ViewBag.FormToken = sessionToken;

            var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

            if (IdProduto != null)
            {
                var ProductListModel = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyProductList) ?? new List<EmprestimoViewModel>();
                EmprestimoViewModel? GetProductDetails = ProductListModel.FirstOrDefault(i => i.IdProduto == IdProduto);
                if (GetProductDetails != null)
                {
                    if (IdControleCA != null)
                    {
                        ControleCA? value = _context.ControleCA.FirstOrDefault(i => i.Id == IdControleCA);

                        if (value != null)
                        {
                            GetProductDetails.IdControleCA = value.Id;
                            GetProductDetails.ControleCA = value.NumeroCA;
                            GetProductDetails.ControleCADate = value.Validade;
                        }
                    }

                    GetProductDetails.ObservacaoFrontEnd = Observacao;
                    GetProductDetails.QuantityFrontEnd = Quantity;
                    GetProductDetails.DataEmprestimoFrontEnd = DataRetorno;


                    emprestimoList.Add(GetProductDetails);
                    HttpContext.Session.SetObject(SessionKeyEmprestimoList, emprestimoList);

                    //RegisteredProductResult.Add(GetProductDetails);

                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                    ViewBag.Obra = StaticObra;

                    return View(nameof(Index), emprestimoList);
                }
                else
                {
                    ViewBag.Error = "IdProduto is null.";
                    return View(nameof(Index), emprestimoList);
                }

            }
            else
            {

                ViewBag.Error = "IdProduto is null.";
                ViewBag.Obra = StaticObra;
                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();

                return View(nameof(Index), emprestimoList);
            }

        }

        public ActionResult AddToCart(int produtoid)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Visualizar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de visualizar a página de Emprestimo!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                UserViewModel? SolicitanteModel = new UserViewModel();
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    ViewBag.Solicitante = SolicitanteModel;
                }

                UserViewModel? LiberadorMOdel = new UserViewModel();
                string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                if (!string.IsNullOrEmpty(LiberadorChapa))
                {
                    LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                    ViewBag.Liberador = LiberadorMOdel;
                }

                var sessionToken = HttpContext.Session.GetString("FormToken");
                ViewBag.FormToken = sessionToken;

                var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();
                var ProductListModel = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyProductList) ?? new List<EmprestimoViewModel>();

                EmprestimoViewModel? productDetails = ProductListModel.FirstOrDefault(i => i.IdProduto == produtoid);
                if (productDetails != null)
                {
                    if (!emprestimoList.Any(item => item.IdProduto == productDetails.IdProduto))
                    {
                        if (productDetails.Quantidade <= 0)
                        {
                            List<ProdutoAlocado?> produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == productDetails.IdProduto).ToList();

                            if (produtoAlocadoCheck.Count != 0)
                            {
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                                ViewBag.Error = "Quantidade indisponivel para emprestimo!";
                                ViewBag.Obra = StaticObra;
                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                return View(nameof(Index), emprestimoList);
                            }
                            else
                            {
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                                ViewBag.Error = "Quantidade indisponivel para emprestimo!";
                                ViewBag.Obra = StaticObra;
                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                return View(nameof(Index), emprestimoList);
                            }
                        }
                        else
                        {
                            if (productDetails.PorAferido == 1)
                            {
                                // ProductList[0].IdProduto is not in the RegisteredProductResult list
                                if (productDetails.DataVencimento.HasValue)
                                {
                                    if (productDetails.DataVencimento.Value < DateTime.Now)
                                    {
                                        ViewBag.Error = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                        //ViewBag.Emprestimo = ProductSearchResult.Count > 1 ? ProductSearchResult : new List<EmprestimoViewModel?>();
                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                        ViewBag.Obra = StaticObra;

                                        return View(nameof(Index), emprestimoList);
                                    }
                                }
                            }

                            if (productDetails.PorAferido == 1 || productDetails.PorSerial == 1)
                            {
                                List<ProdutoAlocado?> produtoAlocadoCheck = _context.ProdutoAlocado.Where(i => i.IdProduto == productDetails.IdProduto).ToList();
                                if (produtoAlocadoCheck.Count > 0)
                                {
                                    ViewBag.Error = $"Este item é emprestado pelo Chapa do funcionário:{produtoAlocadoCheck[0].Solicitante_Chapa}";
                                    ViewBag.Obra = StaticObra;
                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                    return View(nameof(Index), emprestimoList);
                                }
                            }

                            //var result = _context.Catalogo.Where(i => i.Id == productDetails.IdCatalogo).FirstOrDefault();

                            //if (result != null)
                            //{
                            //    if (result.RestricaoEmprestimo == 1)
                            //    {
                            //        ViewBag.Error = "Item Marcado como Restrição de Emprestimo.";
                            //        ViewBag.Obra = StaticObra;
                            //        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                            //        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                            //        return View(nameof(Index), emprestimoList);
                            //    }
                            //}

                            var result = from catalogo in _context.Catalogo
                                         join produto in _context.Produto on catalogo.Id equals produto.IdCatalogo
                                         join produtoAlocado in _context.ProdutoAlocado on produto.Id equals produtoAlocado.IdProduto
                                         where catalogo.RestricaoEmprestimo == 1
                                            && catalogo.Id == productDetails.IdCatalogo
                                            && produtoAlocado.Solicitante_CodColigada == SolicitanteModel.CodColigada
                                            && produtoAlocado.Solicitante_Chapa == SolicitanteModel.Chapa
                                         select produtoAlocado.Id;

                            if (result.Any())
                            {
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                                ViewBag.Error = "Item Marcado como Restrição de Emprestimo.O solicitante já possui um Item marcado como restrição de emprestimo sem devolver em sua ficha!";
                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                ViewBag.Obra = StaticObra;

                                return View(nameof(Index), emprestimoList);
                            }

                            if (productDetails.NomeCatalogo.Contains("INUTILIZAR"))
                            {
                                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                                ViewBag.Error = $"O item informado foi inutilizado e não pode ser emprestado. Contate a Equipe de Suporte para acerto do código abaixo em seu setor. Codigo{productDetails.Codigo} {productDetails.NomeCatalogo}";
                                ViewBag.Obra = StaticObra;
                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                return View(nameof(Index), emprestimoList);
                            }

                            if (productDetails.Classe != 2)
                            {
                                if (SolicitanteModel.BloqueioEmprestimo == true)
                                {
                                    string? blockMessage = searches.SearchBloqueioMessage(SolicitanteModel.Chapa);
                                    if (!string.IsNullOrEmpty(blockMessage))
                                    {
                                        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                                        ViewBag.ErrorForBloqueio = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis. <br> Retire os itens que não sejão EPI da grid. <br> EPI pode ser liberado. Obs. Equipe de Suporte: {blockMessage}";
                                        //ViewBag.Error = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis.Retire os itens que não sejão EPI da grid.EPI pode ser liberado. Obs. Equipe de Suporte:<br/>{blockMessage}";
                                        ViewBag.Obra = StaticObra;
                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                        return View(nameof(Index), emprestimoList);
                                    }
                                }
                            }

                            //if (SolicitanteModel.Secao != LiberadorMOdel.Secao)
                            //{
                            //    int? SupervisorAccepted = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorAccepted);

                            //    if (SupervisorAccepted != 1)
                            //    {
                            //        httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                            //        ViewBag.Error = "Transação falhou porque não foi aprovada pelo supervisor, digite novamente o Liberador e insira a senha para continuar esta transação.";
                            //        ViewBag.Obra = StaticObra;
                            //        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                            //        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                            //        return View(nameof(Index), emprestimoList);
                            //    }
                            //}

                            if (productDetails.Classe == 2)
                            {
                                //ControleCA? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == productDetails.IdCatalogo && i.Ativo == 1).OrderByDescending(i => i.Validade).FirstOrDefault();
                                //if (controleCAData != null)
                                //{
                                //    productDetails.IdControleCA = controleCAData.Id;
                                //    productDetails.ControleCA = controleCAData.NumeroCA;
                                //    productDetails.ControleCADate = controleCAData.Validade;
                                //}

                                List<ControleCA?>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == productDetails.IdCatalogo && i.Ativo == 1).OrderByDescending(i => i.Validade).ToList();
                                if (controleCAData.Count > 0)
                                {
                                    //ProductList[0].ControleCA = controleCAData.NumeroCA;
                                    //ProductList[0].ControleCADate = controleCAData.Validade;
                                    //ProductList[0].IdControleCA = controleCAData.Id;
                                    productDetails.ControleCAList = controleCAData;
                                }

                                productDetails.QuantityFrontEnd = 1;
                                if (productDetails.DataDeRetornoAutomatico != 0)
                                {
                                    productDetails.DataEmprestimoFrontEnd = DateTime.Now.AddDays(productDetails.DataDeRetornoAutomatico.Value);
                                }
                                else
                                {

                                }

                            }

                            if (productDetails.IdCategoriaPai == 1384)
                            {
                                if (productDetails.DataVencimento.HasValue)
                                {
                                    if (productDetails.DataVencimento.Value < DateTime.Now)
                                    {
                                        ViewBag.Error = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";
                                        ViewBag.Obra = StaticObra;
                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();


                                        return View(nameof(Index), emprestimoList);
                                    }
                                    else
                                    {
                                        productDetails.DataEmprestimoFrontEnd = productDetails.DataVencimento.Value;
                                    }
                                }
                            }

                            if (productDetails.Classe == 3)
                            {
                                productDetails.QuantityFrontEnd = 1;
                            }

                            //productDetails.QuantityFrontEnd = 1;
                            //RegisteredProductResult.Add(productDetails);
                            //ProductSearchResult.Clear();

                            ViewBag.ProductDetails = productDetails;

                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                            ViewBag.Obra = StaticObra;

                            return View(nameof(Index), emprestimoList);
                        }

                    }
                    else
                    {
                        ViewBag.Error = "O produto já está no carrinho.";
                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                        ViewBag.Obra = StaticObra;

                        return View(nameof(Index), emprestimoList);
                    }

                }

                return RedirectToAction(nameof(Index));

                //ViewBag.EmprestimoCadastro = RegisteredProductResult;
                //ViewBag.Liberador = LiberadorStaticModel;
                //ViewBag.Solicitante = SolicitanteStaticModel;

                //return View(nameof(Index));
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

        //public ActionResult CloseModal()
        //{
        //    return View();
        //}


        // GET: EmprestimoController/Details/5
        //public ActionResult Details(int produtoid)
        //{
        //    Log log = new Log();
        //    log.LogWhat = pagina + "/Index";
        //    log.LogWhere = pagina;
        //    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
        //    try
        //    {
        //        #region Authenticate User
        //        VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
        //        //usuario.Pagina = "Home/Index";
        //        usuario.Pagina = pagina;
        //        usuario.Pagina1 = log.LogWhat;
        //        usuario.Acesso = log.LogWhat;
        //        usuario = auxiliar.VerificaPermissao(usuario);

        //        if (usuario.Permissao == null)
        //        {
        //            usuario.Retorno = "Usuário sem permissão na página!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Visualizar != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de visualizar a página de Emprestimo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        var cachedEmprestimoList = EmprestimoList;

        //        var getcatalogoid = cachedEmprestimoList.Where(o => o.IdProduto == produtoid).FirstOrDefault();

        //        int? loanstatus = null;
        //        int? aferidostatus = null;
        //        int? inutilizarstatus = null;


        //        //TemRestricaoEmprestimo
        //        var checkloan = (from catalogo in _context.Catalogo
        //                        join produto in _context.Produto on catalogo.Id equals produto.IdCatalogo
        //                        join produtoAlocado in _context.ProdutoAlocado on produto.Id equals produtoAlocado.IdProduto
        //                        where catalogo.RestricaoEmprestimo == 1 &&
        //                              catalogo.Id == getcatalogoid.IdCatalogo &&
        //                              produtoAlocado.Solicitante_Chapa == "*"
        //                        select produtoAlocado.Id).FirstOrDefault();

        //        if (checkloan != null)
        //        {
        //            TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Item Marcado como Restrição de Emprestimo.O solicitante já possui um Item marcado como restrição de emprestimo sem devolver em sua ficha!";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao emprestimo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);
        //            return RedirectToAction(nameof(Index));
        //        }
        //        else
        //        {
        //            loanstatus = 0;
        //        }


        //        //TemDataVencida
        //        var CheckAferido = (from produto in _context.Produto
        //                            join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                            where produto.Id == produtoid
        //                            select catalogo.PorAferido).FirstOrDefault();

        //        if (CheckAferido == 1)
        //        {

        //            var dataVencimento = (from produto in _context.Produto
        //                                  join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
        //                                  where produto.Id == produtoid
        //                                  select produto.DataVencimento ?? DateTime.Now).FirstOrDefault();

        //            if (dataVencimento > DateTime.Now)
        //            {
        //                aferidostatus = 0;
        //            }
        //            else
        //            {
        //                TempData["ShowErrorAlert"] = true;
        //                TempData["ErrorMessage"] = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";

        //                usuario.Retorno = "Erro na validação do modelo em criaçao emprestimo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);

        //                return RedirectToAction(nameof(Index));
        //            }


        //        }
        //        else
        //        {
        //            aferidostatus = 0;
        //        }

        //        //TemInutilizado

        //        var checkinutilizado = from p in _context.Produto
        //                    join c in _context.Catalogo on p.IdCatalogo equals c.Id
        //                    where p.Id == produtoid
        //                    select "Código: " + c.Codigo + " Descrição: " + c.Nome;

        //        var resultinutilizado = checkinutilizado.FirstOrDefault();

        //        if (resultinutilizado.Contains("INUTILIZAR"))
        //        {
        //            TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Item fora do prazo de validade para uma data posterior a data de vencimento do item.";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao emprestimo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction(nameof(Index));
        //        }
        //        else
        //        {
        //            inutilizarstatus = 0;
        //        }

        //        if (loanstatus == 0 && aferidostatus == 0 && inutilizarstatus == 0)
        //        {
        //            var getdata = cachedEmprestimoList.Where(item => item.IdProduto == produtoid).FirstOrDefault();

        //            if (list == null)
        //            {
        //                //list = new List<EmprestimoViewModel>(); // Replace with the actual type of your list items
        //                list.Add(getdata);
        //            }
        //            else
        //            {
        //                list.Add(getdata);
        //            }
        //        }


        //        ViewBag.EmprestimoCadastro = list;


        //        usuario.Retorno = "Emprestimo carregada com sucesso!";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);


        //        return RedirectToAction(nameof(Index));

        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogWhy = ex.Message;
        //        ErrorViewModel erro = new ErrorViewModel();
        //        erro.Tela = log.LogWhere;
        //        erro.Descricao = log.LogWhy;
        //        erro.Mensagem = log.LogWhat;
        //        erro.IdLog = auxiliar.GravaLogRetornoErro(log);
        //        return View();
        //    }
        //}

        // GET: EmprestimoController/Create




        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EmprestimoAction(List<EmprestimoViewModel?> EmprestimoList, int? ObraEmprestimo, string formToken)
        {
            try
            {
                Log log = new Log();
                log.LogWhat = pagina + "/Index";
                log.LogWhere = pagina;
                Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
                Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

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
                    if (usuariofer.Permissao.Visualizar != 1)
                    {
                        usuariofer.Retorno = "Usuário sem permissão de visualizar a página de perguntas!";
                        log.LogWhy = usuariofer.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuariofer);
                    }
                }
                #endregion

                UserViewModel? SolicitanteModel = new UserViewModel();
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    //ViewBag.Solicitante = SolicitanteModel;
                }

                UserViewModel? LiberadorMOdel = new UserViewModel();
                string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                if (!string.IsNullOrEmpty(LiberadorChapa))
                {
                    LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                    //ViewBag.Liberador = LiberadorMOdel;
                }

                //var sessionToken = HttpContext.Session.GetString("FormToken");

                //if (sessionToken == null || sessionToken != formToken)
                //{
                //    // Duplicate or invalid submission
                //    ViewBag.Error = "Duplicate submission detected.";
                //    return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                //}


                if (SolicitanteModel?.Chapa != null && LiberadorMOdel?.Chapa != null)
                {
                    if (EmprestimoList.Count > 0)
                    {
                        int? userId = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID);

                        List<string?> ErrorCounter = new List<string?>();
                        List<EmprestimoViewModel?> finalized = new List<EmprestimoViewModel?>();

                        List<string?> duplicateError = new List<string?>();

                        //List<string> errors = new List<string>();
                        foreach (EmprestimoViewModel item in EmprestimoList)
                        {
                            if (item.Classe != 2)
                            {
                                if (SolicitanteModel.BloqueioEmprestimo == true)
                                {
                                    string? blockMessage = searches.SearchBloqueioMessage(SolicitanteModel.Chapa);
                                    if (!string.IsNullOrEmpty(blockMessage))
                                    {
                                        ViewBag.Error = $"Usuário bloqueado para empréstimos de Ferramentas e Consumíveis. Retire os itens que não sejão EPI da grid. EPI pode ser liberado. Obs. Equipe de Suporte:<br/>{blockMessage}";
                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                        ViewBag.Obra = StaticObra;
                                        //ViewBag.FormToken = sessionToken;
                                        return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                                    }
                                }
                            }

                            List<string> validationErrors = ValidateData(item, ObraEmprestimo);

                            if (!validationErrors.Any())
                            {
                                if (item.Classe == 3)
                                {
                                    //HistoricoAlocacao = Consumiveis
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
                                        int? controleCAalocado = _context.ControleCA
                                          .Where(ca => ca.IdCatalogo == _context.Produto
                                              .Where(p => p.Id == item.IdProduto)
                                              .Select(p => p.IdCatalogo)
                                              .FirstOrDefault()
                                              && ca.Ativo == 1)
                                          .OrderByDescending(ca => ca.Validade)
                                          .Select(ca => ca.Id)
                                          .FirstOrDefault();

                                        //DateTime oneMinuteAgo = DateTime.Now.AddMinutes(-2);
                                        //var recentDuplicate = _context.Set<HistoricoAlocacao_2024>()
                                        //    .Where(h => h.IdProduto == item.IdProduto
                                        //                && h.Solicitante_Chapa == SolicitanteModel.Chapa
                                        //                && h.Balconista_Emprestimo_IdLogin == usuariofer.Id
                                        //                && h.DataEmprestimo > oneMinuteAgo)
                                        //    .FirstOrDefault();

                                        //if (recentDuplicate == null)
                                        //{
                                        var InsertHistoricoAlocacao2025 = new HistoricoAlocacao_2025
                                        {
                                            IdProduto = item.IdProduto,
                                            Solicitante_IdTerceiro = SolicitanteModel.IdTerceiro,
                                            Solicitante_CodColigada = SolicitanteModel.CodColigada,
                                            Solicitante_Chapa = SolicitanteModel.Chapa,
                                            //GetProdutoAlocado.Solicitante_Chapa,
                                            Liberador_IdTerceiro = LiberadorMOdel.IdTerceiro,
                                            Liberador_CodColigada = LiberadorMOdel.CodColigada,
                                            Liberador_Chapa = LiberadorMOdel.Chapa,
                                            Balconista_Emprestimo_IdLogin = userId,
                                            Balconista_Devolucao_IdLogin = userId,
                                            Observacao = item.Observacao,
                                            DataEmprestimo = DateTime.Now,
                                            DataDevolucao = item.DataEmprestimoFrontEnd.HasValue ? item.DataEmprestimoFrontEnd : DateTime.Now,
                                            IdObra = ObraEmprestimo,
                                            Quantidade = item.QuantityFrontEnd,
                                            IdFerrOndeProdRetirado = item.IdFerramentaria,
                                            IdControleCA = controleCAalocado
                                        };

                                        using (var transaction = _context.Database.BeginTransaction())
                                        {
                                            try
                                            {
                                                _context.Add(InsertHistoricoAlocacao2025);
                                                var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                                if (productToUpdate != null)
                                                {
                                                    productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;
                                                }

                                                _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
                                                transaction.Commit(); // Commit the transaction

                                                finalized.Add(item);
                                            }
                                            catch (Exception ex)
                                            {
                                                transaction.Rollback(); // Rollback the transaction in case of an exception
                                                                        // Optionally, log the exception or rethrow it

                                                ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";

                                                List<EmprestimoViewModel?>? remainingitems = EmprestimoList.Where(item => !finalized.Contains(item)).ToList();

                                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                                ViewBag.Obra = StaticObra;
                                                ViewBag.FormToken = formToken;
                                                return View(nameof(Index), remainingitems ?? new List<EmprestimoViewModel?>());
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    log.LogWhat = $"{log.LogWhat}/EmprestimoAction";
                                        //    log.LogWhy = $"Emprestimo Duplicated: HistoricoAlocacao: {recentDuplicate.Id} - Chapa:{SolicitanteModel.Chapa} - IdProduto:{item.IdProduto}";
                                        //    auxiliar.GravaLogDuplicate(log);

                                        //    duplicateError.Add($"Codigo:{item.Codigo} ja inserir no banco do dados para Empregado Chapa:{SolicitanteModel.Chapa}");
                                        //    continue;
                                        //    //TempData["ErrorMessage"] = "Chave Replicated.";
                                        //    //return RedirectToAction(nameof(Index));
                                        //}



                                        //_context.SaveChanges();



                                        //_context.SaveChanges();

                                        //int? SupervisorAccepted = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorAccepted);

                                        //int? SupervisorId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorId);

                                        //    if (SupervisorAccepted == 1)
                                        //    {
                                        //        if (SupervisorId.HasValue && SupervisorId != 0)
                                        //        {
                                        //            var InsertBloqueioEmprestimoVsLiberador_Log = new BloqueioEmprestimoVsLiberador_Log
                                        //            {
                                        //                DataTransacao = DateTime.Now,
                                        //                Autorizador = SupervisorId,
                                        //                Tabela = "ProdutoAlocado",
                                        //                IdRegistro = InsertHistoricoAlocacao2024.Id,

                                        //            };

                                        //            //VW error from the VW_1600006597_LiberacaoExcepcional
                                        //            _context.BloqueioEmprestimoVsLiberador_Log.Add(InsertBloqueioEmprestimoVsLiberador_Log);
                                        //            await _context.SaveChangesAsync();
                                        //        }
                                        //    }


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
                                    //ProdutoAlocado = EPI and Ferramenta
                                    DateTime dtTransacao = DateTime.Now;
                                    string rightSubstring = dtTransacao.ToString("yyyyMMddHHmmssfff").Substring(0, 8); /*DateTime.Now.ToString("HH:mm:ss")*/

                                    string key = item.IdProduto.ToString() + SolicitanteModel.CodColigada + SolicitanteModel.Chapa + usuariofer.Id + dtTransacao.ToString("dd/MM/yyyy") + dtTransacao.ToString("HH:mm:ss") + ObraEmprestimo + item.QuantityFrontEnd + item.IdFerramentaria;
                                    string hash;
                                    using (MD5 md5 = MD5.Create())
                                    {
                                        byte[] inputBytes = Encoding.UTF8.GetBytes(key);
                                        byte[] hashBytes = md5.ComputeHash(inputBytes);
                                        hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                    }

                                    //int? controleCAalocado = _context.ControleCA
                                    //           .Where(ca => ca.IdCatalogo == _context.Produto
                                    //               .Where(p => p.Id == item.IdProduto)
                                    //               .Select(p => p.IdCatalogo)
                                    //               .FirstOrDefault()
                                    //               && ca.Ativo == 1)
                                    //           .OrderByDescending(ca => ca.Validade)
                                    //           .Select(ca => ca.Id)
                                    //           .FirstOrDefault();



                                    var QtdReplicado = _context.ProdutoAlocado.Count(pa => pa.Chave == hash);
                                    //DateTime oneMinuteAgo = DateTime.Now.AddMinutes(-2);
                                    //var recentDuplicate = _context.ProdutoAlocado.Where(h => h.IdProduto == item.IdProduto
                                    //                                              && h.Solicitante_Chapa == SolicitanteModel.Chapa
                                    //                                              && h.Balconista_IdLogin == usuariofer.Id
                                    //                                              && h.DataEmprestimo > oneMinuteAgo)
                                    //                                              .FirstOrDefault();
                                    if (QtdReplicado == 0)
                                    //if (recentDuplicate == null)
                                    {
                                        var productToVerify = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                        Catalogo checkCatalogForAlocado = _context.Catalogo.FirstOrDefault(i => i.Id == productToVerify.IdCatalogo);
                                        if (checkCatalogForAlocado.PorAferido == 1 || checkCatalogForAlocado.PorSerial == 1)
                                        {
                                            List<ProdutoAlocado?>? checkAlocadoDuplicates = _context.ProdutoAlocado.Where(i => i.IdProduto == item.IdProduto).ToList();
                                            if (checkAlocadoDuplicates.Count == 0)
                                            {
                                                var InsertProdutoAlocado = new ProdutoAlocado
                                                {
                                                    IdProduto = item.IdProduto,
                                                    IdObra = ObraEmprestimo,
                                                    IdFerrOndeProdRetirado = item.IdFerramentaria,
                                                    Solicitante_IdTerceiro = SolicitanteModel.IdTerceiro,
                                                    Solicitante_CodColigada = SolicitanteModel.CodColigada,
                                                    Solicitante_Chapa = SolicitanteModel.Chapa,
                                                    Balconista_IdLogin = userId,
                                                    Liberador_IdTerceiro = LiberadorMOdel.IdTerceiro,
                                                    Liberador_CodColigada = LiberadorMOdel.CodColigada,
                                                    Liberador_Chapa = LiberadorMOdel.Chapa,
                                                    Observacao = item.ObservacaoFrontEnd,
                                                    DataPrevistaDevolucao = item.DataEmprestimoFrontEnd,
                                                    DataEmprestimo = dtTransacao,
                                                    Quantidade = item.QuantityFrontEnd,
                                                    Chave = hash,
                                                    IdControleCA = item.IdControleCA

                                                };

                                                using (var transaction = _context.Database.BeginTransaction())
                                                {
                                                    try
                                                    {
                                                        _context.Add(InsertProdutoAlocado);
                                                        var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                                        Catalogo checkCatalog = _context.Catalogo.FirstOrDefault(i => i.Id == productToUpdate.IdCatalogo);
                                                        if (checkCatalog.PorAferido == 1 || checkCatalog.PorSerial == 1)
                                                        {
                                                            productToUpdate.Quantidade = 0;
                                                        }
                                                        else
                                                        {
                                                            productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;
                                                        }

                                                        _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
                                                        transaction.Commit(); // Commit the transaction

                                                        finalized.Add(item);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        transaction.Rollback(); // Rollback the transaction in case of an exception
                                                                                // Optionally, log the exception or rethrow it

                                                        ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";

                                                        List<EmprestimoViewModel?>? remainingitems = EmprestimoList
                                                                                               .Where(item => !finalized.Contains(item))
                                                                                               .ToList();

                                                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                                        ViewBag.Obra = StaticObra;
                                                        ViewBag.FormToken = formToken;
                                                        return View(nameof(Index), remainingitems ?? new List<EmprestimoViewModel?>());
                                                    }
                                                }

                                                //_context.Add(InsertProdutoAlocado);
                                                //_context.SaveChanges();

                                                //if (!InsertProdutoAlocado.Id.HasValue)
                                                //{
                                                //    string? JoinIdAndCodigo = $"Please screenshot and report to IT for Error Insertion of data. IdProduto:{item.IdProduto} Codigo:{item.Codigo}";
                                                //    ErrorCounter.Add(JoinIdAndCodigo);
                                                //    _context.Database.RollbackTransaction();
                                                //    break;
                                                //}
                                                //else
                                                //{
                                                //    var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                                //    if (productToUpdate != null)
                                                //    {
                                                //        Catalogo checkCatalog = _context.Catalogo.FirstOrDefault(i => i.Id == productToUpdate.IdCatalogo);
                                                //        if (checkCatalog.PorAferido == 1 || checkCatalog.PorSerial == 1)
                                                //        {
                                                //            productToUpdate.Quantidade = 0;
                                                //            //_context.SaveChanges();
                                                //        }
                                                //        else
                                                //        {
                                                //            productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                                //            // Save the changes to the database
                                                //            //_context.SaveChanges();
                                                //        }
                                                //        //if ()
                                                //        //{

                                                //        //}
                                                //        //productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                                //        //// Save the changes to the database
                                                //        //_context.SaveChanges();

                                                //    }

                                                //}

                                                //_context.SaveChanges();
                                            }
                                            else
                                            {
                                                continue;
                                            }

                                        }
                                        else
                                        {
                                            var InsertProdutoAlocado = new ProdutoAlocado
                                            {
                                                IdProduto = item.IdProduto,
                                                IdObra = ObraEmprestimo,
                                                IdFerrOndeProdRetirado = item.IdFerramentaria,
                                                Solicitante_IdTerceiro = SolicitanteModel.IdTerceiro,
                                                Solicitante_CodColigada = SolicitanteModel.CodColigada,
                                                Solicitante_Chapa = SolicitanteModel.Chapa,
                                                Balconista_IdLogin = userId,
                                                Liberador_IdTerceiro = LiberadorMOdel.IdTerceiro,
                                                Liberador_CodColigada = LiberadorMOdel.CodColigada,
                                                Liberador_Chapa = LiberadorMOdel.Chapa,
                                                Observacao = item.ObservacaoFrontEnd,
                                                DataPrevistaDevolucao = item.DataEmprestimoFrontEnd,
                                                DataEmprestimo = dtTransacao,
                                                Quantidade = item.QuantityFrontEnd,
                                                Chave = hash,
                                                IdControleCA = item.IdControleCA
                                            };

                                            using (var transaction = _context.Database.BeginTransaction())
                                            {
                                                try
                                                {
                                                    _context.Add(InsertProdutoAlocado);
                                                    var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                                    Catalogo checkCatalog = _context.Catalogo.FirstOrDefault(i => i.Id == productToUpdate.IdCatalogo);
                                                    if (checkCatalog.PorAferido == 1 || checkCatalog.PorSerial == 1)
                                                    {
                                                        productToUpdate.Quantidade = 0;
                                                    }
                                                    else
                                                    {
                                                        productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;
                                                    }

                                                    _context.SaveChanges(); // Make sure to call SaveChangesAsync to persist the changes
                                                    transaction.Commit(); // Commit the transaction

                                                    finalized.Add(item);

                                                }
                                                catch (Exception ex)
                                                {
                                                    transaction.Rollback(); // Rollback the transaction in case of an exception
                                                                            // Optionally, log the exception or rethrow it

                                                    ViewBag.Error = $"SERVER PROBLEM: {ex.Message}";

                                                    List<EmprestimoViewModel?>? remainingitems = EmprestimoList
                                                                                          .Where(item => !finalized.Contains(item))
                                                                                          .ToList();

                                                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                                    ViewBag.Obra = StaticObra;
                                                    ViewBag.FormToken = formToken;
                                                    return View(nameof(Index), remainingitems ?? new List<EmprestimoViewModel?>());
                                                }
                                            }

                                            //_context.Add(InsertProdutoAlocado);
                                            //_context.SaveChanges();

                                            //if (!InsertProdutoAlocado.Id.HasValue)
                                            //{
                                            //    string? JoinIdAndCodigo = $"Please screenshot and report to IT for Error Insertion of data. IdProduto:{item.IdProduto} Codigo:{item.Codigo}";
                                            //    ErrorCounter.Add(JoinIdAndCodigo);
                                            //    _context.Database.RollbackTransaction();
                                            //    break;
                                            //}
                                            //else
                                            //{
                                            //    var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                            //    if (productToUpdate != null)
                                            //    {
                                            //        Catalogo checkCatalog = _context.Catalogo.FirstOrDefault(i => i.Id == productToUpdate.IdCatalogo);
                                            //        if (checkCatalog.PorAferido == 1 || checkCatalog.PorSerial == 1)
                                            //        {
                                            //            productToUpdate.Quantidade = 0;
                                            //            //_context.SaveChanges();
                                            //        }
                                            //        else
                                            //        {
                                            //            productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                            //            // Save the changes to the database
                                            //            //_context.SaveChanges();
                                            //        }
                                            //        //if ()
                                            //        //{

                                            //        //}
                                            //        //productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                            //        //// Save the changes to the database
                                            //        //_context.SaveChanges();

                                            //    }

                                            //}

                                            //_context.SaveChanges();
                                        }

                                        //var InsertProdutoAlocado = new ProdutoAlocado
                                        //{
                                        //    IdProduto = item.IdProduto,
                                        //    IdObra = ObraEmprestimo,
                                        //    IdFerrOndeProdRetirado = item.IdFerramentaria,
                                        //    Solicitante_IdTerceiro = 0,
                                        //    Solicitante_CodColigada = SolicitanteModel.CodColigada,
                                        //    Solicitante_Chapa = SolicitanteModel.Chapa,
                                        //    Balconista_IdLogin = userId,
                                        //    Liberador_IdTerceiro = 0,
                                        //    Liberador_CodColigada = LiberadorMOdel.CodColigada,
                                        //    Liberador_Chapa = LiberadorMOdel.Chapa,
                                        //    Observacao = item.ObservacaoFrontEnd,
                                        //    DataPrevistaDevolucao = item.DataEmprestimoFrontEnd,
                                        //    DataEmprestimo = dtTransacao,
                                        //    Quantidade = item.QuantityFrontEnd,
                                        //    Chave = hash,
                                        //    IdControleCA = controleCAalocado

                                        //};

                                        //_context.Add(InsertProdutoAlocado);
                                        //_context.SaveChanges();

                                        //if (!InsertProdutoAlocado.Id.HasValue)
                                        //{
                                        //    string? JoinIdAndCodigo = $"Please screenshot and report to IT for Error Insertion of data. IdProduto:{item.IdProduto} Codigo:{item.Codigo}";
                                        //    ErrorCounter.Add(JoinIdAndCodigo);
                                        //    _context.Database.RollbackTransaction();
                                        //    break;
                                        //}
                                        //else
                                        //{
                                        //    var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == item.IdProduto);
                                        //    if (productToUpdate != null)
                                        //    {
                                        //        Catalogo checkCatalog = _context.Catalogo.FirstOrDefault(i => i.Id == productToUpdate.IdCatalogo);
                                        //        if (checkCatalog.PorAferido == 1 || checkCatalog.PorSerial == 1)
                                        //        {
                                        //            productToUpdate.Quantidade = 0;
                                        //            _context.SaveChanges();
                                        //        }
                                        //        else
                                        //        {
                                        //            productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                        //            // Save the changes to the database
                                        //            _context.SaveChanges();
                                        //        }
                                        //        //if ()
                                        //        //{

                                        //        //}
                                        //        //productToUpdate.Quantidade = productToUpdate.Quantidade - item.QuantityFrontEnd;

                                        //        //// Save the changes to the database
                                        //        //_context.SaveChanges();

                                        //    }

                                        //}


                                        //int? SupervisorAccepted = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorAccepted);
                                        //int? SupervisorId = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorId);

                                        //    if (SupervisorAccepted == 1)
                                        //    {
                                        //        if (SupervisorId.HasValue && SupervisorId != 0)
                                        //        {
                                        //            var InsertBloqueioEmprestimoVsLiberador_Log = new BloqueioEmprestimoVsLiberador_Log
                                        //            {
                                        //                DataTransacao = dtTransacao,
                                        //                Autorizador = SupervisorId,
                                        //                Tabela = "ProdutoAlocado",
                                        //                IdRegistro = InsertProdutoAlocado.Id,

                                        //            };

                                        //        //VW error from the VW_1600006597_LiberacaoExcepcional
                                        //            _context.BloqueioEmprestimoVsLiberador_Log.Add(InsertBloqueioEmprestimoVsLiberador_Log);
                                        //            await _context.SaveChangesAsync();
                                        //        }                                 
                                        //    }

                                    }
                                    else
                                    {
                                        //log.LogWhat = $"{log.LogWhat}/EmprestimoAction";
                                        //log.LogWhy = $"Emprestimo Duplicated: ProdutoAlocado:{recentDuplicate.Id} - Chapa:{SolicitanteModel.Chapa} - IdProduto:{item.IdProduto}";
                                        //auxiliar.GravaLogDuplicate(log);
                                        //duplicateError.Add($"Codigo:{item.Codigo} ja inserir no banco do dados para Empregado Chapa:{SolicitanteModel.Chapa}");
                                        //continue;
                                        TempData["ErrorMessage"] = "Chave Replicated.";
                                        return RedirectToAction(nameof(Index));
                                    }

                                }

                            }
                            else
                            {
                                string[] errorslist = validationErrors.ToArray();
                                ViewBag.ErrorList = validationErrors;

                                ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                                ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                                ViewBag.Obra = StaticObra;
                                ViewBag.FormToken = formToken;
                                return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                            }
                        }

                        if (ErrorCounter.Count == 0)
                        {
                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.Solicitante);
                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);
                            SolicitanteModel = new UserViewModel();
                            LiberadorMOdel = new UserViewModel();
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                            //RegisteredProductResult.Clear();
                            httpContextAccessor.HttpContext?.Session.Remove(SessionKeyEmprestimoList);
                            //httpContextAccessor.HttpContext?.Session.Remove(SessionKeySupervisorAccepted);
                            //httpContextAccessor.HttpContext?.Session.Remove(SessionKeySupervisorId);
                            //SupervisorId = null;
                            //SupervisorAccepted = null;

                            ViewBag.Liberador = new UserViewModel();
                            ViewBag.Solicitante = new UserViewModel();
                            HttpContext.Session.Remove("FormToken");

                            TempData["ShowSuccessAlertEmprestimo"] = true;
                            return RedirectToAction(nameof(Index));
                            //HttpContext.Session.SetString("FormToken", formToken);
                            //ViewBag.FormToken = formToken;
                            //ViewBag.ShowSuccessAlert = true;
                            //    return View(nameof(Index));
                        }
                        else
                        {
                            string[] errorslist = ErrorCounter.ToArray();
                            ViewBag.ErrorList = errorslist;

                            ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                            ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                            ViewBag.Obra = StaticObra;
                            ViewBag.FormToken = formToken;
                            return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                        }

                    }
                    else
                    {
                        ViewBag.Error = "Nenhum Produto na listagem de Itens a serem Emprestados.";
                        ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                        ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                        ViewBag.Obra = StaticObra;
                        ViewBag.FormToken = formToken;
                        return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                    }
                }
                else
                {
                    ViewBag.Error = "No Solicitante e Liberador.";
                    ViewBag.Liberador = LiberadorMOdel != null ? LiberadorMOdel : new UserViewModel();
                    ViewBag.Solicitante = SolicitanteModel != null ? SolicitanteModel : new UserViewModel();
                    ViewBag.Obra = StaticObra;
                    ViewBag.FormToken = formToken;
                    return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                httpContextAccessor.HttpContext?.Session.Remove(Sessao.Solicitante);
                httpContextAccessor.HttpContext?.Session.Remove(Sessao.Liberador);
                httpContextAccessor.HttpContext?.Session.Remove(SessionKeyProductList);
                HttpContext.Session.Remove(SessionKeyEmprestimoList);
                HttpContext.Session.Remove("FormToken");
                return View(nameof(Index), EmprestimoList ?? new List<EmprestimoViewModel?>());
            }
        }

        // POST: EmprestimoController/Create
        //[HttpPost]
        //public async Task<IActionResult> Create(List<int> IdList,string? obscod, string? CodSituacaoSolicitante, int? CodColigadaSolicitante, int? CodColigadaLiberador, int? ObraEmprestimo, int? qtdcod, int? qtdmincod, string? codigoemprestimo, DateTime? dataemprestimo, int? ProdutoFerramentaria, string? SecaoSolicitante, string? SecaoLiberador)
        //{
        //    //List<string> errors = ValidateData(CodSituacaoSolicitante, ObraEmprestimo);

        //    Log log = new Log();
        //    log.LogWhat = pagina + "/Index";
        //    log.LogWhere = pagina;
        //    Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

        //    try
        //    {
        //        #region Authenticate User
        //        VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
        //        //usuario.Pagina = "Home/Index";
        //        usuario.Pagina = pagina;
        //        usuario.Pagina1 = log.LogWhat;
        //        usuario.Acesso = log.LogWhat;
        //        usuario = auxiliar.VerificaPermissao(usuario);

        //        if (usuario.Permissao == null)
        //        {
        //            usuario.Retorno = "Usuário sem permissão na página!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);
        //            return RedirectToAction("PreserveActionError", "Home", usuario);
        //        }
        //        else
        //        {
        //            if (usuario.Permissao.Inserir != 1)
        //            {
        //                usuario.Retorno = "Usuário sem permissão de visualizar a página de emprestimo!";
        //                log.LogWhy = usuario.Retorno;
        //                auxiliar.GravaLogAlerta(log);
        //                return RedirectToAction("PreserveActionError", "Home", usuario);
        //            }
        //        }
        //        #endregion

        //        if (list == null || list.Count == 0)
        //        {
        //            TempData["ShowErrorAlert"] = true;
        //            TempData["ErrorMessage"] = "Nenhum Produto na listagem de Itens a serem Emprestados.";

        //            usuario.Retorno = "Erro na validação do modelo em criaçao emprestimo!";
        //            log.LogWhy = usuario.Retorno;
        //            auxiliar.GravaLogAlerta(log);

        //            return RedirectToAction(nameof(Index));
        //        }

        //         List<string> errors = new List<string>();

        //        foreach (int id in IdList)
        //        {
        //            //List<string> validationErrors = ValidateData(id, CodSituacaoSolicitante, ObraEmprestimo, qtdcod, qtdmincod, codigoemprestimo, dataemprestimo, ProdutoFerramentaria, SecaoSolicitante, SecaoLiberador);

        //            List<string> validationErrors = new List<string>(); //filler

        //            if (validationErrors.Any())
        //            {
        //                errors.AddRange(validationErrors);
        //            }

        //            if (errors.Any())
        //            {
        //                string[] errorslist = errors.ToArray();

        //                TempData["ErrorList"] = errors;

        //                return RedirectToAction(nameof(Index));
        //            }
        //            else
        //            {
        //                string? insertSolicitanteChapa = httpContextAccessor.HttpContext.Session.GetString(Sessao.Solicitante);
        //                string? insertLiberadorChapa = httpContextAccessor.HttpContext.Session.GetString(Sessao.Liberador);

        //                DateTime dtTransacao = DateTime.Now;
        //                string rightSubstring = dtTransacao.ToString("yyyyMMddHHmmssfff").Substring(0, 8); /*DateTime.Now.ToString("HH:mm:ss")*/

        //                string key = id.ToString() + CodColigadaSolicitante + insertSolicitanteChapa + usuario.Id + dtTransacao.ToString("dd/MM/yyyy") + dtTransacao.ToString("HH:mm:ss") + ObraEmprestimo + qtdcod + ProdutoFerramentaria;
        //                string hash;
        //                using (MD5 md5 = MD5.Create())
        //                {
        //                    byte[] inputBytes = Encoding.UTF8.GetBytes(key);
        //                    byte[] hashBytes = md5.ComputeHash(inputBytes);
        //                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        //                }

        //                int? controleCAalocado = _context.ControleCA
        //                                        .Where(ca => ca.IdCatalogo == _context.Produto
        //                                            .Where(p => p.Id == id)
        //                                            .Select(p => p.IdCatalogo)
        //                                            .FirstOrDefault()
        //                                            && ca.Ativo == 1)
        //                                        .OrderByDescending(ca => ca.Validade)
        //                                        .Select(ca => ca.Id)
        //                                        .FirstOrDefault();

        //                var QtdReplicado = _context.ProdutoAlocado.Count(pa => pa.Chave == hash);

        //                if (QtdReplicado == 0)
        //                {
        //                    var InsertProdutoAlocado = new ProdutoAlocado
        //                    {
        //                        IdProduto = id,
        //                        IdObra = ObraEmprestimo,
        //                        IdFerrOndeProdRetirado = ProdutoFerramentaria,
        //                        Solicitante_IdTerceiro = 0,
        //                        Solicitante_CodColigada = CodColigadaSolicitante,
        //                        Solicitante_Chapa = insertSolicitanteChapa,
        //                        Balconista_IdLogin = usuario.Id,
        //                        Liberador_IdTerceiro = 0,
        //                        Liberador_CodColigada = CodColigadaLiberador,
        //                        Liberador_Chapa = insertLiberadorChapa,
        //                        Observacao = obscod,
        //                        DataPrevistaDevolucao = dataemprestimo,
        //                        DataEmprestimo = dtTransacao,
        //                        Quantidade = qtdcod,
        //                        Chave = hash,
        //                        IdControleCA = controleCAalocado

        //                    };

        //                    _context.Add(InsertProdutoAlocado);
        //                    await _context.SaveChangesAsync();

        //                    var productToUpdate = _context.Produto.FirstOrDefault(x => x.Id == id);

        //                    if (productToUpdate != null)
        //                    {
        //                        productToUpdate.Quantidade = productToUpdate.Quantidade - 1;

        //                        // Save the changes to the database
        //                        _context.SaveChanges();
        //                    }




        //                }
        //            }

        //        }

        //        //httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);

        //        httpContextAccessor.HttpContext.Session.Remove(Sessao.Liberador);

        //        httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);

        //        httpContextAccessor.HttpContext.Session.Remove(Sessao.SupervisorChapa);

        //        httpContextAccessor.HttpContext.Session.Remove(Sessao.SupervisorId);

        //        list = null;

        //        TempData["ShowSuccessAlert"] = true;


        //        usuario.Retorno = "Emprestimo adicionada com sucesso";
        //        log.LogWhy = usuario.Retorno;
        //        auxiliar.GravaLogSucesso(log);

        //        return RedirectToAction(nameof(Index));

        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogWhy = ex.Message;
        //        ErrorViewModel erro = new ErrorViewModel();
        //        erro.Tela = log.LogWhere;
        //        erro.Descricao = log.LogWhy;
        //        erro.Mensagem = log.LogWhat;
        //        erro.IdLog = auxiliar.GravaLogRetornoErro(log);
        //        return View();
        //    }



        //    //try
        //    //{
        //    //    return RedirectToAction(nameof(Index));
        //    //}
        //    //catch
        //    //{
        //    //    return View();
        //    //}
        //}

        // GET: EmprestimoController/Edit/5
        public async Task<IActionResult> SupervisorApprove(string? Username, string? Password)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Inserir != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de visualizar a página de emprestimo!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                UserViewModel? SolicitanteModel = new UserViewModel();
                string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
                if (!string.IsNullOrEmpty(SolicitanteChapa))
                {
                    SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                    ViewBag.Solicitante = SolicitanteModel;
                }

                UserViewModel? LiberadorMOdel = new UserViewModel();
                string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
                if (!string.IsNullOrEmpty(LiberadorChapa))
                {
                    LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                    ViewBag.Liberador = LiberadorMOdel;
                }

                VW_Usuario_NewViewModel uvm = new VW_Usuario_NewViewModel();

                if (Username != null && Password != null)
                {
                    uvm.Retorno = auxiliar.ValidaUsuarioAD(Username, Password);

                    if (uvm == null)
                    {
                        //error
                    }
                    else if (uvm.Retorno.Equals("Erro em conectar com o AD!"))
                    {
                        //wrong password
                        ViewBag.UserTest = true;
                        ViewBag.ErrorSupervisor = "O usuário ou senha está incorreto. / Erro em conectar com o AD!.";
                        ViewBag.Liberador = LiberadorMOdel;
                        ViewBag.Solicitante = SolicitanteModel;
                        return View(nameof(Index));
                    }
                    else
                    {
                        Permissao permissao = new Permissao();
                        permissao = await _contextBS.Permissao.FirstOrDefaultAsync(p => p.SAMAccountName.Equals(Username));

                        if (permissao == null)
                        {
                            //error permissao
                            ViewBag.UserTest = true;
                            ViewBag.ErrorSupervisor = "Usuário não tem acesso ao SIB-Ferramentaria.";
                            ViewBag.Liberador = LiberadorMOdel;
                            ViewBag.Solicitante = SolicitanteModel;
                            return View(nameof(Index));
                        }
                        else
                        {
                            var usu = await _contextBS.VW_Usuario_New.FirstOrDefaultAsync(u => u.Id == permissao.IdUsuario && u.Ativo == 1);

                            if (usu == null)
                            {
                                //error in sib
                                ViewBag.UserTest = true;
                                ViewBag.ErrorSupervisor = "Usuário não tem acesso ao SIB-Ferramentaria.";
                                ViewBag.Liberador = LiberadorMOdel;
                                ViewBag.Solicitante = SolicitanteModel;
                                return View(nameof(Index));
                            }
                            else
                            {
                                var acessoList = _contextBS.Acesso.Where(a => a.IdModulo == 6).GroupJoin(_contextBS.Permissao.Where(p => p.IdUsuario == permissao.IdUsuario), a => a.Id, p => p.IdAcesso, (a, p) => new AcessoViewModel { Pagina = a.Pagina }).AsEnumerable();

                                if (acessoList == null)
                                {
                                    //error user no access
                                    ViewBag.UserTest = true;
                                    ViewBag.ErrorSupervisor = "Usuário não tem acesso ao SIB-Ferramentaria/Emprestimo.";
                                    ViewBag.Liberador = LiberadorMOdel;
                                    ViewBag.Solicitante = SolicitanteModel;
                                    return View(nameof(Index));
                                }
                                else
                                {
                                    BloqueioEmprestimoVsLiberador? CheckUser = _context.BloqueioEmprestimoVsLiberador.FirstOrDefault(i => i.IdLogin == usu.Id);

                                    if (CheckUser != null)
                                    {
                                        //httpContextAccessor.HttpContext.Session.SetInt32(Sessao.SupervisorId, (int)usu.Id);
                                        //httpContextAccessor.HttpContext.Session.SetString(Sessao.SupervisorChapa, usu.Chapa);

                                        usuario.Retorno = "Supervisor approved sucesso";
                                        log.LogWhy = usuario.Retorno;
                                        auxiliar.GravaLogSucesso(log);

                                        //httpContextAccessor.HttpContext?.Session.Remove(SessionKeySupervisorAccepted);
                                        //httpContextAccessor.HttpContext?.Session.Remove(SessionKeySupervisorId);
                                        //httpContextAccessor.HttpContext?.Session.SetInt32(SessionKeySupervisorAccepted, (int)1);
                                        //httpContextAccessor.HttpContext?.Session.SetInt32(SessionKeySupervisorId, (int)CheckUser.IdLogin);

                                        //SupervisorAccepted = true;
                                        //SupervisorId = CheckUser.IdLogin;
                                        ViewBag.ShowSupervisorApproved = true;
                                        ViewBag.Liberador = LiberadorMOdel;
                                        ViewBag.Solicitante = SolicitanteModel;
                                        return View(nameof(Index));

                                        //TempData["ShowSupervisorApproved"] = true;
                                        //return RedirectToAction(nameof(Index));
                                    }
                                    else
                                    {
                                        ViewBag.UserTest = true;
                                        ViewBag.ErrorSupervisor = "O usuário não tem autoridade para aprovar esta transação..";
                                        ViewBag.Liberador = LiberadorMOdel;
                                        ViewBag.Solicitante = SolicitanteModel;
                                        return View(nameof(Index));
                                    }

                                    //if (usu.Funcao.Contains("Supervisor") || usu.Funcao.Contains("Gerente"))
                                    //{
                                    //    // Your code here

                                    //    httpContextAccessor.HttpContext.Session.SetInt32(Sessao.SupervisorId, (int)usu.Id);
                                    //    httpContextAccessor.HttpContext.Session.SetString(Sessao.SupervisorChapa, usu.Chapa);

                                    //    usuario.Retorno = "Supervisor approved sucesso";
                                    //    log.LogWhy = usuario.Retorno;
                                    //    auxiliar.GravaLogSucesso(log);

                                    //    TempData["ShowSupervisorApproved"] = true;
                                    //}
                                    //else
                                    //{
                                    //    TempData["ShowErrorAlert"] = true;
                                    //    TempData["ErrorMessage"] = "User not Gerente or Supervisor.";

                                    //    usuario.Retorno = "User not Gerente or Supervisor!";
                                    //    log.LogWhy = usuario.Retorno;
                                    //    auxiliar.GravaLogAlerta(log);

                                    //    return RedirectToAction(nameof(Index));
                                    //}


                                }

                            }

                        }

                    }

                }
                else
                {
                    ViewBag.UserTest = true;
                    ViewBag.ErrorSupervisor = "Por favor, Insira o usuário e a senha";
                    ViewBag.Liberador = LiberadorMOdel;
                    ViewBag.Solicitante = SolicitanteModel;
                    return View(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }



        // GET: EmprestimoController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //                        #region LoadFerramentaria List

        //                if (FerramentariaValue == null)
        //                {
        //                    var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
        //                                              where ferramentaria.Ativo == 1 &&
        //                                                    !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
        //                                                    _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
        //                                              orderby ferramentaria.Nome
        //                                              select new
        //                                              {
        //                                                  Id = ferramentaria.Id,
        //                                                  Nome = ferramentaria.Nome
        //                                              }).ToList();

        //                    if (ferramentariaItems != null)
        //                    {
        //                        ViewBag.FerramentariaItems = ferramentariaItems;
        //                    }
        //                    else
        //                    {

        //                    }

        //                }
        //                else
        //{
        //    //ViewBag.FerramentariaItems = null;


        //    var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
        //                              where ferramentaria.Ativo == 1 &&
        //                                    !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
        //                                    _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
        //                                    && ferramentaria.Id == FerramentariaValue
        //                              orderby ferramentaria.Nome
        //                              select new
        //                              {
        //                                  Id = ferramentaria.Id,
        //                                  Nome = ferramentaria.Nome
        //                              }).ToList();

        //    ViewBag.FerramentariaItems = ferramentariaItems;
        //}

        //#endregion

        //    return View();
        //}

        // POST: EmprestimoController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]


        public ActionResult Exclude(int? id)
        {
            var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

            if (id != null)
            {
                emprestimoList.RemoveAll(item => item.IdProduto == id);
                HttpContext.Session.SetObject(SessionKeyEmprestimoList, emprestimoList);

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "No ID selected." });
            }
        }

        public ActionResult Delete(int id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                #region Authenticate User
                VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //usuario.Pagina = "Home/Index";
                usuario.Pagina = pagina;
                usuario.Pagina1 = log.LogWhat;
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("PreserveActionError", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Excluir != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de Excluir a página de Emprestimo!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("PreserveActionError", "Home", usuario);
                    }
                }
                #endregion

                var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

                if (id != null)
                {
                    emprestimoList.RemoveAll(item => item.IdProduto == id);
                    HttpContext.Session.SetObject(SessionKeyEmprestimoList, emprestimoList);

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = "No ID selected." });
                }
                //list.RemoveAll(item => item.IdProduto == id);         

                //RegisteredProductResult.RemoveAll(item => item.IdProduto == id);


                //if (list.Count == 0) 
                //{
                //    list = null; 
                //}

                //return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                log.LogWhy = ex.Message;
                ErrorViewModel erro = new ErrorViewModel();
                erro.Tela = log.LogWhere;
                erro.Descricao = log.LogWhy;
                erro.Mensagem = log.LogWhat;
                erro.IdLog = auxiliar.GravaLogRetornoErro(log);
                return View(ex);
            }


        }

        //private void TryGetSolicitanteData(string solicitanteValue)
        //{

        //    var query = _contextBS.Funcionario
        //        .Where(e => e.Chapa == solicitanteValue)
        //        .OrderByDescending(e => e.DataMudanca)
        //        .FirstOrDefault();

        //    if (query != null)
        //    {
        //        ViewBag.Solicitante = query;

        //        if (query.CodPessoa != null || query.CodPessoa != 0)
        //        {
        //            var result = (from pessoa in _contextRM.PPESSOA
        //                          join gImagem in _contextRM.GIMAGEM
        //                          on pessoa.IDIMAGEM equals gImagem.ID
        //                          where pessoa.CODIGO == query.CodPessoa
        //                          select gImagem.IMAGEM)
        //                          .FirstOrDefault();

        //            base64Image = Convert.ToBase64String(result);
        //        }
        //        else
        //        {
        //            base64Image = null;
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        base64Image = null;
        //        return false;
        //    }
        //}

        //private bool TryGetSolicitanteData(string solicitanteValue, out string base64Image)
        //{
        //    _contextBS.Database.CloseConnection();

        //    var query = _contextBS.Funcionario
        //        .Where(e => e.Chapa == solicitanteValue)
        //        .OrderByDescending(e => e.DataMudanca)
        //        .FirstOrDefault();

        //    if (query != null)
        //    {
        //        ViewBag.Solicitante = query;

        //        if (query.CodPessoa != null || query.CodPessoa != 0)
        //        {
        //            var result = (from pessoa in _contextRM.PPESSOA
        //                          join gImagem in _contextRM.GIMAGEM
        //                          on pessoa.IDIMAGEM equals gImagem.ID
        //                          where pessoa.CODIGO == query.CodPessoa
        //                          select gImagem.IMAGEM)
        //                          .FirstOrDefault();

        //            base64Image = Convert.ToBase64String(result);
        //        }
        //        else
        //        {
        //            base64Image = null;
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        base64Image = null;
        //        return false;
        //    }
        //}

        public ActionResult GetEmprestimo(string? CodigoEmprestimo)
        {
            #region Query Produto
            var subquery = from pa in _context.ProdutoAlocado
                           join catalogo in _context.Catalogo on pa.IdProduto equals catalogo.Id
                           where catalogo.PorAferido == 1 && catalogo.PorSerial == 1
                           select pa.IdProduto;

            var queryemprestimo = from produto in _context.Produto
                                  join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                  join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                  join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                  join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaGroup
                                  from empresa in empresaGroup.DefaultIfEmpty()
                                  where produto.Ativo == 1 &&
                                        !subquery.Contains(produto.Id) &&
                                        produto.Quantidade != 0 &&
                                        catalogo.Codigo.Contains("08.08.01.02462") && // Assuming 'Codigo' is a property of the 'Catalogo' class
                                        ferramentaria.Id == 19
                                  orderby ferramentaria.Nome, catalogo.Descricao
                                  select new EmprestimoViewModel
                                  {
                                      DC_DataAquisicao = produto.DC_DataAquisicao,
                                      DC_Valor = produto.DC_Valor,
                                      DC_AssetNumber = produto.DC_AssetNumber,
                                      DC_Fornecedor = produto.DC_Fornecedor,
                                      GC_Contrato = produto.GC_Contrato,
                                      GC_DataInicio = produto.GC_DataInicio,
                                      GC_IdObra = produto.GC_IdObra,
                                      GC_OC = produto.GC_OC,
                                      GC_DataSaida = produto.GC_DataSaida,
                                      GC_NFSaida = produto.GC_NFSaida,
                                      Selo = produto.Selo,
                                      IdProduto = produto.Id,
                                      AF = produto.AF,
                                      PAT = produto.PAT,
                                      Quantidade = produto.Quantidade,
                                      QuantidadeMinima = produto.QuantidadeMinima,
                                      Localizacao = produto.Localizacao,
                                      RFM = produto.RFM,
                                      Observacao = produto.Observacao,
                                      DataRegistroProduto = produto.DataRegistro,
                                      DataVencimento = produto.DataVencimento,
                                      Certificado = produto.Certificado,
                                      Serie = produto.Serie,
                                      AtivoProduto = produto.Ativo,
                                      IdCatalogo = catalogo.Id,
                                      Codigo = catalogo.Codigo,
                                      NomeCatalogo = catalogo.Nome,
                                      Descricao = catalogo.Descricao,
                                      PorMetro = catalogo.PorMetro,
                                      PorAferido = catalogo.PorAferido,
                                      PorSerial = catalogo.PorSerial,
                                      DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                      DataRegistroCatalogo = catalogo.DataRegistro,
                                      AtivoCatalogo = catalogo.Ativo,
                                      IdCategoria = categoria.Id,
                                      IdCategoriaPai = categoria.IdCategoria,
                                      Classe = categoria.Classe,
                                      NomeCategoria = categoria.Nome,
                                      DataRegistroCategoria = categoria.DataRegistro,
                                      AtivoCategoria = categoria.Ativo,
                                      IdFerramentaria = ferramentaria.Id,
                                      NomeFerramentaria = ferramentaria.Nome,
                                      DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                      AtivoFerramentaria = ferramentaria.Ativo,
                                      IdEmpresa = empresa.Id,
                                      NomeEmpresa = empresa.Nome,
                                      GerenteEmpresa = empresa.Gerente,
                                      TelefoneEmpresa = empresa.Telefone,
                                      DataRegistroEmpresa = empresa.DataRegistro,
                                      AtivoEmpresa = empresa.Ativo,
                                      Status = (subquery.Contains(produto.Id) ? "Emprestado" : (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque"))
                                  };

            var resultemprestimo = queryemprestimo.ToList();

            ViewBag.Emprestimo = resultemprestimo;

            ViewBag.OpenModal = true;

            return RedirectToAction(nameof(Index));

            #endregion
        }

        public ActionResult RefreshFerramentaria()
        {
            httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);
            httpContextAccessor.HttpContext.Session.Remove(Sessao.FerramentariaNome);
            return RedirectToAction(nameof(Index));
        }

        //public ActionResult RefreshFerramentaria()
        //{


        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.Ferramentaria);

        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.Liberador);

        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);

        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.SupervisorChapa);

        //    httpContextAccessor.HttpContext.Session.Remove(Sessao.SupervisorId);

        //    GlobalData.list = null;

        //    return RedirectToAction(nameof(Index));

        //}


        public List<string> ValidateData(EmprestimoViewModel? EmprestimoItemValue, int? ObraValue)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            UserViewModel? SolicitanteModel = new UserViewModel();
            string? SolicitanteChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Solicitante);
            if (!string.IsNullOrEmpty(SolicitanteChapa))
            {
                SolicitanteModel = searches.SearchSolicitanteLoad(SolicitanteChapa);
                ViewBag.Solicitante = SolicitanteModel;
            }

            UserViewModel? LiberadorMOdel = new UserViewModel();
            string? LiberadorChapa = httpContextAccessor.HttpContext?.Session.GetString(Sessao.Liberador);
            if (!string.IsNullOrEmpty(LiberadorChapa))
            {
                LiberadorMOdel = searches.SearchLiberadorLoad(LiberadorChapa);
                ViewBag.Liberador = LiberadorMOdel;
            }

            var emprestimoList = HttpContext.Session.GetObject<List<EmprestimoViewModel>>(SessionKeyEmprestimoList) ?? new List<EmprestimoViewModel>();

            //string? ValidateSolicitante = httpContextAccessor.HttpContext.Session.GetString(Sessao.Solicitante);
            //string? ValidateLiberador = httpContextAccessor.HttpContext.Session.GetString(Sessao.Liberador);
            //string? ValidateSupervisor = httpContextAccessor.HttpContext.Session.GetString(Sessao.SupervisorId);
            int? ValidateFerramentaria = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);

            List<string> errors = new List<string>();

            if (!string.IsNullOrEmpty(SolicitanteModel.Chapa) == null)
            {
                errors.Add("Solicitante não selecionado..");
            }

            if (SolicitanteModel.CodSituacao == "D")
            {
                errors.Add("Solicitante com cod. situação diferente de admitido.");
            }

            if (LiberadorMOdel.Chapa == null)
            {
                errors.Add("Liberador não selecionado.");
            }

            if (LiberadorMOdel.CodSituacao == "D")
            {
                errors.Add("Liberador com cod. situação diferente de admitido.");
            }

            if (emprestimoList == null || emprestimoList.Count == 0)
            {
                errors.Add("Nenhum Produto na listagem de Itens a serem Emprestados.");
            }

            if (ObraValue == null)
            {
                errors.Add("Obra não selecionada.");
            }

            if (EmprestimoItemValue.Quantidade < EmprestimoItemValue.QuantityFrontEnd)
            {
                errors.Add($"Produto {EmprestimoItemValue.Codigo} com quantidade {EmprestimoItemValue.Quantidade} inferior ao limite disponível de {EmprestimoItemValue.QuantityFrontEnd}.");
            }

            int? DifferenceOfQuantity = EmprestimoItemValue.Quantidade - EmprestimoItemValue.QuantityFrontEnd;
            if (DifferenceOfQuantity < 0)
            {
                errors.Add($"Produto {EmprestimoItemValue.Codigo} only have {EmprestimoItemValue.Quantidade} left - {EmprestimoItemValue.QuantityFrontEnd} resulting to negative balance.");
            }

            if (EmprestimoItemValue.Classe == 2)
            {
                if (EmprestimoItemValue.IdControleCA != null)
                {
                    if (!EmprestimoItemValue.DataEmprestimoFrontEnd.HasValue)
                    {
                        errors.Add("DATA PREVISTA DE RETORNO não pode ser vazio.");
                    }
                    else
                    {
                        if (EmprestimoItemValue.DataEmprestimoFrontEnd.Value < DateTime.Now)
                        {
                            errors.Add(" DATA PREVISTA DE RETORNO não pode ser inferior a data de hoje.");
                        }
                    }
                }

            }

            if (EmprestimoItemValue.IdFerramentaria != ValidateFerramentaria)
            {
                errors.Add($"Produto {EmprestimoItemValue.Codigo} da ferramentaria {EmprestimoItemValue.IdFerramentaria} não corresponde a ferramentaria {ValidateFerramentaria} da sessão atual.");
            }

            //var result = (from catalogo in _context.Catalogo
            //              join produto in _context.Produto on catalogo.Id equals produto.IdCatalogo
            //              join produtoAlocado in _context.ProdutoAlocado on produto.Id equals produtoAlocado.IdProduto
            //              where catalogo.RestricaoEmprestimo == 1
            //                 && catalogo.Id == id
            //                 && produtoAlocado.Solicitante_Chapa == ValidateSolicitante
            //              select produtoAlocado.Id).FirstOrDefault();

            //if (result != null) 
            //{
            //    errors.Add(" Item Marcado como Restrição de Emprestimo. O solicitante já possui um Item marcado como restrição de emprestimo sem devolver em sua ficha!");
            //}

            if (!EmprestimoItemValue.QuantityFrontEnd.HasValue || EmprestimoItemValue.QuantityFrontEnd == 0)
            {
                errors.Add(" Quantidade não informada.");
            }

            //if (ValidateSolicitante != "*" && ValidateLiberador != "*")
            //{
            //if (SolicitanteModel.Secao != LiberadorMOdel.Secao)
            //{
            //    int? SupervisorAccepted = httpContextAccessor.HttpContext?.Session.GetInt32(SessionKeySupervisorAccepted);

            //    if (SupervisorAccepted != 1)
            //    {
            //        errors.Add("O Solicitante é de Setor diferente do Liberador e a liberação de material para este caso só pode ser feito pela Supervisão (informe sempre no campo OBS do item o motivo da liberação excepcional).");

            //        TempData["OpenModal"] = true;
            //    }

            //}
            //else
            //{
            //    //ViewData["ShowModal"] = false;
            //}
            //}



            return errors;
        }

        public ActionResult ChooseFerramentaria(int selectedValue)
        {
            if (selectedValue != null)
            {
                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.Ferramentaria, (int)selectedValue);
            }

            return RedirectToAction(nameof(Index));
        }
    }


}

//public ActionResult GetSolicitante(UserViewModel? SolicitanteModel)
////public ActionResult GetSolicitante(string? IdSolicitante)
//{          
//    if (SolicitanteModel != null)
//    {
//        //httpContextAccessor.HttpContext.Session.Remove(Sessao.Solicitante);

//        //httpContextAccessor.HttpContext.Session.SetString(Sessao.Solicitante, IdSolicitante);

//        string? SolicitanteValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Solicitante);

//        #region Find Solicitante

//        string base64Image;

//        if (SolicitanteValue != null && TryGetSolicitanteData(SolicitanteValue, out base64Image))
//        {
//            ViewData["Base64ImageSolicitante"] = base64Image;
//        }



//        if (TempData.ContainsKey("ErrorMessage"))
//        {
//            ViewBag.Error = TempData["ErrorMessage"].ToString();
//            TempData.Remove("ErrorMessage"); 
//        }

//        #endregion
//    }
//    else
//    {
//        TempData["ShowErrorAlert"] = true;
//        TempData["ErrorMessage"] = "Please Input Solicitante!";

//        return RedirectToAction(nameof(Index));
//    }



//    return RedirectToAction(nameof(Index));
//}


// GET: EmprestimoController
//public ActionResult FindSolicitante(string? IdSolicitante)
//{

//    _contextBS.Database.CloseConnection();

//    var query = _contextBS.Funcionario
//                .Where(e => e.Chapa == IdSolicitante)
//                .OrderByDescending(e => e.DataMudanca)
//                .FirstOrDefault();

//    if (query == null)
//    {
//        var emptyDataSolicitante = new FuncionarioViewModel();

//        return View(emptyDataSolicitante);
//    }

//    ViewBag.Solicitante = query;

//    if (query.CodPessoa != null || query.CodPessoa != 0)
//    {
//        var result = (from pessoa in _contextRM.PPESSOA
//                      join gImagem in _contextRM.GIMAGEM
//                      on pessoa.IDIMAGEM equals gImagem.ID
//                      where pessoa.CODIGO == query.CodPessoa
//                      select gImagem.IMAGEM)
//                  .FirstOrDefault();

//        string base64Image = Convert.ToBase64String(result);

//        // Pass the base64Image to the view
//        ViewData["Base64Image"] = base64Image;

//    }

//    return RedirectToAction(nameof(Index));     
//}
