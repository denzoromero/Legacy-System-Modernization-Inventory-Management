namespace FerramentariaTest.Models
{
    public class FormEstoqueSaidaViewModel
    {
        public int? CatalogoId { get; set; }
        public string? CatalogoType { get; set; }
        public string? ClasseNome { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CatalogoCodigo { get; set; }
        public string? CatalogoNome { get; set; }
        public string? Controle { get; set; }
        public int? CatalogoPorMetro { get; set; }
        public int? CatalogoPorAferido { get; set; }
        public int? CatalogoPorSerial { get; set; }
        public string? CatalogoDescricao { get; set; }
        public int? ProdutoQuantidade { get; set; }
        public int? ProdutoQuantidadeMinima { get; set; }
        public int? ProdutoId { get; set; }
        public string? ProdutoRFM { get; set; }
        public string? ProdutoObservacao { get; set; }
        public int? QuantidadeSaida { get; set; }
        public string? RFM { get; set; }
        public string? Observacao { get; set; }

        public int? CategoriaId { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }


        //public int? CatalogoPorMetro { get; set; }
        //public int? CatalogoPorAferido { get; set; }
        //public int? CatalogoPorSerial { get; set; }
        //public int? CatalogoRestricaoEmprestimo { get; set; }
        //public int? CatalogoImpedirDescarte { get; set; }
        //public int? CatalogoHabilitarDescarteEpi { get; set; }
        //public int? CatalogoDataDeRetornoAutomatico { get; set; }
        //public DateTime? CatalogoDataRegistro { get; set; }
        //public int? CatalogoAtivo { get; set; }
        //public int? CategoriaId { get; set; }
        //public int? IdCategoriaPai { get; set; }
        //public int? CategoriaClasse { get; set; }          
        //public DateTime? CategoriaDataRegistro { get; set; }
        //public int? CategoriaAtivo { get; set; }
    }

    public class EmprestadoModel
    {
        public DateTime? DataEmprestimo { get; set; }
        public string? Matricula { get; set; }
        public string? Nome { get; set; }
        public string? Empressa { get; set; }
        public string? Funcao { get; set; }
        public string? Secao { get; set; }
        public byte[]? Image { get; set; }
    }

    public class GestorEdit
    {
        public string? CatalogoType { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CatalogoNome { get; set; }
        public int? IdCatalogo { get; set; }
        public string? ClasseNome { get; set; }
        public string? CatalogoCodigo { get; set; }
        public string? CatalogoDescricao { get; set; }
        public string? Controle { get; set; }
        public string? AfSerial { get; set; }
        public int? PAT { get; set; }
        public string? Serie { get; set; }
        public string? Selo { get; set; }
        public int? Empresa { get; set; }
        public DateTime? DatadeVencimento { get; set; }
        public string? Certificado { get; set; }
        public int? Saldo { get; set; }
        public int? QuantidadeMinima { get; set; }
        public string? Observacao { get; set; }
        public string? RFM { get; set; }
        public DateTime? DataAquisicao  { get; set; }
        public decimal? Valor { get; set; }
        public int? Obra { get; set; }
        public string? AssetNumber { get; set; }
        public string? Fornecedor { get; set; }
        public string? Contrato { get; set; }
        public string? OC { get; set; }
        public string? NFSaida { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataSaida { get; set; }
        public int? IdProduto { get; set; }
        public int? IdProdutoAlocado { get; set; }
        public string? Error { get; set; }
        public string? FerramentariaUserValue { get; set; }
        public string? FerramentariaProductValue { get; set; }
        public bool? SaveButton { get; set; }
    }
}
