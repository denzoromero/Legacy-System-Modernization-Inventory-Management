using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace FerramentariaTest.Entities
{
    [Table("FerramentariaVsLiberador")] // Make sure this matches your table name
    //[Keyless]
    public class FerramentariaVsLiberador
    {
        //public int Id { get; set; } // This is a dummy primary key
        [Key]
        [Column("IdFerramentaria")]
        public int? IdFerramentaria { get; set; }

        [Key]
        public int? IdLogin { get; set; }

    }
}
