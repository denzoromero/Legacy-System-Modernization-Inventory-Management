namespace FerramentariaTest.Models
{
    public class LogAtribuicaoFerramentariaViewModel
    {
        public int? Id { get; set; }
        public int? IdUsuario { get; set; }
        public string? ChapaUsuario { get; set; }
        public string? NomeUsuario { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }
        public int? IdUsuarioResponsavel { get; set; }
        public string? ChapaUsuarioResponsavel { get; set; }
        public string? NomeUsuarioResponsavel { get; set; }
        public int? Acao { get; set; }
        public DateTime? DataRegistro { get; set; }

    }
}
