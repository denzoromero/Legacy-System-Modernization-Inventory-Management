
using FerramentariaTest.Entities;

namespace FerramentariaTest.Models
{
    public class EmprestimoViewModel
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
        public int IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }
        public DateTime DataRegistroFerramentaria { get; set; }
        public int AtivoFerramentaria { get; set; }
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
        public int? IdControleCA { get; set; }
        public string? ControleCA { get; set; }
        public DateTime? ControleCADate { get; set; }
        public List<ControleCA?> ControleCAList { get; set; } = new List<ControleCA?>();

    }

    public class MensagemSolicitanteViewModel
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public int? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public int? IdUsuario_Adicionou { get; set; }
        public string? Username { get; set; }
        public int? IdUsuario_Excluiu { get; set; }
        public string? Mensagem { get; set; }
        public int? Fixar { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? LoggedUserId { get; set; }
        public bool? allowdelete { get; set; }
    }

    public class productCheck
    {
        public bool success { get; set; }
        public string? message { get; set; }
    }

    public class emprestimoCart
    {
        public int? IdProduto { get; set; }
        public string? Codigo { get; set; }
    }

    public class submittedEmprestimo
    {
        public int? IdProduto { get; set; }
        public int? IdControleCA { get; set; }
        public int Quantidade { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public string? Observacao { get; set; }
    }

    public class productDetails
    {
        public int? IdProduto { get; set; }
        public string? Codigo { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? Classe { get; set; }
    }

}
