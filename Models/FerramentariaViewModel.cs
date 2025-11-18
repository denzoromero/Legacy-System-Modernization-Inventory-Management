using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class FerramentariaViewModel
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "Nome is Required")]
        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime? DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int? Ativo { get; set; }
    }

    public class SimpleFerramentariaViewModel
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
    }

    public class WithReservationFerramentariaModel
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? IdVirtual { get; set; }
    }

    public class NewFerramentariaModel
    {
        public List<WithReservationFerramentariaModel>? WithReservationFerramentariaModel { get; set; } = new List<WithReservationFerramentariaModel>();
        public string? FerramentariaFilter { get; set; }
        public int? Ativo { get; set; }
        public int? Pagination { get; set; }
        public int? PageNumber { get; set; }
        //public DateTime? DataRegistro { get; set; }
        //public int? Ativo { get; set; }
        //public int? IdVirtual { get; set; }
    }

}
