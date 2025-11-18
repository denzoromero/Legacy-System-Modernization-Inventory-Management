namespace FerramentariaTest.Models
{
    public class FuncionarioViewModel
    {
        public int? IdTerceiro { get; set; }
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

    public class FuncionarioBlockViewModel
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
        public string? Message { get; set; }
        public UserViewModel? UserViewModel { get; set; }

    }

    public class EmployeeInformationBS
    {
        public int? IdTerceiro { get; set; }
        public int? CodPessoa { get; set; }
        public short? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? Secao { get; set; }
        public string? Funcao { get; set; }
        public byte[]? Image { get; set; }
        public string? ImageString { get; set; }
        public string? CodSituacao { get; set; }
        public string? DataAdmissao { get; set; }
        public string? DataDemissao { get; set; }
    }
}
