using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    [PrimaryKey(nameof(IdAcesso), nameof(IdUsuario))]
    public class Permissao
    {
        //[Key]
        public int? IdAcesso { get; set; }
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public string? SAMAccountName { get; set; }
        public int? Visualizar { get; set; }
        public int? Inserir { get; set; }
        public int? Editar { get; set; }
        public int? Excluir { get; set; }
        public DateTime DataRegistro { get; set; }
    }
}
