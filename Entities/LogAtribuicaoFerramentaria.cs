using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class LogAtribuicaoFerramentaria
    {
        [Key]
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdFerramentaria { get; set; }
        public int? IdUsuarioResponsavel { get; set; }
        public int? Acao { get; set; }
        public DateTime? DataRegistro { get; set; }
    }
}
