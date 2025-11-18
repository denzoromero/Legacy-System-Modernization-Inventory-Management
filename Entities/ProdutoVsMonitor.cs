using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class ProdutoVsMonitor
    {
        public int? IdProduto { get; set; }
        [Required]
        public int? IdLogin { get; set; }
    }
}
