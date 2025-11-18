using FerramentariaTest.Entities;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IFerramentariaService
    {
        List<Ferramentaria>? GetFerramentariaList(int? UserId);
        int? GetChosenFerramentariaValue();
        string? GetChosenFerramentariaName();
        Task SetFerramentariaValue(int Ferramentaria);
        void RefreshChosenFerramentaria();
        Task<List<FerramentariaStockModel>?> GetAvailableFerramentaria(int IdCatalogo, int IdFerramentaria);
        Task<string?> GetFerramentariaName(int IdFerramentaria);
    }

    public interface ICatalogService
    {
        Task<List<CatalogoViewModel>?> SearchCatalog(CatalogoSearchModel filter);
        Task UpdateCatalogImage(byte[] img, int IdCatalogo);
        Task<string> GetImageString(int idCatalogo);
    }


}
