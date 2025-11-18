using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class EntradaEmLote_Temp
    {
        public int? IdRequisicao { get; set; }
        public int? IdCatalogo { get; set; }
        public int? Quantidade { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Serie { get; set; }
        public string? UnidadeAfericao { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Propriedade { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string? Certificado { get; set; }
        public DateTime? DC_DataAquisicao { get; set; }
        [Column(TypeName = "decimal(16, 2)")]
        public decimal? DC_Valor { get; set; }
        public string? DC_Fornecedor { get; set; }
        public string? Observacao { get; set; }
        [Key]
        public DateTime? DataRegistro { get; set; }
    }
}
