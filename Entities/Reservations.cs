using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class Reservations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? IdReservationControl { get; set; }
        public int? IdLeaderMemberRel { get; set; }
        public int? IdCatalogo { get; set; }
        public int? IdFerramentaria { get; set; }
        public int? Quantidade { get; set; }
        public int? Status { get; set; }
        public string? Chave { get; set; }
        public int? IdObra { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataRegistro { get; set; }

        [NotMapped]
        public string? StatusString
        {
            get
            {
                return Status switch
                {
                    0 => "Registrado",
                    1 => "Preparing",
                    2 => "Ready for Pickup",
                    3 => "Concluded",
                    7 => "Expired",
                    8 => "Cancellado",
                    _ => string.Empty
                };
            }
        }
    }

    public class TermsControl
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int? Balconista { get; set; }
        public string? Chapa { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? CodPessoa { get; set; }
        public byte[]? ImageData { get; set; }
    }

    public class AuditLogsBalconista
    {
        public int? Id { get; set; }
        public string? Message { get; set; }
        public string? MessageTemplate { get; set; }
        public string? Level { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string? LogEvent { get; set; }
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public string? Outcome { get; set; }
        public string? TraceId { get; set; }
        public string? TraceIdGuid { get; set; }
        public string? SessionId { get; set; }
        public string? TransactionId { get; set; }
    }



}
