using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.EntitySeek
{
    public class FuncionarioSeek
    {
        [Key]
        public int? Id { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public int? IdSecao { get; set; }
        public int? IdFuncao { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public string? CodFuncao { get; set; }
        public string? CodSecao { get; set; }
    }
}
