namespace FerramentariaTest.Models
{
    public class DevolucaoViewModel
    {
        public int? IdProdutoAlocado { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public int? Balconista_IdLogin { get; set; }
        public string? Balconista_Chapa { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        public string? Liberador_Chapa { get; set; }
        public string? Observacao { get; set; }
        public string? Localizacao { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeMinima { get; set; }
        public int? ProdutoQuantidade { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }
        public int? IdObra { get; set; }
        public string? NomeObra { get; set; }
        public int? IdProduto { get; set; }
        public string? AFProduto { get; set; }
        public int? PATProduto { get; set; }
        public DateTime? DataVencimento { get; set; }
        public DateTime? DC_DataAquisicao { get; set; }
        public decimal? DC_Valor { get; set; }
        public int? IdCatalogo { get; set; }
        public string? CodigoCatalogo { get; set; }
        public string? NomeCatalogo { get; set; }
        public int? CatalogoPorAferido { get; set; }
        public int? CatalogoPorSerial { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEPI { get; set; }
        public int? IdCategoria { get; set; }
        public int? ClasseCategoria { get; set; }
        public string? NomeCategoria { get; set; }
        public int? ProdutoAtivo { get; set; }
        public int? IdControleCA { get; set; }
        public string? NumeroControleCA { get; set; }
        public DateTime? ValidadeControlCA { get; set; }
        public int? QuantidadeExtraviada { get; set; }
        public bool? FileFound { get; set; }
        public int? IdReservation { get; set; }
    }

    public class SearchDevolucaoViewModel
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Catalogo { get; set; }
        public int? CatalogoList { get; set; }
        public DateTime? DataDeValidade { get; set; }
        public DateTime? TransacoesDe { get; set; }
        public DateTime? TransacoesAte { get; set; }
        public DateTime? PrevisaoDe { get; set; }
        public DateTime? PrevisaoAte { get; set; }
        public string? Observacao { get; set; }
        public int? Ticket { get; set; }
        public bool ItensExtraviados { get; set; }
        public int? Pagination { get; set; }

    }

    [Serializable]
    public class PassedDevolucaoModel
    {
        public int? IdProdutoAlocado { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public int? Balconista_IdLogin { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        public string? Liberador_Chapa { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeInput { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }
        public int? IdObra { get; set; }
        public string? NomeObra { get; set; }
        public int? IdProduto { get; set; }
        public string? AFProduto { get; set; }
        public int? PATProduto { get; set; }
        public DateTime? DataVencimento { get; set; }
        public DateTime? DC_DataAquisicao { get; set; }
        public decimal? DC_Valor { get; set; }
        //public string? CodigoCatalogo { get; set; }
        public string? NomeCatalogo { get; set; }
        public int? CatalogoPorAferido { get; set; }
        public int? CatalogoPorSerial { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEPI { get; set; }
        public int? IdCategoria { get; set; }
        public int? ClasseCategoria { get; set; }
        public string? NomeCategoria { get; set; }
        public int? ProdutoAtivo { get; set; }
        public int? IdControleCA { get; set; }
        public string? NumeroControleCA { get; set; }
        public DateTime? ValidadeControlCA { get; set; }
        public string? CodigoCatalogo { get; set; } 
        public int? ddlFerramentaria { get; set; } 
        public int? selectedIds { get; set; }

        //public List<string>? CodigoCatalogo { get; set; } = new List<string>();
        //public List<int>? ddlFerramentaria { get; set; } = new List<int>();
        //public List<int>? selectedIds { get; set; } = new List<int>();
    }

    public class ProdutoCompleteViewModel
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
        public int? IdProduto { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeMinima { get; set; }
        public string? Localizacao { get; set; }
        public string? RFM { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataRegistroProduto { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string? Certificado { get; set; }
        public string? Serie { get; set; }
        public int? AtivoProduto { get; set; }
        public int? IdCatalogo { get; set; }
        public string? Codigo { get; set; }
        public string? NomeCatalogo { get; set; }
        public string? Descricao { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? DataRegistroCatalogo { get; set; }
        public int? AtivoCatalogo { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? Classe { get; set; }
        public string? NomeCategoria { get; set; }
        public DateTime? DataRegistroCategoria { get; set; }
        public int? AtivoCategoria { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }
        public DateTime DataRegistroFerramentaria { get; set; }
        public int? AtivoFerramentaria { get; set; }
        public int? IdEmpresa { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? GerenteEmpresa { get; set; }
        public string? TelefoneEmpresa { get; set; }
        public DateTime? DataRegistroEmpresa { get; set; }
        public int? AtivoEmpresa { get; set; }
        public string? Status { get; set; }
        public int? QuantityFrontEnd { get; set; }
        public DateTime? DataEmprestimoFrontEnd { get; set; }
        public string? ObservacaoFrontEnd { get; set; }

    }
}
