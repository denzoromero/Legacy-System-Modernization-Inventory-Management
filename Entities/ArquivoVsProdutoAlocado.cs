using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class ArquivoVsProdutoAlocado
    {
        [Key]
        public int? IdArquivo { get; set; }

        [Key]
        public int? IdProdutoAlocado { get; set; }

        public DateTime? DataRegistro { get; set; }

 
    }
}
