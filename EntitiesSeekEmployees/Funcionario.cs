using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitiesSeekEmployees
{
    public class Funcionario
    {
        [Key]
        public int? Id { get; set; }

        [MaxLength(10)]
        public string? Chapa { get; set; }

        [MaxLength(150)]
        public string? Nome { get; set; }

        public int? IdSecao { get; set; }

        public int? IdFuncao { get; set; }

        [MaxLength(150)]
        public string? Observacao { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

        [MaxLength(10)]
        public string? CodFuncao { get; set; }

        [MaxLength(35)]
        public string? CodSecao { get; set; }
    }
}
