namespace FerramentariaTest.Entities
{
    public class Usuario
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public int? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Senha { get; set; }
        public string? Email { get; set; }
        public int? ReinicializarSenha { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
    }
}
