using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class VW_Usuario_NewViewModel
    {
        public int? Id { get; set; }
        public int? CodColigada { get; set; }
        public int? CodPessoa { get; set; }
        public string? Chapa { get; set; }
        [Required(ErrorMessage = "Informe a Senha!")]
        public string? Senha { get; set; }
        public string? Email { get; set; }
        public int? ReinicializarSenha { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public string? Nome { get; set; }
        public string? Departamento { get; set; }
        public string? CodSecao { get; set; }
        public string? Secao { get; set; }
        public string? CodSecaoAnterior { get; set; }
        public string? SecaoAnterior { get; set; }
        public string? CodFuncao { get; set; }
        public string? Funcao { get; set; }
        public string? CodSituacao { get; set; }
        public string? FimProgFerias1 { get; set; }
        public string? Retorno { get; set; }
        [Required(ErrorMessage = "Informe seu usuário!")]
        public string? SAMAccountName { get; set; }
        public string? Pagina { get; set; }
        public string? Pagina1 { get; set; }
        public string? Acesso { get; set; }
        public PermissaoViewModel? Permissao { get; set; }
        public IEnumerable<AcessoViewModel>? AcessoLista { get; set; }

        public string? Erro { get; set; }

        public bool IsChecked { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? NomeFerramentaria { get; set; }

    }

    public class UserViewModel
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public string? filter { get; set; }
        public byte[]? Image { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? CodSituacao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public short? CodColigada { get; set; }
        public string? Funcao { get; set; }
        public string? Secao { get; set; }
        public bool? BloqueioEmprestimo { get; set; }


    }

    public class NewUserInformationModel
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public byte[]? Image { get; set; }
        public string? Imagebase64 { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? CodSituacao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public short? CodColigada { get; set; }
        public string? Funcao { get; set; }
        public string? Secao { get; set; }

        public List<MensagemSolicitanteViewModel>? blockMessage { get; set; } = new List<MensagemSolicitanteViewModel>();
        public string? blockSolicitanteMessage { get; set; }


    }

    public class LoggedUserData
    {
        public int? Id { get; set; }
        public int? IdTerceiro { get; set; }
        public int? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? Departamento { get; set; }
        public string? Secao { get; set; }
        public string? Funcao { get; set; }
        public string? CodSituacao { get; set; }
        public string? Email { get; set; }
        public List<PermissionAccessModel?>? ListOfPermissionAccess { get; set; } = new List<PermissionAccessModel?>();
        public string? ErrorHandler { get; set; }
    }

    public class MenuModel
    {
        public int? IdPai { get; set; }
        public string? MainMenuName { get; set; }
        public List<PermissionAccessModel?>? ChildrenMenu { get; set; } = new List<PermissionAccessModel?>();
        public int? ActiveMenu { get; set; }
    }

    public class PermissionAccessModel
    {
        public int? IdAcesso { get; set; }
        public int? IdUsuario { get; set; }
        public int? Visualizar { get; set; }
        public int? Inserir { get; set; }
        public int? Editar { get; set; }
        public int? Excluir { get; set; }
        public string? Caminho { get; set; }
        public string? Pagina { get; set; }
        public string? Pagina1 { get; set; }
        public string? Acesso { get; set; }
        public int? IdPai { get; set; }
        public string? Profile { get; set; }
    }

    public class simpleUserModel
    {
        public int? Id { get; set; }
        public int? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
    }

    public class UserClaimModel
    {
        public int Id { get; set; }
        public string Chapa { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

}
