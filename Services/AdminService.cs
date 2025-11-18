using FerramentariaTest.Services.Interfaces;
using FerramentariaTest.DAL;
using FerramentariaTest.Models;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using System;
using FerramentariaTest.Controllers;
using Serilog.Context;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authorization;

namespace FerramentariaTest.Services
{
    public class AdminService : IAdminService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ContextoBancoRM _contextRM;
        private readonly ILogger<AdminService> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public AdminService(ContextoBanco context, ILogger<AdminService> logger, IHttpContextAccessor httpContextAccessor, ContextoBancoBS contextBS, ContextoBancoRM contextRM)
        {
            _context = context;
            _contextBS = contextBS;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _contextRM = contextRM;
        }

        [Authorize(Roles = "Demo")]
        public async Task<List<FuncionarioModel>> getLeaderList(string? givenInfo, int currentPage)
        {
            try
            {
                //List<LeaderData>? leaderList = _context.LeaderData.Where(i => (i.Chapa == null || i.Chapa == givenInfo) || (i.Nome == null || i.Nome.Contains(givenInfo))).ToList();

                _logger.LogInformation("Processing getLeaderList givenInfo:{givenInfo}, currentPage:{currentPage}", givenInfo, currentPage);

                List<LeaderData>? leaderList = await _context.LeaderData
                                                    .Where(i => string.IsNullOrEmpty(givenInfo) ||
                                                                (i.Chapa == null || i.Chapa == givenInfo) ||
                                                                (i.Nome == null || i.Nome.Contains(givenInfo)))
                                                    .ToListAsync();

                if (leaderList == null || leaderList.Count == 0) throw new ModifiedErrorException("No Leader found.");

                List<FuncionarioModel> completeInfo = (from leader in leaderList
                                                            //join funcionario in _contextBS.Funcionario on leader.CodPessoa equals funcionario.CodPessoa
                                                        join recentFunc in (
                                                                from f in _contextBS.Funcionario
                                                                group f by f.CodPessoa into g
                                                                select g.OrderByDescending(x => x.DataMudanca).FirstOrDefault()
                                                            ) on leader.CodPessoa equals recentFunc.CodPessoa
                                                        select new FuncionarioModel()
                                                        {
                                                            IdLeader = leader.Id,
                                                            IdUserSib = leader.IdUser,
                                                            IdTerceiro = 0,
                                                            CodPessoa = recentFunc.CodPessoa,
                                                            CodColigada = recentFunc.CodColigada,
                                                            Chapa = recentFunc.Chapa,
                                                            Nome = recentFunc.Nome,
                                                            CodSituacao = recentFunc.CodSituacao,
                                                            Secao = recentFunc.Secao,
                                                            Funcao = recentFunc.Funcao,
                                                            AtivoLeader = leader.Ativo
                                                        }).ToList();

                if (completeInfo == null || completeInfo.Count == 0) throw new ModifiedErrorException("Cannot complete leader information.");

                return completeInfo;
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

        [Authorize(Roles = "Demo")]
        public async Task<FuncionarioModel> GetEmployeeForLeader(string chapa)
        {
            try
            {

                _logger.LogInformation("Processing GetEmployeeForLeader Chapa:{chapa}.", chapa);

                if (string.IsNullOrWhiteSpace(chapa)) throw new ModifiedErrorException("No chapa provided");

                Funcionario? func = await _contextBS.Funcionario.Where(i => i.Chapa == chapa).OrderByDescending(i => i.DataMudanca).FirstOrDefaultAsync();
                if (func == null) throw new ModifiedErrorException($"No employee found with Matricula:{chapa}");

                UsuarioBS? checkuser = _contextBS.Usuario.FirstOrDefault(i => i.Chapa == func.Chapa && i.Ativo == 1);
                if (checkuser == null) throw new ModifiedErrorException($"Employee:{chapa} is not yet registered in SIB.");

                LeaderData? leadercheck = _context.LeaderData.FirstOrDefault(i => i.CodPessoa == func.CodPessoa);
                if (leadercheck != null) throw new ModifiedErrorException($"Employee:{chapa} already registered as leader.");

                byte[]? base64Image = await (from pessoa in _contextRM.PPESSOA
                                             join gImagem in _contextRM.GIMAGEM
                                             on pessoa.IDIMAGEM equals gImagem.ID
                                             where pessoa.CODIGO == func.CodPessoa
                                             select gImagem.IMAGEM).FirstOrDefaultAsync();

                string imageStringByte = base64Image != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(base64Image)}" : string.Empty;

                FuncionarioModel detailedFunc = new FuncionarioModel()
                {
                    IdUserSib = checkuser.Id,
                    IdTerceiro = 0,
                    CodPessoa = func.CodPessoa,
                    CodColigada = func.CodColigada,
                    Chapa = func.Chapa,
                    Nome = func.Nome,
                    CodSituacao = func.CodSituacao,
                    Secao = func.Secao,
                    Funcao = func.Funcao,
                    ImageStringByte = imageStringByte
                };

                return detailedFunc;

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

        [Authorize(Roles = "Demo")]
        public async Task InsertNewLeader(LeaderData leader)
        {
            try
            {
             
                using (LogContext.PushProperty("LeaderInformation", leader, destructureObjects: true))
                {
                    _logger.LogInformation("Processing InsertNewLeader Chapa:{Leader.Chapa}.", leader.Chapa);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        await _context.LeaderData.AddAsync(leader);
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

        [Authorize(Roles = "Demo")]
        public async Task DeactivateLeader(int id)
        {
            try
            {

                _logger.LogInformation("Processing DeactivateLeader Id:{id}.", id);

                LeaderData? leader = await _context.LeaderData.FindAsync(id);
                if (leader == null) throw new ModifiedErrorException("LeaderData is null");


                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        leader.Ativo = 0;

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

        [Authorize(Roles = "Demo")]
        public async Task ReactivateLeader(int id)
        {
            try
            {
                _logger.LogInformation("Processing ReactivateLeader Id:{id}.", id);

                LeaderData? leader = await _context.LeaderData.FindAsync(id);
                if (leader == null) throw new ModifiedErrorException("LeaderData is null");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        leader.Ativo = 1;

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

    }
}
