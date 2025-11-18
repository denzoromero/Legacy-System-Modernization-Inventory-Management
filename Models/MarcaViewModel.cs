using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class MarcaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome is Required")]
        //[Display(Name = "Nome")]
        public string? Nome { get; set; }
        public DateTime DataRegistro { get; set; }
        public int Ativo { get; set; }
    }
}
