using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class LogArquivo
    {
        [Key]
        public int? Id { get; set; }
        public int? IdArquivo { get; set; }
        public int? IdUsuario { get; set; }
        public int? Tipo { get; set; }
        public DateTime? DataRegistro { get; set; }

    }
}
