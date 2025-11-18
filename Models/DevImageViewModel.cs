using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class DevImageViewModel
    {
        public int? Id { get; set; }

        public int? IdProdutoAlocado { get; set; }

        public int? IdHistoricoAlocacao { get; set; }

        public int? Ano { get; set; }

        public int? Solicitante_IdTerceiro { get; set; }

        public int? Solicitante_CodColigada { get; set; }

        public string? Solicitante_Chapa { get; set; }

        public int? IdUsuario { get; set; }

        public int? Tipo { get; set; }

        public string? ArquivoNome { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

        public byte[] ImageData { get; set; }

        public string? Responsavel { get; set; }
    }
}
