using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class ProdutoExtraviado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int? Id { get; set; }

        public int? IdProdutoExcluido { get; set; }

        public int? IdProdutoAlocado { get; set; }

        public int? Quantidade { get; set; }

        [MaxLength(250)]
        public string? Observacao { get; set; }

        public int? IdUsuario { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

    }
}
