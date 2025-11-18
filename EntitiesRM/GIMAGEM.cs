using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FerramentariaTest.EntitiesRM
{
    public class GIMAGEM
    {
        [Key]
        public int? ID { get; set; }

        public byte[]? IMAGEM { get; set; }

    }
}
