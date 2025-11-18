using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Linq;
using FerramentariaTest.EntitiesRM;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerramentariaTest.Controllers
{
    public class AtribuicaoFerramentaria : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thLogAtribuicaoFerramentaria.aspx";
        private MapperConfiguration mapeamentoClasses;

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public AtribuicaoFerramentaria(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Funcionario, FuncionarioViewModel>();
                cfg.CreateMap<FuncionarioViewModel, Funcionario>();
            });
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: AtribuicaoFerramentaria
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

        public IActionResult GetLog(DateTime? De, DateTime? Ate)
        {
            List<LogAtribuicaoFerramentariaViewModel> LogAtribuicaoFerramentariaResult = null;

            if (De.HasValue && De != DateTime.MinValue && Ate.HasValue && Ate != DateTime.MinValue)
            {
                DateTime TransactionDe = De.Value.Date;
                DateTime TransactionAte = Ate.Value.AddDays(1).AddTicks(-1);

                LogAtribuicaoFerramentariaResult = (from log in _context.LogAtribuicaoFerramentaria                          
                                                     join ferramentaria in _context.Ferramentaria on log.IdFerramentaria equals ferramentaria.Id
                                                     where log.DataRegistro >= TransactionDe
                                                        && log.DataRegistro <= TransactionAte
                                                     select new LogAtribuicaoFerramentariaViewModel
                                                     {
                                                         Id = log.Id,
                                                         IdUsuario = log.IdUsuario,
                                                         IdFerramentaria = log.IdFerramentaria,
                                                         NomeFerramentaria = ferramentaria.Nome,
                                                         IdUsuarioResponsavel = log.IdUsuarioResponsavel,
                                                         Acao = log.Acao,
                                                         DataRegistro = log.DataRegistro
                                                     }).ToList();

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

                LogAtribuicaoFerramentariaResult = (from log in LogAtribuicaoFerramentariaResult
                                                    join user in result on log.IdUsuario equals user.Id
                                                    join UserResponsavel in result on log.IdUsuario equals UserResponsavel.Id
                                                    select new LogAtribuicaoFerramentariaViewModel
                                                    {
                                                        Id = log.Id,
                                                        IdUsuario = log.IdUsuario,
                                                        ChapaUsuario = user.Chapa,
                                                        NomeUsuario = user.Nome,
                                                        IdFerramentaria = log.IdFerramentaria,
                                                        NomeFerramentaria = log.NomeFerramentaria,                                                       
                                                        IdUsuarioResponsavel = log.IdUsuarioResponsavel,
                                                        ChapaUsuarioResponsavel = UserResponsavel.Chapa,
                                                        NomeUsuarioResponsavel = UserResponsavel.Nome,
                                                        Acao = log.Acao,
                                                        DataRegistro = log.DataRegistro
                                                    }).ToList();
                                                   


                if (LogAtribuicaoFerramentariaResult.Count > 0)
                {
                    var ListLogAtribuicaoFerramentaria = LogAtribuicaoFerramentariaResult;
                    List<int?> distinctUsuario = new List<int?>();
                    //List<int?> distinctResponsavel = new List<int?>();
                    List<VW_Usuario_New> ListUsuario = new List<VW_Usuario_New>();
                    //List<FuncionarioViewModel> ListResponsavel = new List<FuncionarioViewModel>();

                    distinctUsuario = LogAtribuicaoFerramentariaResult.Select(x => x.IdUsuario).Distinct().ToList();
                    //distinctResponsavel = LogAtribuicaoFerramentariaResult.Select(x => x.IdUsuarioResponsavel).Distinct().ToList();
                    //ListUsuario = GetDistinctUser(distinctUsuario);
                    //ListResponsavel = GetDistinctResponsavel(distinctResponsavel);


                    DataTable dataTable = new DataTable();
                    // Add columns to the dataTable
                    dataTable.Columns.Add("Id"); //1
                    dataTable.Columns.Add("IdUsuario"); //2
                    dataTable.Columns.Add("NomeUsuario"); //3
                    dataTable.Columns.Add("IdFerramentaria"); //4
                    dataTable.Columns.Add("NomeFerramentaria"); //5
                    dataTable.Columns.Add("IdUsuarioResponsavel"); //6
                    dataTable.Columns.Add("NomeUsuarioResponsavel"); //7
                    dataTable.Columns.Add("Acao"); //8
                    dataTable.Columns.Add("DataRegistro", typeof(DateTime)); //9

                    // Add data rows from listDevolucao
                    foreach (var item in ListLogAtribuicaoFerramentaria)
                    {
                        //var ChapaUsuarioInfo = GetSolicitacaoInfo(item.ChapaUsuario);
                        //var ChapaUsuarioInfo = ListUsuario.FirstOrDefault(i => i.Id == item.IdUsuario);
                        //var ChapaUsuarioResponsavel = GetResponsavelInfo(item.IdUsuarioResponsavel);

                        var row = dataTable.NewRow();
                        row["Id"] = item.Id;
                        row["IdUsuario"] = item.IdUsuario;
                        row["NomeUsuario"] = item.NomeUsuario;
                        row["IdFerramentaria"] = item.IdFerramentaria;
                        row["NomeFerramentaria"] = item.NomeFerramentaria;
                        row["IdUsuarioResponsavel"] = item.IdUsuarioResponsavel;
                        row["NomeUsuarioResponsavel"] = item.NomeUsuarioResponsavel;
                        row["Acao"] = item.Acao;
                        row["DataRegistro"] = item.DataRegistro.HasValue ? (object)item.DataRegistro.Value : DBNull.Value;

                        dataTable.Rows.Add(row);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var package = new ExcelPackage(memoryStream))
                        {

                            var worksheet = package.Workbook.Worksheets.Add("Historico");

                            // Add data from dataTable to the worksheet
                            worksheet.Cells.LoadFromDataTable(dataTable, true);
                            worksheet.Column(9).Style.Numberformat.Format = "dd/MM/yyyy";

                            using (var cells = worksheet.Cells[worksheet.Dimension.Address])
                            {
                                cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            package.Save();
                        }

                        memoryStream.Position = 0;
                        byte[] content = memoryStream.ToArray();

                        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                        return File(content, contentType, fileName);



                        //// Save the Excel file to the physical path
                        //string fileName = "DEVOLUCAO_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                        //string caminhoFisico = "D:\\Ferramentaria\\Relatorio\\" + fileName;
                        //System.IO.File.WriteAllBytes(caminhoFisico, content);

                        //// Define the content type
                        //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                        //// Trigger the download
                        //return File(System.IO.File.ReadAllBytes(caminhoFisico), contentType, fileName);

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
                ViewBag.Error = "Select Dates";
                return View("Index");
            }

    
            //return RedirectToAction(nameof(Index));

        }

        public List<VW_Usuario_New> GetDistinctUser(List<int?> Ids)
        {
            List<VW_Usuario_New> UserValues = new List<VW_Usuario_New>();
            //VW_Usuario_New finduser = new VW_Usuario_New();

            foreach (int? userId in Ids)
            {
                var finduser = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == userId);
                if (finduser != null)
                {
                    UserValues.Add(finduser);
                }
            }
                //var entity = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Chapa == solicitanteChapa);
            return UserValues;
        }

        public List<FuncionarioViewModel> GetDistinctResponsavel(List<int?> Ids)
        {
            List<FuncionarioViewModel> UserValues = new List<FuncionarioViewModel>();

            foreach (int? userId in Ids)
            {
                var result = _context.Usuario.FirstOrDefault(u => u.Id == userId);
                if (result != null)
                {
                    var newresult = _contextBS.Funcionario.FirstOrDefault(i => i.Chapa == result.Chapa);
                    if (newresult != null)
                    {
                        var mapper = mapeamentoClasses.CreateMapper();
                        //responsavel = mapper.Map<FuncionarioViewModel>(newresult);
                        UserValues.Add(mapper.Map<FuncionarioViewModel>(newresult));
                    }
                }
                //var finduser = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == userId);
                //if (finduser != null)
                //{
                //    UserValues.Add(finduser);
                //}
            }
            //var entity = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Chapa == solicitanteChapa);
            return UserValues;
        }

        public VW_Usuario_New GetSolicitacaoInfo(string? solicitanteChapa)
        {
            var entity = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Chapa == solicitanteChapa);

            return entity;
        }

        public FuncionarioViewModel GetResponsavelInfo (int? idResponsavel)
        {
            FuncionarioViewModel responsavel = new FuncionarioViewModel();
            var result = _context.Usuario.FirstOrDefault(u => u.Id == idResponsavel);
            if (result != null)
            {
                var newresult = _contextBS.Funcionario.FirstOrDefault(i => i.Chapa == result.Chapa);
                if (newresult != null)
                {
                    var mapper = mapeamentoClasses.CreateMapper();
                    responsavel = mapper.Map<FuncionarioViewModel>(newresult);
                }
              
            }

            return responsavel;
        }

        // GET: AtribuicaoFerramentaria/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AtribuicaoFerramentaria/Create
        public ActionResult Create()
        {
            //LogAtribuicaoFerramentariaResult = (from log in _context.LogAtribuicaoFerramentaria
            //                                    join usuario in _context.Usuario on log.IdUsuario equals usuario.Id
            //                                    //join func in _contextRM.PFUNC on new { usuario.CodColigada, usuario.Chapa } equals new { func.CODCOLIGADA, func.CHAPA }
            //                                    join usuarioResponsavel in _context.Usuario on log.IdUsuarioResponsavel equals usuarioResponsavel.Id
            //                                    //join funcUsuarioResponsavel in _contextRM.PFUNC on new { usuarioResponsavel.CodColigada, usuarioResponsavel.Chapa } equals new { funcUsuarioResponsavel.CODCOLIGADA, funcUsuarioResponsavel.CHAPA }
            //                                    join ferramentaria in _context.Ferramentaria on log.IdFerramentaria equals ferramentaria.Id
            //                                    where log.DataRegistro >= new DateTime(2012, 4, 16, 0, 0, 0)
            //                                       && log.DataRegistro <= new DateTime(2012, 4, 24, 23, 59, 59)
            //                                    select new LogAtribuicaoFerramentariaViewModel
            //                                    {
            //                                        Id = log.Id,
            //                                        IdUsuario = log.IdUsuario,
            //                                        //ChapaUsuario = usuario.Chapa,
            //                                        //NomeUsuario = func.Nome,
            //                                        IdFerramentaria = log.IdFerramentaria,
            //                                        NomeFerramentaria = ferramentaria.Nome,
            //                                        IdUsuarioResponsavel = log.IdUsuarioResponsavel,
            //                                        //ChapaUsuarioResponsavel = usuarioResponsavel.Chapa,
            //                                        //NomeUsuarioResponsavel = funcUsuarioResponsavel.Nome,
            //                                        Acao = log.Acao,
            //                                        DataRegistro = log.DataRegistro
            //                                    }).ToList();
            return View();
        }

        // POST: AtribuicaoFerramentaria/Create
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

        // GET: AtribuicaoFerramentaria/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AtribuicaoFerramentaria/Edit/5
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

        // GET: AtribuicaoFerramentaria/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AtribuicaoFerramentaria/Delete/5
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
