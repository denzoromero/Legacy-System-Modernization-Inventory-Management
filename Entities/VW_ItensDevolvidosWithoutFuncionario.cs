using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_ItensDevolvidosWithoutFuncionario
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

        public string? Solicitante_Chapa { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }

        [Column("Data Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Data Devolução")]
        public DateTime? DataDevolucao { get; set; }
    }
}
