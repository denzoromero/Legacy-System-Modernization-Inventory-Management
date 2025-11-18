using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class ProductReservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? IdReservation { get; set; }
        public int? IdProduto { get; set; }
        public int? IdControleCA { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public string? Observacao { get; set; }
        public int? PreparedBy { get; set; }
        public int? HandedBy { get; set; }
        public int? Status { get; set; }
        public int? FinalQuantity { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? DataRegistro { get; set; }
        public string? ModifiedTransactionId { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
