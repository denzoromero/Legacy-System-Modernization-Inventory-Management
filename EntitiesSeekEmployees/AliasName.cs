using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitiesSeekEmployees
{
    public class AliasName
    {
        public int? Chapa { get; set; }

        public int? CodColigada { get; set; }

        [MaxLength(50)]
        public string? Alias { get; set; }
    }
}
