using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Acesso
    {
        public int? Id { get; set; }
        public int? Tipo { get; set; }
        [ForeignKey("Acesso")]
        public int? IdAcessoPai { get; set; }
        [ForeignKey("Modulo")]
        public int? IdModulo { get; set; }
        public string? Caminho { get; set; }
        public string? Pagina { get; set; }
        public string? Pagina1 { get; set; }
        public int? AbrirEmOutraPagina { get; set; }
        public string? Acesso_pt { get; set; }
        public string? Acesso_en { get; set; }
        public string? Descricao { get; set; }
        public string? Descricao_en { get; set; }
        public int? Ordenar { get; set; }
        public DateTime DataRegistro { get; set; }
        public int Ativo { get; set; }
    }
}
