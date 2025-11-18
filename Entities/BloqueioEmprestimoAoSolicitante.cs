using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class BloqueioEmprestimoAoSolicitante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public int? IdTerceiro { get; set; }

        public int? CodColigada { get; set; }

        [MaxLength(50)]
        [Display(Name = "Chapa")]
        public string? Chapa { get; set; }

        public int? IdUsuario_Adicionou { get; set; }

        public int? IdUsuario_Excluiu { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Mensagem")]
        public string? Mensagem { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

    }
}
