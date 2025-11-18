using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FerramentariaTest.Entities
{
    public class HistoricoAlocacao_2022
    {
        [Key]
        public int? Id { get; set; }

        public int? IdProduto { get; set; }

        public int? Solicitante_IdTerceiro { get; set; }

        public int? Solicitante_CodColigada { get; set; }

        [MaxLength(10)]
        public string? Solicitante_Chapa { get; set; }

        public int? Liberador_IdTerceiro { get; set; }

        public int? Liberador_CodColigada { get; set; }

        [MaxLength(10)]
        public string? Liberador_Chapa { get; set; }

        public int? Balconista_Emprestimo_IdLogin { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }

        [MaxLength(250)]
        public string? Observacao { get; set; }

        public DateTime? DataEmprestimo { get; set; }

        public DateTime? DataPrevistaDevolucao { get; set; }

        public DateTime? DataDevolucao { get; set; }

        public int? IdObra { get; set; }

        public int? Quantidade { get; set; }

        public int? IdFerrOndeProdRetirado { get; set; }

        public int? IdFerrOndeProdDevolvido { get; set; }

        public decimal? Kilo { get; set; }

        public int? IdControleCA { get; set; }
    }
}
