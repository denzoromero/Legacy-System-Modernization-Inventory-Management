using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using FerramentariaTest.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SelectPdf;
using System.Text;
using System.Text.RegularExpressions;
using FerramentariaTest.Entities;
using Microsoft.AspNetCore.Authorization;

namespace FerramentariaTest.Controllers
{
    public class AudittingController : Controller
    {
        private readonly ILogger<AudittingController> _logger;
        private readonly IUserContextService _userContext;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IEmployeeService _employeeService;
        private readonly IHistoryAlocadoService _historyService;
        private readonly IAuditService _auditService;

        private readonly ICompositeViewEngine _viewEngine;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AudittingController(ILogger<AudittingController> logger, ICorrelationIdService correlationIdService, IUserContextService userContext, IEmployeeService employeeService, ICompositeViewEngine viewEngine, IWebHostEnvironment webHostEnvironment, IHistoryAlocadoService historyService, IAuditService auditService)
        {
            _logger = logger;
            _correlationIdService = correlationIdService;
            _userContext = userContext;
            _employeeService = employeeService;
            _viewEngine = viewEngine;
            _webHostEnvironment = webHostEnvironment;
            _historyService = historyService;
            _auditService = auditService;
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> Index()
        {
            try
            {
                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Landed on Page: AudittingController.Index", user.Id);

                return View();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
                return View();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
                return View();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
                return View();
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
                return View();
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction(actionName: nameof(HomeController.Login), controllerName: nameof(HomeController).Replace("Controller", ""),
                    new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred";
                return View();
            }     
        }


        //html to pdf
        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GenerateReport(string employeeChapa, int employeeCodColigada, int selectedYear, int? selectedCatalogo, int employeeCodPessoa, int employeeIdTerceiro)
        {
            try
            {

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - GenerateReport for EmployeeChapa: {employeeChapa}, " +
                                        "EmployeeCodColigada: {employeeCodColigada}" +
                                        "Year: {selectedYear}" +
                                        "Catalogo: {selectedCatalogo}" +
                                        "CodPessoa: {employeeCodPessoa}" +
                                        "EmployeeTerceiro: {employeeIdTerceiro}",
                                        user.Id, employeeChapa, employeeCodColigada, selectedYear, selectedCatalogo, employeeCodPessoa, employeeIdTerceiro);


                List<HistoryAlocadoReportModel> overAllResult = new List<HistoryAlocadoReportModel> ();

                EmployeeInformationBS employee = employeeCodPessoa != 0 ? await _employeeService.GetSelectedEmployee(employeeCodPessoa) : await _employeeService.GetSelectedThirdParty(employeeIdTerceiro);

                if (employeeIdTerceiro == 0)
                {
                    List<HistoryAlocadoReportModel>? HistoryList = await _historyService.GetEmployeeItemHistory(employeeChapa, employeeCodColigada, selectedYear);
                    if (HistoryList.Count > 0) overAllResult.AddRange(HistoryList);

                    List<HistoryAlocadoReportModel>? AllocationList = await _historyService.GetEmployeeItemAllocation(employeeChapa, employeeCodColigada, selectedYear);
                    if (AllocationList.Count > 0) overAllResult.AddRange(AllocationList);
                }
                else
                {
                    List<HistoryAlocadoReportModel>? HistoryList = await _historyService.GetTerceiroItemHistory(employeeIdTerceiro, selectedYear);
                    if (HistoryList.Count > 0) overAllResult.AddRange(HistoryList);

                    List<HistoryAlocadoReportModel>? AllocationList = await _historyService.GetTerceiroItemAllocation(employeeIdTerceiro, selectedYear);
                    if (AllocationList.Count > 0) overAllResult.AddRange(AllocationList);
                }

             

                if (overAllResult.Count == 0) throw new ProcessErrorException("No result history and allocation found.");

                List<CatalogGroupModel> finalResult = GroupFinalResult(overAllResult);


                //List<CatalogGroupModel>? HistoryList = await _historyService.GetEmployeeItemHistory(employeeChapa, employeeCodColigada, selectedYear);

                StringBuilder htmlToPdf = new StringBuilder();

                htmlToPdf.Append("<!DOCTYPE html>");
                htmlToPdf.Append("<html>");
                //head
                htmlToPdf.Append("<head>");
                
                //style
                htmlToPdf.Append("<style>");
                htmlToPdf.Append("body { margin: 5mm; }");
                htmlToPdf.Append(HeaderCSS());
                htmlToPdf.Append(FooterCSS());
                htmlToPdf.Append(BodyCSS());
                htmlToPdf.Append(pageCSS());
                htmlToPdf.Append("</style>");

                htmlToPdf.Append("</head>");

                //body
                htmlToPdf.Append("<body>");

                //header
                htmlToPdf.Append(HeaderHTML(employee, selectedYear));

                //body
                htmlToPdf.Append(BodyHTML(finalResult));

                //footer
                htmlToPdf.Append(FooterHTML());
                htmlToPdf.Append("</body>");

                htmlToPdf.Append("</html>");

                return Ok(htmlToPdf.ToString());

                //return Content(allHtml.ToString(), "text/html");

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - Argument Client Error.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - InvalidOperationException Client Error.");
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest(ex.Message);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }
        }

        #region CSS

        private string pageCSS()
        {
            string css = @"
                @media print {

                        body {
                            margin: 0;
                            padding: 0;
                            font-size: 8px;
                        }

    .page-break {
                page-break-before: always;
            }

                        .headerPart {
                            position: fixed;
                            top: 0;
                            left: 0;
                            right: 0;
                            background: white;
                            height: 190px;
                        }

                        .footerPart {
                            position: fixed;
                            bottom: 0;
                            left: 0;
                            right: 0;
                            background: white;
                            height: 35px;
                        }

                }
                         ";

            return css;
        }

        private string HeaderCSS()
        {
            string css = @"

            .headerPart {
                font-family: Arial;
            }

            #main-header-container {
                display: flex;
                justify-content: space-between;
                align-items: center;
                height: 40px;
            }

            .logo-container {
                flex: 0 0 auto;
            }

            .title-container {
                flex: 1;
                text-align: center;
                font-size: 11px;
            }

            .datetime-container {
                flex: 0 0 auto;
                font-size: 11px;
            }

            #seatrium-image {
                height: 35px;
                width: auto;
                display: block;
            }


