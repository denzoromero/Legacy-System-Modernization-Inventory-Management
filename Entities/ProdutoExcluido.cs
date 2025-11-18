using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FerramentariaTest.Entities
{
    public class ProdutoExcluido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int? Id { get; set; }

        public int? IdTipoExclusao { get; set; }

        public int? IdProduto { get; set; }

        [MaxLength(400)]
        public string? Observacao { get; set; }

        public int? IdUsuario { get; set; }

        public DateTime? DataRegistro { get; set; }
    }
}
