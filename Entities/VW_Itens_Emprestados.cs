using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Itens_Emprestados
    {
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public int? Quantidade { get; set; }
        public string? Produto { get; set; }
        public string? NumeroCA { get; set; }
        public string? ValidadeCA { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }

        [Column("Observação")]
        public string? Observacao { get; set; }

        [Column("Setor de Origem")]
        public string? SetorOrigem { get; set; }

        [Column("Status do Solicitante")]
        public string? StatusSolicitante { get; set; }

        [Column("Chapa do Solicitante")]
        public string? ChapaSolicitante { get; set; }

        [Column("Nome do Solicitante")]
        public string? NomeSolicitante { get; set; }

        [Column("Função do Solicitante")]
        public string? FuncaoSolicitante { get; set; }

        [Column("Seção do Solicitante")]
        public string? SecaoSolicitante { get; set; }

        [Column("Chapa do Liberador")]
        public string? ChapaLiberador { get; set; }

        [Column("Nome do Liberador")]
        public string? NomeLiberador { get; set; }

        public string? Balconista { get; set; }

        public string? Obra { get; set; }

        [Column("Data do Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Data Prevista para Devolução")]
        public DateTime? DataPrevistaDevolucao { get; set; }

        [Column("Data de Vencimento do Produto")]
        public DateTime? DataVencimentoProduto { get; set; }
    }
}
