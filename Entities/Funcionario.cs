using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Entities
{
    public class Funcionario
    {
        public int? CodPessoa { get; set; }
        public short? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? CodRecebimento { get; set; }
        public string? CodSituacao { get; set; }
        public string? CodTipo { get; set; }
        public string? CodSecao { get; set; }
        public string? Secao { get; set; }
        public string? CodFuncao { get; set; }
        public string? Funcao { get; set; }
        public DateTime? DataMudanca { get; set; }
        public string? Sexo { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public DateTime? FimProgFerias1 { get; set; }
        public DateTime? DataRegistro { get; set; }

    }
}
