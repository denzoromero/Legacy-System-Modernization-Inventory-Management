using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    [Table("BloqueioEmprestimoVsLiberador")] // Make sure this matches your table name

    public class BloqueioEmprestimoVsLiberador
    {

        [Key]
        public int? IdLogin { get; set; }

    }
}
