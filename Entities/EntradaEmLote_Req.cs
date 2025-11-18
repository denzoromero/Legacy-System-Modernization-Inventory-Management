using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class EntradaEmLote_Req
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Key]
        public int? IdFerramentaria { get; set; }

        [MaxLength(50)]
        [Display(Name = "RFM")]
        public string? RFM { get; set; }

        public int? Status { get; set; }

        public int? IdSolicitante { get; set; }

        public DateTime? DataRegistro { get; set; }

    }
}
