using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class LogRelatorio
    {
        [Key]
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public int? Relatorio { get; set; }
        public string? Arquivo { get; set; }
        public int? Processar { get; set; }
        public string? Query { get; set; }
        public int? Ativo { get; set; }
        public DateTime? DataRegistro { get; set; }
    }
}
