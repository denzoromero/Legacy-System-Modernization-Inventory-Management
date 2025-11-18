namespace FerramentariaTest.Models
{
    public class ArquivoViewModel
    {
        public int? Id { get; set; }
        public int? Ano { get; set; }
        public int? Tipo { get; set; }
        public string? ArquivoNome { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public byte[]? ImageData { get; set; }

    }
}
