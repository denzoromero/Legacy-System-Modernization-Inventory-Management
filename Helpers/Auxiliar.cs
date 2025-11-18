using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.Entities;
using System.DirectoryServices;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using X.PagedList;
using System.Linq;
using System.Drawing;
using SelectPdf;

namespace FerramentariaTest.Helpers
{
    public class Auxiliar
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;

        public Auxiliar(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpContext;
            _configuration = configuration;
        }

        public string ValidaUsuarioAD(string? samAccountName, string? senha)
        {
            try
            {
                var search = new DirectorySearcher(new DirectoryEntry
                {                   
                    //Path = "LDAP://10.188.60.31",
                    //Path = "LDAP://10.88.80.209",
                    Path = _configuration["LoginPath:Path"],
                    AuthenticationType = AuthenticationTypes.Secure,
                    Username = samAccountName,
                    Password = senha,

                })
                {
                    Filter = "(SAMAccountName=" + samAccountName + ")"
                };
                search.PropertiesToLoad.Add("extensionName");

                var results = search.FindOne();
                if (results != null)
                {
                    //return (string)(results.Properties["extensionName"][0]);
                    return samAccountName;
                }

                return null;
            }
            catch (Exception)
            {
                return "Erro em conectar com o AD!";
            }
        }

        public VW_Usuario_NewViewModel retornaUsuario()
        {
            VW_Usuario_NewViewModel usuario = new VW_Usuario_NewViewModel();
            usuario.SAMAccountName = httpContextAccessor.HttpContext.Session.GetString(FerramentariaTest.Helpers.Sessao.SAMACCOUNT);
            usuario.Id = httpContextAccessor.HttpContext.Session.GetInt32(FerramentariaTest.Helpers.Sessao.ID);
            usuario.Nome = httpContextAccessor.HttpContext.Session.GetString(FerramentariaTest.Helpers.Sessao.NOME);
            usuario.Chapa = httpContextAccessor.HttpContext.Session.GetString(FerramentariaTest.Helpers.Sessao.CHAPA);
            usuario.NomeFerramentaria = httpContextAccessor.HttpContext.Session.GetString(FerramentariaTest.Helpers.Sessao.NomeFerramentaria);
            usuario.IdFerramentaria = httpContextAccessor.HttpContext.Session.GetInt32(FerramentariaTest.Helpers.Sessao.IdFerramentaria);
            return usuario;
        }

        public VW_Usuario_NewViewModel VerificaPermissao(VW_Usuario_NewViewModel usu)
        {
            usu.Permissao = null;
            try
            {
                //string SAMAccountName = httpContextAccessor.HttpContext.Session.GetString(LGPDQuestRisk.Helpers.Sessao.SAMACCOUNT);
                //int id = (int)httpContextAccessor.HttpContext.Session.GetInt32(LGPDQuestRisk.Helpers.Sessao.ID);

                var testeUsuario = _contextBS.VW_Usuario_New.Where(u => u.Id == usu.Id && u.Ativo == 1);
                if (testeUsuario.Count() == 1)
                {
                    //VW_Usuario_New usuario = testeUsuario.FirstOrDefault<VW_Usuario_New>();
                    //VW_Usuario_New usuario = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == id && u.Ativo == 1);
                    //var testeAcesso = _contextBS.Acesso.Where(a => a.IdModulo == 65 && a.Pagina.Equals(usu.Pagina));
                    var testeAcesso = _contextBS.Acesso.Where(a => a.IdModulo == 6 && a.Pagina.Equals(usu.Pagina));
                    if (testeAcesso.Count() == 1)
                    {
                        Acesso acesso = testeAcesso.FirstOrDefault<Acesso>();
                        var testePermissao = _contextBS.Permissao.Where(p => p.SAMAccountName.Equals(usu.SAMAccountName) && p.IdUsuario == usu.Id && p.IdAcesso == acesso.Id);
                        if (testePermissao.Count() == 1)
                        {
                            Permissao permissao = null;
                            permissao = testePermissao.FirstOrDefault<Permissao>();

                            PermissaoViewModel permissaoViewModel = new PermissaoViewModel
                            {
                                IdAcesso = permissao.IdAcesso,
                                SAMAccountName = permissao.SAMAccountName,
                                IdUsuario = permissao.IdUsuario,
                                Visualizar = permissao.Visualizar,
                                Editar = permissao.Editar,
                                Inserir = permissao.Inserir,
                                Excluir = permissao.Excluir,
                                DataRegistro = permissao.DataRegistro
                            };
                            usu.Permissao = permissaoViewModel;
                            var user = testeUsuario.FirstOrDefault();
                            usu.Email = user.Email;
                            return usu;
                        }
                        else
                        {
                            return usu;
                        }
                    }
                    else
                    {
                        return usu;
                    }
                }
                else
                {
                    return usu;
                }

            }
            catch (Exception ex)
            {
                usu.Erro = ex.Message;
                return usu;
            }
        }

