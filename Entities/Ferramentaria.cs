using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class Ferramentaria
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int Ativo { get; set; }

    }
}
