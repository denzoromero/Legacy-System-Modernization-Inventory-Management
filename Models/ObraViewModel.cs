using X.PagedList.Mvc.Core;
using X.PagedList;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class ObraViewModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome is Required")]
        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "Codigo is Required")]
        [Display(Name = "Codigo")]
        public string? Codigo { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int Ativo { get; set; }
    }

    public class ObraViewModelSearch
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Required(ErrorMessage = "Nome is Required")]
        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "Codigo is Required")]
        [Display(Name = "Codigo")]
        public string? Codigo { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime? DataRegistro { get; set; }

        [Display(Name = "Ativo")]
        public int? Ativo { get; set; }

        public int? Status { get; set; }

        public IPagedList<ObraViewModel>? ObraPagedList { get; set; }
    }

    public class ReservationObraModel
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public string? Codigo { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? Status { get; set; }
        public int? IdExcluded { get; set; }
    }

    public class NewObraModel
    {
        public List<ReservationObraModel>? ReservationObraModel { get; set; } = new List<ReservationObraModel>();
        public string? ObraFilter { get; set; }
        public int? Ativo { get; set; }
        public int? Pagination { get; set; }
        public int? PageNumber { get; set; }
    }


}
