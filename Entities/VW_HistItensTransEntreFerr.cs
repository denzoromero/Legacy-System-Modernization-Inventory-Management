using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_HistItensTransEntreFerr
    {
        public int? IdProduto { get; set; }

        [Column("Código")]
        public string? Codigo { get; set; }

        public string? Produto { get; set; }

        [Column("AF/Serial")]
        public string? AF { get; set; }

        public int? PAT { get; set; }

        [Column("Data do Ocorrência")]
        public DateTime? DataOcorrencia { get; set; }

        [Column("Ferr. Origem")]
        public string? FerrOrigem { get; set; }

        [Column("Ferr. Destino")]
        public string? FerrDestino { get; set; }

        public string? Suporte { get; set; }
        public string? Documento { get; set; }

    }
}
