using FerramentariaTest.Entities;
using FerramentariaTest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Reflection.Emit;

namespace FerramentariaTest.DAL
{
    public class ContextoBanco : DbContext
    {
        public DbSet<Log> Log { get; set; }
        public DbSet<Marca> Marca { get; set; }
        public DbSet<Ferramentaria> Ferramentaria { get; set; }

        public DbSet<FerramentariaVsLiberador> FerramentariaVsLiberador { get; set; }
        public DbSet<VW_Ferramentaria_Ass_Solda> VW_Ferramentaria_Ass_Solda { get; set; }

        public DbSet<BloqueioEmprestimoVsLiberador> BloqueioEmprestimoVsLiberador { get; set; }

        public DbSet<Obra> Obra { get; set; }

        public DbSet<VW_Reativacao_Item> VW_Reativacao_Item { get; set; }
        public DbSet<ProdutoReincluido> ProdutoReincluido { get; set; }
        public DbSet<ProdutoReincluidoExtraviado> ProdutoReincluidoExtraviado { get; set; }

        public DbSet<VW_Reativacao_Item_Extraviado> VW_Reativacao_Item_Extraviado { get; set; }

        public DbSet<Catalogo> Catalogo { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<CatalogoLocal> CatalogoLocal { get; set; }
        public DbSet<ControleCA> ControleCA { get; set; }
        public DbSet<EntradaEmLote_Req> EntradaEmLote_Req { get; set; }
        public DbSet<EntradaEmLote_Comp> EntradaEmLote_Comp { get; set; }
        public DbSet<BloqueioEmprestimoAoSolicitante> BloqueioEmprestimoAoSolicitante { get; set; }
        public DbSet<MensagemSolicitante> MensagemSolicitante { get; set; }
        public DbSet<Produto> Produto { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<ProdutoAlocado> ProdutoAlocado { get; set; }
        public DbSet<ProdutoExcluido> ProdutoExcluido { get; set; }
        public DbSet<ProdutoExtraviado> ProdutoExtraviado { get; set; }
        public DbSet<Arquivo> Arquivo { get; set; }
        public DbSet<ArquivoVsProdutoAlocado> ArquivoVsProdutoAlocado { get; set; }
        public DbSet<ArquivoVsHistorico> ArquivoVsHistorico { get; set; }
        //public DbSet<HistoryAlocacao_2022> HistoricoAlocacao_2022 { get; set; }

        public DbSet<HistoricoAlocacao_2000> HistoricoAlocacao_2000 { get; set; }
        public DbSet<HistoricoAlocacao_2001> HistoricoAlocacao_2001 { get; set; }
        public DbSet<HistoricoAlocacao_2002> HistoricoAlocacao_2002 { get; set; }
        public DbSet<HistoricoAlocacao_2003> HistoricoAlocacao_2003 { get; set; }
        public DbSet<HistoricoAlocacao_2004> HistoricoAlocacao_2004 { get; set; }
        public DbSet<HistoricoAlocacao_2005> HistoricoAlocacao_2005 { get; set; }
        public DbSet<HistoricoAlocacao_2006> HistoricoAlocacao_2006 { get; set; }
        public DbSet<HistoricoAlocacao_2007> HistoricoAlocacao_2007 { get; set; }
        public DbSet<HistoricoAlocacao_2008> HistoricoAlocacao_2008 { get; set; }
        public DbSet<HistoricoAlocacao_2009> HistoricoAlocacao_2009 { get; set; }
        public DbSet<HistoricoAlocacao_2010> HistoricoAlocacao_2010 { get; set; }
        public DbSet<HistoricoAlocacao_2011> HistoricoAlocacao_2011 { get; set; }
        public DbSet<HistoricoAlocacao_2012> HistoricoAlocacao_2012 { get; set; }
        public DbSet<HistoricoAlocacao_2013> HistoricoAlocacao_2013 { get; set; }
        public DbSet<HistoricoAlocacao_2014> HistoricoAlocacao_2014 { get; set; }
        public DbSet<HistoricoAlocacao_2015> HistoricoAlocacao_2015 { get; set; }
        public DbSet<HistoricoAlocacao_2016> HistoricoAlocacao_2016 { get; set; }
        public DbSet<HistoricoAlocacao_2017> HistoricoAlocacao_2017 { get; set; }
        public DbSet<HistoricoAlocacao_2018> HistoricoAlocacao_2018 { get; set; }
        public DbSet<HistoricoAlocacao_2019> HistoricoAlocacao_2019 { get; set; }
        public DbSet<HistoricoAlocacao_2020> HistoricoAlocacao_2020 { get; set; }
        public DbSet<HistoricoAlocacao_2021> HistoricoAlocacao_2021 { get; set; }
        public DbSet<HistoricoAlocacao_2022> HistoricoAlocacao_2022 { get; set; }
        public DbSet<HistoricoAlocacao_2023> HistoricoAlocacao_2023 { get; set; }
        public DbSet<HistoricoAlocacao_2024> HistoricoAlocacao_2024 { get; set; }
        public DbSet<HistoricoAlocacao_2025> HistoricoAlocacao_2025 { get; set; }
        public DbSet<LogProduto> LogProduto { get; set; }
        public DbSet<HistoricoTransferencia> HistoricoTransferencia { get; set; }
        public DbSet<LogAtribuicaoFerramentaria> LogAtribuicaoFerramentaria { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Relatorio> Relatorio { get; set; }
        public DbSet<Relatorio_LogEntradaSaida> Relatorio_LogEntradaSaida { get; set; }
        public DbSet<LogRelatorio> LogRelatorio { get; set; }
        public DbSet<VW_Estoque_Positivo> VW_Estoque_Positivo { get; set; }
        public DbSet<VW_Estoque_Residual> VW_Estoque_Residual { get; set; }
        public DbSet<VW_Extravio_Produto> VW_Extravio_Produto { get; set; }
        public DbSet<VW_Todos_Radios> VW_Todos_Radios { get; set; }
        public DbSet<VW_Radios_Emprestados> VW_Radios_Emprestados { get; set; }
        public DbSet<AlertaAutomaticoVencimentoCA> AlertaAutomaticoVencimentoCA { get; set; }
        public DbSet<LogArquivo> LogArquivo { get; set; }
        public DbSet<SP_1600012731_Estoque> SP_1600012731_Estoque { get; set; }
        public DbSet<TipoExclusao> TipoExclusao { get; set; }
        public DbSet<LogEntradaSaidaInsert> LogEntradaSaidaInsert { get; set; }
        public DbSet<ProdutoVsMonitor> ProdutoVsMonitor { get; set; }
        public DbSet<VW_Itens_Emprestados> VW_Itens_Emprestados { get; set; }
        public DbSet<EntradaEmLote_Temp> EntradaEmLote_Temp { get; set; }
        public DbSet<VW_Historico> VW_Historico { get; set; }

        public DbSet<VW_ItensDevolvidos> VW_ItensDevolvidos { get; set; }

        public DbSet<VW_HistoricoWithoutFuncionario> VW_HistoricoWithoutFuncionario { get; set; }
        //public DbSet<VW_Historico2000ate2005> VW_Historico2000ate2005 { get; set; }
        //public DbSet<VW_Historico2006ate2010> VW_Historico2006ate2010 { get; set; }
        //public DbSet<VW_Historico2011ate2015> VW_Historico2011ate2015 { get; set; }
        //public DbSet<VW_Historico2016ate2020> VW_Historico2016ate2020 { get; set; }
        //public DbSet<VW_Historico2021ate2024> VW_Historico2021ate2024 { get; set; }
        public DbSet<VW_HistItensEmpDev> VW_HistItensEmpDev { get; set; }
        public DbSet<VW_HistItensTransEntreFerr> VW_HistItensTransEntreFerr { get; set; }
        public DbSet<VW_Exclusao_Produto> VW_Exclusao_Produto { get; set; }
        public DbSet<VW_EntradaSaida> VW_EntradaSaida { get; set; }
        public DbSet<Historico_LiberacaoExcepcional> Historico_LiberacaoExcepcional { get; set; }
        public DbSet<VW_ItensDevolvidosWithoutFuncionario> VW_ItensDevolvidosWithoutFuncionario { get; set; }
        public DbSet<VW_1600013295_ProdutoExcluido> VW_1600013295_ProdutoExcluido { get; set; }
        public DbSet<BloqueioEmprestimoVsLiberador_Log> BloqueioEmprestimoVsLiberador_Log { get; set; }
        public DbSet<VirtualFerrmantaria> VirtualFerrmantaria { get; set; }
        public DbSet<ExcludedObra> ExcludedObra { get; set; }
        public DbSet<ProductReservation> ProductReservation { get; set; }
        public DbSet<Reservations> Reservations { get; set; }
        public DbSet<ReservationControl> ReservationControl { get; set; }
        public DbSet<LeaderData> LeaderData { get; set; }
        public DbSet<LeaderMemberRel> LeaderMemberRel { get; set; }
        public DbSet<TermsControl> TermsControl { get; set; }
        public DbSet<AuditLogsBalconista> AuditLogsBalconista { get; set; }


        public ContextoBanco(DbContextOptions<ContextoBanco> options) : base(options) 
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<FerramentariaVsLiberador>()
            //    .HasNoKey();

            builder.Entity<SP_1600012731_Estoque>()
                .HasNoKey();

            //builder.Entity<ProdutoVsMonitor>()
            //  .HasNoKey();

            builder.Entity<EntradaEmLote_Temp>()
                .HasKey(e => new { e.DataRegistro });

            //builder.Entity<EntradaEmLote_Temp>()
            //  .HasNoKey();

            builder.Entity<ProdutoVsMonitor>()
            .HasKey(e => new { e.IdLogin });

            builder.Entity<BloqueioEmprestimoVsLiberador_Log>()
            .HasKey(e => new { e.IdRegistro });

            //builder.Entity<ProdutoAlocado>()
            //.HasKey(e => new { e.Id });

            builder.Entity<VW_Estoque_Positivo>()
                 .HasNoKey();

            builder.Entity<VW_Estoque_Residual>()
                 .HasNoKey();

            //builder.Entity<EntradaEmLote_Temp>()
            //            .HasNoKey();

            //builder.Entity<AlertaAutomaticoVencimentoCA>()
            //        .HasNoKey();

            builder.Entity<AlertaAutomaticoVencimentoCA>()
                .HasKey(a => a.Destinatario);

            builder.Entity<Produto>()
            .HasKey(e => new { e.Id });

            builder.Entity<ControleCA>()
            .HasKey(e => new { e.Id, e.IdCatalogo });

            builder.Entity<FerramentariaVsLiberador>()
            .HasKey(e => new { e.IdFerramentaria, e.IdLogin });

            builder.Entity<CatalogoLocal>()
            .HasKey(e => new { e.IdCatalogo, e.IdFerramentaria });

            builder.Entity<EntradaEmLote_Req>()
            .HasKey(e => new { e.Id, e.IdFerramentaria });


            builder.Entity<EntradaEmLote_Comp>()
            .HasKey(e => new { e.IdRequisicao, e.IdCatalogo });

            builder.Entity<FerramentariaVsLiberador>()
            .Property(f => f.IdFerramentaria)
            .HasColumnName("IdFerramentaria");

            builder.Entity<VW_Extravio_Produto>()
              .Property(f => f.Codigo)
              .HasColumnName("Código");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.AFSerial)
            .HasColumnName("AF Serial");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.MatriculaSolicitante)
            .HasColumnName("Matrícula Solicitante");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.DataEmprestimo)
            .HasColumnName("Data do Empréstimo");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.BalconistaRegistroExtravio)
            .HasColumnName("Balconista do Registro do Extravio");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.JustificativaExtravio)
            .HasColumnName("Justificativa do Extravio");

            builder.Entity<VW_Extravio_Produto>()
            .Property(f => f.DataHoraRegistroExtravio)
            .HasColumnName("Data e Hora do Registro do Extravio");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.Observacao)
            .HasColumnName("Observação");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SetorOrigem)
            .HasColumnName("Setor Origem");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SolicitanteChapa)
            .HasColumnName("Solicitante Chapa");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SolicitanteNome)
            .HasColumnName("Solicitante Nome");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SolicitanteFuncao)
            .HasColumnName("Solicitante Função");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SolicitanteSecao)
            .HasColumnName("Solicitante Seção");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.SolicitanteStatus)
            .HasColumnName("Solicitante Status");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.LiberadorChapa)
            .HasColumnName("Liberador Chapa");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.LiberadorNome)
            .HasColumnName("Liberador Nome");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.DataEmprestimo)
            .HasColumnName("Data Empréstimo");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.DataPrevistaDevolucao)
            .HasColumnName("Data Prevista Devolução");

            builder.Entity<VW_Radios_Emprestados>()
            .Property(f => f.DataVencimento)
            .HasColumnName("Data Vencimento");

            builder.Entity<FerramentariaVsLiberador>()
                    .Property(f => f.IdLogin)
                    .HasColumnName("IdLogin");
            builder.Entity<ProdutoReincluido>()
                    .Property(x => x.DataRegistro_Aprovacao)
                    .HasColumnName("DataRegistro_Aprovacao")
                    .HasColumnType("datetime");
            builder.Entity<ProdutoReincluido>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");
            builder.Entity<ProdutoReincluidoExtraviado>()
                    .Property(x => x.DataRegistro_Aprovacao)
                    .HasColumnName("DataRegistro_Aprovacao")
                    .HasColumnType("datetime");
            builder.Entity<ProdutoReincluidoExtraviado>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<BloqueioEmprestimoAoSolicitante>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<MensagemSolicitante>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<Produto>()
                    .Property(x => x.DataVencimento)
                    .HasColumnName("DataVencimento")
                    .HasColumnType("datetime");

            builder.Entity<Produto>()
                    .Property(x => x.DC_DataAquisicao)
                    .HasColumnName("DC_DataAquisicao")
                    .HasColumnType("datetime");

            builder.Entity<Produto>()
                    .Property(x => x.GC_DataInicio)
                    .HasColumnName("GC_DataInicio")
                    .HasColumnType("datetime");

            builder.Entity<Produto>()
                    .Property(x => x.GC_DataSaida)
                    .HasColumnName("GC_DataSaida")
                    .HasColumnType("datetime");

            builder.Entity<Produto>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<Empresa>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<ProdutoAlocado>()
                    .Property(x => x.DataEmprestimo)
                    .HasColumnName("DataEmprestimo")
                    .HasColumnType("datetime");

            builder.Entity<ProdutoAlocado>()
                    .Property(x => x.DataPrevistaDevolucao)
                    .HasColumnName("DataPrevistaDevolucao")
                    .HasColumnType("datetime");

            builder.Entity<ProdutoExcluido>()
           .Property(x => x.DataRegistro)
           .HasColumnName("DataRegistro")
           .HasColumnType("datetime");

            builder.Entity<ProdutoExtraviado>()
           .Property(x => x.DataRegistro)
           .HasColumnName("DataRegistro")
           .HasColumnType("datetime");

            builder.Entity<ArquivoVsProdutoAlocado>()
              .Property(x => x.DataRegistro)
              .HasColumnName("DataRegistro")
              .HasColumnType("datetime");

            builder.Entity<LogProduto>()
                    .Property(x => x.DataRegistro)
                    .HasColumnName("DataRegistro")
                    .HasColumnType("datetime");

            builder.Entity<HistoricoTransferencia>()
                   .Property(x => x.DataOcorrencia)
                   .HasColumnName("DataOcorrencia")
                   .HasColumnType("datetime");

            builder.Entity<LogAtribuicaoFerramentaria>()
                .Property(x => x.DataRegistro)
                .HasColumnName("DataRegistro")
                .HasColumnType("datetime");

            builder.Entity<Usuario>()
               .Property(x => x.DataRegistro)
               .HasColumnName("DataRegistro")
               .HasColumnType("datetime");

            builder.Entity<Relatorio>()
            .Property(r => r.RelatorioData)
            .HasColumnName("Relatorio");

            builder.Entity<LogRelatorio>()
            .Property(x => x.DataRegistro)
            .HasColumnName("DataRegistro")
            .HasColumnType("datetime");

            builder.Entity<ArquivoVsProdutoAlocado>()
            .HasKey(e => new { e.IdArquivo, e.IdProdutoAlocado });

            builder.Entity<ArquivoVsHistorico>()
            .HasKey(e => new { e.IdArquivo, e.IdHistoricoAlocacao });

            builder.Entity<VW_Radios_Emprestados>()
                .HasNoKey();

            builder.Entity<VW_Itens_Emprestados>()
                .HasNoKey();

            builder.Entity<VW_Itens_Emprestados>()
                .Property(f => f.Observacao)
                .HasColumnName("Observação");

            builder.Entity<VW_Itens_Emprestados>()
                 .Property(f => f.SetorOrigem)
                 .HasColumnName("Setor de Origem");

            builder.Entity<VW_Itens_Emprestados>()
                .Property(f => f.StatusSolicitante)
                .HasColumnName("Status do Solicitante");

            builder.Entity<VW_Itens_Emprestados>()
               .Property(f => f.ChapaSolicitante)
               .HasColumnName("Chapa do Solicitante");

            builder.Entity<VW_Itens_Emprestados>()
                .Property(f => f.NomeSolicitante)
                .HasColumnName("Nome do Solicitante");

            builder.Entity<VW_Itens_Emprestados>()
               .Property(f => f.FuncaoSolicitante)
               .HasColumnName("Função do Solicitante");

            builder.Entity<VW_Itens_Emprestados>()
               .Property(f => f.SecaoSolicitante)
               .HasColumnName("Seção do Solicitante");

            builder.Entity<VW_Itens_Emprestados>()
                 .Property(f => f.ChapaLiberador)
                 .HasColumnName("Chapa do Liberador");

            builder.Entity<VW_Itens_Emprestados>()
                 .Property(f => f.NomeLiberador)
                 .HasColumnName("Nome do Liberador");

            builder.Entity<VW_Itens_Emprestados>()
                 .Property(f => f.DataEmprestimo)
                 .HasColumnName("Data do Empréstimo");

            builder.Entity<VW_Itens_Emprestados>()
                   .Property(f => f.DataPrevistaDevolucao)
                   .HasColumnName("Data Prevista para Devolução");

            builder.Entity<VW_Itens_Emprestados>()
                  .Property(f => f.DataVencimentoProduto)
                  .HasColumnName("Data de Vencimento do Produto");


            //builder.Entity<HistoricoAlocacao_2022>()
            //     .Property(x => x.DataEmprestimo)
            //     .HasColumnName("DataEmprestimo")
            //     .HasColumnType("datetime");

            //builder.Entity<HistoricoAlocacao_2022>()
            //        .Property(x => x.DataPrevistaDevolucao)
            //        .HasColumnName("DataPrevistaDevolucao")
            //        .HasColumnType("datetime");

            //builder.Entity<HistoricoAlocacao_2022>()
            //      .Property(x => x.DataDevolucao)
            //      .HasColumnName("DataDevolucao")
            //      .HasColumnType("datetime");

            //builder.Entity<VW_Historico2016ate2020>()
            //   .HasNoKey();

            //builder.Entity<VW_Historico2016ate2020>()
            //    .Property(f => f.Observacao)
            //    .HasColumnName("Observação");

            //builder.Entity<VW_Historico2016ate2020>()
            //     .Property(f => f.SetorOrigem)
            //     .HasColumnName("Setor de Origem");

            //builder.Entity<VW_Historico2016ate2020>()
            //    .Property(f => f.StatusSolicitante)
            //    .HasColumnName("Status do Solicitante");

            //builder.Entity<VW_Historico2016ate2020>()
            //   .Property(f => f.ChapaSolicitante)
            //   .HasColumnName("Chapa do Solicitante");

            //builder.Entity<VW_Historico2016ate2020>()
            //    .Property(f => f.NomeSolicitante)
            //    .HasColumnName("Nome do Solicitante");

            //builder.Entity<VW_Historico2016ate2020>()
            //   .Property(f => f.FuncaoSolicitante)
            //   .HasColumnName("Função do Solicitante");

            //builder.Entity<VW_Historico2016ate2020>()
            //   .Property(f => f.SecaoSolicitante)
            //   .HasColumnName("Seção do Solicitante");

            //builder.Entity<VW_Historico2016ate2020>()
            //     .Property(f => f.ChapaLiberador)
            //     .HasColumnName("Chapa do Liberador");

            //builder.Entity<VW_Historico2016ate2020>()
            //     .Property(f => f.NomeLiberador)
            //     .HasColumnName("Nome do Liberador");

            //builder.Entity<VW_Historico2016ate2020>()
            //       .Property(f => f.SetorEmprestimo)
            //       .HasColumnName("Setor do Empréstimo");

            //builder.Entity<VW_Historico2016ate2020>()
            //       .Property(f => f.BalconistaEmprestimo)
            //       .HasColumnName("Balconista do Empréstimo");

            //builder.Entity<VW_Historico2016ate2020>()
            //     .Property(f => f.DataEmprestimo)
            //     .HasColumnName("Data do Empréstimo");

            //builder.Entity<VW_Historico2016ate2020>()
            //        .Property(f => f.DataPrevistaDevolucao)
            //        .HasColumnName("Data Prevista para Devolução");

            //builder.Entity<VW_Historico2016ate2020>()
            //      .Property(f => f.DataVencimentoProduto)
            //      .HasColumnName("Data de Vencimento do Produto");

            //builder.Entity<VW_Historico2016ate2020>()
            //        .Property(f => f.SetorDevolucao)
            //        .HasColumnName("Setor da Devolucao");

            //builder.Entity<VW_Historico2016ate2020>()
            //        .Property(f => f.BalconistaDevolucao)
            //        .HasColumnName("Balconista da Devolucao");

            //builder.Entity<VW_Historico2016ate2020>()
            //    .Property(f => f.DataDevolucao)
            //    .HasColumnName("Data de Devolução");

            //builder.Entity<VW_Historico2016ate2020>()
            //    .Property(f => f.StatusAtual)
            //    .HasColumnName("Status Atual");

            //builder.Entity<ArquivoVsProdutoAlocado>()
            //    .HasNoKey();

            //builder.Entity<HistoricoAlocacao>()
            //.HasDiscriminator<int>("Year")
            //.HasValue<HistoricoAlocacao_2016>(2016)
            //.HasValue<HistoricoAlocacao_2017>(2017)
            //.HasValue<HistoricoAlocacao_2018>(2018)
            //.HasValue<HistoricoAlocacao_2019>(2019)
            //.HasValue<HistoricoAlocacao_2020>(2020)
            //.HasValue<HistoricoAlocacao_2021>(2021)
            //.HasValue<HistoricoAlocacao_2022>(2022)
            //.HasValue<HistoricoAlocacao_2023>(2023);

            builder.Entity<VW_HistItensEmpDev>()
             .HasNoKey();

            builder.Entity<VW_HistItensEmpDev>()
                .Property(f => f.Codigo)
                .HasColumnName("Código");

            builder.Entity<VW_HistItensEmpDev>()
                 .Property(f => f.AF)
                 .HasColumnName("AF/Serial");

            builder.Entity<VW_HistItensEmpDev>()
                 .Property(f => f.DataEmprestimo)
                 .HasColumnName("Data do Empréstimo");

            builder.Entity<VW_HistItensEmpDev>()
                   .Property(f => f.FerrEmprestimo)
                   .HasColumnName("Ferr. Empréstimo");

            builder.Entity<VW_HistItensEmpDev>()
                .Property(f => f.StatusSolicitante)
                .HasColumnName("Status do Solicitante");

            builder.Entity<VW_HistItensEmpDev>()
               .Property(f => f.ChapaSolicitante)
               .HasColumnName("Mat. Solicitante");

            builder.Entity<VW_HistItensEmpDev>()
                .Property(f => f.NomeSolicitante)
                .HasColumnName("Nome Solicitante");

            builder.Entity<VW_HistItensEmpDev>()
               .Property(f => f.BalconistaEmprestimo)
               .HasColumnName("Balconista do Empréstimo");

            builder.Entity<VW_HistItensEmpDev>()
                 .Property(f => f.DataDevolucao)
                 .HasColumnName("Data de Devolução");

            builder.Entity<VW_HistItensEmpDev>()
                 .Property(f => f.FerrDevolucao)
                 .HasColumnName("Ferr. Devolução");

            builder.Entity<VW_HistItensEmpDev>()
                   .Property(f => f.BalcDev)
                   .HasColumnName("Balc. Dev.");

            //base.OnModelCreating(builder);

            builder.Entity<VW_HistItensTransEntreFerr>()
                    .HasNoKey();

            builder.Entity<VW_HistItensTransEntreFerr>()
                .Property(f => f.Codigo)
                .HasColumnName("Código");

            builder.Entity<VW_HistItensTransEntreFerr>()
                 .Property(f => f.AF)
                 .HasColumnName("AF/Serial");

            builder.Entity<VW_HistItensTransEntreFerr>()
                 .Property(f => f.DataOcorrencia)
                 .HasColumnName("Data do Ocorrência");

            builder.Entity<VW_HistItensTransEntreFerr>()
                 .Property(f => f.FerrOrigem)
                 .HasColumnName("Ferr. Origem");

            builder.Entity<VW_HistItensTransEntreFerr>()
               .Property(f => f.FerrDestino)
               .HasColumnName("Ferr. Destino");


            //base.OnModelCreating(builder);

            builder.Entity<VW_Exclusao_Produto>()
                .HasNoKey();

            builder.Entity<VW_Exclusao_Produto>()
                .Property(f => f.Codigo)
                .HasColumnName("Código");

            builder.Entity<VW_Exclusao_Produto>()
                 .Property(f => f.AF)
                 .HasColumnName("AF/Serial");

            builder.Entity<VW_Exclusao_Produto>()
                 .Property(f => f.DataOcorrencia)
                 .HasColumnName("Data da Ocorrência");

            builder.Entity<VW_EntradaSaida>()
                .HasNoKey();

            builder.Entity<Historico_LiberacaoExcepcional>()
                .HasNoKey();

            builder.Entity<Historico_LiberacaoExcepcional>()
               .Property(f => f.HashCode)
               .HasColumnType("varbinary(8000)");

            builder.Entity<Historico_LiberacaoExcepcional>()
           .Property(x => x.Data_Emprestimo)
           .HasColumnName("Data_Emprestimo")
           .HasColumnType("datetime");


            //2000ate2005

            //builder.Entity<VW_Historico2000ate2005>()
            //.HasNoKey();

            //builder.Entity<VW_Historico2000ate2005>()
            //    .Property(f => f.Observacao)
            //    .HasColumnName("Observação");

            //builder.Entity<VW_Historico2000ate2005>()
            //     .Property(f => f.SetorOrigem)
            //     .HasColumnName("Setor de Origem");


            //builder.Entity<VW_Historico2000ate2005>()
            //       .Property(f => f.SetorEmprestimo)
            //       .HasColumnName("Setor do Empréstimo");

            //builder.Entity<VW_Historico2000ate2005>()
            //     .Property(f => f.DataEmprestimo)
            //     .HasColumnName("Data do Empréstimo");

            //builder.Entity<VW_Historico2000ate2005>()
            //        .Property(f => f.DataPrevistaDevolucao)
            //        .HasColumnName("Data Prevista para Devolução");

            //builder.Entity<VW_Historico2000ate2005>()
            //      .Property(f => f.DataVencimentoProduto)
            //      .HasColumnName("Data de Vencimento do Produto");

            //builder.Entity<VW_Historico2000ate2005>()
            //        .Property(f => f.SetorDevolucao)
            //        .HasColumnName("Setor da Devolucao");


            //builder.Entity<VW_Historico2000ate2005>()
            //     .Property(f => f.DataDevolucao)
            //     .HasColumnName("Data de Devolução");

            //builder.Entity<VW_Historico2000ate2005>()
            //    .Property(f => f.StatusAtual)
            //    .HasColumnName("Status Atual");

            //builder.Entity<VW_Historico2000ate2005>()
            //    .Property(f => f.StatusSolicitante)
            //    .HasColumnName("Status do Solicitante");

            //builder.Entity<VW_Historico2000ate2005>()
            //   .Property(f => f.ChapaSolicitante)
            //   .HasColumnName("Chapa do Solicitante");

            //builder.Entity<VW_Historico2000ate2005>()
            //    .Property(f => f.NomeSolicitante)
            //    .HasColumnName("Nome do Solicitante");

            //builder.Entity<VW_Historico2000ate2005>()
            //   .Property(f => f.FuncaoSolicitante)
            //   .HasColumnName("Função do Solicitante");

            //builder.Entity<VW_Historico2000ate2005>()
            //   .Property(f => f.SecaoSolicitante)
            //   .HasColumnName("Seção do Solicitante");

            //builder.Entity<VW_Historico2000ate2005>()
            //     .Property(f => f.ChapaLiberador)
            //     .HasColumnName("Chapa do Liberador");

            //builder.Entity<VW_Historico2000ate2005>()
            //     .Property(f => f.NomeLiberador)
            //     .HasColumnName("Nome do Liberador");

            //builder.Entity<VW_Historico2000ate2005>()
            //       .Property(f => f.BalconistaEmprestimo)
            //       .HasColumnName("Balconista do Empréstimo");

            //builder.Entity<VW_Historico2000ate2005>()
            //        .Property(f => f.BalconistaDevolucao)
            //        .HasColumnName("Balconista da Devolucao");


            //2006ate2010

            //builder.Entity<VW_Historico2006ate2010>()
            //.HasNoKey();

            //builder.Entity<VW_Historico2006ate2010>()
            //    .Property(f => f.Observacao)
            //    .HasColumnName("Observação");

            //builder.Entity<VW_Historico2006ate2010>()
            //     .Property(f => f.SetorOrigem)
            //     .HasColumnName("Setor de Origem");


            //builder.Entity<VW_Historico2006ate2010>()
            //       .Property(f => f.SetorEmprestimo)
            //       .HasColumnName("Setor do Empréstimo");

            //builder.Entity<VW_Historico2006ate2010>()
            //     .Property(f => f.DataEmprestimo)
            //     .HasColumnName("Data do Empréstimo");

            //builder.Entity<VW_Historico2006ate2010>()
            //        .Property(f => f.DataPrevistaDevolucao)
            //        .HasColumnName("Data Prevista para Devolução");

            //builder.Entity<VW_Historico2006ate2010>()
            //      .Property(f => f.DataVencimentoProduto)
            //      .HasColumnName("Data de Vencimento do Produto");

            //builder.Entity<VW_Historico2006ate2010>()
            //        .Property(f => f.SetorDevolucao)
            //        .HasColumnName("Setor da Devolucao");


            //builder.Entity<VW_Historico2006ate2010>()
            //     .Property(f => f.DataDevolucao)
            //     .HasColumnName("Data de Devolução");

            //builder.Entity<VW_Historico2006ate2010>()
            //    .Property(f => f.StatusAtual)
            //    .HasColumnName("Status Atual");

            //builder.Entity<VW_Historico2006ate2010>()
            //    .Property(f => f.StatusSolicitante)
            //    .HasColumnName("Status do Solicitante");

            //builder.Entity<VW_Historico2006ate2010>()
            //   .Property(f => f.ChapaSolicitante)
            //   .HasColumnName("Chapa do Solicitante");

            //builder.Entity<VW_Historico2006ate2010>()
            //    .Property(f => f.NomeSolicitante)
            //    .HasColumnName("Nome do Solicitante");

            //builder.Entity<VW_Historico2006ate2010>()
            //   .Property(f => f.FuncaoSolicitante)
            //   .HasColumnName("Função do Solicitante");

            //builder.Entity<VW_Historico2006ate2010>()
            //   .Property(f => f.SecaoSolicitante)
            //   .HasColumnName("Seção do Solicitante");

            //builder.Entity<VW_Historico2006ate2010>()
            //     .Property(f => f.ChapaLiberador)
            //     .HasColumnName("Chapa do Liberador");

            //builder.Entity<VW_Historico2006ate2010>()
            //     .Property(f => f.NomeLiberador)
            //     .HasColumnName("Nome do Liberador");

            //builder.Entity<VW_Historico2006ate2010>()
            //       .Property(f => f.BalconistaEmprestimo)
            //       .HasColumnName("Balconista do Empréstimo");

            //builder.Entity<VW_Historico2006ate2010>()
            //        .Property(f => f.BalconistaDevolucao)
            //        .HasColumnName("Balconista da Devolucao");

            //2011ate2015

            //builder.Entity<VW_Historico2011ate2015>()
            //.HasNoKey();

            //builder.Entity<VW_Historico2011ate2015>()
            //    .Property(f => f.Observacao)
            //    .HasColumnName("Observação");

            //builder.Entity<VW_Historico2011ate2015>()
            //     .Property(f => f.SetorOrigem)
            //     .HasColumnName("Setor de Origem");


            //builder.Entity<VW_Historico2011ate2015>()
            //       .Property(f => f.SetorEmprestimo)
            //       .HasColumnName("Setor do Empréstimo");

            //builder.Entity<VW_Historico2011ate2015>()
            //     .Property(f => f.DataEmprestimo)
            //     .HasColumnName("Data do Empréstimo");

            //builder.Entity<VW_Historico2011ate2015>()
            //        .Property(f => f.DataPrevistaDevolucao)
            //        .HasColumnName("Data Prevista para Devolução");

            //builder.Entity<VW_Historico2011ate2015>()
            //      .Property(f => f.DataVencimentoProduto)
            //      .HasColumnName("Data de Vencimento do Produto");

            //builder.Entity<VW_Historico2011ate2015>()
            //        .Property(f => f.SetorDevolucao)
            //        .HasColumnName("Setor da Devolucao");


            //builder.Entity<VW_Historico2011ate2015>()
            //     .Property(f => f.DataDevolucao)
            //     .HasColumnName("Data de Devolução");

            //builder.Entity<VW_Historico2011ate2015>()
            //    .Property(f => f.StatusAtual)
            //    .HasColumnName("Status Atual");

            //builder.Entity<VW_Historico2011ate2015>()
            //    .Property(f => f.StatusSolicitante)
            //    .HasColumnName("Status do Solicitante");

            //builder.Entity<VW_Historico2011ate2015>()
            //   .Property(f => f.ChapaSolicitante)
            //   .HasColumnName("Chapa do Solicitante");

            //builder.Entity<VW_Historico2011ate2015>()
            //    .Property(f => f.NomeSolicitante)
            //    .HasColumnName("Nome do Solicitante");

            //builder.Entity<VW_Historico2011ate2015>()
            //   .Property(f => f.FuncaoSolicitante)
            //   .HasColumnName("Função do Solicitante");

            //builder.Entity<VW_Historico2011ate2015>()
            //   .Property(f => f.SecaoSolicitante)
            //   .HasColumnName("Seção do Solicitante");

            //builder.Entity<VW_Historico2011ate2015>()
            //     .Property(f => f.ChapaLiberador)
            //     .HasColumnName("Chapa do Liberador");

            //builder.Entity<VW_Historico2011ate2015>()
            //     .Property(f => f.NomeLiberador)
            //     .HasColumnName("Nome do Liberador");

            //builder.Entity<VW_Historico2011ate2015>()
            //       .Property(f => f.BalconistaEmprestimo)
            //       .HasColumnName("Balconista do Empréstimo");

            //builder.Entity<VW_Historico2011ate2015>()
            //        .Property(f => f.BalconistaDevolucao)
            //        .HasColumnName("Balconista da Devolucao");

            //2021ate2024

            //builder.Entity<VW_Historico2021ate2024>()
            //.HasNoKey();

            //builder.Entity<VW_Historico2021ate2024>()
            //    .Property(f => f.Observacao)
            //    .HasColumnName("Observação");

            //builder.Entity<VW_Historico2021ate2024>()
            //     .Property(f => f.SetorOrigem)
            //     .HasColumnName("Setor de Origem");


            //builder.Entity<VW_Historico2021ate2024>()
            //       .Property(f => f.SetorEmprestimo)
            //       .HasColumnName("Setor do Empréstimo");

            //builder.Entity<VW_Historico2021ate2024>()
            //     .Property(f => f.DataEmprestimo)
            //     .HasColumnName("Data do Empréstimo");

            //builder.Entity<VW_Historico2021ate2024>()
            //        .Property(f => f.DataPrevistaDevolucao)
            //        .HasColumnName("Data Prevista para Devolução");

            //builder.Entity<VW_Historico2021ate2024>()
            //      .Property(f => f.DataVencimentoProduto)
            //      .HasColumnName("Data de Vencimento do Produto");

            //builder.Entity<VW_Historico2021ate2024>()
            //        .Property(f => f.SetorDevolucao)
            //        .HasColumnName("Setor da Devolucao");


            //builder.Entity<VW_Historico2021ate2024>()
            //     .Property(f => f.DataDevolucao)
            //     .HasColumnName("Data de Devolução");

            //builder.Entity<VW_Historico2021ate2024>()
            //    .Property(f => f.StatusAtual)
            //    .HasColumnName("Status Atual");

            //builder.Entity<VW_Historico2021ate2024>()
            //    .Property(f => f.StatusSolicitante)
            //    .HasColumnName("Status do Solicitante");

            //builder.Entity<VW_Historico2021ate2024>()
            //   .Property(f => f.ChapaSolicitante)
            //   .HasColumnName("Chapa do Solicitante");

            //builder.Entity<VW_Historico2021ate2024>()
            //    .Property(f => f.NomeSolicitante)
            //    .HasColumnName("Nome do Solicitante");

            //builder.Entity<VW_Historico2021ate2024>()
            //   .Property(f => f.FuncaoSolicitante)
            //   .HasColumnName("Função do Solicitante");

            //builder.Entity<VW_Historico2021ate2024>()
            //   .Property(f => f.SecaoSolicitante)
            //   .HasColumnName("Seção do Solicitante");

            //builder.Entity<VW_Historico2021ate2024>()
            //     .Property(f => f.ChapaLiberador)
            //     .HasColumnName("Chapa do Liberador");

            //builder.Entity<VW_Historico2021ate2024>()
            //     .Property(f => f.NomeLiberador)
            //     .HasColumnName("Nome do Liberador");

            //builder.Entity<VW_Historico2021ate2024>()
            //       .Property(f => f.BalconistaEmprestimo)
            //       .HasColumnName("Balconista do Empréstimo");

            //builder.Entity<VW_Historico2021ate2024>()
            //        .Property(f => f.BalconistaDevolucao)
            //        .HasColumnName("Balconista da Devolucao");

            //Historico

            builder.Entity<VW_Historico>()
         .HasNoKey();

            builder.Entity<VW_Historico>()
                .Property(f => f.Observacao)
                .HasColumnName("Observação");

            builder.Entity<VW_Historico>()
                 .Property(f => f.SetorOrigem)
                 .HasColumnName("Setor de Origem");


            builder.Entity<VW_Historico>()
                   .Property(f => f.SetorEmprestimo)
                   .HasColumnName("Setor do Empréstimo");

            builder.Entity<VW_Historico>()
                 .Property(f => f.DataEmprestimo)
                 .HasColumnName("Data do Empréstimo");

            builder.Entity<VW_Historico>()
                    .Property(f => f.DataPrevistaDevolucao)
                    .HasColumnName("Data Prevista para Devolução");

            builder.Entity<VW_Historico>()
                  .Property(f => f.DataVencimentoProduto)
                  .HasColumnName("Data de Vencimento do Produto");

            builder.Entity<VW_Historico>()
                    .Property(f => f.SetorDevolucao)
                    .HasColumnName("Setor da Devolucao");


            builder.Entity<VW_Historico>()
                 .Property(f => f.DataDevolucao)
                 .HasColumnName("Data de Devolução");

            builder.Entity<VW_Historico>()
                .Property(f => f.StatusAtual)
                .HasColumnName("Status Atual");

            builder.Entity<VW_Historico>()
                .Property(f => f.StatusSolicitante)
                .HasColumnName("Status do Solicitante");

            builder.Entity<VW_Historico>()
               .Property(f => f.ChapaSolicitante)
               .HasColumnName("Chapa do Solicitante");

            builder.Entity<VW_Historico>()
                .Property(f => f.NomeSolicitante)
                .HasColumnName("Nome do Solicitante");

            builder.Entity<VW_Historico>()
               .Property(f => f.FuncaoSolicitante)
               .HasColumnName("Função do Solicitante");

            builder.Entity<VW_Historico>()
               .Property(f => f.SecaoSolicitante)
               .HasColumnName("Seção do Solicitante");

            builder.Entity<VW_Historico>()
                 .Property(f => f.ChapaLiberador)
                 .HasColumnName("Chapa do Liberador");

            builder.Entity<VW_Historico>()
                 .Property(f => f.NomeLiberador)
                 .HasColumnName("Nome do Liberador");

            builder.Entity<VW_Historico>()
                   .Property(f => f.BalconistaEmprestimo)
                   .HasColumnName("Balconista do Empréstimo");

            builder.Entity<VW_Historico>()
                    .Property(f => f.BalconistaDevolucao)
                    .HasColumnName("Balconista da Devolucao");


            //historico without funcionario

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                .HasNoKey();

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                .Property(f => f.Observacao)
                .HasColumnName("Observação");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                 .Property(f => f.SetorOrigem)
                 .HasColumnName("Setor de Origem");


            builder.Entity<VW_HistoricoWithoutFuncionario>()
                   .Property(f => f.SetorEmprestimo)
                   .HasColumnName("Setor do Empréstimo");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                 .Property(f => f.DataEmprestimo)
                 .HasColumnName("Data do Empréstimo");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                    .Property(f => f.DataPrevistaDevolucao)
                    .HasColumnName("Data Prevista para Devolução");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                  .Property(f => f.DataVencimentoProduto)
                  .HasColumnName("Data de Vencimento do Produto");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                    .Property(f => f.SetorDevolucao)
                    .HasColumnName("Setor da Devolucao");


            builder.Entity<VW_HistoricoWithoutFuncionario>()
                 .Property(f => f.DataDevolucao)
                 .HasColumnName("Data de Devolução");

            builder.Entity<VW_HistoricoWithoutFuncionario>()
                .Property(f => f.StatusAtual)
                .HasColumnName("Status Atual");

            //VW_ItensDevolvidos

            builder.Entity<VW_ItensDevolvidos>()
               .HasNoKey();

            builder.Entity<VW_ItensDevolvidos>()
                     .Property(f => f.Catalogo)
                     .HasColumnName("Catálogo");


            builder.Entity<VW_ItensDevolvidos>()
                     .Property(f => f.Codigo)
                     .HasColumnName("Código");


            builder.Entity<VW_ItensDevolvidos>()
                     .Property(f => f.AF)
                     .HasColumnName("AF/Serial");

            builder.Entity<VW_ItensDevolvidos>()
                  .Property(f => f.Observacao)
                  .HasColumnName("Observação");

            builder.Entity<VW_ItensDevolvidos>()
           .Property(f => f.SetorOrigem)
           .HasColumnName("Setor Origem");

            builder.Entity<VW_ItensDevolvidos>()
              .Property(f => f.SetorDevolucao)
              .HasColumnName("Setor Devolucao");

            builder.Entity<VW_ItensDevolvidos>()
       .Property(f => f.Solicitante_Chapa)
       .HasColumnName("Solicitante Chapa");

            builder.Entity<VW_ItensDevolvidos>()
.Property(f => f.Solicitante_Nome)
.HasColumnName("Solicitante Nome");

            builder.Entity<VW_ItensDevolvidos>()
.Property(f => f.DataEmprestimo)
.HasColumnName("Data Empréstimo");



            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                    .HasNoKey();

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.Catalogo)
                     .HasColumnName("Catálogo");


            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.Codigo)
                     .HasColumnName("Código");

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.AF)
                     .HasColumnName("AF/Serial");

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.Observacao)
                     .HasColumnName("Observação");

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.SetorOrigem)
                     .HasColumnName("Setor Origem");

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                     .Property(f => f.SetorDevolucao)
                     .HasColumnName("Setor Devolucao");

            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                    .Property(f => f.DataEmprestimo)
                    .HasColumnName("Data Empréstimo");


            builder.Entity<VW_ItensDevolvidosWithoutFuncionario>()
                    .Property(f => f.DataDevolucao)
                    .HasColumnName("Data Devolução");

            builder.Entity<VW_1600013295_ProdutoExcluido>()
                    .HasNoKey();

        }

    }
}
