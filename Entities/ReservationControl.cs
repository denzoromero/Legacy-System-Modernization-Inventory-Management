using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class ReservationControl
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? IdLeaderData { get; set; }
        public int? Type { get; set; }
        public string? Chave { get; set; }
        public int? Status { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? DataRegistro { get; set; }

        [NotMapped]
        public string? TypeString
        {
            get
            {
                return Type switch
                {
                    1 => "Reservation",
                    2 => "Retirada",
                    _ => string.Empty
                };
            }
        }

        [NotMapped]
        public string? StatusString
        {
            get
            {
                return Status switch
                {
                    0 => "Registrado",
                    7 => "Expired",
                    _ => string.Empty // Default case for undefined values
                };
            }
        }
    }
}
