using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class ArquivoVsHistorico
    {
        [Key]
        public int? IdArquivo { get; set; }

        [Key]
        public int? IdHistoricoAlocacao { get; set; }

        public int? Ano { get; set; }

        public DateTime? DataRegistro { get; set; }
    }
}
