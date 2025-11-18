using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitiesSeekEmployees
{
    public class Secao
    {
        [Key]
        public int? Id { get; set; }

        [MaxLength(150)]
        public string? Nome { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }
    }
}
