using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Helpers;
using FerramentariaTest.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FerramentariaTest.Helpers
{

    public class Edits
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;

        public Edits(ContextoBanco context, ContextoBancoBS contextBS, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            httpContextAccessor = httpContext;
            _configuration = configuration;
        }

        //FROM ToEdit Gestor
        public Produto EditProduto(GestorEdit GestorEditValues)
        {
            Produto ProdutoToEdit = _context.Produto.Where(t => t.Id == GestorEditValues.IdProduto).FirstOrDefault();
            if (ProdutoToEdit != null) 
            {
                if (GestorEditValues.Selo != null)
                {
                    ProdutoToEdit.Selo = GestorEditValues.Selo;
                }
                if (GestorEditValues.DataAquisicao != null)
                {
                    ProdutoToEdit.DC_DataAquisicao = GestorEditValues.DataAquisicao;
                }
                if (GestorEditValues.Valor != null)
                {
                    ProdutoToEdit.DC_Valor = GestorEditValues.Valor;
                }
                if (GestorEditValues.AssetNumber != null)
                {
                    ProdutoToEdit.DC_AssetNumber = GestorEditValues.AssetNumber;
                }
                if (GestorEditValues.Fornecedor != null)
                {
                    ProdutoToEdit.DC_Fornecedor = GestorEditValues.Fornecedor;
                }
                if (GestorEditValues.Fornecedor != null)
                {
                    ProdutoToEdit.DC_Fornecedor = GestorEditValues.Fornecedor;
                }
                if (GestorEditValues.Contrato != null)
                {
                    ProdutoToEdit.GC_Contrato = GestorEditValues.Contrato;
                }
                if (GestorEditValues.DataInicio != null)
                {
                    ProdutoToEdit.GC_DataInicio = GestorEditValues.DataInicio;
                }
                if (GestorEditValues.Obra != null)
                {
                    ProdutoToEdit.GC_IdObra = GestorEditValues.Obra;
                }
                if (GestorEditValues.OC != null)
                {
                    ProdutoToEdit.GC_OC = GestorEditValues.OC;
                }
                if (GestorEditValues.DataSaida != null)
                {
                    ProdutoToEdit.GC_DataSaida = GestorEditValues.DataSaida;
                }
                if (GestorEditValues.NFSaida != null)
                {
                    ProdutoToEdit.GC_NFSaida = GestorEditValues.NFSaida;
                }
                if (GestorEditValues.AfSerial != null)
                {
                    ProdutoToEdit.AF = GestorEditValues.AfSerial;
                }
                if (GestorEditValues.PAT != null)
                {
                    ProdutoToEdit.PAT = GestorEditValues.PAT;
                }
                if (GestorEditValues.Empresa != null)
                {
                    ProdutoToEdit.IdEmpresa = GestorEditValues.Empresa;
                }
                if (GestorEditValues.Observacao != null)
                {
                    ProdutoToEdit.Observacao = GestorEditValues.Observacao;
                }
                if (GestorEditValues.DatadeVencimento != null)
                {
                    ProdutoToEdit.DataVencimento = GestorEditValues.DatadeVencimento;
                }
                if (GestorEditValues.Certificado != null)
                {
                    ProdutoToEdit.Certificado = GestorEditValues.Certificado;
                }
                if (GestorEditValues.Serie != null)
                {
                    ProdutoToEdit.Serie = GestorEditValues.Serie;
                }
                if (GestorEditValues.QuantidadeMinima != null)
                {
                    ProdutoToEdit.QuantidadeMinima = GestorEditValues.QuantidadeMinima;
                }

                _context.SaveChangesAsync();
            }

            return ProdutoToEdit;
        }



    }
}
