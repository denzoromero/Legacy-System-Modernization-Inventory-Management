
namespace FerramentariaTest.Models
{
    public class EntradaEmLote_ReqViewModel
    {
      
        public int? Id { get; set; }

        public int? IdFerramentaria { get; set; }

        public string? RFM { get; set; }

        public int? Status { get; set; }

        public int? IdSolicitante { get; set; }

        public DateTime? DataRegistro { get; set; }
        public string? SolicitanteNome { get; set; }
    }

    public class EntradaEmLoteSearch
    {
        public string? RFM { get; set; }

        public int? Status { get; set; }

        public int? Pagination { get; set; }
        public int? IdFerramentaria { get; set; }
    }

    public class EntradaEmLote_CompViewModel
    {
        public int? IdRequisicao { get; set; }
        public int? IdCatalogo { get; set; }
        public int? Quantidade { get; set; }
        public string? Observacao { get; set; }
        public int? Status { get; set; }
        public DateTime? DataRegistro { get; set; }
    }

    public class CombinedForModal
    {
        //public CatalogoViewModel? CatalogValue { get; set; }
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? RestricaoEmprestimo { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEpi { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CategoriaNomePai { get; set; }
        public DateTime? CategoriaDataRegistro { get; set; }
        public int? CategoriaAtivo { get; set; }
        public string? Observacao { get; set; }
        public int? Quantidade { get; set; }
        public string? FilePath { get; set; }
    }

    public class EntradaEmLote_TempModel
    {
        public int? Id { get; set; }
        public int? IdClasse { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? PorMetro { get; set; }
        public string? Codigo { get; set; }
        public string? CatalogNome { get; set; }
        public int? Quantidade { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Serie { get; set; }
        public string? Propriedade { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string? Certificado { get; set; }
        public DateTime? DC_DataAquisicao { get; set; }
        public decimal? DC_Valor { get; set; }
        public string? DC_Fornecedor { get; set; }
        public string? Observacao { get; set; }
        //public string? FilePath { get; set; }
    }

    public class SimpleProductModel
    {
        public int? IdProduto { get; set; }
        public int? IdCatalogo { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? PorMetro { get; set; }
        public int? AtivoCatalogo { get; set; }
        public int? IdFerramentaria { get; set; }
        public int? Quantidade { get; set; }
        public int? AtivoProduto { get; set; }
        public string? Codigo { get; set; }
        public int? IdCategoriaPai { get; set; }

    }

    public class SerialProductModel
    {
        public int? IdRequisicao { get; set; }
        public int? IdCatalogo { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? Quantidade { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Serie { get; set; }
        public string? Propriedade { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string? Certificado { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public decimal? Valor { get; set; }
        public string? Fornecedor { get; set; }
        public string? Observacao { get; set; }
        public int? IdEmpresa { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
    }
}
