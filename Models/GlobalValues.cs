namespace FerramentariaTest.Models
{
    public class GlobalValues
    {
        public static List<ObraViewModel> ListObraViewModel { get; set; } = new List<ObraViewModel>();
        public static List<MarcaViewModel> ListMarcaViewModel { get; set; } = new List<MarcaViewModel>();
        public static List<FerramentariaViewModel> ListFerramentariaViewModel { get; set; } = new List<FerramentariaViewModel>();
        public static List<VW_Reativacao_ItemViewModel> ListVW_Reativacao_ItemViewModel { get; set; } = new List<VW_Reativacao_ItemViewModel>();
        public static List<VW_Reativacao_Item_ExtraviadoViewModel> VW_Reativacao_Item_ExtraviadoViewModel { get; set; } = new List<VW_Reativacao_Item_ExtraviadoViewModel>();
        public static List<CatalogoViewModel> CatalogoViewModel { get; set; } = new List<CatalogoViewModel>();
        public static List<DevolucaoViewModel> DevolucaoViewModel { get; set; } = new List<DevolucaoViewModel>();
        public static List<RelatorioViewModel> RelatorioViewModel { get; set; } = new List<RelatorioViewModel>();
        public static List<EntradaEmLote_ReqViewModel> EntradaEmLote_ReqViewModel { get; set; } = new List<EntradaEmLote_ReqViewModel>();
        public static List<LogProdutoViewModel> LogProdutoViewModel { get; set; } = new List<LogProdutoViewModel>();
        public static List<HistoricoTransferenciaViewModel> HistoricoTransferenciaViewModel { get; set; } = new List<HistoricoTransferenciaViewModel>();
        public static List<ProdutoList> ProdutoList { get; set; } = new List<ProdutoList>();
        public static void ClearList<T>(List<T> list)
        {
            if (list != null)
            {
                list.Clear();
            }
        }

    }
}
