using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class EntradaEmLote_Comp
    {
        [Key]
        public int? IdRequisicao { get; set; }

        [Key]
        public int? IdCatalogo { get; set; }

        public int? Quantidade { get; set; }

        [MaxLength(250)]
        public string? Observacao { get; set; }

        public int? Status { get; set; }

        public DateTime? DataRegistro { get; set; }

    }
}
