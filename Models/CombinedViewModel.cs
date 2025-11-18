using X.PagedList.Mvc.Core;
using X.PagedList;
using FerramentariaTest.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using FerramentariaTest.Helpers;

namespace FerramentariaTest.Models
{
    #region Gestor Combined Model
    public class CombinedViewModel
    {
        public FormEstoqueSaidaViewModel? FormEstoqueSaidaViewModel { get; set; }
        public CatalogoLocalViewModel? CatalogoLocalViewModel { get; set; }
        public List<VW_Usuario_NewViewModel>? VW_Usuario_NewViewModel { get; set; }
        public List<VW_Usuario_NewViewModel>? VW_Usuario_NewViewModelRelacionado { get; set; }
    }
    #endregion

    #region Inativado - Reinclusao Item Combined Model
    public class CombinedInativoAtivo
    {
        public IPagedList<VW_Reativacao_ItemViewModel>? VW_Reativacao_ItemViewModel { get; set; }
        public VW_Reactivacao_SearchViewModel? VW_Reactivacao_SearchViewModel { get; set; }
        //public VW_Usuario_NewViewModel? LoggedUserDetails { get; set; }
        public PermissionAccessModel? LoggedUserDetails { get; set; }
        public int? ResultCount { get; set; }
        //public int? UserEditPermission { get; set; }
    }

    public class CombinedInativoAtivoList
    {
        public List<VW_Reativacao_ItemViewModel>? VW_Reativacao_ItemViewModelList { get; set; }
        public VW_Reactivacao_SearchViewModel? VW_Reactivacao_SearchViewModel { get; set; }
        //public VW_Usuario_NewViewModel? LoggedUserDetails { get; set; }
        public PermissionAccessModel? LoggedUserDetails { get; set; }
        public int? ResultCount { get; set; }
        public int? PageNumber { get; set; }
        //public int? UserEditPermission { get; set; }
    }
    #endregion

    #region Extraviado - Reinclusao Item Combined Model
    public class CombinedExtraviado
    {
        public IPagedList<VW_Reativacao_Item_ExtraviadoViewModel>? VW_Reativacao_Item_ExtraviadoViewModel { get; set; }
        public VW_Reativacao_Item_ExtraviadoSearchModel? VW_Reativacao_Item_ExtraviadoSearchModel { get; set; }

        public int? UserEditPermission { get; set; }
    }
    #endregion

    #region Catalogo Combined Model
    public class CombinedCatalogo
    {
        public IPagedList<CatalogoViewModel>? CatalogoViewModel { get; set; }
        public CatalogoSearchModel? CatalogoSearchModel { get; set; }
        public int? ResultCount { get; set; }
    }
    #endregion

    #region Devolucao Combined Model
    public class CombinedDevolucao
    {
        public IPagedList<DevolucaoViewModel?>? DevolucaoViewModel { get; set; }
        public List<PassedDevolucaoModel?>? PassedDevolucao { get; set; } = new List<PassedDevolucaoModel>();
        public UserViewModel? UserViewModel { get; set; }
        public SearchDevolucaoViewModel? SearchDevolucaoViewModel { get; set; }
        public int? IdFerramentariaUser { get; set; }
        public int? ResultCount { get; set; }
    }

	#endregion

	#region Historico Combined Model
	public class CombinedHistorico
	{
		public IPagedList<HistoricoViewModel>? HistoricoViewModel { get; set; }
		public UserViewModel? UserViewModel { get; set; }
		public SearchDevolucaoViewModel? SearchDevolucaoViewModel { get; set; }
	}
    #endregion

    #region Emprestimo Combined Model
    public class CombinedEmprestimo
    {
        //public IPagedList<HistoricoViewModel>? HistoricoViewModel { get; set; }
        public UserViewModel? SolicitanteModel { get; set; }
        public UserViewModel? LiberadorModel { get; set; }
        public SearchDevolucaoViewModel? SearchDevolucaoViewModel { get; set; }
    }
    #endregion

    #region Relatorio Combined Model
    public class CombinedRelatorio
    {
        public IPagedList<RelatorioViewModel>? RelatorioViewModel { get; set; }
        public RelatorioSearch? RelatorioSearch { get; set; }

    }
    #endregion

    #region Gestor Combined Model
    public class CombinedGestor
    {
        public IPagedList<SP_1600012731_EstoqueViewModel>? SP_1600012731_EstoqueViewModel { get; set; }
        public SearchGestorModel? SearchGestorModel { get; set; }
        //public VW_Usuario_NewViewModel? LoggedUserDetails { get; set; }
        public string? NomeFerramentaria { get; set; }
        public int? ResultCount { get; set; }

    }

    public class CombinedGestorList
    {
        public List<SP_1600012731_EstoqueViewModel>? SP_1600012731_EstoqueViewModelList { get; set; }
        public SearchGestorModel? SearchGestorModel { get; set; }
        //public VW_Usuario_NewViewModel? LoggedUserDetails { get; set; }
        public string? NomeFerramentaria { get; set; }
        public int? ResultCount { get; set; }
        public int? PageNumber { get; set; }
        public int? Pagination { get; set; }

    }
    #endregion


    #region Gestor Combined Model
    [Serializable]
    public class CombinedEmLote
    {
        [JsonIgnore]
        public IPagedList<EntradaEmLote_ReqViewModel>? EntradaEmLote_ReqViewModel { get; set; }
        public EntradaEmLoteSearch? EntradaEmLoteSearch { get; set; }
        public int? ResultCount { get; set; }

        public PagedListWrapper<EntradaEmLote_ReqViewModel>? EntradaEmLote_ReqViewModelWrapper
        {
            get => EntradaEmLote_ReqViewModel?.ToPagedListWrapper();
            set => EntradaEmLote_ReqViewModel = value?.ToPagedList();
        }

    }

    public class CombinedEmLoteUsingList
    {
        public List<EntradaEmLote_ReqViewModel>? EntradaEmLote_ReqViewModelList { get; set; }
        public EntradaEmLoteSearch? EntradaEmLoteSearch { get; set; }
        public int? ResultCount { get; set; }
        public int? PageNumber { get; set; }
        public int? Pagination { get; set; }

    }

    [Serializable]
    public class PagedListWrapper<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemCount { get; set; }
    }
    #endregion



}
