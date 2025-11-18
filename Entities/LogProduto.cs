using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class LogProduto
    {
        [Key]
        public int? Id { get; set; }
        public int? IdProduto { get; set; }
        [MaxLength(30)]
        public string? AfDe { get; set; }
        [MaxLength(30)]
        public string? AfPara { get; set; }
        public int? PatDe { get; set; }
        public int? PatPara { get; set; }
        public int? QuantidadeDe { get; set; }
        public int? QuantidadePara { get; set; }
        public int? QuantidadeMinimaDe { get; set; }
        public int? QuantidadeMinimaPara { get; set; }
        [MaxLength(100)]
        public string? LocalizacaoDe { get; set; }
        [MaxLength(100)]
        public string? LocalizacaoPara { get; set; }
        [MaxLength(27)]
        public string? DataVencimentoDe { get; set; }
        [MaxLength(27)]
        public string? DataVencimentoPara { get; set; }
        [MaxLength(20)]
        public string? TagDe { get; set; }
        [MaxLength(20)]
        public string? TagPara { get; set; }
        [MaxLength(20)]
        public string? SerieDe { get; set; }
        [MaxLength(20)]
        public string? SeriePara { get; set; }
        public int? IdUnidadeAfericaoDe { get; set; }
        public int? IdUnidadeAfericaoPara { get; set; }
        public int? IdModeloDe { get; set; }
        public int? IdModeloPara { get; set; }
        public int? IdUsuario { get; set; }
        public int? Acao { get; set; }
        [MaxLength(50)]
        public string? RfmDe { get; set; }
        [MaxLength(50)]
        public string? RfmPara { get; set; }
        public string? ObservacaoDe { get; set; }
        public string? ObservacaoPara { get; set; }
        public DateTime? DataRegistro { get; set; }

    }
}
