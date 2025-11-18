using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_HistItensEmpDev
    {
        public int? IdProduto { get; set; }

        [Column("Código")]
        public string? Codigo { get; set; }

        public string? Produto { get; set; }

        [Column("AF/Serial")]
        public string? AF { get; set; }

        public int? PAT { get; set; }

        [Column("Data do Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }


        [Column("Ferr. Empréstimo")]
        public string? FerrEmprestimo { get; set; }

        [Column("Status do Solicitante")]
        public string? StatusSolicitante { get; set; }


        [Column("Mat. Solicitante")]
        public string? ChapaSolicitante { get; set; }

        [Column("Nome Solicitante")]
        public string? NomeSolicitante { get; set; }

        [Column("Balconista do Empréstimo")]
        public string? BalconistaEmprestimo { get; set; }


        [Column("Data de Devolução")]
        public DateTime? DataDevolucao { get; set; }


        [Column("Ferr. Devolução")]
        public string? FerrDevolucao { get; set; }

        [Column("Balc. Dev.")]
        public string? BalcDev { get; set; }

    }
}
