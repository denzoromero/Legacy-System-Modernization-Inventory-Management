
namespace FerramentariaTest.Models
{
    public class RelatorioViewModel
    {
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public string? Nome { get; set; }
        public string? RelatorioData { get; set; }
        public string? Arquivo { get; set; }
        public int? ArquivoStatus { get; set; }
        public string? ArquivoFilename { get; set; }
        public int? Processar { get; set; }
        public string? Query { get; set; }
        public DateTime? ProcessoDataInicio { get; set; }
        public DateTime? ProcessoDataConclusao { get; set; }
        public string? timeDifference { get; set; }
        public string? SAMAccountName { get; set; }
        public int? Ativo { get; set; }
        public DateTime? DataRegistro { get; set; }
    }

    public class RelatorioSearch
    {
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Produto { get; set; }
        public string? CA { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Observacao { get; set; }
        public string? SetorOrigem { get; set; }
        public string? ChapaSolicitante { get; set; }
        public string? NomeSolicitante { get; set; }
        public string? ChapaLiberador { get; set; }
        public string? NomeLiberador { get; set; }
        public string? Balconista { get; set; }
        public string? Obra { get; set; }
        public DateTime? DataEmprestimoDe { get; set; }
        public DateTime? DataEmprestimoAte { get; set; }
        public DateTime? DevolucaoDe { get; set; }
        public DateTime? DevolucaoAte { get; set; }
        public DateTime? VencimentoDe { get; set; }
        public DateTime? VencimentoAte { get; set; }

    }
}
