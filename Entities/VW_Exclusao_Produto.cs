using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Exclusao_Produto
    {
        public string? Ferramentaria { get; set; }
        public int? IdProduto { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }

        [Column("Código")]
        public string? Codigo { get; set; }

        public string? Produto { get; set; }
        public string? RFM { get; set; }

        [Column("AF/Serial")]
        public string? AF { get; set; }

        public int? PAT { get; set; }
        public string? Suporte { get; set; }
        public string? Motivo { get; set; }
        public string? Justificativa { get; set; }

        [Column("Data da Ocorrência")]
        public DateTime? DataOcorrencia { get; set; }

    }
}
