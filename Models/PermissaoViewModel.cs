namespace FerramentariaTest.Models
{
    public class PermissaoViewModel
    {
        public int? IdAcesso { get; set; }
        public int IdUsuario { get; set; }
        public string? SAMAccountName { get; set; }
        public int? Visualizar { get; set; }
        public int? Inserir { get; set; }
        public int? Editar { get; set; }
        public int? Excluir { get; set; }
        public DateTime DataRegistro { get; set; }

    }
}
