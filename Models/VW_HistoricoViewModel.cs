namespace FerramentariaTest.Models
{
    public class VW_HistoricoViewModel
    {
        public int? IdProduto { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public int? Quantidade { get; set; }
        public string? Produto { get; set; }
        public string? CA { get; set; }
        public string? NumeroCA { get; set; }
        public string? ValidadeCA { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Observacao { get; set; }
        public string? SetorOrigem { get; set; }
        public string? StatusSolicitante { get; set; }
        public string? ChapaSolicitante { get; set; }
        public string? NomeSolicitante { get; set; }
        public string? FuncaoSolicitante { get; set; }
        public string? SecaoSolicitante { get; set; }
        public string? ChapaLiberador { get; set; }
        public string? NomeLiberador { get; set; }
        public string? SetorEmprestimo { get; set; }
        public int? BalconistaEmprestimoId { get; set; }
        public string? BalconistaEmprestimo { get; set; }
        public string? Obra { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public DateTime? DataVencimentoProduto { get; set; }
        public string? SetorDevolucao { get; set; }
        public string? BalconistaDevolucao { get; set; }
        public int? BalconistaDevolucaoId { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public string? StatusAtual { get; set; }

        public string? Solicitante_Chapa { get; set; }
        public string? Liberador_Chapa { get; set; }
        public int? Balconista_Emprestimo_IdLogin { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }
    }

    public class GroupedHistorico
    {
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public int? Quantidade { get; set; }
        public string? Setor { get; set; }
        public string? Obra { get; set; }
    }

}
