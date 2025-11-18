using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FerramentariaTest.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertaAutomaticoVencimentoCA",
                columns: table => new
                {
                    Destinatario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UltimoEnvio = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertaAutomaticoVencimentoCA", x => x.Destinatario);
                });

            migrationBuilder.CreateTable(
                name: "Arquivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: true),
                    IdHistoricoAlocacao = table.Column<int>(type: "int", nullable: true),
                    Ano = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: true),
                    Arquivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Responsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arquivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArquivoVsHistorico",
                columns: table => new
                {
                    IdArquivo = table.Column<int>(type: "int", nullable: false),
                    IdHistoricoAlocacao = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivoVsHistorico", x => new { x.IdArquivo, x.IdHistoricoAlocacao });
                });

            migrationBuilder.CreateTable(
                name: "ArquivoVsProdutoAlocado",
                columns: table => new
                {
                    IdArquivo = table.Column<int>(type: "int", nullable: false),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivoVsProdutoAlocado", x => new { x.IdArquivo, x.IdProdutoAlocado });
                });

            migrationBuilder.CreateTable(
                name: "AuditLogsBalconista",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LogEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceIdGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogsBalconista", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloqueioEmprestimoAoSolicitante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    CodColigada = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdUsuario_Adicionou = table.Column<int>(type: "int", nullable: true),
                    IdUsuario_Excluiu = table.Column<int>(type: "int", nullable: true),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueioEmprestimoAoSolicitante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloqueioEmprestimoVsLiberador",
                columns: table => new
                {
                    IdLogin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueioEmprestimoVsLiberador", x => x.IdLogin);
                });

            migrationBuilder.CreateTable(
                name: "BloqueioEmprestimoVsLiberador_Log",
                columns: table => new
                {
                    IdRegistro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataTransacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Autorizador = table.Column<int>(type: "int", nullable: true),
                    Tabela = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueioEmprestimoVsLiberador_Log", x => x.IdRegistro);
                });

            migrationBuilder.CreateTable(
                name: "Catalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PorMetro = table.Column<int>(type: "int", nullable: true),
                    PorAferido = table.Column<int>(type: "int", nullable: true),
                    PorSerial = table.Column<int>(type: "int", nullable: true),
                    RestricaoEmprestimo = table.Column<int>(type: "int", nullable: true),
                    ImpedirDescarte = table.Column<int>(type: "int", nullable: true),
                    HabilitarDescarteEPI = table.Column<int>(type: "int", nullable: true),
                    DataDeRetornoAutomatico = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoLocal",
                columns: table => new
                {
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: false),
                    Pos1Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos1Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos1Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos2Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos2Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos2Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos3Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos3Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos3Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos4Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos4Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos4Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos5Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos5Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos5Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos6Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos6Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos6Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos7Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos7Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos7Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos8Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos8Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos8Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos9Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos9Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos9Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos10Prateleira = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos10Coluna = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Pos10Linha = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoLocal", x => new { x.IdCatalogo, x.IdFerramentaria });
                });

            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: true),
                    Classe = table.Column<int>(type: "int", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControleCA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    NumeroCA = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Validade = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsavel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControleCA", x => new { x.Id, x.IdCatalogo });
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Gerente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntradaEmLote_Comp",
                columns: table => new
                {
                    IdRequisicao = table.Column<int>(type: "int", nullable: false),
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaEmLote_Comp", x => new { x.IdRequisicao, x.IdCatalogo });
                });

            migrationBuilder.CreateTable(
                name: "EntradaEmLote_Req",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: false),
                    RFM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    IdSolicitante = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaEmLote_Req", x => new { x.Id, x.IdFerramentaria });
                });

            migrationBuilder.CreateTable(
                name: "EntradaEmLote_Temp",
                columns: table => new
                {
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdRequisicao = table.Column<int>(type: "int", nullable: true),
                    IdCatalogo = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnidadeAfericao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Propriedade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Certificado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DC_DataAquisicao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DC_Valor = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    DC_Fornecedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaEmLote_Temp", x => x.DataRegistro);
                });

            migrationBuilder.CreateTable(
                name: "ExcludedObra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdObra = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcludedObra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ferramentaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ferramentaria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FerramentariaVsLiberador",
                columns: table => new
                {
                    IdFerramentaria = table.Column<int>(type: "int", nullable: false),
                    IdLogin = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FerramentariaVsLiberador", x => new { x.IdFerramentaria, x.IdLogin });
                });

            migrationBuilder.CreateTable(
                name: "Historico_LiberacaoExcepcional",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Setor_Origem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chapa_Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome_Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Funcao_Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Secao_Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chapa_Liberador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome_Liberador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Funcao_Liberador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Secao_Liberador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balconista_Emprestimo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Obra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data_Emprestimo = table.Column<DateTime>(type: "datetime", nullable: true),
                    Liberacao_Excepcional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashCode = table.Column<byte[]>(type: "varbinary(8000)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2000",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2000", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2001",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2001", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2002",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2002", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2003",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2003", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2004",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2004", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2005",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2005", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2006",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2006", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2007",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2007", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2008",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2008", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2009",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2009", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2010",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2010", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2011",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2011", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2012",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2012", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2013",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2013", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2014",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2014", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2015",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2015", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2016",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2016", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2017",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2017", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2018",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2018", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2019",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2019", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2020",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2020", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2021",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2021", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2022",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2022", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2023",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2023", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2024",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2024", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoAlocacao_2025",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdDevolvido = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmprestimoTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoAlocacao_2025", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoTransferencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    DataOcorrencia = table.Column<DateTime>(type: "datetime", nullable: true),
                    IdFerramentariaOrigem = table.Column<int>(type: "int", nullable: true),
                    IdFerramentariaDestino = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Saldo = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoTransferencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodPessoa = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUser = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderMemberRel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdLeader = table.Column<int>(type: "int", nullable: true),
                    CodPessoa = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderMemberRel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogWhat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogWhy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogWhere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogWhen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogWho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogHow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogHowMuch = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogArquivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdArquivo = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogArquivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogAtribuicaoFerramentaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioResponsavel = table.Column<int>(type: "int", nullable: true),
                    Acao = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogAtribuicaoFerramentaria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogEntradaSaida",
                columns: table => new
                {
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Rfm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntradaSaida", x => x.DataRegistro);
                });

            migrationBuilder.CreateTable(
                name: "LogProduto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    AfDe = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AfPara = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PatDe = table.Column<int>(type: "int", nullable: true),
                    PatPara = table.Column<int>(type: "int", nullable: true),
                    QuantidadeDe = table.Column<int>(type: "int", nullable: true),
                    QuantidadePara = table.Column<int>(type: "int", nullable: true),
                    QuantidadeMinimaDe = table.Column<int>(type: "int", nullable: true),
                    QuantidadeMinimaPara = table.Column<int>(type: "int", nullable: true),
                    LocalizacaoDe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LocalizacaoPara = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataVencimentoDe = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    DataVencimentoPara = table.Column<string>(type: "nvarchar(27)", maxLength: 27, nullable: true),
                    TagDe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TagPara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SerieDe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SeriePara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdUnidadeAfericaoDe = table.Column<int>(type: "int", nullable: true),
                    IdUnidadeAfericaoPara = table.Column<int>(type: "int", nullable: true),
                    IdModeloDe = table.Column<int>(type: "int", nullable: true),
                    IdModeloPara = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Acao = table.Column<int>(type: "int", nullable: true),
                    RfmDe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RfmPara = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ObservacaoDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObservacaoPara = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogProduto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogRelatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Relatorio = table.Column<int>(type: "int", nullable: true),
                    Arquivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processar = table.Column<int>(type: "int", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogRelatorio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MensagemSolicitante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    CodColigada = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdUsuario_Adicionou = table.Column<int>(type: "int", nullable: true),
                    IdUsuario_Excluiu = table.Column<int>(type: "int", nullable: true),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Fixar = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagemSolicitante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Obra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductReservation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreparedBy = table.Column<int>(type: "int", nullable: true),
                    HandedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    FinalQuantity = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReservation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCatalogo = table.Column<int>(type: "int", nullable: true),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    QuantidadeMinima = table.Column<int>(type: "int", nullable: true),
                    Localizacao = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "datetime", nullable: true),
                    TAG = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Selo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdModelo = table.Column<int>(type: "int", nullable: true),
                    IdEmpresa = table.Column<int>(type: "int", nullable: true),
                    IdUnidadeAfericao = table.Column<int>(type: "int", nullable: true),
                    RFM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DC_DataAquisicao = table.Column<DateTime>(type: "datetime", nullable: true),
                    DC_Valor = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    DC_AssetNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DC_Fornecedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GC_Contrato = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GC_DataInicio = table.Column<DateTime>(type: "datetime", nullable: true),
                    GC_IdObra = table.Column<int>(type: "int", nullable: true),
                    GC_OC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GC_DataSaida = table.Column<DateTime>(type: "datetime", nullable: true),
                    GC_NFSaida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    Certificado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoAlocado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    IdControleCA = table.Column<int>(type: "int", nullable: true),
                    IdFerrOndeProdRetirado = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Balconista_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Chave = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdReservation = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CrachaNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoAlocado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoExcluido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoExclusao = table.Column<int>(type: "int", nullable: true),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoExcluido", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoExtraviado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProdutoExcluido = table.Column<int>(type: "int", nullable: true),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoExtraviado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoReincluido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdUsuario_Solicitante = table.Column<int>(type: "int", nullable: true),
                    IdUsuario_Aprovador = table.Column<int>(type: "int", nullable: true),
                    DataRegistro_Aprovacao = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoReincluido", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoReincluidoExtraviado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdUsuario_Solicitante = table.Column<int>(type: "int", nullable: true),
                    IdUsuario_Aprovador = table.Column<int>(type: "int", nullable: true),
                    DataRegistro_Aprovacao = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoReincluidoExtraviado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoVsMonitor",
                columns: table => new
                {
                    IdLogin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProduto = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoVsMonitor", x => x.IdLogin);
                });

            migrationBuilder.CreateTable(
                name: "Relatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Relatorio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Arquivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processar = table.Column<int>(type: "int", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessoDataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessoDataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SAMAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relatorio_LogEntradaSaida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Arquivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Processar = table.Column<int>(type: "int", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessoDataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessoDataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatorio_LogEntradaSaida", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationControl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdLeaderData = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Chave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationControl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdReservationControl = table.Column<int>(type: "int", nullable: true),
                    IdLeaderMemberRel = table.Column<int>(type: "int", nullable: true),
                    IdCatalogo = table.Column<int>(type: "int", nullable: true),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Chave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdObra = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SP_1600012731_Estoque",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Controle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Saldo = table.Column<int>(type: "int", nullable: true),
                    QuantidadeMinima = table.Column<int>(type: "int", nullable: true),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    DatadoRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DatadeVencimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumerodeSerie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Selo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Certificado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true),
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: true),
                    Extraviado = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TermsControl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balconista = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodPessoa = table.Column<int>(type: "int", nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsControl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoExclusao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoExclusao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    CodColigada = table.Column<int>(type: "int", nullable: true),
                    Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReinicializarSenha = table.Column<int>(type: "int", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VirtualFerrmantaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFerramentaria = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualFerrmantaria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VW_1600013295_ProdutoExcluido",
                columns: table => new
                {
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Justificativa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataOcorrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsavel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdResponsavel = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_EntradaSaida",
                columns: table => new
                {
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Movimento = table.Column<int>(type: "int", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataOcorrencia = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Estoque_Positivo",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    SetorOrigem = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Estoque_Residual",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    SetorOrigem = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Exclusao_Produto",
                columns: table => new
                {
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AFSerial = table.Column<string>(name: "AF/Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Suporte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Justificativa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatadaOcorrência = table.Column<DateTime>(name: "Data da Ocorrência", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Extravio_Produto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descrição = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    AFSerial = table.Column<string>(name: "AF Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Obs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatrículaSolicitante = table.Column<string>(name: "Matrícula Solicitante", type: "nvarchar(max)", nullable: true),
                    DatadoEmpréstimo = table.Column<DateTime>(name: "Data do Empréstimo", type: "datetime2", nullable: true),
                    BalconistadoRegistrodoExtravio = table.Column<string>(name: "Balconista do Registro do Extravio", type: "nvarchar(max)", nullable: true),
                    JustificativadoExtravio = table.Column<string>(name: "Justificativa do Extravio", type: "nvarchar(max)", nullable: true),
                    DataeHoradoRegistrodoExtravio = table.Column<DateTime>(name: "Data e Hora do Registro do Extravio", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_Extravio_Produto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VW_Ferramentaria_Ass_Solda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_Ferramentaria_Ass_Solda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VW_HistItensEmpDev",
                columns: table => new
                {
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AFSerial = table.Column<string>(name: "AF/Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    DatadoEmpréstimo = table.Column<DateTime>(name: "Data do Empréstimo", type: "datetime2", nullable: true),
                    FerrEmpréstimo = table.Column<string>(name: "Ferr. Empréstimo", type: "nvarchar(max)", nullable: true),
                    StatusdoSolicitante = table.Column<string>(name: "Status do Solicitante", type: "nvarchar(max)", nullable: true),
                    MatSolicitante = table.Column<string>(name: "Mat. Solicitante", type: "nvarchar(max)", nullable: true),
                    NomeSolicitante = table.Column<string>(name: "Nome Solicitante", type: "nvarchar(max)", nullable: true),
                    BalconistadoEmpréstimo = table.Column<string>(name: "Balconista do Empréstimo", type: "nvarchar(max)", nullable: true),
                    DatadeDevolução = table.Column<DateTime>(name: "Data de Devolução", type: "datetime2", nullable: true),
                    FerrDevolução = table.Column<string>(name: "Ferr. Devolução", type: "nvarchar(max)", nullable: true),
                    BalcDev = table.Column<string>(name: "Balc. Dev.", type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_HistItensTransEntreFerr",
                columns: table => new
                {
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AFSerial = table.Column<string>(name: "AF/Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    DatadoOcorrência = table.Column<DateTime>(name: "Data do Ocorrência", type: "datetime2", nullable: true),
                    FerrOrigem = table.Column<string>(name: "Ferr. Origem", type: "nvarchar(max)", nullable: true),
                    FerrDestino = table.Column<string>(name: "Ferr. Destino", type: "nvarchar(max)", nullable: true),
                    Suporte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Historico",
                columns: table => new
                {
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidadeCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetordeOrigem = table.Column<string>(name: "Setor de Origem", type: "nvarchar(max)", nullable: true),
                    StatusdoSolicitante = table.Column<string>(name: "Status do Solicitante", type: "nvarchar(max)", nullable: true),
                    ChapadoSolicitante = table.Column<string>(name: "Chapa do Solicitante", type: "nvarchar(max)", nullable: true),
                    NomedoSolicitante = table.Column<string>(name: "Nome do Solicitante", type: "nvarchar(max)", nullable: true),
                    FunçãodoSolicitante = table.Column<string>(name: "Função do Solicitante", type: "nvarchar(max)", nullable: true),
                    SeçãodoSolicitante = table.Column<string>(name: "Seção do Solicitante", type: "nvarchar(max)", nullable: true),
                    ChapadoLiberador = table.Column<string>(name: "Chapa do Liberador", type: "nvarchar(max)", nullable: true),
                    NomedoLiberador = table.Column<string>(name: "Nome do Liberador", type: "nvarchar(max)", nullable: true),
                    SetordoEmpréstimo = table.Column<string>(name: "Setor do Empréstimo", type: "nvarchar(max)", nullable: true),
                    BalconistadoEmpréstimo = table.Column<string>(name: "Balconista do Empréstimo", type: "nvarchar(max)", nullable: true),
                    Obra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatadoEmpréstimo = table.Column<DateTime>(name: "Data do Empréstimo", type: "datetime2", nullable: true),
                    DataPrevistaparaDevolução = table.Column<DateTime>(name: "Data Prevista para Devolução", type: "datetime2", nullable: true),
                    DatadeVencimentodoProduto = table.Column<DateTime>(name: "Data de Vencimento do Produto", type: "datetime2", nullable: true),
                    SetordaDevolucao = table.Column<string>(name: "Setor da Devolucao", type: "nvarchar(max)", nullable: true),
                    BalconistadaDevolucao = table.Column<string>(name: "Balconista da Devolucao", type: "nvarchar(max)", nullable: true),
                    DatadeDevolução = table.Column<DateTime>(name: "Data de Devolução", type: "datetime2", nullable: true),
                    StatusAtual = table.Column<string>(name: "Status Atual", type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_HistoricoWithoutFuncionario",
                columns: table => new
                {
                    IdProduto = table.Column<int>(type: "int", nullable: true),
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidadeCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetordeOrigem = table.Column<string>(name: "Setor de Origem", type: "nvarchar(max)", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Liberador_Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Liberador_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Liberador_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Balconista_Emprestimo_IdLogin = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    SetordoEmpréstimo = table.Column<string>(name: "Setor do Empréstimo", type: "nvarchar(max)", nullable: true),
                    Obra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatadoEmpréstimo = table.Column<DateTime>(name: "Data do Empréstimo", type: "datetime2", nullable: true),
                    DataPrevistaparaDevolução = table.Column<DateTime>(name: "Data Prevista para Devolução", type: "datetime2", nullable: true),
                    DatadeVencimentodoProduto = table.Column<DateTime>(name: "Data de Vencimento do Produto", type: "datetime2", nullable: true),
                    SetordaDevolucao = table.Column<string>(name: "Setor da Devolucao", type: "nvarchar(max)", nullable: true),
                    DatadeDevolução = table.Column<DateTime>(name: "Data de Devolução", type: "datetime2", nullable: true),
                    StatusAtual = table.Column<string>(name: "Status Atual", type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Itens_Emprestados",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidadeCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetordeOrigem = table.Column<string>(name: "Setor de Origem", type: "nvarchar(max)", nullable: true),
                    StatusdoSolicitante = table.Column<string>(name: "Status do Solicitante", type: "nvarchar(max)", nullable: true),
                    ChapadoSolicitante = table.Column<string>(name: "Chapa do Solicitante", type: "nvarchar(max)", nullable: true),
                    NomedoSolicitante = table.Column<string>(name: "Nome do Solicitante", type: "nvarchar(max)", nullable: true),
                    FunçãodoSolicitante = table.Column<string>(name: "Função do Solicitante", type: "nvarchar(max)", nullable: true),
                    SeçãodoSolicitante = table.Column<string>(name: "Seção do Solicitante", type: "nvarchar(max)", nullable: true),
                    ChapadoLiberador = table.Column<string>(name: "Chapa do Liberador", type: "nvarchar(max)", nullable: true),
                    NomedoLiberador = table.Column<string>(name: "Nome do Liberador", type: "nvarchar(max)", nullable: true),
                    Balconista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Obra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatadoEmpréstimo = table.Column<DateTime>(name: "Data do Empréstimo", type: "datetime2", nullable: true),
                    DataPrevistaparaDevolução = table.Column<DateTime>(name: "Data Prevista para Devolução", type: "datetime2", nullable: true),
                    DatadeVencimentodoProduto = table.Column<DateTime>(name: "Data de Vencimento do Produto", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_ItensDevolvidos",
                columns: table => new
                {
                    Catálogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AFSerial = table.Column<string>(name: "AF/Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetorOrigem = table.Column<string>(name: "Setor Origem", type: "nvarchar(max)", nullable: true),
                    SetorDevolucao = table.Column<string>(name: "Setor Devolucao", type: "nvarchar(max)", nullable: true),
                    SolicitanteChapa = table.Column<string>(name: "Solicitante Chapa", type: "nvarchar(max)", nullable: true),
                    SolicitanteNome = table.Column<string>(name: "Solicitante Nome", type: "nvarchar(max)", nullable: true),
                    DataEmpréstimo = table.Column<DateTime>(name: "Data Empréstimo", type: "datetime2", nullable: true),
                    BalconistaDevolução = table.Column<string>(name: "Balconista Devolução", type: "nvarchar(max)", nullable: true),
                    DataDevolução = table.Column<DateTime>(name: "Data Devolução", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_ItensDevolvidosWithoutFuncionario",
                columns: table => new
                {
                    Catálogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Código = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AFSerial = table.Column<string>(name: "AF/Serial", type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetorOrigem = table.Column<string>(name: "Setor Origem", type: "nvarchar(max)", nullable: true),
                    SetorDevolucao = table.Column<string>(name: "Setor Devolucao", type: "nvarchar(max)", nullable: true),
                    Solicitante_Chapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solicitante_CodColigada = table.Column<int>(type: "int", nullable: true),
                    Solicitante_IdTerceiro = table.Column<int>(type: "int", nullable: true),
                    Balconista_Devolucao_IdLogin = table.Column<int>(type: "int", nullable: true),
                    DataEmpréstimo = table.Column<DateTime>(name: "Data Empréstimo", type: "datetime2", nullable: true),
                    DataDevolução = table.Column<DateTime>(name: "Data Devolução", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Radios_Emprestados",
                columns: table => new
                {
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Produto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Observação = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetorOrigem = table.Column<string>(name: "Setor Origem", type: "nvarchar(max)", nullable: true),
                    SolicitanteChapa = table.Column<string>(name: "Solicitante Chapa", type: "nvarchar(max)", nullable: true),
                    SolicitanteNome = table.Column<string>(name: "Solicitante Nome", type: "nvarchar(max)", nullable: true),
                    SolicitanteFunção = table.Column<string>(name: "Solicitante Função", type: "nvarchar(max)", nullable: true),
                    SolicitanteSeção = table.Column<string>(name: "Solicitante Seção", type: "nvarchar(max)", nullable: true),
                    SolicitanteStatus = table.Column<string>(name: "Solicitante Status", type: "nvarchar(max)", nullable: true),
                    LiberadorChapa = table.Column<string>(name: "Liberador Chapa", type: "nvarchar(max)", nullable: true),
                    LiberadorNome = table.Column<string>(name: "Liberador Nome", type: "nvarchar(max)", nullable: true),
                    Balconista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataEmpréstimo = table.Column<DateTime>(name: "Data Empréstimo", type: "datetime2", nullable: true),
                    DataPrevistaDevolução = table.Column<DateTime>(name: "Data Prevista Devolução", type: "datetime2", nullable: true),
                    DataVencimento = table.Column<DateTime>(name: "Data Vencimento", type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_Reativacao_Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AF = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Saldo = table.Column<int>(type: "int", nullable: true),
                    Controle = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LocalEmEstoque = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Justificativa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DataInativacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Justificativa_Reativacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MatriculaFuncionarioEmprestimo = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_Reativacao_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VW_Reativacao_Item_Extraviado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AF = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    Saldo = table.Column<int>(type: "int", nullable: true),
                    Controle = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LocalEmEstoque = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Justificativa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DataInativacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Justificativa_Reativacao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MatriculaFuncionarioEmprestimo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IdProdutoAlocado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_Reativacao_Item_Extraviado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VW_Todos_Radios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ferramentaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfSerial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PAT = table.Column<int>(type: "int", nullable: true),
                    QtdEstoque = table.Column<int>(type: "int", nullable: true),
                    QtdMinEstoque = table.Column<int>(type: "int", nullable: true),
                    ControlePor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataValidade = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroSerie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DC_DataAquisicao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DC_Valor = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    DC_AssetNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DC_Fornecedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_Contrato = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_DataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_OC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_DataSaida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GC_NFSaida = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataEmprestimo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SolicitanteChapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolicitanteNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiberadorChapa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiberadorNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balconista = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VW_Todos_Radios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertaAutomaticoVencimentoCA");

            migrationBuilder.DropTable(
                name: "Arquivo");

            migrationBuilder.DropTable(
                name: "ArquivoVsHistorico");

            migrationBuilder.DropTable(
                name: "ArquivoVsProdutoAlocado");

            migrationBuilder.DropTable(
                name: "AuditLogsBalconista");

            migrationBuilder.DropTable(
                name: "BloqueioEmprestimoAoSolicitante");

            migrationBuilder.DropTable(
                name: "BloqueioEmprestimoVsLiberador");

            migrationBuilder.DropTable(
                name: "BloqueioEmprestimoVsLiberador_Log");

            migrationBuilder.DropTable(
                name: "Catalogo");

            migrationBuilder.DropTable(
                name: "CatalogoLocal");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "ControleCA");

            migrationBuilder.DropTable(
                name: "Empresa");

            migrationBuilder.DropTable(
                name: "EntradaEmLote_Comp");

            migrationBuilder.DropTable(
                name: "EntradaEmLote_Req");

            migrationBuilder.DropTable(
                name: "EntradaEmLote_Temp");

            migrationBuilder.DropTable(
                name: "ExcludedObra");

            migrationBuilder.DropTable(
                name: "Ferramentaria");

            migrationBuilder.DropTable(
                name: "FerramentariaVsLiberador");

            migrationBuilder.DropTable(
                name: "Historico_LiberacaoExcepcional");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2000");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2001");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2002");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2003");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2004");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2005");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2006");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2007");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2008");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2009");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2010");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2011");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2012");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2013");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2014");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2015");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2016");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2017");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2018");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2019");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2020");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2021");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2022");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2023");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2024");

            migrationBuilder.DropTable(
                name: "HistoricoAlocacao_2025");

            migrationBuilder.DropTable(
                name: "HistoricoTransferencia");

            migrationBuilder.DropTable(
                name: "LeaderData");

            migrationBuilder.DropTable(
                name: "LeaderMemberRel");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "LogArquivo");

            migrationBuilder.DropTable(
                name: "LogAtribuicaoFerramentaria");

            migrationBuilder.DropTable(
                name: "LogEntradaSaida");

            migrationBuilder.DropTable(
                name: "LogProduto");

            migrationBuilder.DropTable(
                name: "LogRelatorio");

            migrationBuilder.DropTable(
                name: "Marca");

            migrationBuilder.DropTable(
                name: "MensagemSolicitante");

            migrationBuilder.DropTable(
                name: "Obra");

            migrationBuilder.DropTable(
                name: "ProductReservation");

            migrationBuilder.DropTable(
                name: "Produto");

            migrationBuilder.DropTable(
                name: "ProdutoAlocado");

            migrationBuilder.DropTable(
                name: "ProdutoExcluido");

            migrationBuilder.DropTable(
                name: "ProdutoExtraviado");

            migrationBuilder.DropTable(
                name: "ProdutoReincluido");

            migrationBuilder.DropTable(
                name: "ProdutoReincluidoExtraviado");

            migrationBuilder.DropTable(
                name: "ProdutoVsMonitor");

            migrationBuilder.DropTable(
                name: "Relatorio");

            migrationBuilder.DropTable(
                name: "Relatorio_LogEntradaSaida");

            migrationBuilder.DropTable(
                name: "ReservationControl");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "SP_1600012731_Estoque");

            migrationBuilder.DropTable(
                name: "TermsControl");

            migrationBuilder.DropTable(
                name: "TipoExclusao");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "VirtualFerrmantaria");

            migrationBuilder.DropTable(
                name: "VW_1600013295_ProdutoExcluido");

            migrationBuilder.DropTable(
                name: "VW_EntradaSaida");

            migrationBuilder.DropTable(
                name: "VW_Estoque_Positivo");

            migrationBuilder.DropTable(
                name: "VW_Estoque_Residual");

            migrationBuilder.DropTable(
                name: "VW_Exclusao_Produto");

            migrationBuilder.DropTable(
                name: "VW_Extravio_Produto");

            migrationBuilder.DropTable(
                name: "VW_Ferramentaria_Ass_Solda");

            migrationBuilder.DropTable(
                name: "VW_HistItensEmpDev");

            migrationBuilder.DropTable(
                name: "VW_HistItensTransEntreFerr");

            migrationBuilder.DropTable(
                name: "VW_Historico");

            migrationBuilder.DropTable(
                name: "VW_HistoricoWithoutFuncionario");

            migrationBuilder.DropTable(
                name: "VW_Itens_Emprestados");

            migrationBuilder.DropTable(
                name: "VW_ItensDevolvidos");

            migrationBuilder.DropTable(
                name: "VW_ItensDevolvidosWithoutFuncionario");

            migrationBuilder.DropTable(
                name: "VW_Radios_Emprestados");

            migrationBuilder.DropTable(
                name: "VW_Reativacao_Item");

            migrationBuilder.DropTable(
                name: "VW_Reativacao_Item_Extraviado");

            migrationBuilder.DropTable(
                name: "VW_Todos_Radios");
        }
    }
}
