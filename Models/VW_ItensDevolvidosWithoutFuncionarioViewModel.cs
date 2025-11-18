using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Models
{
    public class VW_ItensDevolvidosWithoutFuncionarioViewModel
    {
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Produto { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? Observacao { get; set; }
        public string? SetorOrigem { get; set; }
        public string? SetorDevolucao { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public string? Solicitante_Nome { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }
        public string? Balconista_Devolucao_Nome { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataDevolucao { get; set; }
    }
}
