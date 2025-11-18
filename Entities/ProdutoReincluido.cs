using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class ProdutoReincluido
    {
        [Key]
        public int? Id { get; set; }

        public int? IdProduto { get; set; }

        [Display(Name = "Observacao")]
        [MaxLength(250)]
        public string? Observacao { get; set; }

        public int? IdUsuario_Solicitante { get; set; }

        public int? IdUsuario_Aprovador { get; set; }

        public DateTime? DataRegistro_Aprovacao { get; set; }

        public int? Status { get; set; }

        public DateTime? DataRegistro { get; set; }

    }
}
