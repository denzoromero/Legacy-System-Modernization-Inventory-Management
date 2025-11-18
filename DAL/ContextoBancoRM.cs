using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesRM;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.DAL
{
    public class ContextoBancoRM : DbContext
    {
        public DbSet<GIMAGEM> GIMAGEM { get; set; }

        public DbSet<PPESSOA> PPESSOA { get; set; }
        public DbSet<PFUNC> PFUNC { get; set; }

        public ContextoBancoRM(DbContextOptions<ContextoBancoRM> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           

        }

    }
}
