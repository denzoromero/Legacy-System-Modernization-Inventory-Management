
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.DAL
{
    public class ContextoBancoBS : DbContext
    {
        public DbSet<VW_Usuario_New> VW_Usuario_New { get; set; }
        public DbSet<VW_Funcionario_Registro_Atual> VW_Funcionario_Registro_Atual { get; set; }
        public DbSet<Acesso> Acesso { get; set; }
        public DbSet<Permissao> Permissao { get; set; }
        public DbSet<UsuarioBS> Usuario { get; set; }
        public DbSet<Funcionario> Funcionario { get; set; }
        public DbSet<VW_Usuario> VW_Usuario { get; set; }
        public DbSet<fnRetornaColaboradorCracha> fnRetornaColaboradorCracha { get; set; }


        public ContextoBancoBS(DbContextOptions<ContextoBancoBS> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<fnRetornaColaboradorCracha>()
        .HasNoKey();

            builder.Entity<VW_Usuario_New>()
                .Property(x => x.DataRegistro)
                .HasColumnName("DataRegistro")
                .HasColumnType("datetime");
            builder.Entity<VW_Usuario_New>()
                .Property(x => x.FimProgFerias1)
                .HasColumnName("FimProgFerias1")
                .HasColumnType("datetime");
            builder.Entity<Acesso>()
                .Property(x => x.DataRegistro)
                .HasColumnName("DataRegistro")
                .HasColumnType("datetime");
            builder.Entity<Permissao>()
                .Property(x => x.DataRegistro)
                .HasColumnName("DataRegistro")
                .HasColumnType("datetime");
            builder.Entity<Acesso>()
                .ToTable("Acesso")
                .Property(p => p.Acesso_pt)
                .HasColumnName("Acesso");

            builder.Entity<UsuarioBS>().HasNoKey();

            builder.Entity<Funcionario>()
                .HasNoKey();

            builder.Entity<Funcionario>()
           .Property(x => x.DataRegistro)
           .HasColumnName("DataRegistro")
           .HasColumnType("datetime");


            builder.Entity<Funcionario>()
           .Property(x => x.DataMudanca)
           .HasColumnName("DataMudanca")
           .HasColumnType("datetime");

            builder.Entity<Funcionario>()
            .Property(x => x.DataAdmissao)
            .HasColumnName("DataAdmissao")
            .HasColumnType("datetime");

            builder.Entity<Funcionario>()
       .Property(x => x.DataDemissao)
       .HasColumnName("DataDemissao")
       .HasColumnType("datetime");


            builder.Entity<Funcionario>()
       .Property(x => x.FimProgFerias1)
       .HasColumnName("FimProgFerias1")
       .HasColumnType("datetime");

            builder.Entity<VW_Usuario>()
             .Property(x => x.DataRegistro)
             .HasColumnName("DataRegistro")
             .HasColumnType("datetime");


            builder.Entity<VW_Usuario>()
             .Property(x => x.FimProgFerias1)
             .HasColumnName("FimProgFerias1")
             .HasColumnType("datetime");

            builder.Entity<VW_Funcionario_Registro_Atual>()
                .HasNoKey();

        }

        public IQueryable<fnRetornaColaboradorCracha> GetColaboradorCracha(string employeeId)
        {
            return fnRetornaColaboradorCracha.FromSqlInterpolated($"SELECT * FROM dbo.fnRetornaColaboradorCracha({employeeId})");
        }

    }
}
