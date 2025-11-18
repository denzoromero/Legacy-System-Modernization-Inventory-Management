namespace FerramentariaTest.Entities
{
    public class VW_Usuario
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public int? CodPessoa { get; set; }
        public int? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Senha { get; set; }
        public string? Email { get; set;}
        public int? ReinicializarSenha { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public string? Nome { get; set; }
        public string? CodSecao { get; set; }
        public string? Secao { get; set; }
        public string? CodFuncao { get; set; }
        public string? Funcao { get; set; }
        public string? CodSituacao { get; set; }
        public DateTime? FimProgFerias1 { get; set; }
        public string? SAMAccountName { get; set; }
    }
}
