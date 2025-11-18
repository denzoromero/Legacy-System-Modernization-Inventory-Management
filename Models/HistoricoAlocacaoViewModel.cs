namespace FerramentariaTest.Models
{
    public class HistoricoAlocacaoViewModel
    {
        public int? IdHistoricoAlocacao { get; set; }
        public int? IdProdutoAlocado { get; set; }
        public int? IdProduto { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        public string? Liberador_Chapa { get; set; }
        public int? Balconista_Emprestimo_IdLogin { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public int? IdObra { get; set; }
        public int? Quantidade { get; set; }
        public int? QuantidadeEmprestada { get; set; }
        public int? IdFerrOndeProdRetirado { get; set; }
        public int? IdFerrOndeProdDevolvido { get; set; }
        public string? CodigoCatalogo { get; set; }
        public string? NomeCatalogo { get; set; }
        public string? FerrOrigem { get; set; }
        public string? FerrDevolucao { get; set; }
        public string? AFProduto { get; set; }
        public string? Serie { get; set; }
        public int? PATProduto { get; set; }
        public int? IdControleCA { get; set; }
    }

 
}
