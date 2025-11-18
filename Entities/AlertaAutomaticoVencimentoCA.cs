using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    //[Keyless]
    public class AlertaAutomaticoVencimentoCA 
    {
        [Key]
        public string? Destinatario { get; set; }
        public DateTime? UltimoEnvio { get; set; }
    }
}
