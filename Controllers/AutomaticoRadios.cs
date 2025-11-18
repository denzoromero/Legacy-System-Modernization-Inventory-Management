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
    public class AutomaticoRadios : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "DownloadAutomatico.aspx?p=TOOLHOUSE.VW_Todos_Radios&id=9";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public AutomaticoRadios(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: AutomaticoRadios
        public async Task<IActionResult> Index()
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
                            await _context.SaveChangesAsync();

                            var query = (from e in _context.VW_Todos_Radios
                                         select e).ToList();

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("Id"); //1
                            dataTable.Columns.Add("Ferramentaria"); //2
                            dataTable.Columns.Add("Catalogo"); //3
                            dataTable.Columns.Add("Classe"); //4
                            dataTable.Columns.Add("Tipo"); //5
                            dataTable.Columns.Add("Codigo"); //6
                            dataTable.Columns.Add("Item"); //7
                            dataTable.Columns.Add("AfSerial"); //8
                            dataTable.Columns.Add("PAT"); //9
                            dataTable.Columns.Add("QtdEstoque"); //10
                            dataTable.Columns.Add("QtdMinEstoque"); //11
                            dataTable.Columns.Add("ControlePor"); //12
                            dataTable.Columns.Add("DataRegistro", typeof(DateTime)); //13
                            dataTable.Columns.Add("DataValidade", typeof(DateTime)); //14
                            dataTable.Columns.Add("NumeroSerie"); //15
                            dataTable.Columns.Add("RFM"); //16
                            dataTable.Columns.Add("DC_DataAquisicao", typeof(DateTime)); //17
                            dataTable.Columns.Add("DC_Valor"); //18
                            dataTable.Columns.Add("DC_AssetNumber"); //19
                            dataTable.Columns.Add("DC_Fornecedor"); //20
                            dataTable.Columns.Add("GC_Contrato"); //21
                            dataTable.Columns.Add("GC_DataInicio", typeof(DateTime)); //22
                            dataTable.Columns.Add("Nome"); //23
                            dataTable.Columns.Add("GC_OC"); //24
                            dataTable.Columns.Add("GC_DataSaida", typeof(DateTime)); //25
                            dataTable.Columns.Add("GC_NFSaida"); //26
                            dataTable.Columns.Add("Status"); //27
                            dataTable.Columns.Add("DataEmprestimo"); //28
                            dataTable.Columns.Add("DataPrevistaDevolucao", typeof(DateTime)); //29
                            dataTable.Columns.Add("SolicitanteChapa"); //30
                            dataTable.Columns.Add("SolicitanteNome"); //31
                            dataTable.Columns.Add("LiberadorChapa"); //32
                            dataTable.Columns.Add("LiberadorNome"); //33
                            dataTable.Columns.Add("Balconista"); //34


                            foreach (var item in query)
                            {

                                var row = dataTable.NewRow();
                                row["Id"] = item.Id;
                                row["Ferramentaria"] = item.Ferramentaria;
                                row["Catalogo"] = item.Catalogo;
                                row["Classe"] = item.Classe;
                                row["Tipo"] = item.Tipo;
                                row["Codigo"] = item.Codigo;
                                row["Item"] = item.Item;
                                row["AfSerial"] = item.AfSerial;
                                row["PAT"] = item.PAT;
                                row["QtdEstoque"] = item.QtdEstoque;
                                row["QtdMinEstoque"] = item.QtdMinEstoque;
                                row["ControlePor"] = item.ControlePor;
                                row["DataRegistro"] = item.DataRegistro.HasValue == true ? (object)item.DataRegistro.Value : DBNull.Value; //13
                                row["DataValidade"] = item.DataValidade.HasValue == true ? (object)item.DataValidade.Value : DBNull.Value; //14
                                row["NumeroSerie"] = item.NumeroSerie;
                                row["RFM"] = item.RFM;
                                row["DC_DataAquisicao"] = item.DC_DataAquisicao.HasValue == true ? (object)item.DC_DataAquisicao.Value : DBNull.Value; //17
                                row["DC_Valor"] = item.DC_Valor;
                                row["DC_AssetNumber"] = item.DC_AssetNumber;
                                row["DC_Fornecedor"] = item.DC_Fornecedor;
                                row["GC_Contrato"] = item.GC_Contrato;
                                row["GC_DataInicio"] = item.GC_DataInicio.HasValue == true ? (object)item.GC_DataInicio.Value : DBNull.Value; //22
                                row["Nome"] = item.Nome;
                                row["GC_OC"] = item.GC_OC;
                                row["GC_DataSaida"] = item.GC_DataSaida.HasValue == true ? (object)item.GC_DataSaida.Value : DBNull.Value; //25
                                row["GC_NFSaida"] = item.GC_NFSaida;
                                row["Status"] = item.Status;
                                row["DataEmprestimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value;
                                row["DataPrevistaDevolucao"] = item.DataPrevistaDevolucao.HasValue == true ? (object)item.DataPrevistaDevolucao.Value : DBNull.Value; //29
                                row["SolicitanteChapa"] = item.SolicitanteChapa;
                                row["SolicitanteNome"] = item.SolicitanteNome;
                                row["LiberadorChapa"] = item.LiberadorChapa;
                                row["LiberadorNome"] = item.LiberadorNome;
                                row["Balconista"] = item.Balconista;

                                dataTable.Rows.Add(row);
                            }

                            using (var memoryStream = new MemoryStream())
                            {
                                using (var package = new ExcelPackage(memoryStream))
                                {

                                    var worksheet = package.Workbook.Worksheets.Add("Historico");

                                    // Add data from dataTable to the worksheet
                                    worksheet.Cells.LoadFromDataTable(dataTable, true);
                                    worksheet.Column(13).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(14).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(17).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(22).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(25).Style.Numberformat.Format = "dd/MM/yyyy";
                                    worksheet.Column(29).Style.Numberformat.Format = "dd/MM/yyyy";


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

                                //string? basePath = "C:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";
                                string? basePath = "D:\\Repositorio\\SIB-Ferramentaria\\SIB\\Repositorio\\";


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

        // GET: AutomaticoRadios/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AutomaticoRadios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AutomaticoRadios/Create
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

        // GET: AutomaticoRadios/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AutomaticoRadios/Edit/5
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

        // GET: AutomaticoRadios/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AutomaticoRadios/Delete/5
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
