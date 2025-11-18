using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class ControleCA
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? IdCatalogo { get; set; }

        [MaxLength(30)]
        public string? NumeroCA { get; set; }

        public DateTime? Validade { get; set; }

        [MaxLength(250)]
        public string? Responsavel { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

    }
}