        //public LoggedUserData checklogin(Usuario user)
        //{


        //}



        public string GravaLogRetornoErro(Log log)
        {
            string retorno;
            log.Id = 0;
            log.LogWho = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID).ToString();
            log.LogHow = "ERRO";
            log.LogWhen = DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo);
            _context.Add(log);
            _context.SaveChanges();

            retorno = log.Id.ToString();

            return retorno;

        }

        public void GravaLogErro(Log log)
        {
            log.Id = 0;
            log.LogWho = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID).ToString();
            log.LogHow = "ERRO";
            log.LogWhen = DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo);
            _context.Add(log);
            _context.SaveChanges();
        }

        public void GravaLogAlerta(Log log)
        {
            log.Id = 0;
            log.LogWho = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID).ToString();
            log.LogHow = "ALERTA";
            log.LogWhen = DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo);
            _context.Add(log);
            _context.SaveChanges();
        }

        public void GravaLogSucesso(Log log)
        {
            log.Id = 0;
            log.LogWho = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID).ToString();
            log.LogHow = "SUCESSO";
            log.LogWhen = DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo);
            _context.Add(log);
            _context.SaveChanges();
        }

        public void GravaLogDuplicate(Log log)
        {
            log.Id = 0;
            log.LogWho = httpContextAccessor.HttpContext.Session.GetInt32(Sessao.ID).ToString();
            log.LogHow = "DUPLICATE";
            log.LogWhen = DateTime.Now.ToString("u", DateTimeFormatInfo.InvariantInfo);
            _context.Add(log);
            _context.SaveChanges();
        }

        public byte[] GeneratePdfHtml(string html)
        {
            // read parameters from the webpage
            string htmlString = html;
            string baseUrl = "";

            string pdf_page_size = "A4";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize),
            pdf_page_size, true);

            string pdf_orientation = "Portrait";
            PdfPageOrientation pdfOrientation =
                (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation),
                pdf_orientation, true);

            int webPageWidth = 1024;
            try
            {
                webPageWidth = Convert.ToInt32(0);
            }
            catch { }

            int webPageHeight = 0;
            try
            {
                webPageHeight = Convert.ToInt32(0);
            }
            catch { }

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            //converter.Options.DisplayFooter = true;
            //converter.Footer.DisplayOnFirstPage = true;
            //converter.Footer.DisplayOnEvenPages = true;
            //converter.Footer.DisplayOnOddPages = true;

            //converter.Footer.Height = 28.35f;

            // Add page numbering to footer
            //PdfTextSection text = new PdfTextSection(0, 10, "Page: {page_number} of {total_pages}", new System.Drawing.Font("Arial", 8));
            //text.HorizontalAlign = PdfTextHorizontalAlign.Center;
            //converter.Footer.Add(text);

            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;

            converter.Options.EmbedFonts = true;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(htmlString, baseUrl);


            return doc.Save();
            // save pdf document
            //doc.Save(Response, false, "Sample.pdf");

            // close pdf document
            //doc.Close();
        }

    }

    public static class StringExtensions
    {
        public static string[] SplitAndWrap(this string input, Font font, Graphics graphics, float maxWidth, out int charsFitted, out int linesFilled)
        {
            var result = new List<string>();

            charsFitted = 0;
            linesFilled = 0;

            int startIndex = 0;

            while (startIndex < input.Length)
            {
                int length = input.Length - startIndex;

                if (graphics.MeasureString(input.Substring(startIndex, length), font).Width > maxWidth)
                {
                    // Find the index to break the line
                    length = FindBreakIndex(input.Substring(startIndex), font, graphics, maxWidth);
                }

                var segment = input.Substring(startIndex, length);

                charsFitted += length;
                linesFilled++;

                startIndex += length;

                result.Add(segment);
            }

            return result.ToArray();
        }

        private static int FindBreakIndex(string text, Font font, Graphics graphics, float maxWidth)
        {
            int index = text.Length;

            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (graphics.MeasureString(text.Substring(0, i), font).Width <= maxWidth)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
    }

    public static class PagedListExtensions
    {
        public static PagedListWrapper<T> ToPagedListWrapper<T>(this IPagedList<T> pagedList)
        {
            return new PagedListWrapper<T>
            {
                Items = new List<T>(pagedList),
                PageNumber = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                TotalItemCount = pagedList.TotalItemCount
            };
        }

        public static IPagedList<T> ToPagedList<T>(this PagedListWrapper<T> wrapper)
        {
            return new StaticPagedList<T>(wrapper.Items, wrapper.PageNumber, wrapper.PageSize, wrapper.TotalItemCount);
        }
    }

}
