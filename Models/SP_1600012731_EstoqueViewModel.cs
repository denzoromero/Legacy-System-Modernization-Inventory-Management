namespace FerramentariaTest.Models
{
    public class SP_1600012731_EstoqueViewModel
    {
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? Controle { get; set; }
        public string? AfSerial { get; set; }
        public string? Serie { get; set; }
        public int? PAT { get; set; }
        public int? Saldo { get; set; }
        public int? QuantidadeMinima { get; set; }
        public int? IdProduto { get; set; }
        public DateTime? DatadoRegistro { get; set; }
        public DateTime? DatadeVencimento { get; set; }
        public string? NumerodeSerie { get; set; }
        public string? Selo { get; set; }
        public string? Certificado { get; set; }
        public string? Observacao { get; set; }
        public string? RFM { get; set; }
        public int? Ativo { get; set; }
        public string? Ferramentaria { get; set; }
        public int? IdProdutoAlocado { get; set; }
        public string? Extraviado { get; set; }

        public string Status
        {
            get
            {
                return (IdProdutoAlocado == null) ? "Em Estoque" : "Emprestado";
            }
        }
    }

    public class SearchGestorModel
    {
        public int? Catalogo { get; set; }
        public int? Classe { get; set; }
        public int? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Numero { get; set; }
        public DateOnly? DataVencimento { get; set; }
        public int? SerieCheckbox { get; set; }
        public int? VencidasCheck { get; set; }
        public int? Status { get; set; }
        public int? Saldo { get; set; }
        public int? Situacao { get; set; }
        public int? Pagination { get; set; }
        public bool Opcional { get; set; }

    }

    public class TransferModel
    {
        public int? IdProduto { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? AfSerial { get; set; }
        public int? PAT { get; set; }
        public int? Saldo { get; set; }
        public int? SaldoTransferir { get; set; }
    }

    public class ComunicadoInativoModel
    {
        public SP_1600012731_EstoqueViewModel? ComunicadoInativoValue { get; set; }
        public string? Tipo { get; set; }
        public string? Justificativa { get; set; }
        public DateTime? DataOcorrencia { get; set; }
        public UserViewModel? UsuarioModel { get; set; }
    }

    public class ComunicadoExtraviadoModel
    {
        public SP_1600012731_EstoqueViewModel? ComunicadoExtraviadoValue { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeEmprestimo { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public string? Justificativa { get; set; }
        public DateTime? DataRegistro { get; set; }
        public UserViewModel? Colaborador { get; set; }
        public UserViewModel? Comunicado { get; set; }
    }
}
