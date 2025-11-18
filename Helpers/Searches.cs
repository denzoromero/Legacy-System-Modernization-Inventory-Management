using AutoMapper;
using AutoMapper.QueryableExtensions;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitySeek;
using FerramentariaTest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerramentariaTest.Helpers
{
    public class Searches
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ContextoBancoSeek _contextSeek;
        protected IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private MapperConfiguration mapeamentoClasses;

        public Searches(ContextoBanco context, ContextoBancoBS contextBS, ContextoBancoRM contextRM, ContextoBancoSeek contextSeek, IHttpContextAccessor httpCA, IConfiguration configuration)
        {
            _context = context;
            _contextBS = contextBS;
            _contextRM = contextRM;
            _contextSeek = contextSeek;
            httpContextAccessor = httpCA;
            _configuration = configuration;
            mapeamentoClasses = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Ferramentaria, FerramentariaViewModel>();
                cfg.CreateMap<FerramentariaViewModel, Ferramentaria>();
                cfg.CreateMap<EntradaEmLote_Req, EntradaEmLote_ReqViewModel>();
                cfg.CreateMap<EntradaEmLote_ReqViewModel, EntradaEmLote_Req>();
                cfg.CreateMap<Funcionario, FuncionarioViewModel>();
                cfg.CreateMap<FuncionarioViewModel, Funcionario>();
                cfg.CreateMap<MensagemSolicitante, MensagemSolicitanteViewModel>();
                cfg.CreateMap<MensagemSolicitanteViewModel, MensagemSolicitante>();
                cfg.CreateMap<VW_Reativacao_Item, VW_Reativacao_ItemViewModel>();
                cfg.CreateMap<VW_Reativacao_ItemViewModel, VW_Reativacao_Item>();
                cfg.CreateMap<EntradaEmLote_Comp, EntradaEmLote_CompViewModel>();
                cfg.CreateMap<EntradaEmLote_CompViewModel, EntradaEmLote_Comp>();
            });
        }

        #region Catalogo
        public List<CatalogoViewModel>? SearchCatalogo(CatalogoSearchModel? filters)
        {
            List<CatalogoViewModel>? Result = (from catalogo in _context.Catalogo
                                                join categoria in _context.Categoria
                                                on catalogo.IdCategoria equals categoria.Id
                                                where catalogo.Ativo == 1
                                                && categoria.Ativo == 1
                                                && (filters.CategoriaClasse == null || categoria.Classe == filters.CategoriaClasse)
                                                && (filters.Id == null || categoria.Id == filters.Id)
                                                && (filters.IdCategoria == null || categoria.IdCategoria == filters.IdCategoria)
                                                && (filters.Codigo == null || catalogo.Codigo.Contains(filters.Codigo))
                                                && (filters.Descricao == null || catalogo.Nome.Contains(filters.Descricao))
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
                                                    CategoriaAtivo = categoria.Ativo
                                                }).ToList();

            return Result;
        }

        public CatalogoViewModel? GetCatalogo(int? Id)
        {
            CatalogoViewModel? catalogoValue = (from catalogo in _context.Catalogo
                                           join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                           join c in _context.Categoria on categoria.IdCategoria equals c.Id into parentCategory
                                           from pc in parentCategory.DefaultIfEmpty()
                                           where catalogo.Ativo == 1 && catalogo.Id == Id
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
                                               Ativo = catalogo.HabilitarDescarteEPI,
                                               IdCategoria = categoria.Id,
                                               IdCategoriaPai = categoria.IdCategoria,
                                               CategoriaNome = categoria.Nome,
                                               CategoriaClasse = categoria.Classe,
                                               CategoriaNomePai = pc.Nome,
                                               CategoriaDataRegistro = categoria.DataRegistro,
                                               CategoriaAtivo = categoria.Ativo
                                           }).FirstOrDefault();

            return catalogoValue;
        }

        #endregion

        #region ControleCA
        public List<CatalogoViewModel>? SearchControleCA(string? CodigoCA, string? ItemCA, string? NumeroCA)
        {
            List<CatalogoViewModel>? Result = (from catalogo in _context.Catalogo
                                                join categoria in _context.Categoria
                                                on new { Id = catalogo.IdCategoria } equals new { Id = categoria.Id }
                                                where (categoria.Classe == 2 || categoria.Classe == 3)
                                                    && categoria.Ativo == 1
                                                    && catalogo.Ativo == 1
                                                    && (string.IsNullOrEmpty(CodigoCA) || catalogo.Codigo.Contains(CodigoCA))
                                                    && (string.IsNullOrEmpty(ItemCA) || catalogo.Nome.Contains(ItemCA))
                                                    && (string.IsNullOrEmpty(NumeroCA) || _context.ControleCA.Any(cc => cc.IdCatalogo == catalogo.Id && cc.NumeroCA.Contains(NumeroCA)))
                                                orderby catalogo.Nome ascending
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
                                                    Ativo = catalogo.HabilitarDescarteEPI,
                                                    IdCategoria = categoria.Id,
                                                    IdCategoriaPai = categoria.IdCategoria,
                                                    CategoriaNome = categoria.Nome,
                                                    CategoriaClasse = categoria.Classe,
                                                    CategoriaNomePai = _context.Categoria.Where(cat => cat.Id == categoria.IdCategoria).Select(cat => cat.Nome).FirstOrDefault(),
                                                    CategoriaDataRegistro = categoria.DataRegistro,
                                                    CategoriaAtivo = categoria.Ativo
                                                }).ToList();


            return Result;
        }
        #endregion

        #region Reinclusao Item
        public List<VW_Reativacao_ItemViewModel>? SearchVWInativo(VW_Reactivacao_SearchViewModel? filters)
        {
            List<VW_Reativacao_ItemViewModel>? Result = _context.VW_Reativacao_Item
                                                        .Where(r =>
                                                            (string.IsNullOrEmpty(filters.Codigo) || r.Codigo.Contains(filters.Codigo.Trim()))
                                                            && (string.IsNullOrEmpty(filters.Descricao) || r.Descricao.Contains(filters.Descricao))
                                                            && (string.IsNullOrEmpty(filters.AF) || r.AF.Contains(filters.AF))
                                                            && (filters.PAT == null || r.PAT == filters.PAT)
                                                            && (string.IsNullOrEmpty(filters.Controle) || r.Controle.Contains(filters.Controle))
                                                            && (string.IsNullOrEmpty(filters.LocalEmEstoque) || r.LocalEmEstoque.Contains(filters.LocalEmEstoque))
                                                            && (string.IsNullOrEmpty(filters.Motivo) || r.Motivo.Contains(filters.Motivo))
                                                            && (string.IsNullOrEmpty(filters.Justificativa) || r.Justificativa.Contains(filters.Justificativa))
                                                            && (string.IsNullOrEmpty(filters.Usuario) || r.Usuario.Contains(filters.Usuario))
                                                            && (!filters.De.HasValue || r.DataInativacao >= filters.De.Value.Date)
                                                            && (!filters.Ate.HasValue || r.DataInativacao <= filters.Ate.Value.AddDays(1).AddTicks(-1))
                                                            ).ProjectTo<VW_Reativacao_ItemViewModel>(mapeamentoClasses)
                                                            .ToList();

            return Result;
        }
        #endregion

        #region Ferramentaria
        public List<FerramentariaViewModel> SearchFerramentariaBalconista(int? UsuarioId)
        {
            List<FerramentariaViewModel> ListOfFerramentariaBalconista = new List<FerramentariaViewModel>();

            var ferramentarias = (from ferramentaria in _context.Ferramentaria
                                             where ferramentaria.Ativo == 1 &&
                                                   !_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                   _context.FerramentariaVsLiberador.Any(l => l.IdLogin == UsuarioId && l.IdFerramentaria == ferramentaria.Id)
                                             orderby ferramentaria.Nome
                                             select new FerramentariaViewModel
                                             {
                                                 Id = ferramentaria.Id,
                                                 Nome = ferramentaria.Nome
                                             }).ToList();

            var mapper = mapeamentoClasses.CreateMapper();

            ListOfFerramentariaBalconista = mapper.Map<List<FerramentariaViewModel>>(ferramentarias);

            return ListOfFerramentariaBalconista;
        }
        #endregion

        #region EntradaEmLote
        public List<EntradaEmLote_ReqViewModel?>? SearchEntradaEmLote(EntradaEmLoteSearch? EntradaEmLoteSearchValues)
        {
            List<EntradaEmLote_ReqViewModel?>? ListEntradaEmLote = new List<EntradaEmLote_ReqViewModel>();

            var result = from t in _context.EntradaEmLote_Req
                         where t.IdFerramentaria == EntradaEmLoteSearchValues.IdFerramentaria
                            && (EntradaEmLoteSearchValues.RFM == null || t.RFM.Contains(EntradaEmLoteSearchValues.RFM))
                            && (EntradaEmLoteSearchValues.Status !=  2 || !_context.EntradaEmLote_Temp
                                       .Where(e => e.IdRequisicao == t.Id && e.Observacao.Contains("ERRO"))
                                       .Any())
                            && (EntradaEmLoteSearchValues.Status == 8 || t.Status == EntradaEmLoteSearchValues.Status)
                         orderby t.Id descending
                         select new EntradaEmLote_ReqViewModel
                         {
                             Id = t.Id,
                             IdFerramentaria = t.IdFerramentaria,
                             RFM = t.RFM,
                             Status = t.Status,
                             IdSolicitante = t.IdSolicitante,
                             DataRegistro = t.DataRegistro
                         };

            var mapper = mapeamentoClasses.CreateMapper();

            ListEntradaEmLote = mapper.Map<List<EntradaEmLote_ReqViewModel>>(result);

            return ListEntradaEmLote;
        }

        public List<CatalogoViewModel?>? SearchCatalogoForEntradaLote(string? Codigo, string? Item)
        {
            List <CatalogoViewModel?>? Result = (from catalogo in _context.Catalogo
                                                join categoria in _context.Categoria
                                                on catalogo.IdCategoria equals categoria.Id
                                                where catalogo.Ativo == 1
                                                && (string.IsNullOrEmpty(Codigo) || catalogo.Codigo.Contains(Codigo))
                                                && (string.IsNullOrEmpty(Item) || catalogo.Nome.Contains(Item))
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
                                                    CategoriaNomePai = _context.Categoria.Where(cat => cat.Id == categoria.IdCategoria).Select(cat => cat.Nome).FirstOrDefault(),
                                                    CategoriaDataRegistro = categoria.DataRegistro,
                                                    CategoriaAtivo = categoria.Ativo
                                                }).ToList();

            return Result;
        }

        public ProdutoCompleteViewModel? VerifySaldoForEntradaEmLote(int? Id, int? IdFerramentaria)
        {
            ProdutoCompleteViewModel? ResultProduto = (from produto in _context.Produto
                                                       join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                       join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                       join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                       join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaGroup
                                                       from empresa in empresaGroup.DefaultIfEmpty()
                                                       where produto.Ativo == 1
                                                             //&& catalogo.PorMetro == 0
                                                             //&& catalogo.PorAferido == 0
                                                             //&& catalogo.PorSerial == 0
                                                             //&& produto.Quantidade != 0
                                                             && ferramentaria.Id == IdFerramentaria
                                                             && catalogo.Id == Id
                                                       orderby produto.DataRegistro
                                                       select new ProdutoCompleteViewModel
                                                       {
                                                           DC_DataAquisicao = produto.DC_DataAquisicao,
                                                           DC_Valor = produto.DC_Valor,
                                                           DC_AssetNumber = produto.DC_AssetNumber,
                                                           DC_Fornecedor = produto.DC_Fornecedor,
                                                           GC_Contrato = produto.GC_Contrato,
                                                           GC_DataInicio = produto.GC_DataInicio,
                                                           GC_IdObra = produto.GC_IdObra,
                                                           GC_OC = produto.GC_OC,
                                                           GC_DataSaida = produto.GC_DataSaida,
                                                           GC_NFSaida = produto.GC_NFSaida,
                                                           Selo = produto.Selo,
                                                           IdProduto = produto.Id,
                                                           AF = produto.AF,
                                                           PAT = produto.PAT,
                                                           Quantidade = produto.Quantidade,
                                                           QuantidadeMinima = produto.QuantidadeMinima,
                                                           Localizacao = produto.Localizacao,
                                                           RFM = produto.RFM,
                                                           Observacao = produto.Observacao,
                                                           DataRegistroProduto = produto.DataRegistro,
                                                           DataVencimento = produto.DataVencimento,
                                                           Certificado = produto.Certificado,
                                                           Serie = produto.Serie,
                                                           AtivoProduto = produto.Ativo,
                                                           IdCatalogo = catalogo.Id,
                                                           Codigo = catalogo.Codigo,
                                                           NomeCatalogo = catalogo.Nome,
                                                           Descricao = catalogo.Descricao,
                                                           PorMetro = catalogo.PorMetro,
                                                           PorAferido = catalogo.PorAferido,
                                                           PorSerial = catalogo.PorSerial,
                                                           DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                           DataRegistroCatalogo = catalogo.DataRegistro,
                                                           AtivoCatalogo = catalogo.Ativo,
                                                           IdCategoria = categoria.Id,
                                                           IdCategoriaPai = categoria.IdCategoria,
                                                           Classe = categoria.Classe,
                                                           NomeCategoria = categoria.Nome,
                                                           DataRegistroCategoria = categoria.DataRegistro,
                                                           AtivoCategoria = categoria.Ativo,
                                                           IdFerramentaria = ferramentaria.Id,
                                                           NomeFerramentaria = ferramentaria.Nome,
                                                           DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                                           AtivoFerramentaria = ferramentaria.Ativo,
                                                           IdEmpresa = empresa.Id,
                                                           NomeEmpresa = empresa.Nome,
                                                           GerenteEmpresa = empresa.Gerente,
                                                           TelefoneEmpresa = empresa.Telefone,
                                                           DataRegistroEmpresa = empresa.DataRegistro,
                                                           AtivoEmpresa = empresa.Ativo
                                                       }).FirstOrDefault();

            return ResultProduto;
        }

        public List<EntradaEmLote_CompViewModel?>? GetCatalogoIdsFromComp(int? Id)
        {
            List<EntradaEmLote_CompViewModel> CatalogoIds = _context.EntradaEmLote_Comp.Where(i => i.IdRequisicao == Id).ProjectTo<EntradaEmLote_CompViewModel>(mapeamentoClasses).ToList();

            return CatalogoIds;
        }

        public List<CombinedForModal?>? SearchLoteComp(int? Id)
        {
            List<CombinedForModal?>? Result = (from catalogo in _context.Catalogo
                                               join comp in _context.EntradaEmLote_Comp on catalogo.Id equals comp.IdCatalogo
                                               join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                               where categoria.Ativo == 1 && catalogo.Ativo == 1 && comp.IdRequisicao == Id
                                               orderby catalogo.Nome
                                               select new CombinedForModal
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
                                                   CategoriaClasse = categoria.Classe,
                                                   CategoriaNome = categoria.Nome,
                                                   CategoriaDataRegistro = categoria.DataRegistro,
                                                   CategoriaAtivo = categoria.Ativo,
                                                   Quantidade = comp.Quantidade,
                                                   Observacao = comp.Observacao
                                               }).ToList();

            return Result;
        }

        public List<EntradaEmLote_TempModel?>? SearchLoteTemp(int? Id)
        {
            List<EntradaEmLote_TempModel?>? Results = (from entrada in _context.EntradaEmLote_Temp
                                                      join catalogo in _context.Catalogo on entrada.IdCatalogo equals catalogo.Id
                                                      join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                      where entrada.IdRequisicao == Id
                                                      select new EntradaEmLote_TempModel
                                                      {
                                                          Id = entrada.IdRequisicao,
                                                          IdClasse = categoria.Classe,
                                                          PorAferido = catalogo.PorAferido,
                                                          PorSerial = catalogo.PorSerial,
                                                          PorMetro = catalogo.PorMetro,
                                                          Codigo = catalogo.Codigo,
                                                          CatalogNome = catalogo.Nome,
                                                          Quantidade = entrada.Quantidade,
                                                          AF = entrada.AF,
                                                          PAT = entrada.PAT,
                                                          Serie = entrada.Serie,
                                                          Propriedade = entrada.Propriedade,
                                                          DataVencimento = entrada.DataVencimento,
                                                          Certificado = entrada.Certificado,
                                                          DC_DataAquisicao = entrada.DC_DataAquisicao,
                                                          DC_Valor = entrada.DC_Valor,
                                                          DC_Fornecedor = entrada.DC_Fornecedor,
                                                          Observacao = entrada.Observacao
                                                      }).ToList();

            return Results;
        }

        #endregion

        #region Search Employee
        public List<FuncionarioViewModel> SearchEmployeeChapa(string? employee)
        {
            List<FuncionarioViewModel> queryliberador = new List<FuncionarioViewModel>();

            if (employee.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                var query = _contextBS.Funcionario
                                .Where(e => e.Nome.Contains(employee))
                                .GroupBy(f => f.Chapa)
                                .Select(group => group.OrderByDescending(f => f.DataMudanca).FirstOrDefault())
                                .ToList();

                var mapper = mapeamentoClasses.CreateMapper();

                queryliberador = mapper.Map<List<FuncionarioViewModel>>(query);

            }
            else if (employee.All(char.IsDigit))
            {
                var query = _contextBS.Funcionario
                                .Where(e => e.Chapa.Contains(employee))
                                .GroupBy(f => f.Chapa)
                                .Select(group => group.OrderByDescending(f => f.DataMudanca).FirstOrDefault())
                                .ToList();

                var mapper = mapeamentoClasses.CreateMapper();

                queryliberador = mapper.Map<List<FuncionarioViewModel>>(query);

            }
            
            
            return queryliberador;

        }

        public List<FuncionarioViewModel> SearchTercerio(string? employee)
        {
            List<FuncionarioViewModel> queryliberador = new List<FuncionarioViewModel>();

            if (employee.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                queryliberador = (from funcionario in _contextSeek.Funcionario
                                     join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                                     join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                                     where funcionario.Nome.Contains(employee) 
                                     select new FuncionarioViewModel
                                     {
                                         IdTerceiro = funcionario.Id,
                                         Chapa = funcionario.Chapa,
                                         Nome = funcionario.Nome,
                                         CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                         Secao = secao.Nome,
                                         Funcao = funcao.Nome
                                     }).ToList();


            }
            else if (employee.All(char.IsDigit))
            {
                queryliberador = (from funcionario in _contextSeek.Funcionario
                                  join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                                  join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                                  where funcionario.Chapa.Contains(employee)
                                  select new FuncionarioViewModel
                                  {
                                      IdTerceiro = funcionario.Id,
                                      Chapa = funcionario.Chapa,
                                      Nome = funcionario.Nome,
                                      CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                      Secao = secao.Nome,
                                      Funcao = funcao.Nome
                                  }).ToList();
            }


            return queryliberador;
        }

        public UserViewModel SearchEmployeeOnLoad()
        {
            FuncionarioViewModel EmployeeResult = new FuncionarioViewModel();
            UserViewModel UsuarioModel = new UserViewModel();

            string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Funcionario);
      
            EmployeeResult = SearchEmployee(FuncionarioValue);

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;
                }
            }

            return UsuarioModel;

        }

        public UserViewModel GetEmployeeDetails(string? matricula)
        {
            FuncionarioViewModel EmployeeResult = new FuncionarioViewModel();
            UserViewModel UsuarioModel = new UserViewModel();

            string? FuncionarioValue = matricula;

            EmployeeResult = SearchEmployee(FuncionarioValue);

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;
                }
            }
            else
            {
                EmployeeResult = SearchSingleDataTerceiro(matricula);

                if (EmployeeResult != null)
                {
                    UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                    UsuarioModel.Chapa = EmployeeResult.Chapa;
                    UsuarioModel.Nome = EmployeeResult.Nome;
                    UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                    UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                    UsuarioModel.Funcao = EmployeeResult.Funcao;
                    UsuarioModel.Secao = EmployeeResult.Secao;
                    UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                    UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;
                }
            }

            return UsuarioModel;

        }

        public FuncionarioViewModel SearchSingleDataTerceiro(string? employee)
        {
            FuncionarioViewModel queryliberador = new FuncionarioViewModel();

            queryliberador = (from funcionario in _contextSeek.Funcionario
                              join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                              join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                              where funcionario.Chapa == employee
                              select new FuncionarioViewModel
                              {
                                  IdTerceiro = funcionario.Id,
                                  Chapa = funcionario.Chapa,
                                  Nome = funcionario.Nome,
                                  CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                  Secao = secao.Nome,
                                  Funcao = funcao.Nome
                              }).FirstOrDefault();

            return queryliberador;

        }

        public UserViewModel SearchSolicitanteLoad(string? Chapa)
        {
            FuncionarioViewModel EmployeeResult = new FuncionarioViewModel();
            UserViewModel UsuarioModel = new UserViewModel();

            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Solicitante);

            EmployeeResult = SearchEmployee(Chapa);

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;
                }
            }

            return UsuarioModel;

        }

        public UserViewModel SearchLiberadorLoad(string? Chapa)
        {
            FuncionarioViewModel EmployeeResult = new FuncionarioViewModel();
            UserViewModel UsuarioModel = new UserViewModel();

            //string? FuncionarioValue = httpContextAccessor.HttpContext.Session.GetString(Sessao.Liberador);

            EmployeeResult = SearchEmployee(Chapa);

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;
                }
            }

            return UsuarioModel;

        }

        public List<MensagemSolicitanteViewModel> SearchMensagem(string? chapa, int? LoggedUserId)
        {
            List<MensagemSolicitanteViewModel> mensagems = new List<MensagemSolicitanteViewModel>();
      
            var message = _context.MensagemSolicitante.Where(i => i.Chapa == chapa && i.Ativo == 1).ToList();

            if (message.Count > 0)
            {
                var mapper = mapeamentoClasses.CreateMapper();
                mensagems = mapper.Map<List<MensagemSolicitanteViewModel>>(message);

                BloqueioEmprestimoVsLiberador checkUser = _context.BloqueioEmprestimoVsLiberador.FirstOrDefault(i => i.IdLogin == LoggedUserId);

                List<NewUserInformationModel> result = (
                                            from u in _contextBS.Usuario

                                                // Subquery: get most recent Funcionario for each (Chapa, CodColigada)
                                            let funcionarioMaisRecente = (
                                                from f in _contextBS.Funcionario
                                                where f.Chapa == u.Chapa && f.CodColigada == u.CodColigada
                                                orderby f.DataMudanca descending
                                                select f
                                            ).FirstOrDefault()

                                            select new NewUserInformationModel
                                            {
                                                Id = u.Id,
                                                Chapa = u.Chapa,
                                                Nome = funcionarioMaisRecente != null ? funcionarioMaisRecente.Nome : null,
                                            }
                                        ).ToList();


                foreach (var item in mensagems)
                {
                    VW_Usuario? usuarioNomeOld = new VW_Usuario();

                    var usuarioNome = result.FirstOrDefault(i => i.Id == item.IdUsuario_Adicionou);
                    //if (usuarioNome == null)
                    //{
                    //   usuarioNomeOld = result.FirstOrDefault(i => i.Id == item.IdUsuario_Adicionou);
                    //}

                    item.Username = usuarioNome?.Nome;
                    item.LoggedUserId = LoggedUserId;
                    item.allowdelete = checkUser != null ? true : false;
                }                   
            }

            return mensagems;
        }

        public FuncionarioViewModel SearchEmployee(string? employee)
        {
            FuncionarioViewModel queryliberador = new FuncionarioViewModel();

            queryliberador = _contextBS.Funcionario
                            .Where(e => e.Chapa == employee)
                            .ProjectTo<FuncionarioViewModel>(mapeamentoClasses)
                            .OrderByDescending(e => e.DataMudanca)
                            .FirstOrDefault();

            if (queryliberador == null)
            {
                queryliberador = (from funcionario in _contextSeek.Funcionario
                                  join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                                  join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                                  where funcionario.Chapa.Contains(employee) 
                                  select new FuncionarioViewModel
                                  {
                                      IdTerceiro = funcionario.Id,
                                      Chapa = funcionario.Chapa,
                                      Nome = funcionario.Nome,
                                      CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                      Secao = secao.Nome,
                                      Funcao = funcao.Nome,
                                      DataAdmissao = funcionario.DataRegistro
                                  }).FirstOrDefault();
            }


            return queryliberador;

        }

        public byte[]? searchImage(int? CodPessoa)
        {

            byte[] base64Image = (from pessoa in _contextRM.PPESSOA
                                                join gImagem in _contextRM.GIMAGEM
                                                on pessoa.IDIMAGEM equals gImagem.ID
                                                where pessoa.CODIGO == CodPessoa
                                                select gImagem.IMAGEM)
                                      .FirstOrDefault();


            return base64Image;
        }
        #endregion

        #region Devolucao
        public List<DevolucaoViewModel> SearchDevolucaoList(SearchFilters filters)
        {
            List<DevolucaoViewModel>? ResultDevolucao = new List<DevolucaoViewModel>();

            //ResultDevolucao = (from produtoAlocado in _context.ProdutoAlocado
            //                   join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
            //                   join ferrOndeProdRetirado in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOndeProdRetirado.Id
            //                   join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
            //                   join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
            //                   join obra in _context.Obra on produtoAlocado.IdObra equals obra.Id
            //                   join controleca in _context.ControleCA on produtoAlocado.IdControleCA equals controleca.Id into controlecaGroup
            //                   from controleca in controlecaGroup.DefaultIfEmpty()
            //                   where (filters.CodColigada == null || produtoAlocado.Solicitante_CodColigada == filters.CodColigada)
            //                       && (filters.Chapa == null || produtoAlocado.Solicitante_Chapa == filters.Chapa)
            //                       && (filters.Observacao == null || produtoAlocado.Observacao.Contains(filters.Observacao))
            //                       && (filters.TransacoesDe == null || produtoAlocado.DataEmprestimo >= filters.TransacoesDe.Value)
            //                       && (filters.TransacoesAte == null || produtoAlocado.DataEmprestimo <= filters.TransacoesAte.Value.AddDays(1).AddTicks(-1))
            //                       && (filters.PrevisaoDe == null || produtoAlocado.DataPrevistaDevolucao >= filters.PrevisaoDe.Value)
            //                       && (filters.PrevisaoAte == null || produtoAlocado.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.AddDays(1).AddTicks(-1))
            //                       && (filters.Codigo == null || catalogo.Codigo.Contains(filters.Codigo))
            //                       && (filters.Catalogo == null || catalogo.Nome.Contains(filters.Catalogo))
            //                       && (filters.CatalogoList == null || categoria.Classe == filters.CatalogoList)
            //                       && (filters.AF == null || produto.AF.Contains(filters.AF))
            //                       && (filters.PAT == null || produto.PAT == filters.PAT)
            //                       && (filters.DataDeValidade == null || produto.DataVencimento == filters.DataDeValidade)
            //                       && categoria.Classe != 3
            //                   orderby produtoAlocado.DataEmprestimo descending
            //                   select new DevolucaoViewModel
            //                   {
            //                       IdProdutoAlocado = produtoAlocado.Id,
            //                       Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
            //                       Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
            //                       Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
            //                       Balconista_IdLogin = produtoAlocado.Balconista_IdLogin,
            //                       Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
            //                       Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
            //                       Liberador_Chapa = produtoAlocado.Liberador_Chapa,
            //                       Observacao = produtoAlocado.Observacao,                              
            //                       DataEmprestimo = produtoAlocado.DataEmprestimo,
            //                       DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
            //                       Quantidade = produtoAlocado.Quantidade,
            //                       IdFerramentaria = ferrOndeProdRetirado.Id,
            //                       NomeFerramentaria = ferrOndeProdRetirado.Nome,
            //                       IdObra = obra.Id,
            //                       NomeObra = obra.Nome,
            //                       IdProduto = produto.Id,
            //                       Localizacao = produto.Localizacao,
            //                       AFProduto = produto.AF,
            //                       QuantidadeMinima = produto.QuantidadeMinima,
            //                       ProdutoQuantidade = produto.Quantidade,
            //                       PATProduto = produto.PAT,
            //                       DataVencimento = produto.DataVencimento,
            //                       DC_DataAquisicao = produto.DC_DataAquisicao,
            //                       DC_Valor = produto.DC_Valor,
            //                       IdCatalogo = catalogo.Id,
            //                       CodigoCatalogo = catalogo.Codigo,
            //                       NomeCatalogo = catalogo.Nome,
            //                       CatalogoPorAferido = catalogo.PorAferido,
            //                       CatalogoPorSerial = catalogo.PorSerial,
            //                       ImpedirDescarte = catalogo.ImpedirDescarte,
            //                       HabilitarDescarteEPI = catalogo.HabilitarDescarteEPI,
            //                       IdCategoria = categoria.Id,
            //                       ClasseCategoria = categoria.Classe,
            //                       NomeCategoria = categoria.Nome,
            //                       ProdutoAtivo = produto.Ativo,
            //                       IdControleCA = produtoAlocado.IdControleCA,
            //                       NumeroControleCA = controleca.NumeroCA,
            //                       ValidadeControlCA = controleca.Validade,
            //                   }).ToList();

            ResultDevolucao = (from produtoAlocado in _context.ProdutoAlocado
                        join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
                        join ferrOndeProdRetirado in _context.Ferramentaria on produtoAlocado.IdFerrOndeProdRetirado equals ferrOndeProdRetirado.Id
                        join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                        join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                        join obra in _context.Obra on produtoAlocado.IdObra equals obra.Id
                        join controleca in _context.ControleCA on produtoAlocado.IdControleCA equals controleca.Id into controlecaGroup
                        from controleca in controlecaGroup.DefaultIfEmpty()
                        join produtoExtraviadoGroup in (
                            from pe in _context.ProdutoExtraviado
                            where pe.Ativo == 1
                            group pe by pe.IdProdutoAlocado into g
                            select new
                            {
                                IdProdutoAlocado = g.Key,
                                Quantidade = g.Sum(pe => pe.Quantidade)
                            }
                        ) on produtoAlocado.Id equals produtoExtraviadoGroup.IdProdutoAlocado into extraviadoGroupJoin
                        from extraviado in extraviadoGroupJoin.DefaultIfEmpty()
                        where 
                        //(filters.CodColigada == null || produtoAlocado.Solicitante_CodColigada == filters.CodColigada)
                        //           && 
                                   (filters.Chapa == null || produtoAlocado.Solicitante_Chapa == filters.Chapa)
                                   && (filters.Observacao == null || produtoAlocado.Observacao.Contains(filters.Observacao))
                                   && (filters.TransacoesDe == null || produtoAlocado.DataEmprestimo >= filters.TransacoesDe.Value)
                                   && (filters.TransacoesAte == null || produtoAlocado.DataEmprestimo <= filters.TransacoesAte.Value.AddDays(1).AddTicks(-1))
                                   && (filters.PrevisaoDe == null || produtoAlocado.DataPrevistaDevolucao >= filters.PrevisaoDe.Value)
                                   && (filters.PrevisaoAte == null || produtoAlocado.DataPrevistaDevolucao <= filters.PrevisaoAte.Value.AddDays(1).AddTicks(-1))
                                   && (filters.Codigo == null || catalogo.Codigo.Contains(filters.Codigo))
                                   && (filters.Catalogo == null || catalogo.Nome.Contains(filters.Catalogo))
                                   && (filters.CatalogoList == null || categoria.Classe == filters.CatalogoList)
                                   && (filters.AF == null || produto.AF.Contains(filters.AF))
                                   && (filters.PAT == null || produto.PAT == filters.PAT)
                                   && (filters.DataDeValidade == null || produto.DataVencimento == filters.DataDeValidade)
                                   && categoria.Classe != 3
                        orderby produtoAlocado.DataEmprestimo descending
                        select new DevolucaoViewModel
                        {
                            IdProdutoAlocado = produtoAlocado.Id,
                            Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
                            Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
                            Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
                            Balconista_IdLogin = produtoAlocado.Balconista_IdLogin,
                            Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
                            Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
                            Liberador_Chapa = produtoAlocado.Liberador_Chapa,
                            Observacao = produtoAlocado.Observacao,
                            DataEmprestimo = produtoAlocado.DataEmprestimo,
                            DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
                            Quantidade = produtoAlocado.Quantidade,
                            IdFerramentaria = ferrOndeProdRetirado.Id,
                            NomeFerramentaria = ferrOndeProdRetirado.Nome,
                            IdObra = obra.Id,
                            NomeObra = obra.Nome,
                            IdProduto = produto.Id,
                            Localizacao = produto.Localizacao,
                            AFProduto = produto.AF,
                            QuantidadeMinima = produto.QuantidadeMinima,
                            ProdutoQuantidade = produto.Quantidade,
                            PATProduto = produto.PAT,
                            DataVencimento = produto.DataVencimento,
                            DC_DataAquisicao = produto.DC_DataAquisicao,
                            DC_Valor = produto.DC_Valor,
                            IdCatalogo = catalogo.Id,
                            CodigoCatalogo = catalogo.Codigo,
                            NomeCatalogo = catalogo.Nome,
                            CatalogoPorAferido = catalogo.PorAferido,
                            CatalogoPorSerial = catalogo.PorSerial,
                            ImpedirDescarte = catalogo.ImpedirDescarte,
                            HabilitarDescarteEPI = catalogo.HabilitarDescarteEPI,
                            IdCategoria = categoria.Id,
                            ClasseCategoria = categoria.Classe,
                            NomeCategoria = categoria.Nome,
                            ProdutoAtivo = produto.Ativo,
                            IdControleCA = produtoAlocado.IdControleCA,
                            NumeroControleCA = controleca.NumeroCA,
                            ValidadeControlCA = controleca.Validade,
                            QuantidadeExtraviada = extraviado.Quantidade != null ? extraviado.Quantidade : 0,
                            IdReservation = produtoAlocado.IdReservation
                        }).ToList();


            ResultDevolucao = (from results in ResultDevolucao
                               join user in _contextBS.Usuario on results.Balconista_IdLogin equals user.Id into userGroup
                               from user in userGroup.DefaultIfEmpty()
                               select new DevolucaoViewModel
                               {
                                   IdProdutoAlocado = results.IdProdutoAlocado,
                                   Solicitante_IdTerceiro = results.Solicitante_IdTerceiro,
                                   Solicitante_CodColigada = results.Solicitante_CodColigada,
                                   Solicitante_Chapa = results.Solicitante_Chapa,
                                   Balconista_IdLogin = results.Balconista_IdLogin,
                                   Balconista_Chapa = user != null ? user.Chapa : string.Empty,
                                   Liberador_IdTerceiro = results.Liberador_IdTerceiro,
                                   Liberador_CodColigada = results.Liberador_CodColigada,
                                   Liberador_Chapa = results.Liberador_Chapa,
                                   Observacao = results.Observacao,
                                   DataEmprestimo = results.DataEmprestimo,
                                   DataPrevistaDevolucao = results.DataPrevistaDevolucao,
                                   Quantidade = results.Quantidade,
                                   IdFerramentaria = results.IdFerramentaria,
                                   NomeFerramentaria = results.NomeFerramentaria,
                                   IdObra = results.IdObra,
                                   NomeObra = results.NomeObra,
                                   IdProduto = results.IdProduto,
                                   Localizacao = results.Localizacao,
                                   AFProduto = results.AFProduto,
                                   QuantidadeMinima = results.QuantidadeMinima,
                                   ProdutoQuantidade = results.Quantidade,
                                   PATProduto = results.PATProduto,
                                   DataVencimento = results.DataVencimento,
                                   DC_DataAquisicao = results.DC_DataAquisicao,
                                   DC_Valor = results.DC_Valor,
                                   IdCatalogo = results.IdCatalogo,
                                   CodigoCatalogo = results.CodigoCatalogo,
                                   NomeCatalogo = results.NomeCatalogo,
                                   CatalogoPorAferido = results.CatalogoPorAferido,
                                   CatalogoPorSerial = results.CatalogoPorSerial,
                                   ImpedirDescarte = results.ImpedirDescarte,
                                   HabilitarDescarteEPI = results.HabilitarDescarteEPI,
                                   IdCategoria = results.IdCategoria,
                                   ClasseCategoria = results.ClasseCategoria,
                                   NomeCategoria = results.NomeCategoria,
                                   ProdutoAtivo = results.ProdutoAtivo,
                                   IdControleCA = results.IdControleCA,
                                   NumeroControleCA = results.NumeroControleCA,
                                   ValidadeControlCA = results.ValidadeControlCA,
                                   QuantidadeExtraviada = results.QuantidadeExtraviada,
                                   IdReservation = results.IdReservation
                               }).ToList();

            //if (ResultDevolucao.Any())
            //{
            //    foreach (var item in ResultDevolucao)
            //    {

            //        int? extraviadoQuantity = SearchProdutoExtraviadoQuantity(item.IdProdutoAlocado);

            //        item.QuantidadeExtraviada = extraviadoQuantity;

            //    }
            //}

             return ResultDevolucao;
        }

        public ProdutoCompleteViewModel? VerifyProduct(DevolucaoViewModel? filters)
        {
            ProdutoCompleteViewModel? ResultDevolucao = new ProdutoCompleteViewModel();

            ResultDevolucao = (from produto in _context.Produto
                                                      join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                      join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                      join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                      join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaGroup
                                                      from empresa in empresaGroup.DefaultIfEmpty()
                                                      where produto.Ativo == 1 
                                                            && (!filters.IdCatalogo.HasValue || catalogo.Id == filters.IdCatalogo)
                                                            && (!filters.IdFerramentaria.HasValue || produto.IdFerramentaria == filters.IdFerramentaria)
                                                            && (!filters.IdProduto.HasValue || produto.Id == filters.IdProduto)
                                                      orderby produto.DataRegistro
                                                      select new ProdutoCompleteViewModel
                                                      {
                                                          DC_DataAquisicao = produto.DC_DataAquisicao,
                                                          DC_Valor = produto.DC_Valor,
                                                          DC_AssetNumber = produto.DC_AssetNumber,
                                                          DC_Fornecedor = produto.DC_Fornecedor,
                                                          GC_Contrato = produto.GC_Contrato,
                                                          GC_DataInicio = produto.GC_DataInicio,
                                                          GC_IdObra = produto.GC_IdObra,
                                                          GC_OC = produto.GC_OC,
                                                          GC_DataSaida = produto.GC_DataSaida,
                                                          GC_NFSaida = produto.GC_NFSaida,
                                                          Selo = produto.Selo,
                                                          IdProduto = produto.Id,
                                                          AF = produto.AF,
                                                          PAT = produto.PAT,
                                                          Quantidade = produto.Quantidade,
                                                          QuantidadeMinima = produto.QuantidadeMinima,
                                                          Localizacao = produto.Localizacao,
                                                          RFM = produto.RFM,
                                                          Observacao = produto.Observacao,
                                                          DataRegistroProduto = produto.DataRegistro,
                                                          DataVencimento = produto.DataVencimento,
                                                          Certificado = produto.Certificado,
                                                          Serie = produto.Serie,
                                                          AtivoProduto = produto.Ativo,
                                                          IdCatalogo = catalogo.Id,
                                                          Codigo = catalogo.Codigo,
                                                          NomeCatalogo = catalogo.Nome,
                                                          Descricao = catalogo.Descricao,
                                                          PorMetro = catalogo.PorMetro,
                                                          PorAferido = catalogo.PorAferido,
                                                          PorSerial = catalogo.PorSerial,
                                                          DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                                          DataRegistroCatalogo = catalogo.DataRegistro,
                                                          AtivoCatalogo = catalogo.Ativo,
                                                          IdCategoria = categoria.Id,
                                                          IdCategoriaPai = categoria.IdCategoria,
                                                          Classe = categoria.Classe,
                                                          NomeCategoria = categoria.Nome,
                                                          DataRegistroCategoria = categoria.DataRegistro,
                                                          AtivoCategoria = categoria.Ativo,
                                                          IdFerramentaria = ferramentaria.Id,
                                                          NomeFerramentaria = ferramentaria.Nome,
                                                          DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                                          AtivoFerramentaria = ferramentaria.Ativo,
                                                          IdEmpresa = empresa.Id,
                                                          NomeEmpresa = empresa.Nome,
                                                          GerenteEmpresa = empresa.Gerente,
                                                          TelefoneEmpresa = empresa.Telefone,
                                                          DataRegistroEmpresa = empresa.DataRegistro,
                                                          AtivoEmpresa = empresa.Ativo
                                                      }).FirstOrDefault();

            return ResultDevolucao;
        }

        public int? SearchProdutoExtraviadoQuantity (int? IdProdutoAlocado)
        {

            List <int?> result = (
                                    from produtoExtraviado in _context.ProdutoExtraviado
                                    join produtoAlocado in _context.ProdutoAlocado on produtoExtraviado.IdProdutoAlocado equals produtoAlocado.Id
                                    join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
                                    where produtoExtraviado.IdProdutoAlocado == IdProdutoAlocado && produtoExtraviado.Ativo == 1
                                    select produtoExtraviado.Quantidade
                                  ).ToList();

            int? totalExtraviado = result.Sum();

            return totalExtraviado;
        }

        public int? GetProdutoAlocado(HistoricoViewModel? HistoricoViewModel)
        {

            int? result = _context.ProdutoAlocado.Where(i => i.IdProduto == HistoricoViewModel.IdProduto
                                                    && i.Solicitante_Chapa == HistoricoViewModel.Solicitante_Chapa 
                                                    && i.Solicitante_CodColigada == HistoricoViewModel.Solicitante_CodColigada)
                                                .Select(i => i.Id).FirstOrDefault();


            return result;
        }
        #endregion

        #region DevolucaoExpressa
        public List<DevolucaoExpressaViewModel?>? SearchDevolucaoExpressa(string? AF, int? PAT)
        {
            List <DevolucaoExpressaViewModel?>? Devolucao = (from produtoAlocado in _context.ProdutoAlocado
                                                             join produto in _context.Produto
                                                             on produtoAlocado.IdProduto equals produto.Id
                                                             join ferrOndeProdRetirado in _context.Ferramentaria
                                                             on produtoAlocado.IdFerrOndeProdRetirado equals ferrOndeProdRetirado.Id
                                                             join catalogo in _context.Catalogo
                                                             on produto.IdCatalogo equals catalogo.Id
                                                             join categoria in _context.Categoria
                                                             on catalogo.IdCategoria equals categoria.Id
                                                             join obra in _context.Obra
                                                             on produtoAlocado.IdObra equals obra.Id
                                                             where
                                                                 //(string.IsNullOrEmpty(AF) || produto.AF.Contains(AF))
                                                                 (string.IsNullOrEmpty(AF) || EF.Functions.Like(produto.AF, $"%{AF}%"))
                                                                 && (PAT == null || produto.PAT == PAT)
                                                                 && (catalogo.PorAferido == 1 || catalogo.PorSerial == 1) && categoria.Classe == 1
                                                             orderby produtoAlocado.DataEmprestimo descending
                                                             select new DevolucaoExpressaViewModel
                                                             {
                                                                 IdProdutoAlocado = produtoAlocado.Id,
                                                                 Solicitante_IdTerceiro = produtoAlocado.Solicitante_IdTerceiro,
                                                                 Solicitante_CodColigada = produtoAlocado.Solicitante_CodColigada,
                                                                 Solicitante_Chapa = produtoAlocado.Solicitante_Chapa,
                                                                 Balconista_IdLogin = produtoAlocado.Balconista_IdLogin,
                                                                 Liberador_IdTerceiro = produtoAlocado.Liberador_IdTerceiro,
                                                                 Liberador_CodColigada = produtoAlocado.Liberador_CodColigada,
                                                                 Liberador_Chapa = produtoAlocado.Liberador_Chapa,
                                                                 Observacao = produtoAlocado.Observacao,
                                                                 DataEmprestimo = produtoAlocado.DataEmprestimo,
                                                                 DataPrevistaDevolucao = produtoAlocado.DataPrevistaDevolucao,
                                                                 Quantidade = produtoAlocado.Quantidade,
                                                                 IdFerramentaria = ferrOndeProdRetirado.Id,
                                                                 NomeFerramentaria = ferrOndeProdRetirado.Nome,
                                                                 IdObra = obra.Id,
                                                                 NomeObra = obra.Nome,
                                                                 IdProduto = produto.Id,
                                                                 AFProduto = produto.AF,
                                                                 PATProduto = produto.PAT,
                                                                 DataVencimento = produto.DataVencimento,
                                                                 DC_DataAquisicao = produto.DC_DataAquisicao,
                                                                 DC_Valor = produto.DC_Valor,
                                                                 CodigoCatalogo = catalogo.Codigo,
                                                                 NomeCatalogo = catalogo.Nome,
                                                                 ImpedirDescarte = catalogo.ImpedirDescarte,
                                                                 HabilitarDescarteEPI = catalogo.HabilitarDescarteEPI,
                                                                 IdCategoria = categoria.Id,
                                                                 ClasseCategoria = categoria.Classe,
                                                                 NomeCategoria = categoria.Nome,
                                                                 ProdutoAtivo = produto.Ativo,
                                                                 IdControleCA = produtoAlocado.IdControleCA
                                                             }).ToList();

            return Devolucao;
        }

        public List<HistoricoAlocacaoViewModel?>? SearchHistoricoAlocacaoById(int? IdHistoricoAlocacao)
        {
            List<HistoricoAlocacaoViewModel> historicoAlocacaoList = (from hist in _context.HistoricoAlocacao_2025
                                                                      join produto in _context.Produto on hist.IdProduto equals produto.Id
                                                                      join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                                      join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                      join ferrOrigem in _context.Ferramentaria on hist.IdFerrOndeProdRetirado equals ferrOrigem.Id
                                                                      join ferrDevolucao in _context.Ferramentaria on hist.IdFerrOndeProdDevolvido equals ferrDevolucao.Id into devolucaoGroup
                                                                      from devolucao in devolucaoGroup.DefaultIfEmpty()
                                                                      where hist.Id == IdHistoricoAlocacao
                                                                      select new HistoricoAlocacaoViewModel
                                                                      {
                                                                          IdHistoricoAlocacao = hist.Id,
                                                                          IdProdutoAlocado = 0,
                                                                          IdProduto = hist.IdProduto,
                                                                          Solicitante_IdTerceiro = hist.Solicitante_IdTerceiro,
                                                                          Solicitante_CodColigada = hist.Solicitante_CodColigada,
                                                                          Solicitante_Chapa = hist.Solicitante_Chapa,
                                                                          Liberador_IdTerceiro = hist.Liberador_IdTerceiro,
                                                                          Liberador_CodColigada = hist.Liberador_CodColigada,
                                                                          Liberador_Chapa = hist.Liberador_Chapa,
                                                                          Balconista_Emprestimo_IdLogin = hist.Balconista_Emprestimo_IdLogin,
                                                                          Balconista_Devolucao_IdLogin = hist.Balconista_Devolucao_IdLogin,
                                                                          Observacao = hist.Observacao,
                                                                          DataEmprestimo = hist.DataEmprestimo,
                                                                          DataPrevistaDevolucao = hist.DataPrevistaDevolucao,
                                                                          DataDevolucao = hist.DataDevolucao,
                                                                          IdObra = hist.IdObra,
                                                                          Quantidade = hist.Quantidade,
                                                                          QuantidadeEmprestada = catalogo.PorAferido == 0 && catalogo.PorSerial == 0
                                                                              ? (_context.ProdutoAlocado
                                                                                     .Where(pa =>
                                                                                         pa.IdProduto == hist.IdProduto &&
                                                                                         pa.Solicitante_IdTerceiro == hist.Solicitante_IdTerceiro &&
                                                                                         pa.Solicitante_CodColigada == hist.Solicitante_CodColigada &&
                                                                                         pa.Solicitante_Chapa == hist.Solicitante_Chapa)
                                                                                     .OrderBy(pa => pa.Id)
                                                                                     .Select(pa => pa.Quantidade)
                                                                                     .FirstOrDefault() ?? 0)
                                                                              : 0,
                                                                          IdFerrOndeProdRetirado = hist.IdFerrOndeProdRetirado,
                                                                          IdFerrOndeProdDevolvido = hist.IdFerrOndeProdDevolvido,
                                                                          CodigoCatalogo = catalogo.Codigo,
                                                                          NomeCatalogo = catalogo.Nome,
                                                                          FerrOrigem = ferrOrigem.Nome,
                                                                          FerrDevolucao = devolucao.Nome,
                                                                          AFProduto = produto.AF,
                                                                          Serie = produto.Serie,
                                                                          PATProduto = produto.PAT,
                                                                          IdControleCA = hist.IdControleCA
                                                                      }).ToList();

            return historicoAlocacaoList;
        }

        public ProdutoCompleteViewModel? SearchProductByAF(string? AF, int? PAT)
        {
            ProdutoCompleteViewModel? ResultDevolucao = new ProdutoCompleteViewModel();

            ResultDevolucao = (from produto in _context.Produto
                               join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                               join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                               join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                               join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaGroup
                               from empresa in empresaGroup.DefaultIfEmpty()
                               where produto.Ativo == 1
                                     && categoria.Classe == 1
                                     && (string.IsNullOrEmpty(AF) || produto.AF.Contains(AF))
                                     && (PAT == null || produto.PAT == PAT)
                               orderby produto.DataRegistro
                               select new ProdutoCompleteViewModel
                               {
                                   DC_DataAquisicao = produto.DC_DataAquisicao,
                                   DC_Valor = produto.DC_Valor,
                                   DC_AssetNumber = produto.DC_AssetNumber,
                                   DC_Fornecedor = produto.DC_Fornecedor,
                                   GC_Contrato = produto.GC_Contrato,
                                   GC_DataInicio = produto.GC_DataInicio,
                                   GC_IdObra = produto.GC_IdObra,
                                   GC_OC = produto.GC_OC,
                                   GC_DataSaida = produto.GC_DataSaida,
                                   GC_NFSaida = produto.GC_NFSaida,
                                   Selo = produto.Selo,
                                   IdProduto = produto.Id,
                                   AF = produto.AF,
                                   PAT = produto.PAT,
                                   Quantidade = produto.Quantidade,
                                   QuantidadeMinima = produto.QuantidadeMinima,
                                   Localizacao = produto.Localizacao,
                                   RFM = produto.RFM,
                                   Observacao = produto.Observacao,
                                   DataRegistroProduto = produto.DataRegistro,
                                   DataVencimento = produto.DataVencimento,
                                   Certificado = produto.Certificado,
                                   Serie = produto.Serie,
                                   AtivoProduto = produto.Ativo,
                                   IdCatalogo = catalogo.Id,
                                   Codigo = catalogo.Codigo,
                                   NomeCatalogo = catalogo.Nome,
                                   Descricao = catalogo.Descricao,
                                   PorMetro = catalogo.PorMetro,
                                   PorAferido = catalogo.PorAferido,
                                   PorSerial = catalogo.PorSerial,
                                   DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                   DataRegistroCatalogo = catalogo.DataRegistro,
                                   AtivoCatalogo = catalogo.Ativo,
                                   IdCategoria = categoria.Id,
                                   IdCategoriaPai = categoria.IdCategoria,
                                   Classe = categoria.Classe,
                                   NomeCategoria = categoria.Nome,
                                   DataRegistroCategoria = categoria.DataRegistro,
                                   AtivoCategoria = categoria.Ativo,
                                   IdFerramentaria = ferramentaria.Id,
                                   NomeFerramentaria = ferramentaria.Nome,
                                   DataRegistroFerramentaria = ferramentaria.DataRegistro,
                                   AtivoFerramentaria = ferramentaria.Ativo,
                                   IdEmpresa = empresa.Id,
                                   NomeEmpresa = empresa.Nome,
                                   GerenteEmpresa = empresa.Gerente,
                                   TelefoneEmpresa = empresa.Telefone,
                                   DataRegistroEmpresa = empresa.DataRegistro,
                                   AtivoEmpresa = empresa.Ativo
                               }).FirstOrDefault();

            return ResultDevolucao;
        }

        #endregion

        #region BloqueioMensagem
        public string? SearchBloqueioMessage(string? filters)
        {
            string? value = null;

            var result = _context.BloqueioEmprestimoAoSolicitante.FirstOrDefault(i => i.Chapa == filters && i.Ativo == 1);
            if (result != null)
            {
                value = result.Mensagem;
            }

            return value;
        }
        #endregion

        #region Logs
        public List<VW_Usuario_New> GetDistinctUser(List<int?> Ids)
        {
            List<VW_Usuario_New> UserValues = new List<VW_Usuario_New>();

            foreach (int? userId in Ids)
            {
                var finduser = _contextBS.VW_Usuario_New.FirstOrDefault(u => u.Id == userId);
                if (finduser != null)
                {
                    UserValues.Add(finduser);
                }
            }

            return UserValues;
        }

        public List<VW_Usuario> GetOldDistinctUser(List<int?> Ids)
        {
            List<VW_Usuario> UserValues = new List<VW_Usuario>();

            foreach (int? userId in Ids)
            {
                var finduser = _contextBS.VW_Usuario.FirstOrDefault(u => u.Id == userId);
                if (finduser != null)
                {
                    UserValues.Add(finduser);
                }
            }

            return UserValues;
        }

        public List<VW_Funcionario_Registro_Atual> GetFuncionarioRegistroAtual(List<string?> chapa)
        {
            List<VW_Funcionario_Registro_Atual> EmployeeValues = new List<VW_Funcionario_Registro_Atual>();

            foreach (string? matricula in chapa)
            {
                var finduser = _contextBS.VW_Funcionario_Registro_Atual.FirstOrDefault(u => u.Chapa == matricula);
                if (finduser != null)
                {
                    EmployeeValues.Add(finduser);
                }
            }

            return EmployeeValues;
        }

        public List<VW_Usuario_New> OnLoadUsers()
        {
            List<VW_Usuario_New> UserValues = new List<VW_Usuario_New>();

            UserValues = (from usuario in _contextBS.VW_Usuario_New
                           where usuario.Ativo == 1 &&
                                 (
                                     (usuario.CodSituacao == "F" &&
                                      (from func in _contextBS.Funcionario
                                       where func.FimProgFerias1 <= DateTime.Now &&
                                             func.Chapa == usuario.Chapa &&
                                             func.CodColigada == usuario.CodColigada
                                       select func.CodSituacao).FirstOrDefault() != null) ||
                                     usuario.CodSituacao == "A"
                                 ) &&
                                 (from acesso in _contextBS.Acesso
                                  join permissao in _contextBS.Permissao on acesso.Id equals permissao.IdAcesso
                                  where acesso.IdModulo == 6
                                  select permissao.IdUsuario).ToList().Contains(usuario.Id ?? 0) // Use ?? to provide a default value if usuario.Id is null
                           orderby usuario.Nome
                           select usuario).ToList();

            return UserValues;

        }

        public List<FerramentariaViewModel> OnLoadFerramentaria()
        {
            List<FerramentariaViewModel> FerramentariaValues = new List<FerramentariaViewModel>();

            FerramentariaValues = (from ferramentaria in _context.Ferramentaria
                                  where ferramentaria.Ativo == 1
                                  orderby ferramentaria.Nome
                                  select new FerramentariaViewModel
                                  {
                                      Id = ferramentaria.Id,
                                      Nome = ferramentaria.Nome
                                  }).ToList();

            return FerramentariaValues;
        }

        public List<ObraViewModel> OnLoadObra()
        {
            List<ObraViewModel> ObraValues = new List<ObraViewModel>();

            ObraValues = (from obra in _context.Obra
                                   where obra.Ativo == 1
                                   orderby obra.Nome
                                   select new ObraViewModel
                                   {
                                       Id = obra.Id,
                                       Nome = obra.Nome
                                   }).ToList();

            return ObraValues;
        }

        public List<FerramentariaViewModel> OnLoadJustificativarFerramentaria()
        {
            List<FerramentariaViewModel> FerramentariaValues = new List<FerramentariaViewModel>();

            FerramentariaValues = (from ferramentaria in _context.Ferramentaria
                                   where ferramentaria.Ativo == 1 &&
                                   !(_context.VW_Ferramentaria_Ass_Solda.Select(x => x.Id).Contains(ferramentaria.Id))
                                   orderby ferramentaria.Nome
                                   select new FerramentariaViewModel
                                   {
                                       Id = ferramentaria.Id,
                                       Nome = ferramentaria.Nome
                                   }).ToList();

            return FerramentariaValues;
        }

        public List<ProdutoList> FindProduto(ProdutoFilter values)
        {
            List<ProdutoList?> ProdutoList = new List<ProdutoList?>();

            ProdutoList = (from produto in _context.Produto
                          join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                          join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                          join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                          where _context.ProdutoAlocado.Any(ap => ap.IdProduto == produto.Id) 
                          && (values.Codigo == null || catalogo.Codigo.Contains(values.Codigo))
                          && (values.AF == null || produto.AF.Contains(values.AF))
                          && (values.PAT == null || produto.PAT == values.PAT)
                          && (values.NumeroSerie == null || produto.Serie.Contains(values.NumeroSerie))
                          && (values.Catalogo == null || categoria.Classe == values.Catalogo)
                          && (values.Classe == null || categoria.IdCategoria == values.Classe)
                          && (values.Tipo == null || categoria.Id == values.Tipo)
                          orderby ferramentaria.Nome, catalogo.Descricao
                          select new ProdutoList
                          {
                              Id = produto.Id,
                              AF = produto.AF,
                              PAT = produto.PAT,
                              PorMetro = catalogo.PorMetro,
                              PorAferido = catalogo.PorAferido,
                              PorSerial = catalogo.PorSerial,
                              Item = catalogo.Nome,
                              Codigo = catalogo.Codigo                             
                          }).Take(1000).ToList();


            return ProdutoList;
        }


        public List<ProdutoList> FindProdutoForLog(string? AF, int? PAT)
        {
            List<ProdutoList?> ProdutoList = new List<ProdutoList?>();

            ProdutoList = (from produto in _context.Produto
                           join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                           join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                           join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                           join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaJoin
                           from empresa in empresaJoin.DefaultIfEmpty()
                           where (catalogo.PorAferido == 1 || catalogo.PorSerial == 1) 
                                 && (AF == null || produto.AF.Contains(AF))
                                 && (PAT == null || produto.PAT == PAT)
                           orderby catalogo.Nome, ferramentaria.Nome
                           select new ProdutoList
                           {
                               Id = produto.Id,
                               Codigo = catalogo.Codigo,
                               AF = produto.AF,
                               PAT = produto.PAT,
                               PorMetro = catalogo.PorMetro,
                               PorAferido = catalogo.PorAferido,
                               PorSerial = catalogo.PorSerial,
                               Item = catalogo.Nome,
                               FerramentariaOrigem = ferramentaria.Nome,
                           }).ToList();


            return ProdutoList;
        }

        #endregion

        #region Gestor
        public GestorProduto? SearchProductGestor(int? IdProduto)
        {
            GestorProduto? Result = (from produto in _context.Produto
                                         join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                         join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                         join empresa in _context.Empresa on produto.IdEmpresa equals empresa.Id into empresaJoin
                                         from empresa in empresaJoin.DefaultIfEmpty()
                                         where produto.Id == IdProduto
                                         select new GestorProduto
                                         {
                                             DC_DataAquisicao = produto.DC_DataAquisicao,
                                             DC_Valor = produto.DC_Valor,
                                             DC_AssetNumber = produto.DC_AssetNumber,
                                             DC_Fornecedor = produto.DC_Fornecedor,
                                             GC_Contrato = produto.GC_Contrato,
                                             GC_DataInicio = produto.GC_DataInicio,
                                             GC_IdObra = produto.GC_IdObra,
                                             GC_OC = produto.GC_OC,
                                             GC_DataSaida = produto.GC_DataSaida,
                                             GC_NFSaida = produto.GC_NFSaida,
                                             Selo = produto.Selo,
                                             Id = produto.Id,
                                             AF = produto.AF,
                                             PAT = produto.PAT,
                                             Quantidade = produto.Quantidade,
                                             QuantidadeMinima = produto.QuantidadeMinima,
                                             Localizacao = produto.Localizacao,
                                             RFM = produto.RFM,
                                             Observacao = produto.Observacao,
                                             DataRegistro = produto.DataRegistro,
                                             DataVencimento = produto.DataVencimento,
                                             Certificado = produto.Certificado,
                                             Serie = produto.Serie,
                                             Ativo = produto.Ativo,
                                             CatalogoId = catalogo.Id,
                                             CatalogoCodigo = catalogo.Codigo,
                                             CatalogoNome = catalogo.Nome,
                                             CatalogoDescricao = catalogo.Descricao,
                                             CatalogoPorMetro = catalogo.PorMetro,
                                             CatalogoPorAferido = catalogo.PorAferido,
                                             CatalogoPorSerial = catalogo.PorSerial,
                                             DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                             CatalogoDataRegistro = catalogo.DataRegistro,
                                             CatalogoAtivo = catalogo.Ativo,
                                             CategoriaId = categoria.Id,
                                             IdCategoriaPai = categoria.IdCategoria,
                                             CategoriaClasse = categoria.Classe,
                                             CategoriaNome = categoria.Nome,
                                             CategoriaDataRegistro = categoria.DataRegistro,
                                             CategoriaAtivo = categoria.Ativo,
                                             FerramentariaId = ferramentaria.Id,
                                             FerramentariaNome = ferramentaria.Nome,
                                             FerramentariaDataRegistro = ferramentaria.DataRegistro,
                                             FerramentariaAtivo = ferramentaria.Ativo,
                                             EmpresaId = empresa.Id,
                                             EmpresaNome = empresa.Nome,
                                             EmpresaGerente = empresa.Gerente,
                                             EmpresaTelefone = empresa.Telefone,
                                             EmpresaDataRegistro = empresa.DataRegistro,
                                             EmpresaAtivo = empresa.Ativo,
                                             Status = ((from pa in _context.ProdutoAlocado
                                                        where pa.IdProduto == produto.Id
                                                        select pa).Count() >= 1) ? "Emprestado" :
                                                      (produto.Quantidade == 0 ? "INDISPONÍVEL" : "Em Estoque")
                                         }).FirstOrDefault();

            return Result;
        }

        public GestorProduto? SearchInside(Produto produto, GestorProduto query, int? FerramentariaDestination)
        {
            GestorProduto? Result = (from produtoinside in _context.Produto
                                     join catalogo in _context.Catalogo on produtoinside.IdCatalogo equals catalogo.Id
                                     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                     join ferramentaria in _context.Ferramentaria on produtoinside.IdFerramentaria equals ferramentaria.Id
                                     join empresa in _context.Empresa on produtoinside.IdEmpresa equals empresa.Id into empresaJoin
                                     from empresa in empresaJoin.DefaultIfEmpty()
                                     where produtoinside.Ativo == 1 &&
                                           catalogo.Id == query.CatalogoId &&
                                           ferramentaria.Id == FerramentariaDestination
                                     orderby produto.DataRegistro descending
                                     select new GestorProduto
                                     {
                                         DC_DataAquisicao = produtoinside.DC_DataAquisicao,
                                         DC_Valor = produtoinside.DC_Valor,
                                         DC_AssetNumber = produtoinside.DC_AssetNumber,
                                         DC_Fornecedor = produtoinside.DC_Fornecedor,
                                         GC_Contrato = produtoinside.GC_Contrato,
                                         GC_DataInicio = produtoinside.GC_DataInicio,
                                         GC_IdObra = produtoinside.GC_IdObra,
                                         GC_OC = produtoinside.GC_OC,
                                         GC_DataSaida = produtoinside.GC_DataSaida,
                                         GC_NFSaida = produtoinside.GC_NFSaida,
                                         Selo = produtoinside.Selo,
                                         Id = produtoinside.Id,
                                         AF = produtoinside.AF,
                                         PAT = produtoinside.PAT,
                                         Quantidade = produtoinside.Quantidade,
                                         QuantidadeMinima = produtoinside.QuantidadeMinima,
                                         Localizacao = produtoinside.Localizacao,
                                         RFM = produtoinside.RFM,
                                         Observacao = produtoinside.Observacao,
                                         DataRegistro = produtoinside.DataRegistro,
                                         DataVencimento = produtoinside.DataVencimento,
                                         Certificado = produtoinside.Certificado,
                                         Serie = produtoinside.Serie,
                                         Ativo = produtoinside.Ativo,
                                         CatalogoId = catalogo.Id,
                                         CatalogoCodigo = catalogo.Codigo,
                                         CatalogoNome = catalogo.Nome,
                                         CatalogoDescricao = catalogo.Descricao,
                                         CatalogoPorMetro = catalogo.PorMetro,
                                         CatalogoPorAferido = catalogo.PorAferido,
                                         CatalogoPorSerial = catalogo.PorSerial,
                                         DataDeRetornoAutomatico = catalogo.DataDeRetornoAutomatico,
                                         CatalogoDataRegistro = catalogo.DataRegistro,
                                         CatalogoAtivo = catalogo.Ativo,
                                         CategoriaId = categoria.Id,
                                         IdCategoriaPai = categoria.IdCategoria,
                                         CategoriaClasse = categoria.Classe,
                                         CategoriaNome = categoria.Nome,
                                         CategoriaDataRegistro = categoria.DataRegistro,
                                         CategoriaAtivo = categoria.Ativo,
                                         FerramentariaId = ferramentaria.Id,
                                         FerramentariaNome = ferramentaria.Nome,
                                         FerramentariaDataRegistro = ferramentaria.DataRegistro,
                                         FerramentariaAtivo = ferramentaria.Ativo,
                                         EmpresaId = empresa.Id,
                                         EmpresaNome = empresa.Nome,
                                         EmpresaGerente = empresa.Gerente,
                                         EmpresaTelefone = empresa.Telefone,
                                         EmpresaDataRegistro = empresa.DataRegistro,
                                         EmpresaAtivo = empresa.Ativo
                                     }).FirstOrDefault();

            return Result;
        }
        #endregion

        public NewUserInformationModel newSearchEmployee(string? chapa)
        {

            NewUserInformationModel UsuarioModel = new NewUserInformationModel();

            FuncionarioViewModel? EmployeeResult = SearchEmployee(chapa);

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;

                    UsuarioModel.Imagebase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}";
                }
            }

            return UsuarioModel;

        }

        public employeeNewInformationModel searchEmployeeInformation(string? chapa)
        {

            FuncionarioViewModel? EmployeeResult = _contextBS.Funcionario
                                                    .Where(e => e.Chapa == chapa)
                                                    .ProjectTo<FuncionarioViewModel>(mapeamentoClasses)
                                                    .OrderByDescending(e => e.DataMudanca)
                                                    .FirstOrDefault();

            employeeNewInformationModel UsuarioModel = new employeeNewInformationModel();

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.CodPessoa = EmployeeResult.CodPessoa;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;

                    UsuarioModel.Imagebase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}";
                }
            }

            return UsuarioModel;
        }

        public employeeNewInformationModel searchEmployeeInformationUsingCodPessoa(int? CodPessoa)
        {

            FuncionarioViewModel? EmployeeResult = _contextBS.Funcionario
                                                    .Where(e => e.CodPessoa == CodPessoa)
                                                    .ProjectTo<FuncionarioViewModel>(mapeamentoClasses)
                                                    .OrderByDescending(e => e.DataMudanca)
                                                    .FirstOrDefault();

            employeeNewInformationModel UsuarioModel = new employeeNewInformationModel();

            if (EmployeeResult != null)
            {
                UsuarioModel.IdTerceiro = EmployeeResult.IdTerceiro != null ? EmployeeResult.IdTerceiro : 0;
                UsuarioModel.CodPessoa = EmployeeResult.CodPessoa;
                UsuarioModel.Chapa = EmployeeResult.Chapa;
                UsuarioModel.Nome = EmployeeResult.Nome;
                UsuarioModel.CodSituacao = EmployeeResult.CodSituacao;
                UsuarioModel.CodColigada = EmployeeResult.CodColigada != null ? EmployeeResult.CodColigada : 0;
                UsuarioModel.Funcao = EmployeeResult.Funcao;
                UsuarioModel.Secao = EmployeeResult.Secao;
                UsuarioModel.DataDemissao = EmployeeResult.DataDemissao;
                UsuarioModel.DataAdmissao = EmployeeResult.DataAdmissao;

                byte[] base64Image = searchImage(EmployeeResult.CodPessoa);

                if (base64Image != null)
                {
                    UsuarioModel.Image = base64Image;

                    UsuarioModel.Imagebase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}";
                }
            }

            return UsuarioModel;
        }

    }
}
