using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.IO;

namespace FerramentariaTest.Controllers
{
    public class AutomaticoRadiosEmprestados : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "DownloadAutomatico.aspx?p=TOOLHOUSE.VW_Radios_Emprestados&id=3";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public AutomaticoRadiosEmprestados(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: AutomaticoRadiosEmprestados
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

                            int? IdUsuario = loggedUser.Id;
                            int? Relatorio = 9;
                            int? Processar = 1;
                            string? Arquivo = String.Format("REL_{0}_{1}_{2}.xlsx", Relatorio, DateTime.Now.ToString("yyyyMMddHHmmmss"), IdUsuario);

                            var InsertLogRelatorio = new LogRelatorio
                            {
                                IdUsuario = IdUsuario,
                                Relatorio = Relatorio,
                                Arquivo = Arquivo,
                                Processar = Processar,
                                Ativo = 1,
                                DataRegistro = DateTime.Now
                            };



                            _context.Add(InsertLogRelatorio);
                            _context.SaveChanges();

                            var query = (from e in _context.VW_Radios_Emprestados
                                         select e).ToList();

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("Catalogo");  //1
                            dataTable.Columns.Add("Classe"); //2
                            dataTable.Columns.Add("Tipo"); //3
                            dataTable.Columns.Add("Codigo"); //4
                            dataTable.Columns.Add("Produto"); //5
                            dataTable.Columns.Add("AfSerial"); //6
                            dataTable.Columns.Add("PAT"); //7
                            dataTable.Columns.Add("Observação"); //8
                            dataTable.Columns.Add("Setor Origem"); //9
                            dataTable.Columns.Add("Solicitante Chapa"); //10
                            dataTable.Columns.Add("Solicitante Nome"); //11 
                            dataTable.Columns.Add("Solicitante Função"); //12 
                            dataTable.Columns.Add("Solicitante Seção"); //13
                            dataTable.Columns.Add("Solicitante Status"); //14
                            dataTable.Columns.Add("Liberador Chapa"); //15
                            dataTable.Columns.Add("Liberador Nome"); //16
                            dataTable.Columns.Add("Balconista"); //17
                            dataTable.Columns.Add("Data Empréstimo", typeof(DateTime)); //18
                            dataTable.Columns.Add("Data Prevista Devolução", typeof(DateTime)); //19
                            dataTable.Columns.Add("Data Vencimento", typeof(DateTime)); //20

                            foreach (var item in query)
                            {

                                var row = dataTable.NewRow();
                                row["Catalogo"] = item.Catalogo;
                                row["Classe"] = item.Classe;
                                row["Tipo"] = item.Tipo;
                                row["Codigo"] = item.Codigo;
                                row["Produto"] = item.Produto;
                                row["AfSerial"] = item.AfSerial;
                                row["PAT"] = item.PAT;
                                row["Observação"] = item.Observacao;
                                row["Setor Origem"] = item.SetorOrigem;
                                row["Solicitante Chapa"] = item.SolicitanteChapa;
                                row["Solicitante Nome"] = item.SolicitanteNome;
                                row["Solicitante Função"] = item.SolicitanteFuncao;
                                row["Solicitante Seção"] = item.SolicitanteSecao;
                                row["Solicitante Status"] = item.SolicitanteStatus;
                                row["Liberador Chapa"] = item.LiberadorChapa;
                                row["Liberador Nome"] = item.LiberadorNome;
                                row["Balconista"] = item.Balconista;
                                row["Data Empréstimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value; //18
                                row["Data Prevista Devolução"] = item.DataPrevistaDevolucao.HasValue == true ? (object)item.DataPrevistaDevolucao.Value : DBNull.Value; //19
                                row["Data Vencimento"] = item.DataVencimento.HasValue == true ? (object)item.DataVencimento.Value : DBNull.Value;    //20

                                dataTable.Rows.Add(row);
                            }

                            using (var memoryStream = new MemoryStream())
                            {
                                using (var package = new ExcelPackage(memoryStream))
                                {

                                    var worksheet = package.Workbook.Worksheets.Add("Historico");

                                    // Add data from dataTable to the worksheet
                                    worksheet.Cells.LoadFromDataTable(dataTable, true);
                                    worksheet.Column(18).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(19).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(20).Style.Numberformat.Format = "dd/MM/yyyy";

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

                                FileContentResult fileResult = File(content, contentType, Arquivo);

                                string? basePath = "D:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";
                                //string? basePath = "C:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";

                                if (!Directory.Exists(Path.Combine(basePath, "Relatorio")))
                                {
                                    // Create the directory if it doesn't exist
                                    Directory.CreateDirectory(Path.Combine(basePath, "Relatorio"));
                                }

                                string caminhoFisico = basePath + "Relatorio\\" + Arquivo;
                                System.IO.File.WriteAllBytes(caminhoFisico, content);

                                return fileResult;

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
                return View();
            }
        }

        // GET: AutomaticoRadiosEmprestados/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AutomaticoRadiosEmprestados/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AutomaticoRadiosEmprestados/Create
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

        // GET: AutomaticoRadiosEmprestados/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AutomaticoRadiosEmprestados/Edit/5
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

        // GET: AutomaticoRadiosEmprestados/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AutomaticoRadiosEmprestados/Delete/5
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
