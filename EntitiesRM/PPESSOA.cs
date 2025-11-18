using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.EntitiesRM
{
    public class PPESSOA
    {
        [Key]
        public int? CODIGO { get; set; }

        [MaxLength(120)]
        public string? NOME { get; set; }

        [MaxLength(40)]
        public string? APELIDO { get; set; }

        //[ForeignKey("IDIMAGEM")]
        public int? IDIMAGEM { get; set; }

    }
}
