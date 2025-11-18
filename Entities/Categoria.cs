using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Categoria
    {
        [Key]
        public int? Id { get; set; }


        public int? IdCategoria { get; set; }


        public int? Classe { get; set; }

        [Display(Name = "Nome")]
        [MaxLength(150)]
        public string? Nome { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime? DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int? Ativo { get; set; }

        [NotMapped]
        public string? ClassType
        {
            get
            {
                return Classe switch
                {
                    1 => "Ferramenta",
                    2 => "EPI",
                    3 => "Consumable",
                    _ => string.Empty // Default case for undefined values
                };
            }
        }


    }
}
