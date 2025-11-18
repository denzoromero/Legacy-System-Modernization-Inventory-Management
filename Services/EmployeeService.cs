using FerramentariaTest.Controllers;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.EntitySeek;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FerramentariaTest.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoSeek _contextSeek;
        private readonly ContextoBancoRM _contextRM;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ContextoBanco context, ILogger<EmployeeService> logger, ContextoBancoBS contextBS, ContextoBancoSeek contextSeek, ContextoBancoRM contextRM)
        {
            _context = context;
            _contextBS = contextBS;
            _contextSeek = contextSeek;
            _logger = logger;
            _contextRM = contextRM;
        }

        public async Task<fnRetornaColaboradorCracha?> GetEmployeeCardInfo(string Icard)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeCardInfo Icard:{Icard}", Icard);

                if (string.IsNullOrWhiteSpace(Icard)) throw new ArgumentException("Icard is empty.");

                return await _contextBS.GetColaboradorCracha(Icard).SingleOrDefaultAsync();
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

        public async Task<EmployeeInformationBS> GetEmployeeInformationBS(string matricula)
        {
            try
            {
                
                _logger.LogInformation("Processing GetEmployeeInformationBS matricula:{matricula}", matricula);

                if (string.IsNullOrWhiteSpace(matricula)) throw new ArgumentException("matricula is empty.");

                return await _contextBS.Funcionario.Where(i => i.Chapa == matricula)
                                                     .Select(emp => new EmployeeInformationBS
                                                     {
                                                         IdTerceiro = 0,
                                                         CodPessoa = emp.CodPessoa,
                                                         CodColigada = emp.CodColigada,
                                                         Chapa = emp.Chapa,
                                                         Nome = emp.Nome,
                                                         Secao = emp.Secao,
                                                         Funcao = emp.Funcao
                                                     }).FirstOrDefaultAsync() ?? new EmployeeInformationBS();
               
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

        public async Task<List<EmployeeInformationBS>?> SearchEmployees(string givenInformation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(givenInformation)) throw new ProcessErrorException("givenInformation is empty");

                _logger.LogInformation("Processing SearchEmployees with GivenInformation:{givenInformation}", givenInformation);

                List<EmployeeInformationBS>? employees = await _contextBS.Funcionario
                                                        .Where(e => (e.Nome!.Contains(givenInformation) || e.Chapa!.Contains(givenInformation)) 
                                                        && e.DataMudanca == _contextBS.Funcionario.Where(f => f.Chapa == e.Chapa).Max(f => f.DataMudanca))
                                                        .Select(i => new EmployeeInformationBS
                                                        {
                                                             IdTerceiro = 0,
                                                             CodPessoa = i.CodPessoa,
                                                             CodColigada = i.CodColigada,
                                                             Chapa = i.Chapa,
                                                             Nome = i.Nome,
                                                             Secao = i.Secao,
                                                             Funcao = i.Funcao,
                                                             CodSituacao = i.CodSituacao
                                                        }).ToListAsync() ?? new List<EmployeeInformationBS>();

    //            List<Funcionario>? query = await _contextBS.Funcionario.Where(e => e.Nome.Contains(givenInformation) || e.Chapa.Contains(givenInformation))
    //                                        .GroupBy(f => f.Chapa)
    //                                        .Select(group => group.OrderByDescending(f => f.DataMudanca).FirstOrDefault())
    //                                        .ToListAsync() ?? new List<Funcionario>();

                                        //            List<EmployeeInformationBS> employees = query
                                        //.Where(i => i != null)
                                        //.Select(i => new EmployeeInformationBS
                                        //{
                                        //    IdTerceiro = 0,
                                        //    CodPessoa = i.CodPessoa,
                                        //    CodColigada = i.CodColigada,
                                        //    Chapa = i.Chapa,
                                        //    Nome = i.Nome,
                                        //    Secao = i.Secao,
                                        //    Funcao = i.Funcao,
                                        //    CodSituacao = i.CodSituacao
                                        //})
                                        //.ToList();


                return employees;

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

        public async Task<List<EmployeeInformationBS>?> SearchThirdParty(string givenInformation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(givenInformation)) throw new ProcessErrorException("givenInformation is empty");

                _logger.LogInformation("Processing SearchThirdParty with GivenInformation:{givenInformation}", givenInformation);

                List<EmployeeInformationBS>? thirdParties = await (from funcionario in _contextSeek.Funcionario
                                                                   join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                                                                   join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                                                                   where (funcionario.Nome!.Contains(givenInformation) || funcionario.Chapa!.Contains(givenInformation))
                                                                   select new EmployeeInformationBS
                                                                   {
                                                                       IdTerceiro = funcionario.Id,
                                                                       CodPessoa = 0,
                                                                       CodColigada = 0,
                                                                       Chapa = funcionario.Chapa,
                                                                       Nome = funcionario.Nome,
                                                                       Secao = secao.Nome,
                                                                       Funcao = funcao.Nome,
                                                                       CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                                                   }).ToListAsync() ?? new List<EmployeeInformationBS>();

                return thirdParties;

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

        public async Task<string?> GetEmployeeImage(int codpessoa)
        {
            try
            {
                if (codpessoa <= 0) throw new ProcessErrorException("codpessoa is less than or equal to 0");

                _logger.LogInformation("Processing GetEmployeeImage with CodPessoa:{codpessoa}", codpessoa);

                byte[]? base64Image = await (from pessoa in _contextRM.PPESSOA
                                          join gImagem in _contextRM.GIMAGEM on pessoa.IDIMAGEM equals gImagem.ID
                                          where pessoa.CODIGO == codpessoa
                                          select gImagem.IMAGEM)

                                          .FirstOrDefaultAsync();

                if (base64Image == null) return null;

                return $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}";

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

        public async Task<EmployeeInformationBS> GetSelectedEmployee(int codpessoa)
        {
            try
            {
                if (codpessoa <= 0) throw new ProcessErrorException("codpessoa is less than or equal to 0");

                _logger.LogInformation("Processing GetSelectedEmployee with CodPessoa:{codpessoa}", codpessoa);

                EmployeeInformationBS? employee = await _contextBS.Funcionario
                                                        .Where(e => (e.CodPessoa == codpessoa)
                                                        && e.DataMudanca == _contextBS.Funcionario.Where(f => f.CodPessoa == e.CodPessoa).Max(f => f.DataMudanca))
                                                        .Select(i => new EmployeeInformationBS
                                                        {
                                                            IdTerceiro = 0,
                                                            CodPessoa = i.CodPessoa,
                                                            CodColigada = i.CodColigada,
                                                            Chapa = i.Chapa,
                                                            Nome = i.Nome,
                                                            Secao = i.Secao,
                                                            Funcao = i.Funcao,
                                                            CodSituacao = i.CodSituacao,
                                                            DataAdmissao = i.DataAdmissao.HasValue == true ? i.DataAdmissao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                            DataDemissao = i.DataDemissao.HasValue == true ? i.DataDemissao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                        }).FirstOrDefaultAsync();

                if (employee == null) throw new InvalidOperationException("no result found.");

                employee.ImageString = await GetEmployeeImage(codpessoa);

                return employee;
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

        public async Task<EmployeeInformationBS> GetSelectedThirdParty(int idTerceiro)
        {
            try
            {
                if (idTerceiro <= 0) throw new ProcessErrorException("idTerceiro is less than or equal to 0");

                _logger.LogInformation("Processing GetSelectedThirdParty with IdTerceiro:{idTerceiro}", idTerceiro);

                EmployeeInformationBS? thirdParty = await (from funcionario in _contextSeek.Funcionario
                                                           join secao in _contextSeek.Secao on funcionario.IdSecao equals secao.Id
                                                           join funcao in _contextSeek.Funcao on funcionario.IdFuncao equals funcao.Id
                                                           where funcionario.Id == idTerceiro
                                                           select new EmployeeInformationBS
                                                           {
                                                               IdTerceiro = funcionario.Id,
                                                               CodPessoa = 0,
                                                               CodColigada = 0,
                                                               Chapa = funcionario.Chapa,
                                                               Nome = funcionario.Nome,
                                                               Secao = secao.Nome,
                                                               Funcao = funcao.Nome,
                                                               CodSituacao = funcionario.Ativo == 1 ? "A" : "D",
                                                           }).FirstOrDefaultAsync();

                if (thirdParty == null) throw new InvalidOperationException("no result found.");

                return thirdParty;
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

        public async Task<TermsControlModel?> CheckTermsControl(string chapa)
        {
            try
            {
                _logger.LogInformation("Processing CheckTermsControl with Chapa:{chapa}", chapa);

                TermsControlModel? termsControlModel = await _context.TermsControl.Where(i => i.Chapa == chapa)
                                                      .Select(x => new TermsControlModel
                                                      {
                                                          Id = x.Id,
                                                          Balconista = x.Balconista,
                                                          Chapa = x.Chapa,
                                                          TransactionId = x.TransactionId,
                                                          DataRegistro = x.DataRegistro,
                                                          ImageData = x.ImageData
                                                      }).FirstOrDefaultAsync();

                return termsControlModel;

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

        public async Task<Result> AddToTermsControl(TermsControl termsInformation)
        {
            try
            {

                if (termsInformation == null) throw new ProcessErrorException("termsInformation is null.");

                using var _ = LogContext.PushProperty("termsInformation", termsInformation, destructureObjects: true);
                {
                    _logger.LogInformation("Processing AddToTermsControl");
                }


                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        await _context.AddAsync(termsInformation);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Result.Success();

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

        public async Task<TermsControlResultModel> GetTermsConrolResult(int CodPessoa)
        {
            try
            {
                _logger.LogInformation("Processing GetTermsConrolResult with CodPessoa:{CodPessoa}", CodPessoa);

                if (CodPessoa <= 0) throw new ProcessErrorException("CodPessoa is less than or equal to 0");

                TermsControlResultModel? termsControlModel = await _context.TermsControl.Where(i => i.CodPessoa == CodPessoa)
                                                          .Select(x => new TermsControlResultModel
                                                          {
                                                              Id = x.Id,
                                                              Balconista = x.Balconista,
                                                              Chapa = x.Chapa,
                                                              DataRegistroString = x.DataRegistro.HasValue == true ? x.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                              CodPessoa = x.CodPessoa,
                                                              ImageData = x.ImageData,
                                                          }).FirstOrDefaultAsync();

                if (termsControlModel == null) throw new InvalidOperationException("No result found.");

                EmployeeInformationBS? employee = await _contextBS.Funcionario
                                        .Where(e => (e.CodPessoa == CodPessoa)
                                        && e.DataMudanca == _contextBS.Funcionario.Where(f => f.CodPessoa == e.CodPessoa).Max(f => f.DataMudanca))
                                        .Select(i => new EmployeeInformationBS
                                        {
                                            IdTerceiro = 0,
                                            CodPessoa = i.CodPessoa,
                                            CodColigada = i.CodColigada,
                                            Chapa = i.Chapa,
                                            Nome = i.Nome,
                                            Secao = i.Secao,
                                            Funcao = i.Funcao,
                                            CodSituacao = i.CodSituacao,
                                            DataAdmissao = i.DataAdmissao.HasValue == true ? i.DataAdmissao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                            DataDemissao = i.DataDemissao.HasValue == true ? i.DataDemissao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                        }).FirstOrDefaultAsync();

                if (employee == null) throw new InvalidOperationException("No result found for employee.");

                termsControlModel.Nome = employee.Nome;

                VW_Usuario_New? balconista = _contextBS.VW_Usuario_New.FirstOrDefault(i => i.Id == termsControlModel.Balconista);

                if (balconista == null) throw new InvalidOperationException("No result found for balconista.");

                termsControlModel.Responsavel = balconista.Nome;

                return termsControlModel;

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

        public async Task UploadTermsPDF(int idTerms, byte[] imgByte)
        {
            try
            {
                _logger.LogInformation("Processing UploadTermsPDF with IdTerms:{idTerms}", idTerms);

                if (idTerms <= 0) throw new ProcessErrorException("idTerms is less than or equal to 0");
                if (imgByte == null) throw new ProcessErrorException("Image byte array is null");
                if (imgByte.Length == 0) throw new ProcessErrorException("Image byte array is empty");

                TermsControl? termControl = await _context.TermsControl.FindAsync(idTerms);
                if (termControl == null) throw new ProcessErrorException($"TermControl:{idTerms} não encontrado.");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {
                        termControl.ImageData = imgByte;

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
