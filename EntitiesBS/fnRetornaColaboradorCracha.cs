using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.EntitiesBS
{
    public class fnRetornaColaboradorCracha
    {
        public string? MATRICULA { get; set; }
        public string? COLIGADA { get; set; }
        public string? NOME { get; set; }
        public string? TIPOCOLA { get; set; }
        [Precision(9, 0)]
        public decimal? IDCOLAB { get; set; }
        public string? MATRICULA_SURICATO { get; set; }
        public string? TIPO { get; set; }
        public int? TIPOCRAC { get; set; }
    }
}
