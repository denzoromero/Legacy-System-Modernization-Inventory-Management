using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Catalogo
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "IdCategoria")]
        public int? IdCategoria { get; set; }

        [Display(Name = "Codigo")]
        [MaxLength(20)]
        public string? Codigo { get; set; }

        [Display(Name = "Nome")]
        [MaxLength(500)]
        public string? Nome { get; set; }

        [Display(Name = "Descricao")]
        [MaxLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "PorMetro")]
        public int? PorMetro { get; set; }

        [Display(Name = "PorAferido")]
        public int? PorAferido { get; set; }

        [Display(Name = "PorSerial")]
        public int? PorSerial { get; set; }

        [Display(Name = "RestricaoEmprestimo")]
        public int? RestricaoEmprestimo { get; set; }

        [Display(Name = "ImpedirDescarte")]
        public int? ImpedirDescarte { get; set; }

        [Display(Name = "HabilitarDescarteEPI")]
        public int? HabilitarDescarteEPI { get; set; }

        [Display(Name = "DataDeRetornoAutomatico")]
        public int? DataDeRetornoAutomatico { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime? DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int? Ativo { get; set; }

        public byte[]? ImageData { get; set; }

        [NotMapped]
        public string? PorType
        {
            get
            {
                if (PorMetro == 1 && PorAferido == 0 && PorSerial == 0)
                {
                    return "PorMetro";
                }
                else if (PorAferido == 1 && PorMetro == 0 && PorSerial == 0)
                {
                    return "PorAferido";
                }
                else if (PorSerial == 1 && PorMetro == 0 && PorAferido == 0)
                {
                    return "PorSerial";
                }
                else if (PorSerial == 0 && PorMetro == 0 && PorAferido == 0)
                {
                    return "PorQuantidade";
                }
                else
                {
                    return string.Empty; // or handle other cases if needed
                }
            }
        }


    }
}
