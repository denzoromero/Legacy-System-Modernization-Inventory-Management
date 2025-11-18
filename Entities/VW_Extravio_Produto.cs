using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Extravio_Produto
    {
        public int? Id { get; set; }
        public string? Ferramentaria { get; set; }

        [Column("Código")]
        public string? Codigo { get; set; }
        public string? Descrição { get; set; }
        public int? Quantidade { get; set; }
        [Column("AF Serial")]
        public string? AFSerial { get; set; }
        public int? PAT { get; set; }
        public string? Obs { get; set; }
        [Column("Matrícula Solicitante")]
        public string? MatriculaSolicitante { get; set; }
        [Column("Data do Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }
        [Column("Balconista do Registro do Extravio")]
        public string? BalconistaRegistroExtravio { get; set; }
        [Column("Justificativa do Extravio")]
        public string? JustificativaExtravio { get; set; }
        [Column("Data e Hora do Registro do Extravio")]
        public DateTime? DataHoraRegistroExtravio { get; set; }

    }
}
