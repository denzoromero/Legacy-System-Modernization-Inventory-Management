
using FerramentariaTest.EntitiesSeekEmployees;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.DAL
{
    public class ContextoBancoSeekEmployee : DbContext
    {

        public DbSet<AliasName> AliasName { get; set; }

        public DbSet<Funcao> Funcao { get; set; }

        public DbSet<Funcionario> Funcionario { get; set; }

        public DbSet<Secao> Secao { get; set; }

        public ContextoBancoSeekEmployee(DbContextOptions<ContextoBancoSeekEmployee> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AliasName>()
                .HasNoKey();

            builder.Entity<Funcao>()
            .Property(x => x.DataRegistro)
            .HasColumnName("DataRegistro")
            .HasColumnType("datetime");

            builder.Entity<Funcionario>()
           .Property(x => x.DataRegistro)
           .HasColumnName("DataRegistro")
           .HasColumnType("datetime");

            builder.Entity<Secao>()
            .Property(x => x.DataRegistro)
            .HasColumnName("DataRegistro")
            .HasColumnType("datetime");

        }
    }
}
