using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class Arquivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int? Id { get; set; }

        public int? IdProdutoAlocado { get; set; }

        public int? IdHistoricoAlocacao { get; set; }

        public int? Ano { get; set; }

        public int? Solicitante_IdTerceiro { get; set; }

        public int? Solicitante_CodColigada { get; set; }

        [MaxLength(20)]
        public string? Solicitante_Chapa { get; set; }

        public int? IdUsuario { get; set; }

        public int? Tipo { get; set; }

        [Column("Arquivo")] // Specify the column name explicitly
        public string? ArquivoNome { get; set; }

        public DateTime? DataRegistro { get; set; }

        public int? Ativo { get; set; }

        public byte[]? ImageData { get; set; }

        [MaxLength(100)]
        public string? Responsavel { get; set; }

    }
}
