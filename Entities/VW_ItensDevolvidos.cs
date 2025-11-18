using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_ItensDevolvidos
    {
        [Column("Catálogo")]
        public string? Catalogo { get; set; }

        public string? Classe { get; set; }
        public string? Tipo { get; set; }

        [Column("Código")]
        public string? Codigo { get; set; }

        public string? Produto { get; set; }

        [Column("AF/Serial")]
        public string? AF { get; set; }

        public int? PAT { get; set; }

        [Column("Observação")]
        public string? Observacao { get; set; }

        [Column("Setor Origem")]
        public string? SetorOrigem { get; set; }

        [Column("Setor Devolucao")]
        public string? SetorDevolucao { get; set; }

        [Column("Solicitante Chapa")]
        public string? Solicitante_Chapa { get; set; }

        [Column("Solicitante Nome")]
        public string? Solicitante_Nome { get; set; }

        [Column("Data Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Balconista Devolução")]
        public string? Balconista_Devolucao { get; set; }

        [Column("Data Devolução")]
        public DateTime? DataDevolucao { get; set; }
    }
}
