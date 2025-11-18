using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class CatalogoLocal
    {
        [Key]
        public int? IdCatalogo { get; set; }

        [Key]
        public int? IdFerramentaria { get; set; }

        [MaxLength(5)]
        public string? Pos1Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos1Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos1Linha { get; set; }

        [MaxLength(5)]
        public string? Pos2Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos2Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos2Linha { get; set; }

        [MaxLength(5)]
        public string? Pos3Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos3Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos3Linha { get; set; }

        [MaxLength(5)]
        public string? Pos4Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos4Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos4Linha { get; set; }

        [MaxLength(5)]
        public string? Pos5Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos5Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos5Linha { get; set; }

        [MaxLength(5)]
        public string? Pos6Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos6Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos6Linha { get; set; }

        [MaxLength(5)]
        public string? Pos7Prateleira { get; set; }


        [MaxLength(5)]
        public string? Pos7Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos7Linha { get; set; }

        [MaxLength(5)]
        public string? Pos8Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos8Coluna { get; set; }


        [MaxLength(5)]
        public string? Pos8Linha { get; set; }

        [MaxLength(5)]
        public string? Pos9Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos9Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos9Linha { get; set; }


        [MaxLength(5)]
        public string? Pos10Prateleira { get; set; }

        [MaxLength(5)]
        public string? Pos10Coluna { get; set; }

        [MaxLength(5)]
        public string? Pos10Linha { get; set; }

        public DateTime DataRegistro { get; set; }
    }
}
