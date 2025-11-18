using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitiesSeekEmployees
{
    public class Funcao
    {
        [Key]
        public int? Id { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

    }
}
