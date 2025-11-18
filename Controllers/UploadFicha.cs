using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.EntitiesBS;

namespace FerramentariaTest.Controllers
{
    public class UploadFicha : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thUploadFichas.aspx";

        //private static VW_Usuario_NewViewModel? LoggedUserDetails = new VW_Usuario_NewViewModel();

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public UploadFicha(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
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

        // GET: UploadFicha
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

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {


                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.Solicitante);

                            int? FerramentariaValue = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.Ferramentaria);
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
                                httpContextAccessor.HttpContext.Session.SetInt32(Sessao.IdFerramentaria, (int)FerramentariaValue);
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

                            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                            //string base64Image;
                            //if (FuncionarioValue != null)
                            //{

                            //    //_contextBS.Database.CloseConnection();

                            //    var queryliberador = _contextBS.Funcionario
                            //                .Where(e => e.Chapa == FuncionarioValue)
                            //                .OrderByDescending(e => e.DataMudanca)
                            //                .FirstOrDefault();

                            //    if (queryliberador == null)
                            //    {
                            //        return View();
                            //    }

                            //    ViewBag.Funcionario = queryliberador;

                            //    if (queryliberador.CodPessoa != null || queryliberador.CodPessoa != 0)
                            //    {
                            //        var result = (from pessoa in _contextRM.PPESSOA
                            //                      join gImagem in _contextRM.GIMAGEM
                            //                      on pessoa.IDIMAGEM equals gImagem.ID
                            //                      where pessoa.CODIGO == queryliberador.CodPessoa
                            //                      select gImagem.IMAGEM)
                            //                  .FirstOrDefault();

                            //        base64Image = Convert.ToBase64String(result);

                            //        // Pass the base64Image to the view
                            //        ViewData["Base64ImageFunctionario"] = base64Image;

                            //    }
                            //}
                            UserViewModel? UsuarioModel = new UserViewModel();

                            httpContextAccessor.HttpContext?.Session.Remove(Sessao.Funcionario);
                            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
                            //if (FuncionarioValue != null)
                            //{
                            //    UsuarioModel = searches.SearchEmployeeOnLoad();
                            //    ViewBag.Funcionario = UsuarioModel;
                            //}


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

                //if (LoggedUserDetails.Id == null)
                //{
                //    LoggedUserDetails = usuariofer;
                //}

                //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
                //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

              
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

        public ActionResult SearchDev(string? IdFuncionario)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            List<FuncionarioViewModel> listEmployeeResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> listTerceiroResult = new List<FuncionarioViewModel>();
            List<FuncionarioViewModel> TotalResult = new List<FuncionarioViewModel>();
            UserViewModel? UsuarioModel = new UserViewModel();

            VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            if (IdFuncionario != null)
            {
                listTerceiroResult = searches.SearchTercerio(IdFuncionario);
                TotalResult.AddRange(listTerceiroResult);

                listEmployeeResult = searches.SearchEmployeeChapa(IdFuncionario);
                TotalResult.AddRange(listEmployeeResult);


                if (TotalResult.Count > 1)
                {
                    ViewBag.ListOfEmployees = TotalResult;
                    return View("Index");
                }
                else if (TotalResult.Count == 1)
                {
                    httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                    httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, TotalResult[0].Chapa);

                    UsuarioModel = searches.SearchEmployeeOnLoad();
                    List<MensagemSolicitanteViewModel> messages = new List<MensagemSolicitanteViewModel>();
                    messages = searches.SearchMensagem(UsuarioModel.Chapa, usuariofer.Id);
                    ViewBag.Messages = messages.Count > 0 ? messages : null;

                    ViewBag.Funcionario = UsuarioModel;

                    return View("Index");
                    //return RedirectToAction(nameof(Index));
                }
                else if (listEmployeeResult.Count == 0)
                {
                    ViewBag.Error = "No Searched has been found.";
                    return View("Index");
                }
            }
            else
            {
                ViewBag.Error = "Matricula/Nome is Required";
                return View("Index");
            }
            return View("Index");
        }

        public ActionResult SelectedUser(string? chapa)
        {
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);
            UserViewModel? UsuarioModel = new UserViewModel();

            VW_Usuario_NewViewModel usuariofer = auxiliar.retornaUsuario();

            if (chapa != null)
            {
                httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);
                httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, chapa);

                UsuarioModel = searches.SearchEmployeeOnLoad();
                List<MensagemSolicitanteViewModel> messages = new List<MensagemSolicitanteViewModel>();
                messages = searches.SearchMensagem(UsuarioModel.Chapa, usuariofer.Id);
                ViewBag.Messages = messages.Count > 0 ? messages : null;

                ViewBag.Funcionario = UsuarioModel;

                return View("Index");
            }

            return RedirectToAction(nameof(Index));
        }

        //public ActionResult SearchDev(string? IdFuncionario)
        //{
        //    if (IdFuncionario != null)
        //    {

        //        ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];

        //        ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

        //        httpContextAccessor.HttpContext.Session.Remove(Sessao.Funcionario);


        //        if (IdFuncionario != null)
        //        {
        //            httpContextAccessor.HttpContext.Session.SetString(Sessao.Funcionario, IdFuncionario);
        //        }

        //        var queryliberador = _contextBS.Funcionario
        //                         .Where(e => e.Chapa == IdFuncionario)
        //                         .OrderByDescending(e => e.DataMudanca)
        //                         .FirstOrDefault();

        //        ViewBag.Funcionario = queryliberador;

        //        string base64Image;

        //        if (queryliberador.CodPessoa != null || queryliberador.CodPessoa != 0)
        //        {
        //            var result = (from pessoa in _contextRM.PPESSOA
        //                          join gImagem in _contextRM.GIMAGEM
        //                          on pessoa.IDIMAGEM equals gImagem.ID
        //                          where pessoa.CODIGO == queryliberador.CodPessoa
        //                          select gImagem.IMAGEM)
        //                      .FirstOrDefault();

        //            base64Image = Convert.ToBase64String(result);

        //            // Pass the base64Image to the view
        //            ViewData["Base64ImageFunctionario"] = base64Image;

        //        }
        //    }

        //    return View("Index");
        //}

        public IActionResult GetArquivo(string? chapaFuncionario,int? coligadaFuncionario, DateTime? De, DateTime? Ate, int? Tipo)
        {
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            //ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
            //ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

            List<UploadFichaViewModel> AqruivoResult = new List<UploadFichaViewModel>();

            UserViewModel? UsuarioModel = new UserViewModel();
            string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
            if (FuncionarioValue != null)
            {
                UsuarioModel = searches.SearchEmployeeOnLoad();
                ViewBag.Funcionario = UsuarioModel;
            }


            AqruivoResult = (from arquivo in _context.Arquivo
                                 join logArquivo in _context.LogArquivo on arquivo.Id equals logArquivo.IdArquivo into logGroup
                                 from logArquivo in logGroup.DefaultIfEmpty()
                                 where arquivo.Ativo == 1
                                 && arquivo.Solicitante_CodColigada == coligadaFuncionario 
                                 && arquivo.Solicitante_Chapa == chapaFuncionario
                                 && (De == null || arquivo.DataRegistro >= De)
                                 && (Ate == null || arquivo.DataRegistro <= Ate.Value.AddDays(1).AddTicks(-1))
                                 //&& arquivo.DataRegistro >= TransactionDe.Date
                                 //&& arquivo.DataRegistro <= TransactionAte.Date.AddDays(1).AddTicks(-1)
                                 && (Tipo == 0 || arquivo.Tipo == Tipo)
                                 orderby arquivo.DataRegistro
                                 select new UploadFichaViewModel
                                 {
                                     Id = arquivo.Id,
                                     Ano = arquivo.Ano,
                                     Tipo = arquivo.Tipo,
                                     ArquivoNome = arquivo.ArquivoNome,
                                     DataRegistro = arquivo.DataRegistro,
                                     Ativo = arquivo.Ativo,
                                     ImageData = arquivo.ImageData,
                                     Solicitante_IdTerceiro = arquivo.Solicitante_IdTerceiro,
                                     Solicitante_CodColigada = arquivo.Solicitante_CodColigada,
                                     Solicitante_Chapa = arquivo.Solicitante_Chapa,
                                     IdUsuario = logArquivo.IdUsuario,
                                     ResponsavelUploadNome = arquivo.Responsavel,
                                 }).ToList();

                if (AqruivoResult != null)
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


                    foreach (var item in AqruivoResult)
                    {
                        if (item.ResponsavelUploadNome == null)
                        {
                            var GetLogArquvio = _context.LogArquivo.FirstOrDefault(i => i.IdArquivo == item.Id);
                            if (GetLogArquvio != null)
                            {
                                int? IdUsuario = GetLogArquvio.IdUsuario;
                                if (IdUsuario != null)
                                {
                                    var usuario = result.FirstOrDefault(u => u.Id == IdUsuario);

                                    if (usuario != null)
                                    {
                                        item.ResponsavelUploadNome = usuario.Nome;
                                    }

                                }
                            }
                        }                       
                    }
                }
            //}

            return View("Index", AqruivoResult);
        }

        [HttpPost]
        public IActionResult UploadAction(IFormFile file, int? below,string? chapaFuncionarioBelow,int? CodColigadaFuncionarioBelow)
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

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Visualizar == 1)
                        {


                            #region Validation
                            if (file == null)
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "No File Selected.";

                                return RedirectToAction(nameof(Index));
                            }

                            if (chapaFuncionarioBelow == null)
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "No Employee Searched.";

                                return RedirectToAction(nameof(Index));
                            }

                            if (file.Length > 1048576) // 1MB (1024 bytes * 1024)
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "File size should not exceed 1MB.";

                                return RedirectToAction(nameof(Index));
                            }

                            if (!file.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) && !file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Invalid file format. Only JPG images are allowed.";

                                return RedirectToAction(nameof(Index));
                            }

                            if (file.FileName.Length > 250)
                            {
                                TempData["ShowErrorAlert"] = true;
                                TempData["ErrorMessage"] = "Filename should not exceed 250 characters.";

                                return RedirectToAction(nameof(Index));
                            }
                            #endregion

                            string? FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            string? FileExtension = Path.GetExtension(file.FileName);
                            //string? FolderPath = "C:\\Repositorio\\SIB-Ferramentaria\\\\UploadFicha\\Repositorio\\" + chapaFuncionarioBelow;
                            string? FolderPath = "D:\\Ferramentaria\\UploadFicha\\" + chapaFuncionarioBelow;


                            string? FinalFileName = String.Concat(FileName, "_", DateTime.Now.Millisecond, FileExtension);
                            string? FilePath = Path.Combine(FolderPath, FinalFileName);

                            if (!Directory.Exists(FolderPath))
                            {
                                Directory.CreateDirectory(FolderPath);
                            }

                            // Save the file to the specified path
                            using (var stream = new FileStream(FilePath, FileMode.Create))
                            {
                                 file.CopyTo(stream);
                            }

                            byte[] imageData;

                            using (var stream = new MemoryStream())
                            {
                                file.CopyTo(stream);
                                imageData = stream.ToArray();
                            }

                            var InsertToArquivo = new Arquivo
                            {
                                Ano = DateTime.Now.Year,
                                Solicitante_IdTerceiro = 0,
                                Solicitante_CodColigada = CodColigadaFuncionarioBelow,
                                Solicitante_Chapa = chapaFuncionarioBelow,
                                IdUsuario = loggedUser.Id,
                                Tipo = below,
                                ArquivoNome = FinalFileName,
                                DataRegistro = DateTime.Now,
                                Ativo = 1,
                                ImageData = imageData,
                                Responsavel = loggedUser.Nome
                            };

                            _context.Add(InsertToArquivo);
                            _context.SaveChanges();

                            TempData["ShowSuccessAlert"] = true;

                            return RedirectToAction("Index");



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



        public IActionResult Exclude(int? id)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);

            try
            {
                //#region Authenticate User
                //    VW_Usuario_NewViewModel usuario = auxiliar.retornaUsuario();
                //    //usuario.Pagina = "Home/Index";
                //    usuario.Pagina = pagina;
                //    usuario.Pagina1 = log.LogWhat;
                //    usuario.Acesso = log.LogWhat;
                //    usuario = auxiliar.VerificaPermissao(usuario);

                //    if (usuario.Permissao == null)
                //    {
                //        usuario.Retorno = "Usuário sem permissão na página!";
                //        log.LogWhy = usuario.Retorno;
                //        auxiliar.GravaLogAlerta(log);
                //        return RedirectToAction("PreserveActionError", "Home", usuario);
                //    }
                //    else
                //    {
                //        if (usuario.Permissao.Excluir != 1)
                //        {
                //            usuario.Retorno = "Usuário sem permissão de Excluir a página de obra!";
                //            log.LogWhy = usuario.Retorno;
                //            auxiliar.GravaLogAlerta(log);
                //            return RedirectToAction("PreserveActionError", "Home", usuario);
                //        }
                //    }
                //#endregion

                LoggedUserData? loggedUser = HttpContext.Session.GetObject<LoggedUserData>(SessionKeyLoggedUserInformation) ?? new LoggedUserData();
                if (loggedUser != null)
                {
                    PermissionAccessModel? checkPermission = loggedUser?.ListOfPermissionAccess?.FirstOrDefault(i => i.Pagina == pagina);
                    if (checkPermission != null)
                    {
                        if (checkPermission.Excluir == 1)
                        {



                            ViewBag.ShowSuccessAlert = TempData["ShowSuccessAlert"] != null && (bool)TempData["ShowSuccessAlert"];
                            ViewBag.ShowErrorAlert = TempData["ShowErrorAlert"] != null && (bool)TempData["ShowErrorAlert"];

                            var Arquivo =  _context.Arquivo.Find(id);
                            if (Arquivo != null)
                            {
                                Arquivo.Ativo = 0;
                            }

                             _context.SaveChanges();

                            var InsertToLogArquivo = new LogArquivo
                            {
                                IdArquivo = id,
                                IdUsuario = loggedUser.Id,
                                Tipo = 2,
                                DataRegistro = DateTime.Now
                            };

                            _context.Add(InsertToLogArquivo);
                            _context.SaveChanges();

                            TempData["ShowSuccessAlert"] = true;

                            return RedirectToAction("Index");


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

        // GET: UploadFicha/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UploadFicha/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UploadFicha/Create
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

        // GET: UploadFicha/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UploadFicha/Edit/5
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

        // GET: UploadFicha/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UploadFicha/Delete/5
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
