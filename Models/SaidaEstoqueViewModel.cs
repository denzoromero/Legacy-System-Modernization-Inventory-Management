using FerramentariaTest.Entities;

namespace FerramentariaTest.Models
{
    public class SaidaEstoqueViewModel
    {
        public int? CatalogoId { get; set; }
        public string? CatalogoCodigo { get; set; }
        public string? CatalogoNome { get; set; }
        public string? CatalogoDescricao { get; set; }
        public int? CatalogoPorMetro { get; set; }
        public int? CatalogoPorAferido { get; set; }
        public int? CatalogoPorSerial { get; set; }
        public int? CatalogoRestricaoEmprestimo { get; set; }
        public int? CatalogoImpedirDescarte { get; set; }
        public int? CatalogoHabilitarDescarteEpi { get; set; }
        public int? CatalogoDataDeRetornoAutomatico { get; set; }
        public DateTime? CatalogoDataRegistro { get; set; }
        public int? CatalogoAtivo { get; set; }
        public int? CategoriaId { get; set; }
        public int? IdCategoriaPai { get; set; }
        public int? CategoriaClasse { get; set; }
        public string? CategoriaNome { get; set; }
        public string? ClasseNome { get; set; }
        public DateTime? CategoriaDataRegistro { get; set; }
        public int? CategoriaAtivo { get; set; }
    }
}
