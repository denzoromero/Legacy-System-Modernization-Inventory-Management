using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.EntitySeek;

using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;

namespace FerramentariaTest.DAL.Demo
{
    public class DemoDataSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DemoDataSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DemoDB>();

            //var contextSeek = scope.ServiceProvider.GetRequiredService<ContextoBancoSeek>();

            await SeedDemoData(context);
            //await SeedDemoSeek(contextSeek);
        }

        //private async Task SeedDemoSeek(ContextoBancoSeek contextSeek)
        //{
        //    if (!contextSeek.Funcionario.Any())
        //    {
        //        contextSeek.AddRange(
        //            new FuncionarioSeek
        //            {
        //                Id = 1,
        //                Chapa = "01010102391",
        //                Nome = "Third Party",
        //                IdSecao = 1,
        //                IdFuncao = 1,
        //                Observacao = null,
        //                DataRegistro = DateTime.Now,
        //                Ativo = 1,
        //                CodFuncao = null,
        //                CodSecao = null,
        //            }
        //            );
        //    }

        //    if (!contextSeek.Funcionario.Any())
        //    {
        //        contextSeek.Funcao.AddRange(
        //            new Funcao
        //            {
        //                Id = 1,
        //                Nome = "NA",
        //                DataRegistro = DateTime.Now,
        //                Ativo = 1
        //            }
        //            );
        //    }

        //    if (!contextSeek.Secao.Any())
        //    {
        //        contextSeek.Secao.AddRange(
        //            new Secao
        //            {
        //                Id = 1,
        //                Nome = "NA",
        //                DataRegistro = DateTime.Now,
        //                Ativo = 1
        //            }
        //            );
        //    }


        //}

        private async Task SeedDemoData(DemoDB context)
        {
            if (!context.Usuario.Any())
            {
                context.AddRange(
                    new UsuarioBS { Id = 1, IdTerceiro = 0, CodColigada = 2, Chapa = "admin.demo", Senha = "admin123", Email = "demoAdmin@sample.com", DataRegistro = DateTime.Now, Ativo = 1 },
                    new UsuarioBS { Id = 2, IdTerceiro = 0, CodColigada = 2, Chapa = "user.demo", Senha = "user123", Email = "demoUser@sample.com", DataRegistro = DateTime.Now, Ativo = 1 }
                );

            }

            if (!context.Obra.Any())
            {
                context.Obra.Add(
                    new Obra { Id = 1, Nome = "Project Demo", Codigo = "DEMO", Ativo = 1, DataRegistro = DateTime.Now }
                    );
            }

            if (!context.Ferramentaria.Any())
            {
                context.Ferramentaria.Add(
                    new Ferramentaria { Id = 1, Nome = "Central Shop", Ativo = 1, DataRegistro = DateTime.Now });
            }

            if (!context.Categoria.Any())
            {
                context.Categoria.AddRange(
                    new Categoria { Id = 1, IdCategoria = 0, Classe = 3, Nome = "Consumable", DataRegistro = DateTime.Now, Ativo = 1 }
                    );
            }

            if (!context.Catalogo.Any())
            {
                context.Catalogo.AddRange(
                    new Catalogo
                    {
                        Id = 1,
                        IdCategoria = 1,
                        Codigo = "01.01.01.00025",
                        Nome = "Gloves",
                        Descricao = "",
                        PorMetro = 0,
                        PorAferido = 0,
                        PorSerial = 0,
                        RestricaoEmprestimo = 0,
                        ImpedirDescarte = 0,
                        HabilitarDescarteEPI = 0,
                        DataDeRetornoAutomatico = 0,
                        DataRegistro = DateTime.Now,
                        Ativo = 1,
                        ImageData = null
                    },
                       new Catalogo
                       {
                           Id = 2,
                           IdCategoria = 1,
                           Codigo = "01.01.01.00001",
                           Nome = "Shirt",
                           Descricao = "",
                           PorMetro = 0,
                           PorAferido = 0,
                           PorSerial = 0,
                           RestricaoEmprestimo = 0,
                           ImpedirDescarte = 0,
                           HabilitarDescarteEPI = 0,
                           DataDeRetornoAutomatico = 0,
                           DataRegistro = DateTime.Now,
                           Ativo = 1,
                           ImageData = null
                       }
                );
            }

            if (!context.Produto.Any())
            {
                context.Produto.AddRange(
                 new Produto
                 {
                     Id = 1,
                     IdCatalogo = 1,
                     IdFerramentaria = 1,
                     AF = "",
                     PAT = 0,
                     Quantidade = 40,
                     QuantidadeMinima = 1,
                     Localizacao = "",
                     DataVencimento = null,
                     TAG = null,
                     Serie = null,
                     Selo = null,
                     IdModelo = null,
                     IdEmpresa = null,
                     IdUnidadeAfericao = 0,
                     RFM = "",
                     Observacao = "",
                     DataRegistro = DateTime.Now,
                     Ativo = 1
                 },
                                  new Produto
                                  {
                                      Id = 2,
                                      IdCatalogo = 2,
                                      IdFerramentaria = 1,
                                      AF = "",
                                      PAT = 0,
                                      Quantidade = 23,
                                      QuantidadeMinima = 1,
                                      Localizacao = "",
                                      DataVencimento = null,
                                      TAG = null,
                                      Serie = null,
                                      Selo = null,
                                      IdModelo = null,
                                      IdEmpresa = null,
                                      IdUnidadeAfericao = 0,
                                      RFM = "",
                                      Observacao = "",
                                      DataRegistro = DateTime.Now,
                                      Ativo = 1
                                  }
                    );
            }

            if (!context.LeaderData.Any())
            {
                context.LeaderData.AddRange(
                    new LeaderData { Id = 1, CodPessoa = 00000, Chapa = "00011", Nome = "Admin Demo", Ativo = 1, DataRegistro = DateTime.Now, IdUser = 1 },
                    new LeaderData { Id = 2, CodPessoa = 11111, Chapa = "00022", Nome = "User Demo", Ativo = 1, DataRegistro = DateTime.Now, IdUser = 2 }
                );
            }

            if (!context.ReservationControl.Any())
            {
                context.ReservationControl.AddRange(
                  new ReservationControl { Id = 1, IdLeaderData = 1, Chave = "abcde-asdgf", Status = 0, ExpirationDate = DateTime.Now.AddDays(3), DataRegistro = DateTime.Now, Type = 1 },
                  new ReservationControl { Id = 2, IdLeaderData = 2, Chave = "qwerty-aslke", Status = 0, ExpirationDate = DateTime.Now.AddDays(3), DataRegistro = DateTime.Now, Type = 2 },
                  new ReservationControl { Id = 3, IdLeaderData = 1, Chave = "qwerty-aslke", Status = 0, ExpirationDate = DateTime.Now.AddDays(3), DataRegistro = DateTime.Now, Type = 1 }
              );
            }

            if (!context.Reservations.Any())
            {
                context.Reservations.AddRange(
                new Reservations { Id = 1, IdReservationControl = 1, Chave = "abcde-asdgf", Status = 0, IdLeaderMemberRel = 1, IdCatalogo = 1,
                    IdFerramentaria = 1, Quantidade = 1, DataRegistro = DateTime.Now, IdObra = 1 },
                new Reservations { Id = 2, IdReservationControl = 2, Chave = "qwerty-aslke", Status = 0, IdLeaderMemberRel = 3,
                    IdCatalogo = 2,
                    IdFerramentaria = 1,
                    Quantidade = 1,
                    DataRegistro = DateTime.Now,
                    IdObra = 1
                },
                  new Reservations
                  {
                      Id = 3,
                      IdReservationControl = 3,
                      Chave = "qwerty-aslke",
                      Status = 1,
                      IdLeaderMemberRel = 1,
                      IdCatalogo = 2,
                      IdFerramentaria = 1,
                      Quantidade = 1,
                      DataRegistro = DateTime.Now,
                      IdObra = 1
                  }, 
                  new Reservations
                  {
                      Id = 4,
                      IdReservationControl = 3,
                      Chave = "qwerty-aslke",
                      Status = 2,
                      IdLeaderMemberRel = 1,
                      IdCatalogo = 2,
                      IdFerramentaria = 1,
                      Quantidade = 1,
                      DataRegistro = DateTime.Now,
                      IdObra = 1
                  }
                );
            }

            if (!context.LeaderMemberRel.Any())
            {
                context.LeaderMemberRel.AddRange(
                    new LeaderMemberRel { Id = 1, IdLeader = 1, CodPessoa = 12345, Chapa = "21212", Nome = "Jane Doe", Ativo = 1, DataRegistro = DateTime.Now },
                    new LeaderMemberRel { Id = 2, IdLeader = 1, CodPessoa = 54321, Chapa = "31313", Nome = "Kazuha Le sserafim", Ativo = 1, DataRegistro = DateTime.Now, },
                    new LeaderMemberRel { Id = 3, IdLeader = 2, CodPessoa = 09876, Chapa = "41414", Nome = "Sakura Le sserafim", Ativo = 1, DataRegistro = DateTime.Now, },
                    new LeaderMemberRel { Id = 4, IdLeader = 2, CodPessoa = 67890, Chapa = "51515", Nome = "Chaewon Le sserafim", Ativo = 1, DataRegistro = DateTime.Now, }
                );
            }

            if (!context.Funcionario.Any())
            {
                context.AddRange(
                    new Funcionario { CodPessoa = 00000, CodColigada = 2, Chapa = "00011", Nome = "Admin Demo", CodRecebimento = "M", CodSituacao = "A", Secao = "IT", Funcao = "Admin", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 11111, CodColigada = 2, Chapa = "00022", Nome = "User Demo", CodRecebimento = "M", CodSituacao = "A", Secao = "IT", Funcao = "Staff", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 22222, CodColigada = 2, Chapa = "00100", Nome = "Cortis Keonho", CodRecebimento = "M", CodSituacao = "A", Secao = "Cortis", Funcao = "Dancer", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 33333, CodColigada = 2, Chapa = "00101", Nome = "Cortis Martin", CodRecebimento = "M", CodSituacao = "A", Secao = "Cortis", Funcao = "Rapper", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 44444, CodColigada = 2, Chapa = "00200", Nome = "BTS Jin", CodRecebimento = "M", CodSituacao = "A", Secao = "BTS", Funcao = "Visual", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 12345, CodColigada = 2, Chapa = "21212", Nome = "Jane Doe", CodRecebimento = "M", CodSituacao = "A", Secao = "Sample", Funcao = "Data", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now },
                    new Funcionario { CodPessoa = 09876, CodColigada = 2, Chapa = "41414", Nome = "Sakura Le sserafim", CodRecebimento = "M", CodSituacao = "A", Secao = "Sample", Funcao = "Data", DataMudanca = DateTime.Now, DataRegistro = DateTime.Now, DataAdmissao = DateTime.Now }
                );
            }

            if (!context.fnRetornaColaboradorCracha.Any())
            {
                context.fnRetornaColaboradorCracha.AddRange(
                    new fnRetornaColaboradorCracha
                    {
                        MATRICULA = "21212",
                        COLIGADA = "2",
                        NOME = "Jane Doe",
                        TIPOCOLA = "1",
                        IDCOLAB = 21212,
                        MATRICULA_SURICATO = "21212",
                        TIPO = "COLABORADOR",
                        TIPOCRAC = 1
                    },
                       new fnRetornaColaboradorCracha
                       {
                           MATRICULA = "31313",
                           COLIGADA = "2",
                           NOME = "Kazuha Le sserafim",
                           TIPOCOLA = "1",
                           IDCOLAB = 31313,
                           MATRICULA_SURICATO = "31313",
                           TIPO = "COLABORADOR",
                           TIPOCRAC = 1
                       },
                          new fnRetornaColaboradorCracha
                          {
                              MATRICULA = "51515",
                              COLIGADA = "2",
                              NOME = "Chaewon Le sserafim",
                              TIPOCOLA = "1",
                              IDCOLAB = 51515,
                              MATRICULA_SURICATO = "51515",
                              TIPO = "COLABORADOR",
                              TIPOCRAC = 1
                          },
                              new fnRetornaColaboradorCracha
                              {
                                  MATRICULA = "41414",
                                  COLIGADA = "2",
                                  NOME = "Sakura Le sserafim",
                                  TIPOCOLA = "1",
                                  IDCOLAB = 41414,
                                  MATRICULA_SURICATO = "41414",
                                  TIPO = "COLABORADOR",
                                  TIPOCRAC = 1
                              }
                    );
            }

            if (!context.FuncionarioSeek.Any())
            {
                context.AddRange(
                    new FuncionarioSeek
                    {
                        Id = 1,
                        Chapa = "01010102391",
                        Nome = "Third Party",
                        IdSecao = 1,
                        IdFuncao = 1,
                        Observacao = null,
                        DataRegistro = DateTime.Now,
                        Ativo = 1,
                        CodFuncao = null,
                        CodSecao = null,
                    }
                    );
            }

            if (!context.Funcionario.Any())
            {
                context.Funcao.AddRange(
                    new Funcao
                    {
                        Id = 1,
                        Nome = "NA",
                        DataRegistro = DateTime.Now,
                        Ativo = 1
                    }
                    );
            }

            if (!context.Secao.Any())
            {
                context.Secao.AddRange(
                    new Secao
                    {
                        Id = 1,
                        Nome = "NA",
                        DataRegistro = DateTime.Now,
                        Ativo = 1
                    }
                    );
            }

            if (!context.VW_Usuario_New.Any())
            {
                context.VW_Usuario_New.AddRange(
                    new VW_Usuario_New
                    {
                        Id = 1,
                        CodPessoa = 12,
                        CodColigada = 2,
                        Chapa = "admin.demo",
                        Senha = "admin123",
                        Email = "demoAdmin@sample.com",
                        DataRegistro = DateTime.Now,
                        Ativo = 1,
                        Nome = "Admin Demo",
                        Departamento = "Demo Team",
                        CodSecao = "3.0102.0129",
                        Secao = "IT",
                        CodFuncao = "123",
                        Funcao = "Developer",
                        CodSituacao = "A",
                    }
                    );
            }

            if (!context.ProductReservation.Any())
            {
                context.ProductReservation.AddRange(
                    new ProductReservation
                    {
                        Id = 1,
                        IdReservation = 4,
                        IdProduto = 2,
                        IdControleCA = null,
                        DataPrevistaDevolucao = null,
                        Observacao = null,
                        PreparedBy = 1,
                        Status = 0,
                        FinalQuantity = 1,
                        TransactionId = "trans-action"
                    }
                    );
            }


            await context.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Cleanup code if needed, or just return completed task
            return Task.CompletedTask;
        }

    }
}
