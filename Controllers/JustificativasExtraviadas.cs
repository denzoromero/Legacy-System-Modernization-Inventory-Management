using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Net.Mail;
using System.Text;

namespace FerramentariaTest.Controllers
{
    public class GlobalJustificativas
    {
        public static List<JustificativasExtraviadasViewModel> ListJustificativas { get; set; }
    }

    public class JustificativasExtraviadas : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thRelProdutoExtraviado.aspx";
        private static int? GlobalPagination;
        //private List<JustificativasExtraviadasViewModel> JustificativasExtraviadasExport = null;

        private const string SessionKeyListJustificativasExtraviadas = "ListJustificativasExtraviadas";

        private const string SessionKeyLoggedUserInformation = "LoggedUserInformation";

        public JustificativasExtraviadas(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: JustificativasExtraviadas
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

                            var FerramentariaList = from ferramentaria in _context.Ferramentaria
                                                    where ferramentaria.Ativo == 1 &&
                                                    !(_context.VW_Ferramentaria_Ass_Solda.Select(x => x.Id).Contains(ferramentaria.Id))
                                                    orderby ferramentaria.Nome
                                                    select new
                                                    {
                                                        ferramentaria.Id,
                                                        ferramentaria.Nome
                                                    };

                            ViewBag.FerramentariaList = FerramentariaList;

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

        public IActionResult GetLog(int? page, DateTime? De, DateTime? Ate, string? Ferramentaria)
        {
            #region Dropdown List
            var FerramentariaList = from ferramentaria in _context.Ferramentaria
                                    where ferramentaria.Ativo == 1 &&
                                    !(_context.VW_Ferramentaria_Ass_Solda.Select(x => x.Id).Contains(ferramentaria.Id))
                                    orderby ferramentaria.Nome
                                    select new
                                    {
                                        ferramentaria.Id,
                                        ferramentaria.Nome
                                    };

            ViewBag.FerramentariaList = FerramentariaList;
            #endregion

            List<JustificativasExtraviadasViewModel> JustificativasExtraviadasResult = new List<JustificativasExtraviadasViewModel>();

            if (De != null && De != DateTime.MinValue && Ate != null && Ate != DateTime.MinValue)
            {
                DateTime TransactionDe = De.Value;
                DateTime TransactionAte = Ate.Value;

                if (Ferramentaria == "Selecionar...")
                {
                    Ferramentaria = null;
                }

                JustificativasExtraviadasResult = (from extravioProduto in _context.VW_Extravio_Produto
                            where extravioProduto.DataHoraRegistroExtravio >= TransactionDe.Date
                                && extravioProduto.DataHoraRegistroExtravio <= TransactionAte.Date.AddDays(1).AddTicks(-1)
                                && (Ferramentaria == null || extravioProduto.Ferramentaria == Ferramentaria)
                            orderby extravioProduto.Codigo, extravioProduto.Descrição
                            select new JustificativasExtraviadasViewModel
                            {
                                Id = extravioProduto.Id,
                                Ferramentaria = extravioProduto.Ferramentaria,
                                Codigo = extravioProduto.Codigo,
                                Descricao = extravioProduto.Descrição,
                                Quantidade = extravioProduto.Quantidade,
                                AF = extravioProduto.AFSerial,
                                PAT = extravioProduto.PAT,
                                Obs = extravioProduto.Obs,
                                Solicitante_Chapa = extravioProduto.MatriculaSolicitante,
                                DataEmprestimo = extravioProduto.DataEmprestimo,
                                Balconista_Nome = extravioProduto.BalconistaRegistroExtravio,
                                Justificativa = extravioProduto.JustificativaExtravio,
                                DataRegistroExtravio = extravioProduto.DataHoraRegistroExtravio
                            }).ToList();

                if (JustificativasExtraviadasResult.Count > 0)
                {
                    GlobalPagination = 10;
                    int pageSize = GlobalPagination ?? 10;
                    int pageNumber = 1;

                    httpContextAccessor.HttpContext?.Session.Remove(SessionKeyListJustificativasExtraviadas);
                    HttpContext.Session.SetObject(SessionKeyListJustificativasExtraviadas, JustificativasExtraviadasResult);

                    //GlobalJustificativas.ListJustificativas = JustificativasExtraviadasResult;

                    IPagedList<JustificativasExtraviadasViewModel> JustificativasExtraviadasPagedList = JustificativasExtraviadasResult.ToPagedList(pageNumber, pageSize);

                    return View("Index", JustificativasExtraviadasPagedList);
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
            #region Dropdown List
            var FerramentariaList = from ferramentaria in _context.Ferramentaria
                                    where ferramentaria.Ativo == 1 &&
                                    !(_context.VW_Ferramentaria_Ass_Solda.Select(x => x.Id).Contains(ferramentaria.Id))
                                    orderby ferramentaria.Nome
                                    select new
                                    {
                                        ferramentaria.Id,
                                        ferramentaria.Nome
                                    };

            ViewBag.FerramentariaList = FerramentariaList;
            #endregion

            int pageSize = GlobalPagination ?? 10;
            int pageNumber = (page ?? 1);

            List<JustificativasExtraviadasViewModel>? ListCatalogoModel = HttpContext.Session.GetObject<List<JustificativasExtraviadasViewModel>>(SessionKeyListJustificativasExtraviadas) ?? new List<JustificativasExtraviadasViewModel>();

            IPagedList<JustificativasExtraviadasViewModel> JustificativasExtraviadasViewModelPagedList = ListCatalogoModel.ToPagedList(pageNumber, pageSize);

            return View("Index", JustificativasExtraviadasViewModelPagedList);
        }

        public async Task<IActionResult> Export()
        {

            List<JustificativasExtraviadasViewModel>? itemsToExport = HttpContext.Session.GetObject<List<JustificativasExtraviadasViewModel>>(SessionKeyListJustificativasExtraviadas) ?? new List<JustificativasExtraviadasViewModel>();

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            DataTable dataTable = new DataTable();
            // Add columns to the dataTable
            dataTable.Columns.Add("Id"); //1
            dataTable.Columns.Add("Ferramentaria"); //2
            dataTable.Columns.Add("Codigo"); //3
            dataTable.Columns.Add("Descricao"); //4
            dataTable.Columns.Add("Quantidade"); //5
            dataTable.Columns.Add("AF Serial"); //6
            dataTable.Columns.Add("PAT"); //7
            dataTable.Columns.Add("Obs"); //8
            dataTable.Columns.Add("Matricula Solicitante"); //9
            dataTable.Columns.Add("Data do Emprestimo", typeof(DateTime)); //10
            dataTable.Columns.Add("Balconista do Registro do Extravio"); //11
            dataTable.Columns.Add("Justificativa do Extravio"); //12
            dataTable.Columns.Add("Data e Hora do Registro do Extravio", typeof(DateTime)); //13

            foreach (JustificativasExtraviadasViewModel item in itemsToExport)
            {
                var row = dataTable.NewRow();
                row["Id"] = item.Id; // Replace with the actual property name
                row["Ferramentaria"] = item.Ferramentaria; // Replace with the actual property name
                row["Codigo"] = item.Codigo;
                row["Descricao"] = item.Descricao;
                row["Quantidade"] = item.Quantidade;
                row["AF Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["Obs"] = item.Obs;
                row["Matricula Solicitante"] = item.Solicitante_Chapa;
                row["Data do Emprestimo"] = item.DataEmprestimo.HasValue == true ? (object)item.DataEmprestimo.Value : DBNull.Value;
                row["Balconista do Registro do Extravio"] = item.IdUsuario;
                row["Justificativa do Extravio"] = item.Justificativa;
                row["Data e Hora do Registro do Extravio"] = item.DataRegistroExtravio.HasValue == true ? (object)item.DataRegistroExtravio.Value : DBNull.Value;

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {

                    var worksheet = package.Workbook.Worksheets.Add("ITENS PARA DEVOLUÇÃO");

                    // Merge cells A1 to P1 and set the content to "ITENS PARA DEVOLUÇÃO"
                    worksheet.Cells["A1:M1"].Merge = true;
                    worksheet.Cells["A1"].Value = "JUSTIFICATIVAS EXTRAVIADAS";
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.Font.Name = "Arial";
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Column(10).Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Column(13).Style.Numberformat.Format = "dd/MM/yyyy";

                    using (var range = worksheet.Cells["A2:P2"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    // Get the columns starting from A2
                    var currentColumn = 'A'; // Start from column A


                    foreach (DataColumn column in dataTable.Columns)
                    {
                        worksheet.Cells[$"{currentColumn}2"].Value = column.ColumnName;
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Name = "Arial";
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Bold = true;
                        currentColumn++;
                    }


                    // Load data into the worksheet starting from A3
                    worksheet.Cells["A3"].LoadFromDataTable(dataTable, PrintHeaders: false);

                    var dataRange = worksheet.Cells["A3:M" + (dataTable.Rows.Count + 2)]; // Adjust the range to include all data rows

                    using (var dataCells = dataRange)
                    {
                        dataCells.Style.Font.Name = "Arial";
                        dataCells.Style.Font.Size = 10;
                    }

                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;


                    package.Save();
                }

                memoryStream.Position = 0;
                byte[] content = memoryStream.ToArray();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "JUSTIFICATIVAS_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                FileContentResult fileResult = File(content, contentType, fileName);
                //string? basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
                string? basePath = "D:\\Ferramentaria\\Relatorio\\";

                if (!Directory.Exists(basePath))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(basePath);
                }

                string caminhoFisico = basePath + fileName;
                System.IO.File.WriteAllBytes(caminhoFisico, content);

                return File(content, contentType, fileName);
            }
        }


        public ActionResult Enviar()
        {
            #region Dropdown List
            var FerramentariaList = from ferramentaria in _context.Ferramentaria
                                    where ferramentaria.Ativo == 1 &&
                                    !(_context.VW_Ferramentaria_Ass_Solda.Select(x => x.Id).Contains(ferramentaria.Id))
                                    orderby ferramentaria.Nome
                                    select new
                                    {
                                        ferramentaria.Id,
                                        ferramentaria.Nome
                                    };

            ViewBag.FerramentariaList = FerramentariaList;
            #endregion

            try
            {
                if (GlobalJustificativas.ListJustificativas.Count > 0)
                {
                    string filename = MakeExcel();

                    //string url = "http://localhost:5296/Ferramentaria/Repositorio/Relatorio/" + filename;
                    //string url = "C:/Ferramentaria/Repositorio/Relatorio/" + filename;
                    string url = filename;

                    ViewBag.Filepathname = url;

                    int pageSize = GlobalPagination ?? 10;
                    int pageNumber = 1;

                    IPagedList<JustificativasExtraviadasViewModel> JustificativasExtraviadasViewModelPagedList = GlobalJustificativas.ListJustificativas.ToPagedList(pageNumber, pageSize);

                    return View("Index", JustificativasExtraviadasViewModelPagedList);
                }


                return View("Index");
            }
            catch (Exception ex)
            {
                return View(ex);
            }
      
        }

        [HttpPost]
        public ActionResult SendToEmail(string? For, string? CC, string? Anexo, string? Message)
        {
            Log log = new Log();
            log.LogWhat = pagina + "/Index";
            log.LogWhere = pagina;
            Auxiliar auxiliar = new Auxiliar(_context, _contextBS, httpContextAccessor, _configuration);
            Searches searches = new Searches(_context, _contextBS, _contextRM, _contextSeek, httpContextAccessor, _configuration);

            try
            {
                var FerramentariaList = searches.OnLoadJustificativarFerramentaria();
                ViewBag.FerramentariaList = FerramentariaList;

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

                StringBuilder validationErrors = new StringBuilder();

                if (ValidateForm(validationErrors, For, CC, Anexo, Message))
                {
                    SmtpClient smtpClient = new SmtpClient("smtprelay.brasfels.com.br");
                    smtpClient.UseDefaultCredentials = true;
                 
                    MailMessage mailMessage = new MailMessage();
                    string? tryemail = "Daby.Romero@seatrium.com";
                    mailMessage.From = new MailAddress(tryemail);
                    mailMessage.To.Add(For);
                    if (!string.IsNullOrEmpty(CC))
                    {
                        mailMessage.CC.Add(CC);
                    }
                    mailMessage.Subject = "JUSTIFICATIVAS EXTRAVIADAS";
                    mailMessage.Body = Message;

                    //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
                    string? basePath = "D:\\Ferramentaria\\Relatorio\\";
                    string filePath = Path.Combine(basePath, Anexo);
                    mailMessage.Attachments.Add(new Attachment(filePath));

                    smtpClient.Timeout = 10000; // Set timeout to 10 seconds (adjust as needed)
                    smtpClient.Send(mailMessage);


                    return View("Index");
                }
                else
                {
                    ViewBag.ErrorModal = validationErrors.ToString();
                    ViewBag.Filepathname = Anexo;
                    int pageSize = GlobalPagination ?? 10;
                    int pageNumber = 1;

                    IPagedList<JustificativasExtraviadasViewModel> JustificativasExtraviadasViewModelPagedList = GlobalJustificativas.ListJustificativas.ToPagedList(pageNumber, pageSize);

                    return View("Index", JustificativasExtraviadasViewModelPagedList);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error sending email: " + ex.Message;
                return View("Index");
            }
            
        }

        private bool ValidateForm(StringBuilder validationErrors, string? For, string? CC, string? Anexo, string? Message)
        {
            if (For != null)
            {
                if (For.Contains(";"))
                {
                    string[] recipients = For.Split(';');
                    List<string> recipientList = recipients.ToList();
                    List<string> errorList = new List<string>();

                    foreach (var recipient in recipientList)
                    {
                        if (!recipient.Trim().EndsWith("@seatrium.com", StringComparison.OrdinalIgnoreCase))
                        {
                            errorList.Add(recipient);                         
                        }
                    }

                    if (errorList.Count > 0)
                    {
                        validationErrors.AppendLine($"Invalid recipient email: {string.Join(", ", errorList)} , Only for @seatrium.com");
                    }
                }
                else
                {
                    if (!For.Trim().EndsWith("@seatrium.com", StringComparison.OrdinalIgnoreCase))
                    {
                        validationErrors.AppendLine($"Invalid recipient email: {For}, Only for @seatrium.com");
                    }
                }                
            }
            else
            {
                validationErrors.AppendLine("Please Input Receipent.");
            }

            if (CC != null)
            {
                if (CC.Contains(";"))
                {
                    string[] recipients = For.Split(';');
                    List<string> recipientList = recipients.ToList();
                    List<string> errorList = new List<string>();

                    foreach (var recipient in recipientList)
                    {
                        if (!recipient.Trim().EndsWith("@seatrium.com", StringComparison.OrdinalIgnoreCase))
                        {
                            errorList.Add(recipient);
                        }
                    }

                    if (errorList.Count > 0)
                    {
                        validationErrors.AppendLine($"Invalid recipient email: {string.Join(", ", errorList)} , Only for @seatrium.com");
                    }
                }
                else
                {
                    if (!For.Trim().EndsWith("@seatrium.com", StringComparison.OrdinalIgnoreCase))
                    {
                        validationErrors.AppendLine($"Invalid recipient email: {For}, Only for @seatrium.com");
                    }
                }
            }

            if (Anexo != null)
            {
                //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
                string? basePath = "D:\\Ferramentaria\\Relatorio\\";
                string filePath = Path.Combine(basePath, Anexo);

                if (!System.IO.File.Exists(filePath))
                {
                    validationErrors.AppendLine("No File Found. Please generate new report.");
                }
            }

            return validationErrors.Length == 0;
        }

        public ActionResult ViewFile(string? filename)
        {
            //string basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
            string? basePath = "D:\\Ferramentaria\\Relatorio\\";
            string filePath = Path.Combine(basePath, filename);
            //string filename = Path.GetFileName(filepath);
            byte[] fileContents = System.IO.File.ReadAllBytes(filePath);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(fileContents, contentType, filename);
        }

        public string MakeExcel()
        {
            DataTable dataTable = new DataTable();
            // Add columns to the dataTable
            dataTable.Columns.Add("Id");
            dataTable.Columns.Add("Ferramentaria");
            dataTable.Columns.Add("Codigo");
            dataTable.Columns.Add("Descricao");
            dataTable.Columns.Add("Quantidade");
            dataTable.Columns.Add("AF Serial");
            dataTable.Columns.Add("PAT");
            dataTable.Columns.Add("Obs");
            dataTable.Columns.Add("Matricula Solicitante");
            dataTable.Columns.Add("Data do Emprestimo");
            dataTable.Columns.Add("Balconista do Registro do Extravio");
            dataTable.Columns.Add("Justificativa do Extravio");
            dataTable.Columns.Add("Data e Hora do Registro do Extravio");

            foreach (var item in GlobalJustificativas.ListJustificativas)
            {
                var row = dataTable.NewRow();
                row["Id"] = item.Id; // Replace with the actual property name
                row["Ferramentaria"] = item.Ferramentaria; // Replace with the actual property name
                row["Codigo"] = item.Codigo;
                row["Descricao"] = item.Descricao;
                row["Quantidade"] = item.Quantidade;
                row["AF Serial"] = item.AF;
                row["PAT"] = item.PAT;
                row["Obs"] = item.Obs;
                row["Matricula Solicitante"] = item.Solicitante_Chapa;
                row["Data do Emprestimo"] = item.DataEmprestimo.HasValue == true ? item.DataEmprestimo.Value.ToString("dd/MM/yyyy") : string.Empty;
                row["Balconista do Registro do Extravio"] = item.IdUsuario;
                row["Justificativa do Extravio"] = item.Justificativa;
                row["Data e Hora do Registro do Extravio"] = item.DataRegistroExtravio.HasValue == true ? item.DataRegistroExtravio.Value.ToString("dd/MM/yyyy") : string.Empty;

                dataTable.Rows.Add(row);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage(memoryStream))
                {

                    var worksheet = package.Workbook.Worksheets.Add("ITENS PARA DEVOLUÇÃO");

                    // Merge cells A1 to P1 and set the content to "ITENS PARA DEVOLUÇÃO"
                    worksheet.Cells["A1:M1"].Merge = true;
                    worksheet.Cells["A1"].Value = "JUSTIFICATIVAS EXTRAVIADAS";
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.Font.Name = "Arial";
                    worksheet.Cells["A1"].Style.Font.Size = 16;

                    using (var range = worksheet.Cells["A2:P2"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    // Get the columns starting from A2
                    var currentColumn = 'A'; // Start from column A


                    foreach (DataColumn column in dataTable.Columns)
                    {
                        worksheet.Cells[$"{currentColumn}2"].Value = column.ColumnName;
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Name = "Arial";
                        worksheet.Cells[$"{currentColumn}2"].Style.Font.Bold = true;
                        currentColumn++;
                    }


                    // Load data into the worksheet starting from A3
                    worksheet.Cells["A3"].LoadFromDataTable(dataTable, PrintHeaders: false);

                    var dataRange = worksheet.Cells["A3:M" + (dataTable.Rows.Count + 2)]; // Adjust the range to include all data rows

                    using (var dataCells = dataRange)
                    {
                        dataCells.Style.Font.Name = "Arial";
                        dataCells.Style.Font.Size = 10;
                    }

                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    worksheet.Cells.AutoFitColumns();

                    package.Save();
                }

                memoryStream.Position = 0;
                byte[] content = memoryStream.ToArray();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "JUSTIFICATIVAS_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                FileContentResult fileResult = File(content, contentType, fileName);
                //string? basePath = "C:\\Ferramentaria\\Repositorio\\Relatorio\\";
                string? basePath = "D:\\Ferramentaria\\Relatorio\\";

                if (!Directory.Exists(basePath))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(basePath);
                }

                string caminhoFisico = basePath + fileName;
                System.IO.File.WriteAllBytes(caminhoFisico, content);

                return fileName;
            }
        }

        // GET: JustificativasExtraviadas/Details/5
        public ActionResult Details(int id)
        {
            //JustificativasExtraviadasResult = (from pe in _context.ProdutoExtraviado
            //            join pa in _context.ProdutoAlocado on pe.IdProdutoAlocado equals pa.Id
            //            join p in _context.Produto on pa.IdProduto equals p.Id
            //            join f in _context.Ferramentaria on p.IdFerramentaria equals f.Id
            //            join c in _context.Catalogo on p.IdCatalogo equals c.Id
            //            where pe.DataRegistro >= TransactionDe.Date
            //                  && pe.DataRegistro <= TransactionAte.Date.AddDays(1).AddTicks(-1)
            //                  && f.Nome == Ferramentaria
            //                  && pe.Ativo == 1
            //            select new JustificativasExtraviadasViewModel
            //            {
            //                Id = pe.Id,
            //                Ferramentaria = f.Nome,
            //                Codigo = c.Codigo,
            //                Descricao = c.Nome,
            //                Quantidade = pe.Quantidade,
            //                AF = p.AF,
            //                PAT = p.PAT,
            //                Obs = pa.Observacao,
            //                Solicitante_Chapa = pa.Solicitante_Chapa,
            //                DataEmprestimo = pa.DataEmprestimo,
            //                Justificativa = pe.Observacao,
            //                DataRegistroExtravio = pe.DataRegistro,
            //                IdUsuario = pe.IdUsuario
            //            }).ToList();

            //if (JustificativasExtraviadasResult != null)
            //{
            //    foreach (var item in JustificativasExtraviadasResult)
            //    {
            //        int? IdUsuario = item.IdUsuario;

            //        // Use await to asynchronously wait for the result
            //        var usuario = await _contextBS.VW_Usuario_New
            //            .FirstOrDefaultAsync(u => u.Id == IdUsuario);

            //        // Check if a user was found
            //        if (usuario != null)
            //        {
            //            // Assign the Chapa value to the item
            //            item.Balconista_Nome = usuario.Nome;
            //        }
            //        else
            //        {
            //            var usuarioOld = await _contextBS.VW_Usuario
            //            .FirstOrDefaultAsync(u => u.Id == IdUsuario);

            //            item.Balconista_Nome = usuarioOld.Nome;
            //        }

            //    }
            //}


            return View();
        }

 



        // GET: JustificativasExtraviadas/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: JustificativasExtraviadas/Edit/5
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

        // GET: JustificativasExtraviadas/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: JustificativasExtraviadas/Delete/5
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
