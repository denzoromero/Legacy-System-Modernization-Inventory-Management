namespace FerramentariaTest.Models
{
    public class Historico_LiberacaoExcepcionalViewModel
    {
        public string? Catalogo { get; set; }
        public string? Codigo { get; set; }
        public string? Produto { get; set; }
        public int? Quantidade { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Observacao { get; set; }
        public string? Setor_Origem { get; set; }
        public string? Chapa_Solicitante { get; set; }
        public string? Nome_Solicitante { get; set; }
        public string? Funcao_Solicitante { get; set; }
        public string? Secao_Solicitante { get; set; }
        public string? Chapa_Liberador { get; set; }
        public string? Nome_Liberador { get; set; }
        public string? Funcao_Liberador { get; set; }
        public string? Secao_Liberador { get; set; }
        public string? Balconista_Emprestimo { get; set; }
        public string? Obra { get; set; }
        public DateTime? Data_Emprestimo { get; set; }
        public string? Liberacao_Excepcional { get; set; }
        public byte[]? HashCode { get; set; }
    }
}
