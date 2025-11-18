using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Radios_Emprestados
    {
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Produto { get; set; }
        public string? AfSerial { get; set; }
        public int? PAT { get; set; }

        [Column("Observação")]
        public string? Observacao { get; set; }

        [Column("Setor Origem")]
        public string? SetorOrigem { get; set; }

        [Column("Solicitante Chapa")]
        public string? SolicitanteChapa { get; set; }

        [Column("Solicitante Nome")]
        public string? SolicitanteNome { get; set; }

        [Column("Solicitante Função")]
        public string? SolicitanteFuncao { get; set; }

        [Column("Solicitante Seção")]
        public string? SolicitanteSecao { get; set; }

        [Column("Solicitante Status")]
        public string? SolicitanteStatus { get; set; }

        [Column("Liberador Chapa")]
        public string? LiberadorChapa { get; set; }

        [Column("Liberador Nome")]
        public string? LiberadorNome { get; set; }

        public string? Balconista { get; set; }

        [Column("Data Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Data Prevista Devolução")]
        public DateTime? DataPrevistaDevolucao { get; set; }

        [Column("Data Vencimento")]
        public DateTime? DataVencimento { get; set; }

    }
}
