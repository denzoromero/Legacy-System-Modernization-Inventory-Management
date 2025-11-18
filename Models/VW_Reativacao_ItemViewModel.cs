using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class VW_Reativacao_ItemViewModel
    {
        //[Key]
        public int Id { get; set; }

        //[Display(Name = "Codigo")]
        //[MaxLength(20)]
        public string? Codigo { get; set; }


        //[Display(Name = "Descricao")]
        //[MaxLength(500)]
        public string? Descricao { get; set; }

        //[Display(Name = "AF")]
        //[MaxLength(30)]
        public string? AF { get; set; }

        //[Display(Name = "PAT")]
        public int? PAT { get; set; }

        //[Display(Name = "Saldo")]
        public int? Saldo { get; set; }

        //[Display(Name = "Controle")]
        //[MaxLength(10)]
        public string? Controle { get; set; }

        //[Display(Name = "LocalEmEstoque")]
        //[MaxLength(250)]
        public string? LocalEmEstoque { get; set; }

        //[Display(Name = "Motivo")]
        //[MaxLength(50)]
        public string? Motivo { get; set; }

        //[Display(Name = "Justificativa")]
        //[MaxLength(400)]
        public string? Justificativa { get; set; }

        //[Display(Name = "Usuario")]
        //[MaxLength(150)]
        public string? Usuario { get; set; }

        //[Display(Name = "DataInativacao")]
        public DateTime DataInativacao { get; set; }

        //[Display(Name = "Status")]
        public int? Status { get; set; }


        //[Display(Name = "Justificativa_Reativacao")]
        //[MaxLength(250)]
        public string? Justificativa_Reativacao { get; set; }


        //[Display(Name = "MatriculaFuncionarioEmprestimo")]
        //[MaxLength(1)]
        public string? MatriculaFuncionarioEmprestimo { get; set; }

        //[Display(Name = "IdProdutoAlocado")]
        public int? IdProdutoAlocado { get; set; }
    }

    public class VW_Reactivacao_SearchViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Codigo")]
        [MaxLength(20)]
        public string? Codigo { get; set; }

        [Display(Name = "Descricao")]
        [MaxLength(500)]
        public string? Descricao { get; set; }


        [Display(Name = "AF")]
        [MaxLength(30)]
        public string? AF { get; set; }

        [Display(Name = "PAT")]
        public int? PAT { get; set; }

        [Display(Name = "Controle")]
        [MaxLength(10)]
        public string? Controle { get; set; }

        [Display(Name = "LocalEmEstoque")]
        [MaxLength(250)]
        public string? LocalEmEstoque { get; set; }

        [Display(Name = "Motivo")]
        [MaxLength(50)]
        public string? Motivo { get; set; }

        [Display(Name = "Justificativa_Reativacao")]
        [MaxLength(250)]
        public string? Justificativa { get; set; }


        [Display(Name = "Usuario")]
        [MaxLength(150)]
        public string? Usuario { get; set; }

        public DateTime? De { get; set; }
        public DateTime? Ate { get; set; }
        public int? Pagination { get; set; }

    }
}
