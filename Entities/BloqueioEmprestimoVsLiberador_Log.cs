using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class BloqueioEmprestimoVsLiberador_Log
    {
        public DateTime? DataTransacao { get; set; }
        public int? Autorizador { get; set; }
        public string? Tabela { get; set; }

        [Key]
        public int? IdRegistro { get; set; }
    }
}
