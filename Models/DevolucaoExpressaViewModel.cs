namespace FerramentariaTest.Models
{
    public class DevolucaoExpressaViewModel
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
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public int? Quantidade { get; set; }
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
        public string? CodigoCatalogo { get; set; }
        public string? NomeCatalogo { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEPI { get; set; }
        public int? IdCategoria { get; set; }
        public int? ClasseCategoria { get; set; }
        public string? NomeCategoria { get; set; }
        public int? ProdutoAtivo { get; set; }
        public int? IdControleCA { get; set; }
        public int? QuantidadeInput { get; set; }
        public int? QuantidadeExtraviada { get; set; }
        public int? selectedItem { get; set; }
        public int? FerramentariaReturnId { get; set; }
        public int? LoggedFerramentariaId { get; set; }
    }

    public class FilterModel
    {
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? Printer { get; set; }
    }

}
