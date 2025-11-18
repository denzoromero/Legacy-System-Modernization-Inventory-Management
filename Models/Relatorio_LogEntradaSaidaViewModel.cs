
namespace FerramentariaTest.Models
{
    public class Relatorio_LogEntradaSaidaViewModel
    {
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public string? Arquivo { get; set; }
        public int? ArquivoStatus { get; set; }
        public string? ArquivoFilename { get; set; }
        public int? Processar { get; set; }
        public string? Query { get; set; }
        public DateTime? ProcessoDataInicio { get; set; }
        public DateTime? ProcessoDataConclusao { get; set; }
        public string? timeDifference { get; set; }
        public int? Ativo { get; set; }
        public DateTime? DataRegistro { get; set; }
        public string? Nome { get; set; }
    }
}
