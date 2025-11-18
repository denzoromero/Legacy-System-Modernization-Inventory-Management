using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Relatorio_LogEntradaSaida
    {
        [Key]
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public string? Arquivo { get; set; }
        public int? Processar { get; set; }
        public string? Query { get; set; }
        public DateTime? ProcessoDataInicio { get; set; }
        public DateTime? ProcessoDataConclusao { get; set; }
        public int? Ativo { get; set; }
        public DateTime? DataRegistro { get; set; }
    }
}
