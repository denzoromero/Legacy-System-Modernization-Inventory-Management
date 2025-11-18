using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Produto
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public int? IdCatalogo { get; set; }
  
        public int? IdFerramentaria { get; set; }

        [MaxLength(30)]
        public string? AF { get; set; }

        public int? PAT { get; set; }

        public int? Quantidade { get; set; }

        public int? QuantidadeMinima { get; set; }

        [MaxLength(30)]
        public string? Localizacao { get; set; }

        public DateTime? DataVencimento { get; set; }

        [MaxLength(20)]
        public string? TAG { get; set; }

        [MaxLength(20)]
        public string? Serie { get; set; }

        [MaxLength(50)]
        public string? Selo { get; set; }

        public int? IdModelo { get; set; }

        public int? IdEmpresa { get; set; }

        public int? IdUnidadeAfericao { get; set; }

        [MaxLength(50)]
        public string? RFM { get; set; }

        [MaxLength(400)]
        public string? Observacao { get; set; }

        public DateTime? DC_DataAquisicao { get; set; }

        [Column(TypeName = "decimal(16, 2)")]
        public decimal? DC_Valor { get; set; }

        [MaxLength(50)]
        public string? DC_AssetNumber { get; set; }

        [MaxLength(100)]
        public string? DC_Fornecedor { get; set; }


        [MaxLength(50)]
        public string? GC_Contrato { get; set; }

        public DateTime? GC_DataInicio { get; set; }

        public int? GC_IdObra { get; set; }

        [MaxLength(50)]
        public string? GC_OC { get; set; }

        public DateTime? GC_DataSaida { get; set; }

        [MaxLength(50)]
        public string? GC_NFSaida { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

        [MaxLength(50)]
        public string? Certificado { get; set; }

        //public virtual ICollection<ProdutoAlocado> ProdutoAlocados { get; set; }

    }
}
