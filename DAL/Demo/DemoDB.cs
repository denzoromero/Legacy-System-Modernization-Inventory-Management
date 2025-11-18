using Microsoft.EntityFrameworkCore;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Entities;

using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using FerramentariaTest.EntitySeek;
using FerramentariaTest.EntitiesRM;

namespace FerramentariaTest.DAL.Demo
{
    public class DemoDB : DbContext
    {
        public DemoDB(DbContextOptions<DemoDB> options) : base(options)
        {

        }

        public DbSet<UsuarioBS> Usuario { get; set; }
        public DbSet<Ferramentaria> Ferramentaria { get; set; }
        public DbSet<Catalogo> Catalogo { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Produto> Produto { get; set; }
        public DbSet<Obra> Obra { get; set; }

        public DbSet<LeaderData> LeaderData { get; set; }
        public DbSet<ReservationControl> ReservationControl { get; set; }
        public DbSet<Reservations> Reservations { get; set; }
        public DbSet<LeaderMemberRel> LeaderMemberRel { get; set; }
        public DbSet<ProductReservation> ProductReservation { get; set; }
        public DbSet<Funcionario> Funcionario { get; set; }
        public DbSet<HistoricoAlocacao_2025> HistoricoAlocacao_2025 { get; set; }
        public DbSet<fnRetornaColaboradorCracha> fnRetornaColaboradorCracha { get; set; }

        public DbSet<FuncionarioSeek> FuncionarioSeek { get; set; }
        public DbSet<Secao> Secao { get; set; }
        public DbSet<Funcao> Funcao { get; set; }

        public DbSet<GIMAGEM> GIMAGEM { get; set; }

        public DbSet<PPESSOA> PPESSOA { get; set; }
        public DbSet<TermsControl> TermsControl { get; set; }

        public DbSet<VW_Usuario_New> VW_Usuario_New { get; set; }

        public DbSet<AuditLogsBalconista> AuditLogsBalconista { get; set; }
        public DbSet<ControleCA> ControleCA { get; set; }
        public DbSet<ProdutoAlocado> ProdutoAlocado { get; set; }
        public DbSet<ProdutoExtraviado> ProdutoExtraviado { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Funcionario>().HasKey(f => new { f.Chapa });

            builder.Entity<fnRetornaColaboradorCracha>().HasKey(f => new { f.MATRICULA });

            base.OnModelCreating(builder);
        }


    }
}
