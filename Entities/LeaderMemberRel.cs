using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class LeaderMemberRel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? IdLeader { get; set; }
        public int? CodPessoa { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public int? Ativo { get; set; }
        public DateTime DataRegistro { get; set; }
    }
}
