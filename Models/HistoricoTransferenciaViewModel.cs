namespace FerramentariaTest.Models
{
    public class HistoricoTransferenciaViewModel
    {
        public int? IdProduto { get; set; }
        public string? Ferramentaria { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? RFM { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? IdUsuario { get; set; }
        public string? Usuario { get; set; }
        public int? Quantidade { get; set; }
        public string? FerramentariaOrigem { get; set; }
        public string? FerramentariaDestino { get; set; }
        public DateTime? DataOcorrencia { get; set; }
        public string? Documento { get; set; }
    }

    public class ProdutoFilter
    {
        public int? Catalogo { get; set; }
        public int? Classe { get; set; }
        public int? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public string? NumeroSerie { get; set; }
    }

    public class ProdutoList
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? PorMetro { get; set; }
        public int? PorSerial { get; set; }
        public int? PorAferido { get; set; }
        public string? FerramentariaOrigem { get; set; }
    }
}
