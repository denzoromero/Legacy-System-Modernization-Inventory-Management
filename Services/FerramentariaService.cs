using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.Helpers;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static NuGet.Packaging.PackagingConstants;
using static System.Net.WebRequestMethods;

namespace FerramentariaTest.Services
{
    public class FerramentariaService : IFerramentariaService
    {
        private readonly ContextoBanco _context;
        private readonly ILogger<FerramentariaService> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public FerramentariaService(ContextoBanco context, ILogger<FerramentariaService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        public List<Ferramentaria>? GetFerramentariaList(int? UserId)
        {
            try
            {
                if (UserId == null || UserId == 0) throw new ProcessErrorException("UserId is null or 0", nameof(UserId));

                List<Ferramentaria>? ferramentariaItems = (from ferramentaria in _context.Ferramentaria
                                          where ferramentaria.Ativo == 1 &&
                                                !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                _context.FerramentariaVsLiberador.Any(l => l.IdLogin == UserId && l.IdFerramentaria == ferramentaria.Id)
                                          orderby ferramentaria.Nome
                                          select new Ferramentaria
                                          {
                                              Id = ferramentaria.Id,
                                              Nome = ferramentaria.Nome
                                          }).ToList() ?? new List<Ferramentaria>();

                return ferramentariaItems;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Service Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public int? GetChosenFerramentariaValue() => _httpContextAccessor.HttpContext?.Session.GetInt32("Ferramentaria");

        public string? GetChosenFerramentariaName() => _httpContextAccessor.HttpContext?.Session.GetString("FerramentariaNome");

        //public void SetFerramentariaValue(int Ferramentaria) => _httpContextAccessor.HttpContext?.Session.SetInt32("Ferramentaria", (int)Ferramentaria);
        public async Task SetFerramentariaValue(int Ferramentaria)
        {
            try 
            {

                if (Ferramentaria == 0) throw new ProcessErrorException("Ferramentaria is 0");

                Ferramentaria? ferrmentaria = await _context.Ferramentaria.FindAsync(Ferramentaria);
                if (ferrmentaria == null) throw new ProcessErrorException($"No Ferramentaria Found with Id{Ferramentaria}");

                _httpContextAccessor.HttpContext?.Session.SetInt32("Ferramentaria", (int)ferrmentaria.Id);
                _httpContextAccessor.HttpContext?.Session.SetString("FerramentariaNome", ferrmentaria.Nome);

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Service Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public void RefreshChosenFerramentaria()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("Ferramentaria");
            _httpContextAccessor.HttpContext?.Session.Remove("FerramentariaNome");
        }

        public async Task<List<FerramentariaStockModel>?> GetAvailableFerramentaria(int IdCatalogo, int IdFerramentaria)
        {
            try
            {
                _logger.LogInformation("Processing GetAvailableFerramentaria IdCatalogo:{IdCatalogo}, IdFerramentaria:{IdFerramentaria}", IdCatalogo, IdFerramentaria);

                if (IdCatalogo == 0) throw new ProcessErrorException("IdCatalogo is 0, invalid for query");
                if (IdFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0, invalid for query");

                List<FerramentariaStockModel>? listFerramentaria = await (from produto in _context.Produto
                                                                    join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                                    where produto.IdFerramentaria != IdFerramentaria
                                                                    && produto.IdCatalogo == IdCatalogo
                                                                    && produto.Ativo == 1
                                                                    && produto.Quantidade > 0
                                                                    && produto.IdFerramentaria != 17
                                                                    select new FerramentariaStockModel
                                                                    {
                                                                        Id = produto.IdFerramentaria,
                                                                        Nome = ferramentaria.Nome
                                                                    }).ToListAsync();

                return listFerramentaria;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Service Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<string?> GetFerramentariaName(int IdFerramentaria)
        {
            try
            {
                if (IdFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0, invalid for query");
                return await _context.Ferramentaria.Where(i => i.Id == IdFerramentaria).Select(i => i.Nome).FirstOrDefaultAsync();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Arguments Service Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Service Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

    }


    public class CatalogService : ICatalogService
    {
        private readonly ContextoBanco _context;
        private readonly ILogger<CatalogService> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public CatalogService(ContextoBanco context, ILogger<CatalogService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CatalogoViewModel>?> SearchCatalog(CatalogoSearchModel filter)
        {
            try
            {

                _logger.LogInformation("Processing SearchCatalog");

                List<CatalogoViewModel>? Result = await (from catalogo in _context.Catalogo
                                                       join categoria in _context.Categoria
                                                       on catalogo.IdCategoria equals categoria.Id
                                                       where catalogo.Ativo == 1
                                                        && categoria.Ativo == 1
                                                        && (filter.CategoriaClasse == null || categoria.Classe == filter.CategoriaClasse)
                                                        && (filter.Id == null || categoria.Id == filter.Id)
                                                        && (filter.IdCategoria == null || categoria.IdCategoria == filter.IdCategoria)
                                                        && (filter.Codigo == null || catalogo.Codigo.Contains(filter.Codigo))
                                                        && (filter.Descricao == null || catalogo.Nome.Contains(filter.Descricao))
                                                        orderby catalogo.Nome
                                                        select new CatalogoViewModel
                                                        {
                                                           Id = catalogo.Id,
                                                           Codigo = catalogo.Codigo,
                                                           Nome = catalogo.Nome,
                                                           Descricao = catalogo.Descricao,
                                                           PorMetro = catalogo.PorMetro,
                                                           PorAferido = catalogo.PorAferido,
                                                           PorSerial = catalogo.PorSerial,
                                                           RestricaoEmprestimo = catalogo.RestricaoEmprestimo,
                                                           ImpedirDescarte = catalogo.ImpedirDescarte,
                                                           HabilitarDescarteEpi = catalogo.HabilitarDescarteEPI,
                                                           DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                           DataRegistro = catalogo.DataRegistro,
                                                           Ativo = catalogo.Ativo,
                                                           IdCategoria = categoria.Id,
                                                           IdCategoriaPai = categoria.IdCategoria,
                                                           CategoriaNome = categoria.Nome,
                                                           CategoriaClasse = categoria.Classe,
                                                           CategoriaNomePai = _context.Categoria
                                                               .Where(cat => cat.Id == categoria.IdCategoria)
                                                               .Select(cat => cat.Nome)
                                                               .FirstOrDefault(),
                                                           CategoriaDataRegistro = categoria.DataRegistro,
                                                           CategoriaAtivo = categoria.Ativo,
                                                           ImageByteString = catalogo.ImageData != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(catalogo.ImageData)}" : string.Empty
                                                        }).ToListAsync();

                if (Result == null || Result.Count == 0) throw new ModifiedErrorException("No result found.");

                return Result;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task UpdateCatalogImage(byte[] img, int IdCatalogo)
        {
            try
            {
                if (IdCatalogo <= 0) throw new ProcessErrorException("IdCatalogo is less than or equal to 0");
                if (img == null) throw new ProcessErrorException("Image byte array is null");
                if (img.Length == 0) throw new ProcessErrorException("Image byte array is empty");

                Catalogo? catalog = await _context.Catalogo.FindAsync(IdCatalogo);
                if (catalog == null) throw new ProcessErrorException($"Catalogo with Id:{IdCatalogo} not found.");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {
                        catalog.ImageData = img;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

        public async Task<string> GetImageString(int idCatalogo)
        {
            try
            {

                byte[]? img = await _context.Catalogo.Where(i => i.Id == idCatalogo).Select(i => i.ImageData).FirstOrDefaultAsync();
                if (img == null) return string.Empty;

                return $"data:image/jpeg;base64,{Convert.ToBase64String(img)}";

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException Client Error.");
                throw;
            }
            catch (ModifiedErrorException ex)
            {
                _logger.LogWarning(ex, "ModifiedErrorException Client Error.");
                throw;
            }
            catch (ProcessErrorException ex)
            {
                _logger.LogWarning(ex, "Processing Service Error.");
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Server Unavailable.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database timeout occurred.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Operation timed out.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error Occured.");
                throw;
            }
        }

    }


}
