using System.ComponentModel.DataAnnotations.Schema;

namespace FerramentariaTest.Entities
{
    public class VW_Historico
    {
        public int? IdProduto { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        [Column("Código")]
        public string? Codigo { get; set; }
        public int? Quantidade { get; set; }
        public string? Produto { get; set; }
        public string? CA { get; set; }
        public string? NumeroCA { get; set; }
        public string? ValidadeCA { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        [Column("Observação")]
        public string? Observacao { get; set; }
        [Column("Setor de Origem")]
        public string? SetorOrigem { get; set; }

        [Column("Status do Solicitante")]
        public string? StatusSolicitante { get; set; }

        [Column("Chapa do Solicitante")]
        public string? ChapaSolicitante { get; set; }

        [Column("Nome do Solicitante")]
        public string? NomeSolicitante { get; set; }

        [Column("Função do Solicitante")]
        public string? FuncaoSolicitante { get; set; }

        [Column("Seção do Solicitante")]
        public string? SecaoSolicitante { get; set; }
        //public string? Solicitante_Chapa { get; set; }

        [Column("Chapa do Liberador")]
        public string? ChapaLiberador { get; set; }

        [Column("Nome do Liberador")]
        public string? NomeLiberador { get; set; }
        //public string? Liberador_Chapa { get; set; }
        //public int? Balconista_Emprestimo_IdLogin { get; set; }
        //public int? Balconista_Devolucao_IdLogin { get; set; }

        [Column("Setor do Empréstimo")]
        public string? SetorEmprestimo { get; set; }

        [Column("Balconista do Empréstimo")]
        public string? BalconistaEmprestimo { get; set; }

        public string? Obra { get; set; }

        [Column("Data do Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Data Prevista para Devolução")]
        public DateTime? DataPrevistaDevolucao { get; set; }

        [Column("Data de Vencimento do Produto")]
        public DateTime? DataVencimentoProduto { get; set; }

        [Column("Setor da Devolucao")]
        public string? SetorDevolucao { get; set; }

        [Column("Balconista da Devolucao")]
        public string? BalconistaDevolucao { get; set; }

        [Column("Data de Devolução")]
        public DateTime? DataDevolucao { get; set; }

        [Column("Status Atual")]
        public string? StatusAtual { get; set; }

    }

    public class VW_HistoricoWithoutFuncionario
    {
        public int? IdProduto { get; set; }
        public string? Catalogo { get; set; }
        public string? Classe { get; set; }
        public string? Tipo { get; set; }
        [Column("Código")]
        public string? Codigo { get; set; }
        public int? Quantidade { get; set; }
        public string? Produto { get; set; }
        public string? CA { get; set; }
        public string? NumeroCA { get; set; }
        public string? ValidadeCA { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        [Column("Observação")]
        public string? Observacao { get; set; }
        [Column("Setor de Origem")]
        public string? SetorOrigem { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Liberador_Chapa { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        public int? Balconista_Emprestimo_IdLogin { get; set; }
        public int? Balconista_Devolucao_IdLogin { get; set; }

        [Column("Setor do Empréstimo")]
        public string? SetorEmprestimo { get; set; }

        public string? Obra { get; set; }

        [Column("Data do Empréstimo")]
        public DateTime? DataEmprestimo { get; set; }

        [Column("Data Prevista para Devolução")]
        public DateTime? DataPrevistaDevolucao { get; set; }

        [Column("Data de Vencimento do Produto")]
        public DateTime? DataVencimentoProduto { get; set; }

        [Column("Setor da Devolucao")]
        public string? SetorDevolucao { get; set; }

        [Column("Data de Devolução")]
        public DateTime? DataDevolucao { get; set; }


        [Column("Status Atual")]
        public string? StatusAtual { get; set; }

    }

    //public class VW_Historico2000ate2005 : VW_Historico
    //{

    //}

    //public class VW_Historico2006ate2010 : VW_Historico
    //{

    //}

    //public class VW_Historico2011ate2015 : VW_Historico
    //{

    //}

    //public class VW_Historico2016ate2020 : VW_Historico
    //{

    //}

    //public class VW_Historico2021ate2024 : VW_Historico
    //{

    //}

}
