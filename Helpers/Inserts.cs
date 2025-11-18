using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Models;

namespace FerramentariaTest.Helpers
{
    public class Inserts
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;

        public Inserts(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpContext;
            _configuration = configuration;
        }


    }
}
