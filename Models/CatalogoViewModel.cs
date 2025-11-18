using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class CatalogoViewModel
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? RestricaoEmprestimo { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEpi { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CategoriaNomePai { get; set; }
        public DateTime? CategoriaDataRegistro { get; set; }
        public int? CategoriaAtivo { get; set; }
        public string? SelectedCatalogo { get; set; } // Add this property
        public string? SelectedClasse { get; set; } // Add this property
        public int? Saldo { get; set; }
        public int? Uploaded { get; set; }
        public string? FilePath { get; set; }
        public int? QuantityFromComp { get; set; }
        public string? ObservacaoFromComp { get; set; }
        public string? ImageByteString { get; set; }
    }

    public class CatalogoSearchModel
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? RestricaoEmprestimo { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEpi { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? Ativo { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }
        public string? CategoriaNome { get; set; }
        public string? CategoriaNomePai { get; set; }
        public DateTime? CategoriaDataRegistro { get; set; }
        public int? CategoriaAtivo { get; set; }
        public string? SelectedCatalogo { get; set; } // Add this property
        public string? SelectedClasse { get; set; } // Add this property
        public int? Pagination { get; set; }
        public int? Pagenumber { get; set; }
    }

    public class CatalogoCreateModel
    {
        public int? Id { get; set; }
        public int? SelectedCatalogo { get; set; }
        public int? SelectedClasse { get; set; }
        public int? SelectedTipo { get; set; }
        public int? Controle { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? PorQuantidade { get; set; }
        public int? RestricaoEmprestimo { get; set; }
        public int? ImpedirDescarte { get; set; }
        public int? HabilitarDescarteEpi { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public int? chkDataDeRetornoAutomatico { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? Ferramentaria { get; set; }

        public int? txtPos1Prateleira { get; set; }
        public int? txtPo2Prateleira { get; set; }
        public int? txtPos3Prateleira { get; set; }

        public int? txtPos1Coluna { get; set; }
        public int? txtPos2Coluna { get; set; }
        public int? txtPos3Coluna { get; set; }

        public int? txtPos1Linha { get; set; }
        public int? txtPos2Linha { get; set; }
        public int? txtPos3Linha { get; set; }

        public int? txtPos4Prateleira { get; set; }
        public int? txtPo5Prateleira { get; set; }
        public int? txtPos6Prateleira { get; set; }

        public int? txtPos4Coluna { get; set; }
        public int? txtPos5Coluna { get; set; }
        public int? txtPos6Coluna { get; set; }

        public int? txtPos4Linha { get; set; }
        public int? txtPos5Linha { get; set; }
        public int? txtPos6Linha { get; set; }



    }

    public class SaveModel
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? PorMetro { get; set; }
        public int? PorAferido { get; set; }
        public int? PorSerial { get; set; }
        public int? PorQuantidade { get; set; }
        public int? CategoriaClasse { get; set; }
        public int? Saldo { get; set; }
        public int? Quantidade { get; set; }
        public string? Observacao { get; set; }
    }

}
