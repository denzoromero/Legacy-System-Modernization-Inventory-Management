using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitiesRM
{
    public class PFUNC
    {
        [Key]
        public Int16? CODCOLIGADA { get; set; }
        public string? NOME { get; set; }
        public string? CHAPA { get; set; }

    }
}
