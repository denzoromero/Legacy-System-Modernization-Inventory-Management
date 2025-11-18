using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    [Table("LogEntradaSaida")]
    public class LogEntradaSaidaInsert
    {
  
        public int? IdProduto { get; set; }
        public int? IdFerramentaria { get; set; }
        public int? Quantidade { get; set; }
        public string? Rfm { get; set; }
        public string? Observacao { get; set; }
        public int? IdUsuario { get; set; }
        [Key]
        public DateTime? DataRegistro { get; set; }

    }
}
