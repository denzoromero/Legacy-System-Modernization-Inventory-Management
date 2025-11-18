using Azure.Core;
using FerramentariaTest.DAL;
using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;

namespace FerramentariaTest.Services
{
    public class AuditService : IAuditService
    {
        private readonly ContextoBanco _context;
        private readonly ContextoBancoBS _contextBS;
        private readonly ILogger<AuditService> _logger;


        public AuditService(ContextoBanco context, ContextoBancoBS contextBS, ILogger<AuditService> logger)
        {
            _context = context;
            _contextBS = contextBS;
            _logger = logger;
        }


        public async Task<AuditLogModel> GetAuditLogs(string TransactionId)
        {
            try
            {

                //List<AuditLogsBalconista>? auditLogs = await _context.AuditLogsBalconista.Where(i => i.TransactionId == TransactionId).ToListAsync();

                AuditLogsBalconista? auditLog = await _context.AuditLogsBalconista.Where(i => i.TransactionId == TransactionId).FirstOrDefaultAsync();

                if (auditLog == null) throw new InvalidOperationException("No result found.");

                AuditLogModel result = new AuditLogModel
                {
                    Message = auditLog.Message,
                    TimeStamp = auditLog.TimeStamp?.ToString("dd/MM/yyyy HH:mm"),
                    Outcome = auditLog.Outcome,
                    TransactionId = auditLog.TransactionId,
                    Property = auditLog.LogEvent != null ? JsonConvert.DeserializeObject<PropertyModel>(auditLog.LogEvent) : new PropertyModel()
                };


                //AuditLogModel? auditlog = await _context.AuditLogsBalconista.Where(i => i.TransactionId == TransactionId)
                //                         .Select(i => new AuditLogModel
                //                         {
                //                             Message = i.Message,
                //                             TimeStamp = i.TimeStamp!.Value.ToString("dd/MM/yyyy HH:mm"),
                //                             Outcome = i.Outcome,
                //                             Property = JsonConvert.DeserializeObject<PropertyModel>(i.LogEvent),
                //                             TransactionId = i.TransactionId,
                //                         }).FirstOrDefaultAsync();


                //if (auditLogs == null || auditLogs.Count == 0) throw new InvalidOperationException("No result found.");


                return result;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Service Error");
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

        public async Task<FinalAuditResultModel> MakeAuditModel(List<FinalSubmissionProcess> FinalProcessList, combineFixModel MoreInformation)
        {
            try
            {
                List<AuditMaterialsModel> auditInformationMaterials = (from item in FinalProcessList
                                                                       join reservations in _context.Reservations on item.IdReservation equals reservations.Id
                                                                       join members in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals members.Id
                                                                       join leaders in _context.LeaderData on members.IdLeader equals leaders.Id
                                                                       join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
                                                                       join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                       join obra in _context.Obra on reservations.IdObra equals obra.Id
                                                                       select new AuditMaterialsModel
                                                                       {
                                                                           Code = catalogo.Codigo,
                                                                           Catalog = categoria.ClassType,
                                                                           CatalogDescription = catalogo.Nome,
                                                                           QtyRequested = item.QtyRequested,
                                                                           ChapaLiberador = leaders.Chapa,
                                                                           NomeLiberador = leaders.Nome,
                                                                           Obra = obra.Nome,
                                                                           ObservacaoBalconista = item.Observacao
                                                                       }).ToList();

                AuditBalconistaModel balconistaInformation = new AuditBalconistaModel()
                {
                    BalconistaChapa = MoreInformation.Balconista.Chapa,
                    BalconistaNome = MoreInformation.Balconista.Nome,
                };


                AuditSolicitanteModel employeeInformation = new AuditSolicitanteModel()
                {
                    CrachaTypeRequester = MoreInformation.CrachaInformation.TIPOCRAC == 1 ? "Primary Badge" : "Temporary Badge",
                    CrachaNoRequester = MoreInformation.CrachaNumber,
                    ChapaRequester = MoreInformation.Employee.Chapa,
                    NameRequester = MoreInformation.Employee.Nome,
                    FunctionRequester = MoreInformation.Employee.Funcao,
                    SectionRequester = MoreInformation.Employee.Secao
                };

                FinalAuditResultModel finalResult = new FinalAuditResultModel()
                {
                    TransactionId = MoreInformation.TransactionId,
                    Balconista = balconistaInformation,
                    Requester = employeeInformation,
                    Materials = auditInformationMaterials
                };



                return finalResult;


            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Service Error");
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
}
