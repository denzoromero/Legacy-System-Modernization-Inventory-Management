using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitySeek
{
    public class Funcao
    {
        [Key]
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
    }
}
