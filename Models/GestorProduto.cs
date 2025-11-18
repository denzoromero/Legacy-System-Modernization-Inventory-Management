namespace FerramentariaTest.Models
{
    public class GestorProduto
    {
        public DateTime? DC_DataAquisicao { get; set; }
        public decimal? DC_Valor { get; set; }
        public string? DC_AssetNumber { get; set; }
        public string? DC_Fornecedor { get; set; }
        public string? GC_Contrato { get; set; }
        public DateTime? GC_DataInicio { get; set; }
        public int? GC_IdObra { get; set; }
        public string? GC_OC { get; set; }
        public DateTime? GC_DataSaida { get; set; }
        public string? GC_NFSaida { get; set; }
        public string? Selo { get; set; }
        public int? Id { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeMinima { get; set; }
        public string? Localizacao { get; set; }
        public string? RFM { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataRegistro { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string? Certificado { get; set; }
        public string? Serie { get; set; }
        public int? Ativo { get; set; }
        public int? CatalogoId { get; set; }
        public string? CatalogoCodigo { get; set; }
        public string? CatalogoNome { get; set; }
        public string? CatalogoDescricao { get; set; }
        public int? CatalogoPorMetro { get; set; }
        public int? CatalogoPorAferido { get; set; }
        public int? CatalogoPorSerial { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? CatalogoDataRegistro { get; set; }
        public int? CatalogoAtivo { get; set; }
        public int? CategoriaId { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }
        public string? CategoriaNome { get; set; }
        public DateTime? CategoriaDataRegistro { get; set; }
        public int? CategoriaAtivo { get; set; }
        public int FerramentariaId { get; set; }
        public string? FerramentariaNome { get; set; }
        public DateTime FerramentariaDataRegistro { get; set; }
        public int FerramentariaAtivo { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaNome { get; set; }
        public string? EmpresaGerente { get; set; }
        public string? EmpresaTelefone { get; set; }
        public DateTime? EmpresaDataRegistro { get; set; }
        public int? EmpresaAtivo { get; set; }
        public string? Status { get; set; }
    }
}
