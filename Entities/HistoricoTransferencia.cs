using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class HistoricoTransferencia
    {
        [Key]
        public int? Id { get; set; }
        public int? IdProduto { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? DataOcorrencia { get; set; }
        public int? IdFerramentariaOrigem { get; set; }
        public int? IdFerramentariaDestino { get; set; }
        public int? Quantidade { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal? Saldo { get; set; }
        public string? Documento { get; set; }

    }
}
