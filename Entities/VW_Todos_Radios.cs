using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Todos_Radios
    {
        public int? Id { get; set; }
        public string? Ferramentaria { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public string? AfSerial { get; set; }
        public int? PAT { get; set; }
        public int? QtdEstoque { get; set; }
        public int? QtdMinEstoque { get; set; }
        public string? ControlePor { get; set; }
        public DateTime? DataRegistro { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? NumeroSerie { get; set; }
        public string? RFM { get; set; }
        public DateTime? DC_DataAquisicao { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal? DC_Valor { get; set; }
        public string? DC_AssetNumber { get; set; }
        public string? DC_Fornecedor { get; set; }
        public string? GC_Contrato { get; set; }
        public DateTime? GC_DataInicio { get; set; }
        public string? Nome { get; set; }
        public string? GC_OC { get; set; }
        public DateTime? GC_DataSaida { get; set; }
        public string? GC_NFSaida { get; set; }
        public string? Status { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public string? SolicitanteChapa { get; set; }
        public string? SolicitanteNome { get; set; }
        public string? LiberadorChapa { get; set; }
        public string? LiberadorNome { get; set; }
        public string? Balconista { get; set; }

    }
}