 #employee-information-container {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            flex-wrap: wrap;
            min-height: 110px;
            page-break-inside: avoid;
            align-items: flex-start;
            gap: 10px;
            width: 100%;
        }

   .information-container {
            display: flex;
            flex-direction: column;
        }

 .info-name {
            width: 20%;
            min-width: 20%;
        }

   .info-id {
            width: 15%;
            min-width: 15%;
        }

 .info-status {
            width: 5%;
            min-width: 5%;
        }

        .info-role {
            width: 15%;
            min-width: 15%;
        }

        .info-date {
            width: 10%;
            min-width: 10%;
        }

        .information-label {
            font-size: 8px;
        }

        .information-value {
            font-weight: bold;
            font-size: 10px;
            word-wrap: break-word;
            overflow-wrap: break-word;
            width: 100%;
            min-height: 1.2em;
        }

 #employeePicture {
            height: 100px;
            width: 90px;
            display: block;
            object-fit: cover;
            border: 1px solid #ccc;
            align-self: flex-start;
            flex-shrink: 0;
        }

    .table-header-row {
            display: flex;
            width: 100%;
            background-color: #e9ecef;
            font-weight: bold;
            border-top: 2px dashed #000;
            border-bottom: 2px dashed #000;
            border-left: none;
            border-right: none;
            page-break-after: avoid;
            font-size: 9px;
        }

   .col-movimento {
            width: 7%;
            min-width: 7%;
            padding: 5px 0px;
        }

  .col-data {
            width: 10%;
            min-width: 10%;
           padding: 5px 0px;
            text-align: center;
        }

   .col-qtde {
            width: 5%;
            min-width: 5%;
           padding: 5px 0px;
            text-align: center;
        }

   .col-local {
            width: 10%;
            min-width: 10%;
              padding: 5px 0px;
            text-align: center;
        }

  .col-ca {
            width: 5%;
            min-width: 5%;
               padding: 5px 0px;
            text-align: center;
        }

   .col-balconista {
            width: 28%;
            min-width: 28%;
              padding: 5px 0px;
        }

 .col-id {
            width: 24%;
            min-width: 24%;
              padding: 5px 0px;
        }

  .col-validation {
            width: 10%;
            min-width: 10%;
              padding: 5px 0px;
               text-align: center;
        }

      
                         ";

            return css;
        }

        private string FooterCSS()
        {
            string css = @"

    .footerPart {
            font-family: Arial;
            font-size: 8px;
            text-align: justify;
        }

        .footerPart p {
            margin: 0;
            padding: 0;
            font-size: 8px;
        }
                         ";

            return css;
        }

        private string BodyCSS()
        {
            string css = @"

       .content {
            margin-top: 195px;
            margin-bottom: 30px;
            font-family: Arial, sans-serif;
            font-size: 8px;
            width: 100%;
        }

        .product-section {
            margin-bottom: 4px;
            page-break-inside: avoid;
            break-inside: avoid;
        }

     .product-header {
            background-color: #f8f9fa;
            font-weight: bold;
            border-bottom: 2px dashed #000;
            margin-bottom: 2px;
            padding: 3px 0px 0px 0px;
            font-size: 8px;
            width: 100%;
        }

    .table-header-row {
            display: flex;
            width: 100%;
            background-color: #e9ecef;
            font-weight: bold;
            border-top: 2px dashed #000;
            border-bottom: 2px dashed #000;
            border-left: none;
            border-right: none;
            page-break-after: avoid;
            font-size: 9px;
        }

        .table-row {
            display: flex;
            width: 100%;
            min-height: 18px;
            max-height: 36px;
            page-break-inside: avoid;
            break-inside: avoid;
            overflow: hidden;
            font-family: Arial, sans-serif;
        }

    .col-movimento {
            width: 7%;
            min-width: 7%;
            padding: 5px 0px;
        }

        .col-data {
            width: 10%;
            min-width: 10%;
           padding: 5px 0px;
            text-align: center;
        }

        .col-qtde {
            width: 5%;
            min-width: 5%;
           padding: 5px 0px;
            text-align: center;
        }

        .col-local {
            width: 10%;
            min-width: 10%;
              padding: 5px 0px;
            text-align: center;
        }

        .col-ca {
            width: 5%;
            min-width: 5%;
               padding: 5px 0px;
            text-align: center;
        }

        .col-balconista {
            width: 28%;
            min-width: 28%;
              padding: 5px 0px;
        }

        .col-id {
            width: 24%;
            min-width: 24%;
              padding: 5px 0px;
        }

        .col-validation {
            width: 10%;
            min-width: 10%;
              padding: 5px 0px;
               text-align: center;
        }

        .lost-item {
            color: red;
        }


                        ";

            return css;
        }

        #endregion

        #region header

        private string HeaderHTML(EmployeeInformationBS employee,int year)
        {
          
            StringBuilder headerHTML = new StringBuilder();

            //headerPart
            headerHTML.Append("<div class='headerPart'>");

            headerHTML.Append(HeaderContainer(year));
            headerHTML.Append("<hr />");
            headerHTML.Append(EmployeeContainer(employee));
            headerHTML.Append(TableHeader());
            headerHTML.Append("</div>");

            return headerHTML.ToString();
        }

        private string HeaderContainer(int year)
        {
            StringBuilder headContainer = new StringBuilder();


            var SeatriumImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "seatrium grande.png");
            if (!System.IO.File.Exists(SeatriumImagePath))
            {

            }

            var SeatriumImageBytes = System.IO.File.ReadAllBytes(SeatriumImagePath);
            var Seatrium64Image = Convert.ToBase64String(SeatriumImageBytes);

            headContainer.Append("<div id='main-header-container'>");

            headContainer.Append($"<div class='logo-container'> <img id='seatrium-image' src='#' alt='companyLogo' /> </div>");
            headContainer.Append("<div class='title-container'> <b>Ficha do Colaborador</b> </div>");
            headContainer.Append($"<div class='datetime-container'> <b>Year:{year}</b> </div>");

            headContainer.Append("</div>");

            return headContainer.ToString();
        }

        private string EmployeeContainer(EmployeeInformationBS emloyee)
        {

            StringBuilder employeeContainer = new StringBuilder();

            employeeContainer.Append("<div id='employee-information-container'>");

            employeeContainer.Append("<div class='information-container info-name'>");
            employeeContainer.Append("<span class='information-label'>Name</span>");
            employeeContainer.Append($"<span class='information-value'>{emloyee.Nome}</span>");
            employeeContainer.Append("</div>");

            employeeContainer.Append("<div class='information-container info-id'>");
            employeeContainer.Append("<span class='information-label'>ID No</span>");
            employeeContainer.Append($"<span class='information-value'>{emloyee.Chapa}</span>");
            employeeContainer.Append("</div>");

            employeeContainer.Append("<div class='information-container info-status'>");
            employeeContainer.Append("<span class='information-label'>Status</span>");
            employeeContainer.Append($"<span class='information-value'>{emloyee.CodSituacao}</span>");
            employeeContainer.Append("</div>");

            employeeContainer.Append("<div class='information-container info-role'>");
            employeeContainer.Append("<span class='information-label'>Function</span>");
            employeeContainer.Append($"<span class='information-value'>{emloyee.Funcao}</span>");
            employeeContainer.Append("</div>");

            employeeContainer.Append("<div class='information-container info-date'>");
            employeeContainer.Append("<span class='information-label'>Admission</span>");
            employeeContainer.Append($"<span class='information-value'>{emloyee.DataAdmissao}</span>");
            employeeContainer.Append("</div>");

            employeeContainer.Append("<div class='information-container info-date'>");
            employeeContainer.Append("<span class='information-label'>Dismissal</span>");
            employeeContainer.Append($"<span class='information-value'></span>");
            employeeContainer.Append("</div>");

            //employeeContainer.Append($"<div>  <img src='image-not-available.jpg' id='employeePicture' />     </div>");
            employeeContainer.Append($"<div>  <img src='{emloyee.ImageString}' id='employeePicture' />     </div>");

            employeeContainer.Append("</div>");

            return employeeContainer.ToString();

        }

        private string TableHeader()
        {
            StringBuilder TableHTML = new StringBuilder();
            TableHTML.Append("<div class='table-header-row'>");

            TableHTML.Append("<div class='col-movimento'>Movement</div>");
            TableHTML.Append("<div class='col-data'>Date/Time</div>");
            TableHTML.Append("<div class='col-qtde'>Qty</div>");
            TableHTML.Append("<div class='col-local'>Loc</div>");
            TableHTML.Append("<div class='col-ca'>CA</div>");
            TableHTML.Append("<div class='col-balconista'>Balconista</div>");
            TableHTML.Append("<div class='col-id'>Transaction ID</div>");
            TableHTML.Append("<div class='col-validation'>Validation</div>");

            TableHTML.Append("</div>");

            return TableHTML.ToString();
        }

        #endregion

        #region footer

        private string FooterHTML()
        {
            StringBuilder footerHTML = new StringBuilder();
            footerHTML.Append("<div class='footerPart'>");
            footerHTML.Append("<hr>");
            footerHTML.Append("<p>");
            footerHTML.Append(@"
                                Em atendimento a alinea h do item 6.6.1 da NR06, nesta ficha de controle electronico encontram-se relacionados os.
                                Equipamentos de Protecao Individual - EPI fornecidos oportunamente pela Estaleiro Brasfels Ltda. ao empregado,
                                atraves da utilizacao do cartao de ponto e senha electronica individual e intransferivel do empregado.
                                ");
            footerHTML.Append("</p>");
            footerHTML.Append("</div>");

            return footerHTML.ToString();
        }

        #endregion

        #region body

        private string BodyHTML(List<CatalogGroupModel> HistoryList)
        {
            StringBuilder bodyHTML = new StringBuilder();

            const int PAGE_MAX_HEIGHT = 810; // px
            //const int ROW_MIN_HEIGHT = 18; // px
            //const int ROW_MAX_HEIGHT = 35; // px
            const int PRODUCT_HEADER_HEIGHT = 18; // px (estimated)
            const int SECTION_MARGIN = 2; // px

            //content
            bodyHTML.Append("<div class='content'>");

            int currentPageHeight = 0;
            bool isFirstProduct = true;

            foreach (CatalogGroupModel item in HistoryList)
            {
                // Calculate total height needed for this product section
                int sectionHeight = PRODUCT_HEADER_HEIGHT;

             

                foreach (HistoryAlocadoReportModel history in item.ItemAllocation)
                {
               
                    // Calculate row height based on content
                    int rowHeight = CalculateRowHeight(history);
                    sectionHeight += rowHeight;

                    if (history.ItemClass != "Consumable")
                    {
                        // Add return row height if exists
                        if (!string.IsNullOrEmpty(history.ReturnedDateString))
                        {
                            int returnRowHeight = CalculateRowHeight(history);
                            sectionHeight += returnRowHeight;
                        }

                        if (history.LostItems.HasValue && history.LostItems != 0)
                        {
                            int returnRowHeight = CalculateRowHeight(history);
                            sectionHeight += returnRowHeight;
                        }
                    }
                }

                sectionHeight += SECTION_MARGIN;

                // Check if this product section fits on current page
                if (currentPageHeight + sectionHeight > PAGE_MAX_HEIGHT)
                {
                    // If it doesn't fit and current page has content, add page break
                    if (currentPageHeight > 0)
                    {
                        bodyHTML.Append("<div class='page-break'></div>");
                        bodyHTML.Append("<div style='height: 190px; background: transparent;'></div> ");
                        currentPageHeight = 0;
                    }

                    // If the section itself is too large for one page, split it
                    if (sectionHeight > PAGE_MAX_HEIGHT)
                    {
                        bodyHTML.Append(SplitLargeProductSection(item, PAGE_MAX_HEIGHT, ref currentPageHeight));
                    }
                    else
                    {
                        bodyHTML.Append(RenderProductSection(item));
                        currentPageHeight += sectionHeight;
                    }
                }
                else
                {
                    // Section fits on current page
                    bodyHTML.Append(RenderProductSection(item));
                    currentPageHeight += sectionHeight;
                }

                isFirstProduct = false;
            }     

            //end div for content
            bodyHTML.Append("</div>");

            return bodyHTML.ToString();
        }


        private int CalculateRowHeight(HistoryAlocadoReportModel history)
        {
            const int BASE_ROW_HEIGHT = 18;
            const int HEIGHT_PER_LINE = 18; // Approximate height per text line
            const int MAX_CHARS_PER_LINE = 54;

            // Calculate height for location fields (most likely to wrap)
            int locationOriginLines = CalculateTextLines(history.LocationOrigin, MAX_CHARS_PER_LINE);
            int locationReturnLines = CalculateTextLines(history.LocationReturn, MAX_CHARS_PER_LINE);

            int maxLocationLines = Math.Max(locationOriginLines, locationReturnLines);

            // If text wraps beyond 1 line, add extra height
            int calculatedHeight = BASE_ROW_HEIGHT + ((maxLocationLines - 1) * HEIGHT_PER_LINE);

            return Math.Min(calculatedHeight, 36); // Cap at max height
        }

        private int CalculateTextLines(string text, int maxCharsPerLine)
        {

            if (string.IsNullOrEmpty(text)) return 1;

            //if (text.Length <= maxCharsPerLine) return 1;

            if (text.Length <= 15) return 1;

            return 2;

            // Simple calculation: divide text length by max chars per line
            //int lines = (int)Math.Ceiling((double)text.Length / maxCharsPerLine);
            //return Math.Max(lines, 1);
        }

        //private string SplitLargeProductSection(CatalogGroupModel item, int pageMaxHeight)
        //{
        //    StringBuilder sectionHTML = new StringBuilder();
        //    int currentHeight = 0;
        //    bool isFirstChunk = true;

        //    // Product header (always show at top of first page)
        //    sectionHTML.Append($"<div class='product-header'>{item.Description} - {item.Code}</div>");
        //    currentHeight += 25; // Product header height

        //    foreach (HistoryAlocadoReportModel history in item.ItemAllocation)
        //    {
        //        int rowHeight = CalculateRowHeight(history);

        //        // Check if we need page break
        //        if (currentHeight + rowHeight > pageMaxHeight)
        //        {
        //            sectionHTML.Append("</div>"); // Close current product-section
        //            sectionHTML.Append("<div class='page-break'></div>");
        //            //sectionHTML.Append("<div class='product-section'>");

        //            sectionHTML.Append("<div style='height: 350px; background: transparent;'></div> ");

        //            // Repeat product header on new page for continuity
        //            //sectionHTML.Append($"<div class='product-header-continued'>{item.Description} - {item.Code} (continued)</div>");
        //            //currentHeight = 25; // Reset height with continued header
        //        }

        //        // Render borrow row
        //        sectionHTML.Append("<div class='table-row'>");
        //        sectionHTML.Append("<div class='col-movimento'>Emprestimo</div>");
        //        sectionHTML.Append($"<div class='col-data'>{history.BorrowedDateString}</div>");
        //        sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
        //        sectionHTML.Append($"<div class='col-local'>{history.LocationOrigin}</div>");
        //        sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
        //        sectionHTML.Append($"<div class='col-balconista'>{history.BalconistaBorrow}</div>");
        //        sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
        //        sectionHTML.Append("</div>");

        //        currentHeight += rowHeight;

        //        // Render return row if exists
        //        if (!string.IsNullOrEmpty(history.ReturnedDateString))
        //        {
        //            int returnRowHeight = CalculateRowHeight(history);

        //            if (currentHeight + returnRowHeight > pageMaxHeight)
        //            {
        //                sectionHTML.Append("</div>");
        //                sectionHTML.Append("<div class='page-break'></div>");

        //                sectionHTML.Append("<div style='height: 350px; background: transparent;'></div> ");
        //                //sectionHTML.Append("<div class='product-section'>");
        //                //sectionHTML.Append($"<div class='product-header-continued'>{item.Description} - {item.Code} (continued)</div>");
        //                //currentHeight = 25;
        //            }

        //            sectionHTML.Append("<div class='table-row'>");
        //            sectionHTML.Append("<div class='col-movimento'>Devolucao</div>");
        //            sectionHTML.Append($"<div class='col-data'>{history.ReturnedDateString}</div>");
        //            sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
        //            sectionHTML.Append($"<div class='col-local'>{(string.IsNullOrEmpty(history.LocationReturn) ? history.LocationOrigin : history.LocationReturn)}</div>");
        //            sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
        //            sectionHTML.Append($"<div class='col-balconista'>{(string.IsNullOrEmpty(history.BalconistaReturn) ? history.BalconistaBorrow : history.BalconistaReturn)}</div>");
        //            sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
        //            sectionHTML.Append("</div>");

        //            currentHeight += returnRowHeight;
        //        }
        //    }

        //    return sectionHTML.ToString();
        //}

        private string SplitLargeProductSection(CatalogGroupModel item, int pageMaxHeight, ref int currentPageHeight)
        {
            StringBuilder sectionHTML = new StringBuilder();

            const int PRODUCT_HEADER_HEIGHT = 18;

            sectionHTML.Append("<div class='product-section'>");

            // Product header (always show at top of first page)
            sectionHTML.Append($"<div class='product-header'>{item.Description} - {item.Code}</div>");
            currentPageHeight += PRODUCT_HEADER_HEIGHT;

            foreach (HistoryAlocadoReportModel history in item.ItemAllocation)
            {
                int borrowRowHeight = CalculateRowHeight(history);

                string Validation = string.IsNullOrEmpty(history.IdTransaction) ? "Papel" : (history.CrachaNo ?? string.Empty);

                string FirstRowMovement = history.ItemClass == "Consumable" ? "Delivered" : "Emprestimo"; 

                // Check if we need page break before borrow row
                if (currentPageHeight + borrowRowHeight > pageMaxHeight)
                {
                    /*sectionHTML.Append("</div>");*/ // Close current product-section
                    sectionHTML.Append("<div class='page-break'></div>");

                    sectionHTML.Append("<div style='height: 190px; background: transparent;'></div> ");

                    currentPageHeight = PRODUCT_HEADER_HEIGHT;
                    //sectionHTML.Append("<div class='product-section'>");

                    //// Repeat product header on new page for continuity
                    //sectionHTML.Append($"<div class='product-header-continued'>{item.Description} - {item.Code} (continued)</div>");
                    //currentPageHeight = PRODUCT_HEADER_HEIGHT; // Reset height with continued header
                }

          

                // Render borrow row
                sectionHTML.Append("<div class='table-row'>");
                sectionHTML.Append($"<div class='col-movimento'>{FirstRowMovement}</div>");
                sectionHTML.Append($"<div class='col-data'>{history.BorrowedDateString}</div>");
                sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
                sectionHTML.Append($"<div class='col-local'>{history.LocationOrigin}</div>");
                sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                sectionHTML.Append($"<div class='col-balconista'>{history.BalconistaBorrow}</div>");
                sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                sectionHTML.Append($"<div class='col-validation'>{Validation}</div>");
                sectionHTML.Append("</div>");

                currentPageHeight += borrowRowHeight;

                if (history.ItemClass != "Consumable")
                {
                    // Render return row if exists
                    if (!string.IsNullOrEmpty(history.ReturnedDateString))
                    {
                        int returnRowHeight = CalculateRowHeight(history);

                        if (currentPageHeight + returnRowHeight > pageMaxHeight)
                        {
                            //sectionHTML.Append("</div>");
                            sectionHTML.Append("<div class='page-break'></div>");

                            sectionHTML.Append("<div style='height: 190px; background: transparent;'></div> ");
                            //sectionHTML.Append("<div class='product-section'>");
                            //sectionHTML.Append($"<div class='product-header-continued'>{item.Description} - {item.Code} (continued)</div>");
                            //currentPageHeight = PRODUCT_HEADER_HEIGHT;
                            currentPageHeight = PRODUCT_HEADER_HEIGHT;
                        }

                        sectionHTML.Append("<div class='table-row'>");
                        sectionHTML.Append("<div class='col-movimento'>Devolucao</div>");
                        sectionHTML.Append($"<div class='col-data'>{history.ReturnedDateString}</div>");
                        sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
                        sectionHTML.Append($"<div class='col-local'>{(string.IsNullOrEmpty(history.LocationReturn) ? history.LocationOrigin : history.LocationReturn)}</div>");
                        sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                        sectionHTML.Append($"<div class='col-balconista'>{(string.IsNullOrEmpty(history.BalconistaReturn) ? history.BalconistaBorrow : history.BalconistaReturn)}</div>");
                        sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                        sectionHTML.Append($"<div class='col-validation'>{Validation}</div>");
                        sectionHTML.Append("</div>");

                        currentPageHeight += returnRowHeight;
                    }

                    if (history.LostItems.HasValue && history.LostItems != 0)
                    {
                        int returnRowHeight = CalculateRowHeight(history);

                        if (currentPageHeight + returnRowHeight > pageMaxHeight)
                        {
                            //sectionHTML.Append("</div>");
                            sectionHTML.Append("<div class='page-break'></div>");

                            sectionHTML.Append("<div style='height: 190px; background: transparent;'></div> ");
                            //sectionHTML.Append("<div class='product-section'>");
                            //sectionHTML.Append($"<div class='product-header-continued'>{item.Description} - {item.Code} (continued)</div>");
                            //currentPageHeight = PRODUCT_HEADER_HEIGHT;
                            currentPageHeight = PRODUCT_HEADER_HEIGHT;
                        }

                        sectionHTML.Append("<div class='table-row lost-item'>");
                        sectionHTML.Append("<div class='col-movimento'>Lost</div>");
                        sectionHTML.Append($"<div class='col-data'>{history.ReturnedDateString}</div>");
                        sectionHTML.Append($"<div class='col-qtde'>{history.LostItems}</div>");
                        sectionHTML.Append($"<div class='col-local'></div>");
                        sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                        sectionHTML.Append($"<div class='col-balconista'>{history.LostDateString}</div>");
                        sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                        sectionHTML.Append($"<div class='col-validation'>{Validation}</div>");
                        sectionHTML.Append("</div>");

                        currentPageHeight += returnRowHeight;
                    }
                }
           
            }

            sectionHTML.Append("</div>");
            return sectionHTML.ToString();
        }

        private string RenderProductSection(CatalogGroupModel item)
        {
            StringBuilder sectionHTML = new StringBuilder();

            sectionHTML.Append("<div class='product-section'>");
            sectionHTML.Append($"<div class='product-header'>{item.Description} - {item.Code}</div>");

            foreach (HistoryAlocadoReportModel history in item.ItemAllocation)
            {
               
                string Validation = string.IsNullOrEmpty(history.IdTransaction) ? "Papel" : (history.CrachaNo ?? string.Empty);

                string FirstRowMovement = history.ItemClass == "Consumable" ? "Delivered" : "Emprestimo";

                // Render borrow row
                sectionHTML.Append("<div class='table-row'>");
                sectionHTML.Append($"<div class='col-movimento'>{FirstRowMovement}</div>");
                sectionHTML.Append($"<div class='col-data'>{history.BorrowedDateString}</div>");
                sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
                sectionHTML.Append($"<div class='col-local'>{history.LocationOrigin}</div>");
                sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                sectionHTML.Append($"<div class='col-balconista'>{history.BalconistaBorrow}</div>");
                sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                sectionHTML.Append($"<div class='col-validation'>{Validation}</div>");
                sectionHTML.Append("</div>");


                if (history.ItemClass != "Consumable")
                {
                    // Render return row if exists
                    if (!string.IsNullOrEmpty(history.ReturnedDateString))
                    {
                        sectionHTML.Append("<div class='table-row'>");
                        sectionHTML.Append("<div class='col-movimento'>Devolucao</div>");
                        sectionHTML.Append($"<div class='col-data'>{history.ReturnedDateString}</div>");
                        sectionHTML.Append($"<div class='col-qtde'>{history.Qty}</div>");
                        sectionHTML.Append($"<div class='col-local'>{(string.IsNullOrEmpty(history.LocationReturn) ? history.LocationOrigin : history.LocationReturn)}</div>");
                        sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                        sectionHTML.Append($"<div class='col-balconista'>{(string.IsNullOrEmpty(history.BalconistaReturn) ? history.BalconistaBorrow : history.BalconistaReturn)}</div>");
                        sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                        sectionHTML.Append($"<div class='col-validation'>{Validation}</div>");
                        sectionHTML.Append("</div>");
                    }


                    if (history.LostItems.HasValue && history.LostItems != 0)
                    {
                        sectionHTML.Append("<div class='table-row lost-item'>");
                        sectionHTML.Append("<div class='col-movimento'>Lost</div>");
                        sectionHTML.Append($"<div class='col-data'>{history.LostDateString}</div>");
                        sectionHTML.Append($"<div class='col-qtde'>{history.LostItems}</div>");
                        sectionHTML.Append($"<div class='col-local'></div>");
                        sectionHTML.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                        sectionHTML.Append($"<div class='col-balconista'>{history.BalconistaReturn}</div>");
                        sectionHTML.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                        sectionHTML.Append("</div>");
                    }
                }

                   


            }

            sectionHTML.Append("</div>");
            return sectionHTML.ToString();
        }

        #endregion


        #region SelectPdf

        //select pdf
        //public async Task<IActionResult> GenerateReport(string employeeChapa, int employeeCodColigada, int selectedYear, int? selectedCatalogo, int employeeCodPessoa, int employeeIdTerceiro)
        //{
        //    try
        //    {
        //        //IQueryable<HistoryAlocadoReportModel>? HistoryList = await _historyService.GetEmployeeItemHistory(employeeChapa, employeeCodColigada, selectedYear);

        //        //IEnumerable<CatalogGroupModel> resulttest = HistoryList.GroupBy(i => i.IdCatalogo)
        //        //                                                   .Select(catalog => new CatalogGroupModel
        //        //                                                   {
        //        //                                                       IdCatalogo = catalog.Key,
        //        //                                                       Description = catalog.First()!.Description,
        //        //                                                       Code = catalog.First()!.Code,
        //        //                                                       ItemAllocation = catalog.OrderByDescending(x => x.BorrowedDate).ToList()
        //        //                                                   }).AsEnumerable();

        //        List<CatalogGroupModel>? HistoryList = await _historyService.GetEmployeeItemHistory(employeeChapa, employeeCodColigada, selectedYear);

        //        EmployeeInformationBS employee = employeeCodPessoa != 0 ? await _employeeService.GetSelectedEmployee(employeeCodPessoa) : await _employeeService.GetSelectedThirdParty(employeeIdTerceiro);
        //        ViewBag.EmployeeInformation = employee;

        //        var SeatriumImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "seatrium grande.png");
        //        var SeatriumImageBytes = System.IO.File.ReadAllBytes(SeatriumImagePath);
        //        var Seatrium64Image = Convert.ToBase64String(SeatriumImageBytes);

        //        var NotFoundPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "image-not-available.jpg");
        //        var NotFoundImageBytes = System.IO.File.ReadAllBytes(NotFoundPath);
        //        var NotFound64Image = Convert.ToBase64String(NotFoundImageBytes);


        //        var headerHtml = RenderViewToString("MakeHeader");
        //        var footerHtml = RenderViewToString("AuditFooter");

        //        headerHtml = headerHtml.Replace("/img/seatrium grande.png", $"data:image/bmp;base64,{Seatrium64Image}");
        //        headerHtml = employeeCodPessoa != 0 ? headerHtml.Replace("/img/image-not-available.jpg", employee.ImageString) : headerHtml.Replace("/img/image-not-available.jpg", $"data:image/bmp;base64,{NotFound64Image}");

        //        HtmlToPdf converter = new HtmlToPdf();
        //        converter.Options.DisplayHeader = true;
        //        converter.Header.Height = 125;
        //        converter.Options.PdfPageSize = PdfPageSize.A4;
        //        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

        //        PdfHtmlSection headerSection = new PdfHtmlSection(headerHtml, null);
        //        converter.Header.Add(headerSection);


        //        converter.Options.DisplayFooter = true;
        //        converter.Footer.Height = 40;
        //        PdfHtmlSection footerSection = new PdfHtmlSection(footerHtml, null);
        //        converter.Footer.Add(footerSection);

        //        converter.Options.MaxPageLoadTime = 120;

        //        //MakePDFBody(HistoryList)
        //        PdfDocument doc = converter.ConvertHtmlString(MakePDFBody(HistoryList));
        //        //doc.Margins = 10;

        //        var stream = new MemoryStream();
        //        doc.Save(stream);
        //        doc.Close();

        //        return File(stream.ToArray(), "application/pdf", "test.pdf");

        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "Argument Client Error.");
        //        ViewBag.Error = $"{ex.Message}";
        //        return View(nameof(Index));
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "InvalidOperationException Client Error.");
        //        ViewBag.Error = $"{ex.Message}";
        //        return View(nameof(Index));
        //    }
        //    catch (ProcessErrorException ex)
        //    {
        //        _logger.LogError(ex, "Processing Error.");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
        //        return View(nameof(Index));
        //    }
        //    catch (System.Net.Sockets.SocketException ex)
        //    {
        //        _logger.LogError(ex, "Server Unavailable.");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
        //        return View(nameof(Index));
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LogError(ex, "Database timeout occurred.");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
        //        return View(nameof(Index));
        //    }
        //    catch (TimeoutException ex)
        //    {
        //        _logger.LogError(ex, "Operation timed out.");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
        //        return View(nameof(Index));
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        _logger.LogError(ex, "File Not Found.");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - File Not Found Error";
        //        return View(nameof(Index));
        //    }
        //    catch (UserContextException ex)
        //    {
        //        await HttpContext.SignOutAsync();
        //        return RedirectToAction(actionName: nameof(HomeController.Login), controllerName: nameof(HomeController).Replace("Controller", ""),
        //            new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred");
        //        ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred";
        //        return View(nameof(Index));
        //    }
        //}
        private string MakePDFBody(IEnumerable<CatalogGroupModel> HistoryList)
        {
            StringBuilder sb = new StringBuilder();

            //sb.Append("<style>");
            //sb.Append(GenerateCSSBody());
            //sb.Append("</style>");

            //sb.Append("<div class='content'>");

            //sb.Append("<div class='product-section'>");

            ////catalog description
            //sb.Append("<div class='product-header'> AVENTAL DE COURO TIPO RASPA </div>");

            ////history information
            //sb.Append("<div class='table-row'>");
            //sb.Append("<div class='col-movimento'>Empréstimo</div>");
            //sb.Append("<div class='col-data'>10/11/2023 14:59</div>");
            //sb.Append("<div class='col-qtde'>1</div>");
            //sb.Append("<div class='col-local'>EPI-002</div>");
            //sb.Append("<div class='col-ca'>6316</div>");
            //sb.Append("<div class='col-balconista'>EVALDO AMARAL SINORA SANTIAGO</div>");
            //sb.Append("<div class='col-id'>AÁGDF38-845F-43E7-80FC-41A890F6FFE2</div>");
            //sb.Append("</div>");


            //sb.Append("</div>");

            //sb.Append("</div>");


            sb.Append("<style>");
            sb.Append(GenerateCSSBody());
            sb.Append("</style>");

            sb.Append("<div class='content'>");
            sb.Append("<div class='product-section'>");

            foreach (CatalogGroupModel item in HistoryList)
            {
                //catalog description
                sb.Append($"<div class='product-header'>{item.Description} - {item.Code} </div>");

                foreach (HistoryAlocadoReportModel history in item.ItemAllocation)
                {
                    //history information
                    sb.Append("<div class='table-row'>");
                    sb.Append("<div class='col-movimento'>Emprestimo</div>");
                    sb.Append($"<div class='col-data'>{history.BorrowedDateString}</div>");
                    sb.Append($"<div class='col-qtde'>{history.Qty}</div>");
                    sb.Append($"<div class='col-local'>{history.LocationOrigin}</div>");
                    sb.Append($"<div class='col-ca'>{history.ControlCA}</div>");
                    sb.Append($"<div class='col-balconista'>{history.BalconistaBorrow}</div>");
                    sb.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                    sb.Append("</div>");

                    //history return information
                    if (!string.IsNullOrEmpty(history.ReturnedDateString))
                    {
                        sb.Append("<div class='table-row'>");
                        sb.Append("<div class='col-movimento'>Devolucao</div>");
                        sb.Append($"<div class='col-data'>{history.ReturnedDateString}</div>");
                        sb.Append($"<div class='col-qtde'>{history.Qty}</div>");
                        //if (string.IsNullOrEmpty(history.LocationReturn))
                        //{
                        //    sb.Append($"<div class='col-local'>{history.LocationOrigin.Trim()}</div>");
                        //}
                        //else
                        //{
                        //    sb.Append($"<div class='col-local'>{history.LocationReturn}</div>");
                        //}
                        sb.Append($"<div class='col-local'>{(string.IsNullOrEmpty(history.LocationReturn) ? history.LocationOrigin : history.LocationReturn)}</div>");

                        sb.Append($"<div class='col-ca'>{history.ControlCA}</div>");

                        //if (string.IsNullOrEmpty(history.BalconistaReturn))
                        //{
                        //    sb.Append($"<div class='col-local'>{history.BalconistaBorrow.Trim()}</div>");
                        //}
                        //else
                        //{
                        //    sb.Append($"<div class='col-local'>{history.LocationReturn}</div>");
                        //}
                        sb.Append($"<div class='col-balconista'>{(string.IsNullOrEmpty(history.BalconistaReturn) ? history.BalconistaBorrow : history.BalconistaReturn)}</div>");

                        sb.Append($"<div class='col-id'>{history.IdTransaction}</div>");
                        sb.Append("</div>");
                    }
                }

            }




            //end div for product-section
            sb.Append("</div>");
            //end div for content
            sb.Append("</div>");

            return sb.ToString();
        }

        private string GenerateCSSBody()
        {
            string CSSBody =
            @"
                body {
                    margin: 5mm;
                }

                   .content {
                        font-family: Arial, sans-serif;
                        font-size: 9px;
                        width: 100%;
                    }

        .product-section {
            margin-bottom: 5px;
            page-break-inside: avoid;
            break-inside: avoid;
        }

      .product-header {
            font-weight: bold;
            border: 1px ;
            border-bottom: 1px dashed #000;
            margin-bottom: 4px;
            page-break-after: avoid;
            font-size: 10px;
            width: 100%;
            padding: 7px 0px 0px 0px;
        }

     .table-row {
            display: flex;
            width: 100%;
            page-break-inside: avoid;
            break-inside: avoid;
            min-height: 18px;
        }

    .col-movimento {
            width: 10%;
            min-width: 10%;
            padding: 5px 0px;
        }

        .col-data {
            width: 10%;
            min-width: 10%;
           padding: 5px 0px;
        }

        .col-qtde {
            width: 5%;
            min-width: 5%;
           padding: 5px 0px;
            text-align: center;
        }

        .col-local {
            width: 10%;
            min-width: 10%;
              padding: 5px 0px;
            text-align: center;
        }

        .col-ca {
            width: 5%;
            min-width: 5%;
               padding: 5px 0px;
            text-align: center;
        }

        .col-balconista {
            width: 30%;
            min-width: 25%;
              padding: 5px 0px;
        }

        .col-id {
            width: 30%;
            min-width: 25%;
              padding: 5px 0px;
        }


            ";

            return CSSBody;
        }

        public async Task<IActionResult> MakeHeader()
        {
            return View();
        }

        public async Task<IActionResult> AuditFooter()
        {
            return View();
        }

        private string RenderViewToString(string viewPath)
        {
            var actionContext = new ActionContext
            {
                HttpContext = HttpContext,
                RouteData = RouteData,
                ActionDescriptor = ControllerContext.ActionDescriptor
            };

            var viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: false);

            if (!viewResult.Success)
            {
                var message = $"View not found: {viewPath}. Check that the view file exists and the path is correct.";
                throw new FileNotFoundException(message, viewPath);
            }

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                //viewContext.ViewData["Layout"] = cssPath;

                viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        #endregion

        [Authorize(Roles = "Demo")]
        private List<CatalogGroupModel> GroupFinalResult(List<HistoryAlocadoReportModel> result)
        {
            List<CatalogGroupModel> FinalResult = result.GroupBy(i => i.IdCatalogo)
                                                  .Select(catalog => new CatalogGroupModel
                                                  {
                                                      IdCatalogo = catalog.Key,
                                                      ItemClass = catalog.First().ItemClass,
                                                      Description = catalog.FirstOrDefault()!.Description,
                                                      Code = catalog.FirstOrDefault()!.Code,
                                                      ItemAllocation = catalog.OrderByDescending(x => x.BorrowedDate).ToList()
                                                  }).ToList();



            return FinalResult;
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> RetrieveEmployeeInformation(string givenInformation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(givenInformation)) throw new ArgumentException("Please provide Chapa/Nome");

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to RetrieveEmployeeInformation for GivenInformation:{givenInformation}", user.Id, givenInformation);

                List<EmployeeInformationBS>? retrievedResults = new List<EmployeeInformationBS>();

                List<EmployeeInformationBS>? employees = await _employeeService.SearchEmployees(givenInformation);
                if (employees != null && employees.Count > 0) retrievedResults.AddRange(employees);

                List<EmployeeInformationBS>? thirdParties = await _employeeService.SearchThirdParty(givenInformation);
                if (thirdParties != null && thirdParties.Count > 0) retrievedResults.AddRange(thirdParties);

                if (retrievedResults.Count == 0) throw new InvalidOperationException("No result found.");

                if (retrievedResults.Count == 1)
                {
                    if (retrievedResults[0].IdTerceiro == 0)
                    {
                        retrievedResults[0].ImageString = await _employeeService.GetEmployeeImage(retrievedResults[0].CodPessoa!.Value);
                    }
                }

                return Ok(retrievedResults);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> SelectedEmployee(int codPessoa)
        {
            try
            {
                if (codPessoa <= 0) throw new ArgumentException("Invalid CodPessoa");

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to SelectedEmployee for CodPessoa:{codPessoa}", user.Id, codPessoa);

                EmployeeInformationBS employee = await _employeeService.GetSelectedEmployee(codPessoa);

                return Ok(employee);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> SelectedThirdParty(int idTerceiro)
        {
            try
            {
                if (idTerceiro <= 0) throw new ArgumentException("Invalid idTerceiro");

                UserClaimModel user = _userContext.GetUserClaimData();

                _logger.LogInformation("User:{UserId} - Attempt to SelectedThirdParty for IdTerceiro:{idTerceiro}", user.Id, idTerceiro);

                EmployeeInformationBS thirdParty = await _employeeService.GetSelectedThirdParty(idTerceiro);

                return Ok(thirdParty);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }
        }




        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> ConsultTransaction()
        {
            return View();
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> SearchAuditLog(string Filtro)
        {

            try
            {
                AuditLogModel logResult = await _auditService.GetAuditLogs(Filtro);

                ViewBag.LogResult = logResult;

                return View(nameof(ConsultTransaction));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                ViewBag.Error = $"{ex.Message}";
                return View();
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}";
                return View();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Server Unavailable.";
                return View();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Database timeout occurred";
                return View();
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out";
                return View();
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction(actionName: nameof(HomeController.Login), controllerName: nameof(HomeController).Replace("Controller", ""),
                    new { message = $"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}" }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                ViewBag.Error = $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred";
                return View();
            }

        }



        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> TermsConditionPage()
        {
            return View();
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> GetTermsData(int CodPessoa)
        {
            try
            {

                //List<EmployeeInformationBS>? retrievedResults = new List<EmployeeInformationBS>();

                //List<EmployeeInformationBS>? employees = await _employeeService.SearchEmployees(givenInformation);
                //if (employees != null && employees.Count > 0) retrievedResults.AddRange(employees);

                TermsControlResultModel termsControl = await _employeeService.GetTermsConrolResult(CodPessoa);

                return Ok(termsControl);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }   
        }

        [Authorize(Roles = "Demo")]
        public async Task<IActionResult> uploadTermPDF(int idTerms, IFormFile pdfFile)
        {
            try
            {

                if (idTerms <= 0) throw new ArgumentException("IdTerms is 0");

                if (pdfFile == null || pdfFile.Length == 0) throw new ArgumentException("No file uploaded.");

                if (pdfFile.ContentType != "application/pdf") throw new ArgumentException("File must be a PDF.");

                if (pdfFile.Length > 10 * 1024 * 1024) throw new ArgumentException("File size exceeds 10MB limit.");

                using (var memoryStream = new MemoryStream())
                {
                    await pdfFile.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    await _employeeService.UploadTermsPDF(idTerms, fileBytes);

                    return Ok("Success");
                }
      
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument Client Error.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                return NotFound(ex.Message);
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogError(ex, "Processing Error.");
                return BadRequest($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"{_correlationIdService.GetCurrentCorrelationId()} - Server unavailable.");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database operation timeout occurred.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Database operation timed out");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                return StatusCode(StatusCodes.Status408RequestTimeout, $"{_correlationIdService.GetCurrentCorrelationId()} - Operation timed out.");
            }
            catch (UserContextException ex)
            {
                await HttpContext.SignOutAsync();
                return Unauthorized($"{_correlationIdService.GetCurrentCorrelationId()} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, $"{_correlationIdService.GetCurrentCorrelationId()} - An unexpected error occurred");
            }        
        }



    }
}
