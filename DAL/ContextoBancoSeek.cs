using FerramentariaTest.EntitiesRM;
using FerramentariaTest.EntitySeek;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.DAL
{
    public class ContextoBancoSeek : DbContext
    {
        public DbSet<FuncionarioSeek> Funcionario { get; set; }
        public DbSet<Secao> Secao { get; set; }
        public DbSet<Funcao> Funcao { get; set; }

        public ContextoBancoSeek(DbContextOptions<ContextoBancoSeek> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {


        }
    }
}
