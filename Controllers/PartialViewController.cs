using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FerramentariaTest.Controllers
{
    public class PartialViewController : Controller
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thEmprestimo.aspx";

        public PartialViewController(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
        }

        // GET: PartialViewController
        public ActionResult FerramentariaPartialView(string? PageOrigin)
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
                usuario.Pagina1 = "thEmprestimo.aspx";
                usuario.Acesso = log.LogWhat;
                usuario = auxiliar.VerificaPermissao(usuario);

                if (usuario.Permissao == null)
                {
                    usuario.Retorno = "Usuário sem permissão na página!";
                    log.LogWhy = usuario.Retorno;
                    auxiliar.GravaLogAlerta(log);
                    return RedirectToAction("Login", "Home", usuario);
                }
                else
                {
                    if (usuario.Permissao.Visualizar != 1)
                    {
                        usuario.Retorno = "Usuário sem permissão de visualizar a página de Emprestimo!";
                        log.LogWhy = usuario.Retorno;
                        auxiliar.GravaLogAlerta(log);
                        return RedirectToAction("Login", "Home", usuario);
                    }
                }
                #endregion

                var ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                          where ferramentaria.Ativo == 1 &&
                                                                          !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                          _context.FerramentariaVsLiberador.Any(l => l.IdLogin == usuario.Id && l.IdFerramentaria == ferramentaria.Id)
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

        // GET: PartialViewController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PartialViewController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PartialViewController/Create
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

        // GET: PartialViewController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PartialViewController/Edit/5
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

        // GET: PartialViewController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PartialViewController/Delete/5
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

    public class ValuePartialViewComponent : ViewComponent
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private static string pagina = "thEmprestimo.aspx";

        public ValuePartialViewComponent(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            httpContextAccessor = httpCA;
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {
            string? FerramentariaNome = httpContextAccessor.HttpContext.Session.GetString(Sessao.FerramentariaNome);

            ViewBag.FerramentariaNome = FerramentariaNome;

            return View("/Views/Shared/_ValuePartialView.cshtml");
        }
    }
}
