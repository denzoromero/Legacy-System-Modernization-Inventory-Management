namespace FerramentariaTest.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? Mensagem { get; set; }
        public string? Tela { get; set; }
        public string? Descricao { get; set; }
        public string? IdLog { get; set; }
    }
}