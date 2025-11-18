using FerramentariaTest.Models;
using FerramentariaTest.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.DAL.Demo;
using FerramentariaTest.Entities;


using UsuarioBS = FerramentariaTest.EntitiesBS.Usuario;
using Serilog.Context;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace FerramentariaTest.Services.Demo
{
    public class UserServiceDemo : IUserService
    {
        private readonly DemoDB _context;
        private readonly ILogger<UserServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public UserServiceDemo(ILogger<UserServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UsuarioBS> ValidateUser(VW_Usuario_NewViewModel credentials)
        {
            try
            {

                UsuarioBS? user = await _context.Usuario.FirstOrDefaultAsync(i => i.Chapa == credentials.SAMAccountName && i.Senha == credentials.Senha);
                if (user == null) throw new ProcessErrorException("User or Password is invalid.");

                return user;

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

    public class FerramentariaServiceDemo : IFerramentariaService
    {
        private readonly DemoDB _context;
        private readonly ILogger<FerramentariaServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public FerramentariaServiceDemo(ILogger<FerramentariaServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
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
                                                           where ferramentaria.Ativo == 1
                                                                 //!_context.VW_Ferramentaria_Ass_Solda.Select(s => s.Id).Contains(ferramentaria.Id) &&
                                                                 //_context.FerramentariaVsLiberador.Any(l => l.IdLogin == UserId && l.IdFerramentaria == ferramentaria.Id)
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

    public class ReservationServiceDemo : IReservationService
    {
        private readonly DemoDB _context;
        private readonly ILogger<ReservationServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public ReservationServiceDemo(ILogger<ReservationServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ReservationsModel>?> GetGroupReservation(int? ferramentariaValue, int? controlStatus)
        {
            try
            {
                _logger.LogInformation("Processing GetGroupReservation IdFerramentaria:{ferramentariaValue}, ControlStatus:{controlStatus}", ferramentariaValue, controlStatus);

                List<ReservationsControlModel>? reservations = await (from reservationControl in _context.ReservationControl
                                                                      join reserve in _context.Reservations on reservationControl.Id equals reserve.IdReservationControl
                                                                      join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                      where reserve.IdFerramentaria == ferramentariaValue
                                                                      && reservationControl.Type == 1
                                                                      && (controlStatus == null || reserve.Status == controlStatus)
                                                                      select new ReservationsControlModel
                                                                      {
                                                                          ControlId = reservationControl.Id,
                                                                          Chave = reservationControl.Chave,
                                                                          LeadercodPessoa = leader.CodPessoa,
                                                                          LeaderName = leader.Nome,
                                                                          ControlStatusString = reservationControl.StatusString,
                                                                          ControlStatus = reservationControl.Status,
                                                                          reserveStatus = reserve.Status,
                                                                          reserveStatusString = reserve.StatusString,
                                                                          controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                      }).ToListAsync();

                List<ReservationsModel>? groupReservation = reservations
                                                            .GroupBy(r => r.ControlId)
                                                            .Select(group => new ReservationsModel
                                                            {
                                                                ControlId = group.Key,
                                                                //itemCount = group.Count(),
                                                                itemCount = group.Count(r => r.reserveStatus == 0),
                                                                Chave = group.First().Chave,
                                                                LeaderName = group.First().LeaderName,
                                                                RegisteredCount = group.Count(),
                                                                ActualStatus = group.First().ControlStatusString,
                                                                controlDataRegistroString = group.First().controlDataRegistroString,
                                                                GroupStatus = group.All(r => r.reserveStatus == 1) ? 1 : 0
                                                            }).OrderBy(i => i.controlDataRegistroString).ToList();

                return groupReservation ?? new List<ReservationsModel>();

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
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

        public async Task<List<ItemReservationDetailModel>?> PrepareModel(int id, int? FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing PrepareModel IdReservationControl:{id} IdFerramentaria:{FerramentariaValue}", id, FerramentariaValue);

                List<ItemReservationDetailModel> itemdetail = await (from reserv in _context.Reservations
                                                                     join catalogo in _context.Catalogo on reserv.IdCatalogo equals catalogo.Id
                                                                     join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                     join reservationControl in _context.ReservationControl on reserv.IdReservationControl equals reservationControl.Id
                                                                     join member in _context.LeaderMemberRel on reserv.IdLeaderMemberRel equals member.Id
                                                                     join leader in _context.LeaderData on member.IdLeader equals leader.Id
                                                                     join obra in _context.Obra on reserv.IdObra equals obra.Id
                                                                     where reserv.IdFerramentaria == FerramentariaValue
                                                                     && reserv.IdReservationControl == id
                                                                     && reservationControl.Type == 1
                                                                     select new ItemReservationDetailModel
                                                                     {
                                                                         IdReservation = reserv.Id,
                                                                         Classe = categoria.ClassType,
                                                                         Type = catalogo.PorType,
                                                                         Codigo = catalogo.Codigo,
                                                                         itemNome = catalogo.Nome,
                                                                         QuantidadeResquested = reserv.Quantidade,
                                                                         MemberCodPessoa = member.CodPessoa,
                                                                         LeaderCodPessoa = leader.CodPessoa,
                                                                         DataRegistro = reserv.DataRegistro.HasValue == true ? reserv.DataRegistro.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty, // Correct format
                                                                         Status = reserv.StatusString,
                                                                         IdObra = reserv.IdObra,
                                                                         ObraName = $"{obra.Codigo}-{obra.Nome}",
                                                                         intStatus = reserv.Status
                                                                     }).ToListAsync() ?? new List<ItemReservationDetailModel>();

                List<Funcionario?>? recentFuncionario = await _context.Funcionario
                                                          .GroupBy(e => e.CodPessoa)
                                                          .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
                                                          .ToListAsync();

                List<ItemReservationDetailModel> completeDetail = (from item in itemdetail
                                                                   join memberinfo in recentFuncionario on item.MemberCodPessoa equals memberinfo.CodPessoa
                                                                   join leaderinfo in recentFuncionario on item.LeaderCodPessoa equals leaderinfo.CodPessoa
                                                                   select new ItemReservationDetailModel
                                                                   {
                                                                       IdReservation = item.IdReservation,
                                                                       Classe = item.Classe,
                                                                       Type = item.Type,
                                                                       Codigo = item.Codigo,
                                                                       itemNome = item.itemNome,
                                                                       QuantidadeResquested = item.QuantidadeResquested,
                                                                       MemberCodPessoa = item.MemberCodPessoa,
                                                                       MemberInfo = new employeeNewInformationModel
                                                                       {
                                                                           Chapa = memberinfo.Chapa,
                                                                           Nome = memberinfo.Nome,
                                                                           CodSituacao = memberinfo.CodSituacao,
                                                                           CodColigada = memberinfo.CodColigada,
                                                                           Funcao = memberinfo.Funcao,
                                                                           Secao = memberinfo.Secao,
                                                                       },
                                                                       LeaderCodPessoa = item.LeaderCodPessoa,
                                                                       LeaderInfo = new employeeNewInformationModel
                                                                       {
                                                                           Chapa = leaderinfo.Chapa,
                                                                           Nome = leaderinfo.Nome,
                                                                           CodSituacao = leaderinfo.CodSituacao,
                                                                           CodColigada = leaderinfo.CodColigada,
                                                                           Funcao = leaderinfo.Funcao,
                                                                           Secao = leaderinfo.Secao,
                                                                       },
                                                                       DataRegistro = item.DataRegistro,
                                                                       Status = item.Status,
                                                                       intStatus = item.intStatus,
                                                                       IdObra = item.IdObra,
                                                                       ObraName = item.ObraName,
                                                                   }).ToList();


                return completeDetail;

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
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

        public async Task<ReservationControl?> GetReservationControl(int id)
        {
            try
            {
                _logger.LogInformation("Processing GetReservationControl IdReservationControl:{id}", id);
                return await _context.ReservationControl.FindAsync(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
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

        public async Task UpdateReservationStatus(List<int?> IdReservations)
        {
            try
            {
                using var _ = LogContext.PushProperty("IdReservations", IdReservations, destructureObjects: true);
                {
                    _logger.LogInformation("Processing UpdateReservationStatus");
                }

                if (IdReservations == null || IdReservations.Count == 0) throw new ProcessErrorException("IdReservations is null or count 0.");

                var reservations = await _context.Reservations
    .Where(r => IdReservations.Contains(r.Id))
    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.Status = 1;
                }

                //await _context.Reservations.Where(r => IdReservations.Contains(r.Id)).ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, 1));

                await _context.SaveChangesAsync();

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

        public async Task<List<ReservationsControlModel>?> GetPreparingReservations(int IdReservationControl, int? FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing GetPreparingReservations IdReservationControl:{id}, IdFerramentaria:{IdFerramentaria}", IdReservationControl, FerramentariaValue);

                List<ReservationsControlModel>? reservations = await (from reservation in _context.Reservations
                                                                      join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
                                                                      where reservation.IdReservationControl == IdReservationControl
                                                                      && reservation.IdFerramentaria == FerramentariaValue
                                                                      && reservation.Status != 8
                                                                      && reservation.Status != 3
                                                                      select new ReservationsControlModel
                                                                      {
                                                                          ControlId = reservationControl.Id,
                                                                          Chave = reservationControl.Chave,
                                                                          ControlStatusString = reservationControl.StatusString,
                                                                          ControlStatus = reservationControl.Status,
                                                                          reserveStatus = reservation.Status,
                                                                          controlDataRegistroString = reservationControl.DataRegistro.HasValue == true ? reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                      }).ToListAsync() ?? new List<ReservationsControlModel>();

                return reservations;

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

        public async Task<List<ReservedProductModel>?> GetReservedProducts(int IdReservationControl, int FerramentariaValue)
        {
            try
            {
                _logger.LogInformation("Processing GetReservedProducts IdReservationControl:{id}, IdFerramentaria:{IdFerramentaria}", IdReservationControl, FerramentariaValue);



                List<ReservedProductModel>? reservedList = await (from reservation in _context.Reservations
                                                                  join reservationControl in _context.ReservationControl on reservation.IdReservationControl equals reservationControl.Id
                                                                  join member in _context.LeaderMemberRel on reservation.IdLeaderMemberRel equals member.Id
                                                                  join leader in _context.LeaderData on member.IdLeader equals leader.Id
                                                                  join obra in _context.Obra on reservation.IdObra equals obra.Id
                                                                  join produto in _context.Produto on new { reservation.IdCatalogo, reservation.IdFerramentaria }
                                                                          equals new { produto.IdCatalogo, produto.IdFerramentaria }
                                                                  join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                                  join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                  where reservation.Status == 1
                                                                  && produto.Ativo == 1
                                                                  && catalogo.Ativo == 1
                                                                  && categoria.Ativo == 1
                                                                  && reservation.IdReservationControl == IdReservationControl
                                                                  && reservation.IdFerramentaria == FerramentariaValue
                                                                  select new ReservedProductModel
                                                                  {
                                                                      IdReservationControl = reservation.IdReservationControl,
                                                                      IdReservation = reservation.Id,
                                                                      IdCatalogo = reservation.IdCatalogo,
                                                                      IdFerramentaria = reservation.IdFerramentaria,
                                                                      IdProduto = produto.Id,

                                                                      intClasse = categoria.Classe,
                                                                      Classe = categoria.ClassType,
                                                                      Type = catalogo.PorType,
                                                                      Codigo = catalogo.Codigo,
                                                                      itemNome = catalogo.Nome,
                                                                      intStatus = reservation.Status,
                                                                      Status = reservation.StatusString,
                                                                      MemberCodPessoa = member.CodPessoa,
                                                                      LeaderCodPessoa = leader.CodPessoa,
                                                                      IdObra = obra.Id,
                                                                      ObraName = $"{obra.Codigo}-{obra.Nome}",

                                                                      QtyRequested = reservation.Quantidade,
                                                                      QtyStock = produto.Quantidade,
                                                                  }).ToListAsync() ?? new List<ReservedProductModel>();

                Dictionary<int, Funcionario?> funcionarioDict = _context.Funcionario.AsEnumerable()
                                                .GroupBy(e => e.CodPessoa)
                                                .Select(g => g.OrderByDescending(e => e.DataMudanca).FirstOrDefault())
                                                .Where(f => f != null)  // Filter out nulls if any
                                                .ToDictionary(
                                                    f => f.CodPessoa.Value,   // Key selector
                                                    f => f              // Value selector
                                                );

                var enrichedReservations = reservedList.Select(r =>
                {
                    // Get member and leader information from dictionary
                    var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : null;
                    var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : null;

                    return new ReservedProductModel
                    {


                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        IdCatalogo = r.IdCatalogo,
                        IdFerramentaria = r.IdFerramentaria,
                        IdProduto = r.IdProduto,

                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        intStatus = r.intStatus,
                        Status = r.Status,
                        MemberCodPessoa = member.CodPessoa,
                        LeaderCodPessoa = leader.CodPessoa,
                        IdObra = r.IdObra,
                        ObraName = r.ObraName,

                        QtyRequested = r.QtyRequested,
                        QtyStock = r.QtyStock,
                        IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != r.IdFerramentaria && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

                        // Add new properties with funcionario data
                        MemberInfo = new employeeNewInformationModel()
                        {
                            Chapa = memberInfo.Chapa,
                            Nome = memberInfo.Nome,
                            CodSituacao = memberInfo.CodSituacao,
                            CodColigada = memberInfo.CodColigada,
                            Funcao = memberInfo.Funcao,
                            Secao = memberInfo.Secao,
                            CodPessoa = memberInfo.CodPessoa,
                        },
                        LeaderInfo = new employeeNewInformationModel()
                        {
                            Chapa = leaderInfo.Chapa,
                            Nome = leaderInfo.Nome,
                            CodSituacao = leaderInfo.CodSituacao,
                            CodColigada = leaderInfo.CodColigada,
                            Funcao = leaderInfo.Funcao,
                            Secao = leaderInfo.Secao,
                            CodPessoa = leaderInfo.CodPessoa,
                        },
                    };
                }).ToList();

                return enrichedReservations;

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

        public async Task CancelReservation(int? IdReservation, string Chapa, string Observacao)
        {
            try
            {
                _logger.LogInformation("Processing CancelReservation IdReservation:{id}, Chapa:{Chapa}, Observacao:{Observacao}", IdReservation, Chapa, Observacao);

                if (IdReservation == null) throw new ProcessErrorException("IdReservation is required");

                if (string.IsNullOrEmpty(Chapa)) throw new ProcessErrorException("Chapa is required", nameof(Chapa));

                Reservations? reservation = await _context.Reservations.FindAsync(IdReservation);
                if (reservation == null) throw new ProcessErrorException($"Reserva:{IdReservation} não encontrado.");

                if (reservation.Status == 8) throw new ProcessErrorException($"Reserva:{IdReservation} já está cancelada.");

                //using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {

                        reservation.Status = 8;
                        reservation.Observacao = $"Cancellado por: {Chapa} - {Observacao}";


                        await _context.SaveChangesAsync();
                        //await transaction.CommitAsync();

                //    }
                //    catch (Exception)
                //    {
                //        await transaction.RollbackAsync();
                //        throw;
                //    }

                //}

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

        public async Task<Reservations?> GetReservations(int id)
        {
            try
            {
                _logger.LogInformation("Processing GetReservations id:{id}", id);
                return await _context.Reservations.FindAsync(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Arguments Error");
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

        public async Task TransferReservation(int IdReservation, string Observacao, int IdFerramentariaTo)
        {
            try
            {
                _logger.LogInformation("Processing TransferReservation IdReservation:{id}, Observacao:{Observacao}, FerramentariaTo:{IdFerramentariaTo}", IdReservation, Observacao, IdFerramentariaTo);

                if (IdReservation <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                if (string.IsNullOrWhiteSpace(Observacao)) throw new ProcessErrorException("Observation cannot be empty", nameof(Observacao));

                if (IdFerramentariaTo <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                Reservations? reservation = await _context.Reservations.FindAsync(IdReservation);

                if (reservation == null) throw new ProcessErrorException("Reservation is null");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 0;
                        reservation.Observacao = Observacao;
                        reservation.IdFerramentaria = IdFerramentariaTo;

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

        public async Task FinalizeProcessReservation(List<ProductReservation> finalSubmissions)
        {
            try
            {
                using var _ = LogContext.PushProperty("FinalReservation", finalSubmissions, destructureObjects: true);
                _logger.LogInformation("Processing FinalizeProcessReservation");

                if (finalSubmissions == null || finalSubmissions.Count == 0) throw new ProcessErrorException("ProcessList is empty");
                if (finalSubmissions.Any(i => i.IdReservation == null)) throw new ProcessErrorException("Some of the IdReservation is null");
                if (finalSubmissions.Any(i => i.IdProduto == null)) throw new ProcessErrorException("Some of the IdProduto is null");
                if (finalSubmissions.Any(i => i.FinalQuantity == null)) throw new ProcessErrorException("Some of the QtyRequested is null");


                //using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {
                var reservationIds = finalSubmissions
                                    .Select(i => i.IdReservation.Value)
                                    .Distinct()
                                    .ToList();

                var reservations = await _context.Reservations
    .Where(r => reservationIds.Contains(r.Id.Value))
    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.Status = 2;
                }


                //await _context.Reservations.Where(r => reservationIds.Contains(r.Id.Value)).ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, 2));



                _context.ProductReservation.AddRange(finalSubmissions);
                await _context.SaveChangesAsync();
                //        await transaction.CommitAsync();
                //    }
                //    catch (Exception)
                //    {
                //        await transaction.RollbackAsync();
                //        throw;
                //    }
                //}

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

        public async Task<List<ProductReservation>?> VerifyReservations(List<FinalSubmissionProcess> finalSubmissions)
        {
            try
            {
                using var _ = LogContext.PushProperty("FinalReservation", finalSubmissions, destructureObjects: true);
                _logger.LogInformation("Processing VerifyReservations");

                if (finalSubmissions == null || finalSubmissions.Count == 0) throw new ProcessErrorException("ProcessList is empty");
                if (finalSubmissions.Any(i => i.IdReservation == null)) throw new ProcessErrorException("Some of the IdReservation is null");
                if (finalSubmissions.Any(i => i.IdProduto == null)) throw new ProcessErrorException("Some of the IdProduto is null");
                if (finalSubmissions.Any(i => i.QtyRequested == null)) throw new ProcessErrorException("Some of the QtyRequested is null");

                List<int>? reservationIds = finalSubmissions.Select(i => i.IdReservation!.Value).ToList();
                var existingReservations = await _context.Reservations.Where(r => r.Id.HasValue && reservationIds.Contains(r.Id.Value)).ToDictionaryAsync(r => r.Id.Value);

                var missingIds = reservationIds.Except(existingReservations.Keys).ToList();
                if (missingIds.Any())
                {
                    throw new InvalidOperationException($"Reservations not found: {string.Join(", ", missingIds)}");
                }

                var invalidStatus = existingReservations.Values.Where(r => r.Status != 1).ToList();
                if (invalidStatus.Any())
                {
                    throw new InvalidOperationException($"Invalid status for reservations: {string.Join(", ", invalidStatus.Select(r => r.Id))}");
                }



                List<ProductReservation>? entities = finalSubmissions.Select(item =>
                                                        new ProductReservation()
                                                        {
                                                            IdReservation = item.IdReservation,
                                                            IdProduto = item.IdProduto,
                                                            DataPrevistaDevolucao = item.DateReturn,
                                                            Observacao = item.Observacao,
                                                            Status = 0,
                                                            DataRegistro = DateTime.Now,
                                                            FinalQuantity = item.QtyRequested,
                                                        }).ToList();

                return entities;

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

        public async Task<bool> VerifyFinalizeTransactionId(string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing VerifyFinalizeTransactionId TransactionId:{transactionId}", transactionId);

                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("Transaction is empty.");

                return await _context.ProductReservation.AnyAsync(i => i.TransactionId == transactionId);

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




        public async Task<List<FinalReservationResult>?> GetFinalizedReservation(int codPessoa, int idFerramentaria)
        {
            try
            {
                _logger.LogInformation("Processing GetFinalizedReservation codPessoa:{codPessoa}, idFerramentaria:{idFerramentaria}", codPessoa, idFerramentaria);

                List<FinalReservationResult>? reservedproducts = await (from finalproduct in _context.ProductReservation
                                                                        join reservations in _context.Reservations on finalproduct.IdReservation equals reservations.Id
                                                                        join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
                                                                        join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
                                                                        join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
                                                                        join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
                                                                        join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                        join obra in _context.Obra on reservations.IdObra equals obra.Id
                                                                        where control.Type == 1
                                                                            && finalproduct.Status == 0
                                                                            && reservations.Status == 2
                                                                            && reservations.IdFerramentaria == idFerramentaria
                                                                            && member.CodPessoa == codPessoa
                                                                        select new FinalReservationResult
                                                                        {
                                                                            IdProductReservation = finalproduct.Id,
                                                                            IdReservationControl = reservations.IdReservationControl,
                                                                            IdReservation = reservations.Id,
                                                                            IdProduto = finalproduct.IdProduto,
                                                                            IdObra = reservations.IdObra,
                                                                            intClasse = categoria.Classe,
                                                                            Classe = categoria.ClassType,
                                                                            Type = catalogo.PorType,
                                                                            Codigo = catalogo.Codigo,
                                                                            Nome = catalogo.Nome,
                                                                            QtyFinal = finalproduct.FinalQuantity,
                                                                            DateReturn = finalproduct.DataPrevistaDevolucao.HasValue == true ? finalproduct.DataPrevistaDevolucao.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                                            DateReturnProper = finalproduct.DataPrevistaDevolucao,
                                                                            Observacao = finalproduct.Observacao,
                                                                            MemberCodPessoa = member.CodPessoa,
                                                                            LeaderCodPessoa = leader.CodPessoa,
                                                                        }).ToListAsync() ?? new List<FinalReservationResult>();

                //if (reservedproducts == null || reservedproducts.Count == 0) throw new InvalidOperationException("Nenhuma reserva finalizada encontrada.");


                Dictionary<int, employeeNewInformationModel> funcionarioDict = _context.Funcionario.Where(f => f.CodPessoa != null)
                                                                                .GroupBy(f => f.CodPessoa)
                                                                                .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                                .ToDictionary(
                                                                                    f => f.CodPessoa!.Value,
                                                                                    f => new employeeNewInformationModel
                                                                                    {
                                                                                        Chapa = f.Chapa,
                                                                                        Nome = f.Nome,
                                                                                        CodSituacao = f.CodSituacao,
                                                                                        CodColigada = f.CodColigada,
                                                                                        Funcao = f.Funcao,
                                                                                        Secao = f.Secao,
                                                                                        CodPessoa = f.CodPessoa,
                                                                                    }
                                                                                );

                var enrichedReservations = reservedproducts.Select(r =>
                {
                    // Get member and leader information from dictionary
                    //var memberInfo = funcionarioDict.TryGetValue(r.MemberCodPessoa.Value, out var member) ? member : new Funcionario();
                    //var leaderInfo = funcionarioDict.TryGetValue(r.LeaderCodPessoa.Value, out var leader) ? leader : new Funcionario();

                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new FinalReservationResult
                    {
                        IdProductReservation = r.IdProductReservation,
                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        IdProduto = r.IdProduto,
                        IdObra = r.IdObra,
                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        Nome = r.Nome,
                        QtyFinal = r.QtyFinal,
                        DateReturn = r.DateReturn,
                        DateReturnProper = r.DateReturnProper,
                        Observacao = r.Observacao,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;
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

        public async Task<Result> FinalizeProcessHandoutReservation(List<FinalSubmissionProcess> submissions, string transactionId, int userId, int idFerramentaria)
        {
            try
            {
                using (LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true))
                {
                    _logger.LogInformation("Processing FinalizeProcessHandoutReservation with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                }

                if (submissions.Count == 0) throw new ProcessErrorException("submissions are empty", nameof(submissions));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is empty", nameof(transactionId));
                if (userId == 0) throw new ProcessErrorException("userId is 0", nameof(userId));
                if (idFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0", nameof(idFerramentaria));

                //using var _ = LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true);
                //{
                //    _logger.LogInformation("Processing FinalizeProcessHandoutReservation with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                //}


                List<string?> errors = new List<string?>();

                //await using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {
                foreach (var item in submissions)
                {
                    Result result = await ProcessSubmissionItem(item, transactionId, userId, idFerramentaria);
                    if (result.IsFailure)
                    {
                        errors.Add(result.Error);
                    }
                }

                if (errors.Count > 0)
                {
                    //await transaction.RollbackAsync();
                    return Result.Failure(string.Join("<br>", errors));
                }

                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                return Result.Success();

                //    }
                //    catch (Exception)
                //    {
                //        _logger.LogError("Transaction Rollback - Insertion Fail.");
                //        await transaction.RollbackAsync();
                //        throw;
                //    }
                //}

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


        private async Task<Result> ProcessSubmissionItem(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            // Validate entities
            var reservation = await _context.Reservations.FirstOrDefaultAsync(i => i.Id == item.IdReservation);
            if (reservation == null)
                return Result.Failure($"IdReservation:{item.IdReservation} not found.");

            var productReservation = await _context.ProductReservation.FirstOrDefaultAsync(i => i.Id == item.IdProductReservation);
            if (productReservation == null)
                return Result.Failure($"Cannot find product reservation with the Id:{item.IdProductReservation}");

            var product = await _context.Produto.FirstOrDefaultAsync(x => x.Id == item.IdProduto);
            if (product == null)
                return Result.Failure($"Cannot find product with the Id:{item.IdProduto}");

            if (product.Quantidade < item.QtyRequested)
                return Result.Failure($"Insufficient stock for Product ID {item.IdProduto}. Available: {product.Quantidade}.");

            var historico = await _context.HistoricoAlocacao_2025.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
            if (historico != null)
                return Result.Failure($"IdReservation:{item.IdReservation} has already been allocated with TransactionId:{transactionId}. possible risk of duplication.");

            await CreateAllocationHistory(item, productReservation, transactionId, userId, idFerramentaria);
            UpdateProductStock(product, productReservation.FinalQuantity!.Value, transactionId);
            UpdateFinalReservationStatus(reservation, transactionId);
            UpdateProductReservationStatus(productReservation, userId, transactionId);

            return Result.Success();
        }

        private async Task CreateAllocationHistory(FinalSubmissionProcess item, ProductReservation productReservation, string transactionId, int userId, int idFerramentaria)
        {
            var historico = new HistoricoAlocacao_2025
            {
                IdProduto = item.IdProduto,
                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                Solicitante_CodColigada = item.CodColigadaSolicitante,
                Solicitante_Chapa = item.ChapaSolicitante,
                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                Liberador_CodColigada = item.CodColigadaLiberador,
                Liberador_Chapa = item.ChapaLiberador,
                Balconista_Emprestimo_IdLogin = userId,
                Balconista_Devolucao_IdLogin = userId,
                Observacao = $"Reservation: {productReservation.Observacao}",
                DataEmprestimo = DateTime.Now,
                DataDevolucao = productReservation.DataPrevistaDevolucao ?? DateTime.Now,
                IdObra = item.IdObra,
                Quantidade = productReservation.FinalQuantity,
                IdFerrOndeProdRetirado = idFerramentaria,
                IdControleCA = productReservation.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                EmprestimoTransactionId = transactionId,
                CrachaNo = item.CrachaNo
            };

            //using var _ = LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true);
            //{
            //    _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            //};

            using (LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true))
            {
                _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            //using var logScope = LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true);
            //_logger.LogInformation("Allocation - Transaction:{TransactionId}", transactionId);

            await _context.AddAsync(historico);
        }

        private void UpdateProductStock(Produto product, int finalQuantity, string transactionId)
        {
            int qtyBefore = product.Quantidade!.Value;
            int qtyAfter = product.Quantidade.Value - finalQuantity;

            _logger.LogInformation("StockUpdate - Transaction:{TransactionId} | ProductId:{ProductId} | Before:{QtyFrom} - After:{QtyAfter}", transactionId, product.Id, qtyBefore, qtyAfter);

            product.Quantidade = qtyAfter;
            _context.Update(product);
        }

        private void UpdateFinalReservationStatus(Reservations reservation, string transactionId)
        {
            _logger.LogInformation("ReservationUpdate - Transaction:{TransactionId} | ReservationId:{IdReservation} | From:{OldStatus} To:{NewStatus}", transactionId, reservation.Id, reservation.Status, 3);
            reservation.Status = 3;
            _context.Update(reservation);
        }

        private void UpdateProductReservationStatus(ProductReservation productReservation, int userId, string transactionId)
        {
            productReservation.Status = 3;
            productReservation.HandedBy = userId;
            productReservation.ModifiedTransactionId = transactionId;
            productReservation.ModifiedDate = DateTime.Now;
            _context.Update(productReservation);
        }



    }

    public class EmployeeServiceDemo : IEmployeeService
    {
        private readonly DemoDB _context;
        private readonly ILogger<EmployeeServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public EmployeeServiceDemo(ILogger<EmployeeServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<fnRetornaColaboradorCracha?> GetEmployeeCardInfo(string Icard)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeCardInfo Icard:{Icard}", Icard);

                if (string.IsNullOrWhiteSpace(Icard)) throw new ArgumentException("Icard is empty.");

                return await _context.fnRetornaColaboradorCracha
                        .Where(x => x.MATRICULA == Icard)
                        .FirstOrDefaultAsync();

                //return await _context.GetColaboradorCracha(Icard).SingleOrDefaultAsync();
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

                return await _context.Funcionario.Where(i => i.Chapa == matricula)
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

                List<EmployeeInformationBS>? employees = await _context.Funcionario
                                                        .Where(e => (e.Nome!.Contains(givenInformation) || e.Chapa!.Contains(givenInformation))
                                                        && e.DataMudanca == _context.Funcionario.Where(f => f.Chapa == e.Chapa).Max(f => f.DataMudanca))
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

                List<EmployeeInformationBS>? thirdParties = await (from funcionario in _context.FuncionarioSeek
                                                                   join secao in _context.Secao on funcionario.IdSecao equals secao.Id
                                                                   join funcao in _context.Funcao on funcionario.IdFuncao equals funcao.Id
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

                byte[]? base64Image = await (from pessoa in _context.PPESSOA
                                             join gImagem in _context.GIMAGEM on pessoa.IDIMAGEM equals gImagem.ID
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

                EmployeeInformationBS? employee = await _context.Funcionario
                                                        .Where(e => (e.CodPessoa == codpessoa)
                                                        && e.DataMudanca == _context.Funcionario.Where(f => f.CodPessoa == e.CodPessoa).Max(f => f.DataMudanca))
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

                EmployeeInformationBS? thirdParty = await (from funcionario in _context.FuncionarioSeek
                                                           join secao in _context.Secao on funcionario.IdSecao equals secao.Id
                                                           join funcao in _context.Funcao on funcionario.IdFuncao equals funcao.Id
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


                //using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {

                await _context.AddAsync(termsInformation);
                await _context.SaveChangesAsync();
                //        await transaction.CommitAsync();

                return Result.Success();

                //    }
                //    catch (Exception)
                //    {
                //        await transaction.RollbackAsync();
                //        throw;
                //    }

                //}



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

                EmployeeInformationBS? employee = await _context.Funcionario
                                        .Where(e => (e.CodPessoa == CodPessoa)
                                        && e.DataMudanca == _context.Funcionario.Where(f => f.CodPessoa == e.CodPessoa).Max(f => f.DataMudanca))
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

                VW_Usuario_New? balconista = _context.VW_Usuario_New.FirstOrDefault(i => i.Id == termsControlModel.Balconista);

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

    public class AuditServiceDemo : IAuditService
    {
        private readonly DemoDB _context;
        private readonly ILogger<AuditServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public AuditServiceDemo(ILogger<AuditServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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

    public class RetiradaServiceDemo : IRetiradaService
    {
        private readonly DemoDB _context;
        private readonly ILogger<RetiradaServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public RetiradaServiceDemo(ILogger<RetiradaServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<newCatalogInformationModel>> GetRetiradaOrders(int codpessoa, int idFerramentaria)
        {
            try
            {
                if (codpessoa == 0) throw new ProcessErrorException("codpessoa is 0");
                if (idFerramentaria == 0) throw new ProcessErrorException("idFerramentaria is 0");

                List<newCatalogInformationModel> reservedproducts = await (from reservations in _context.Reservations
                                                                           join control in _context.ReservationControl on reservations.IdReservationControl equals control.Id
                                                                           join member in _context.LeaderMemberRel on reservations.IdLeaderMemberRel equals member.Id
                                                                           join leader in _context.LeaderData on control.IdLeaderData equals leader.Id
                                                                           join catalogo in _context.Catalogo on reservations.IdCatalogo equals catalogo.Id
                                                                           join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                           join obra in _context.Obra on reservations.IdObra equals obra.Id
                                                                           where control.Type == 2
                                                                               && reservations.Status == 0
                                                                               && reservations.IdFerramentaria == idFerramentaria
                                                                               && member.CodPessoa == codpessoa
                                                                           group new
                                                                           {
                                                                               reservations,
                                                                               control,
                                                                               member,
                                                                               leader,
                                                                               catalogo,
                                                                               categoria,
                                                                               obra
                                                                           } by new { reservations.IdCatalogo, reservations.IdReservationControl } into grouped
                                                                           select new newCatalogInformationModel
                                                                           {
                                                                               IdCatalogo = grouped.Key.IdCatalogo,
                                                                               IdCategoria = grouped.First().catalogo.IdCategoria,
                                                                               intClasse = grouped.First().categoria.Classe,
                                                                               Classe = grouped.First().categoria.ClassType,
                                                                               Type = grouped.First().catalogo.PorType,
                                                                               Codigo = grouped.First().catalogo.Codigo,
                                                                               itemNome = grouped.First().catalogo.Nome,
                                                                               DataDeRetornoAutomatico = grouped.First().catalogo.DataDeRetornoAutomatico,
                                                                               IdObra = grouped.First().reservations.IdObra,
                                                                               ObraName = $"{grouped.First().obra.Codigo}-{grouped.First().obra.Nome}",
                                                                               IdReservationControl = grouped.First().control.Id,
                                                                               IdReservation = grouped.First().reservations.Id,
                                                                               QuantidadeResquested = grouped.Sum(x => x.reservations.Quantidade),
                                                                               MemberCodPessoa = grouped.First().member.CodPessoa,
                                                                               LeaderCodPessoa = grouped.First().leader.CodPessoa,
                                                                           }
                                                                     ).ToListAsync() ?? new List<newCatalogInformationModel>();

                if (reservedproducts.Count == 0) throw new InvalidOperationException("Nenhuma retirada encontrada.");

                foreach (newCatalogInformationModel item in reservedproducts)
                {
                    List<Produto>? listProducts = _context.Produto.Where(i => i.IdCatalogo == item.IdCatalogo
                                                                        && i.IdFerramentaria == idFerramentaria
                                                                        && i.Ativo == 1
                                                                        && i.Quantidade > 0).ToList();

                    if (listProducts.Count == 1)
                    {
                        item.IdProdutoSelected = listProducts[0].Id;
                    }

                    item.listProducts = listProducts
                                        .Where(p => item.Type != "PorAferido" || p.DataVencimento > DateTime.Now)
                                        .Select(p => new newProductInformation
                                        {
                                            IdProduto = p.Id,
                                            IdFerramentaria = p.IdFerramentaria,
                                            AF = p.AF,
                                            PAT = p.PAT,
                                            StockQuantity = p.Quantidade,
                                            DataVencimento = p.DataVencimento,
                                            AllowedToBorrow = item.Type == "PorAferido"
                                                ? p.DataVencimento > DateTime.Now
                                                : true,
                                            Reason = item.Type == "PorAferido"
                                                ? (p.DataVencimento > DateTime.Now ? "Valid" : "Expired")
                                                : string.Empty
                                        })
                                        .ToList();

                    if (item.intClasse == 2)
                    {
                        List<ControleCA>? controleCAData = _context.ControleCA.Where(i => i.IdCatalogo == item.IdCatalogo && i.Ativo == 1 && i.Validade > DateTime.Now).OrderByDescending(i => i.Validade).ToList() ?? new List<ControleCA>();

                        if (controleCAData.Count > 0)
                        {
                            item.listCA = controleCAData;
                        }

                        if (item.DataDeRetornoAutomatico.HasValue && item.DataDeRetornoAutomatico != 0)
                        {
                            item.DataReturn = DateTime.Now.AddDays(item.DataDeRetornoAutomatico.Value);
                        }

                    }

                }

                Dictionary<int, employeeNewInformationModel> funcionarioDict = _context.Funcionario.Where(f => f.CodPessoa != null)
                                                                                   .GroupBy(f => f.CodPessoa)
                                                                                   .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                                   .ToDictionary(
                                                                                       f => f.CodPessoa!.Value,
                                                                                       f => new employeeNewInformationModel
                                                                                       {
                                                                                           Chapa = f.Chapa,
                                                                                           Nome = f.Nome,
                                                                                           CodSituacao = f.CodSituacao,
                                                                                           CodColigada = f.CodColigada,
                                                                                           Funcao = f.Funcao,
                                                                                           Secao = f.Secao,
                                                                                           CodPessoa = f.CodPessoa,
                                                                                       }
                                                                                   );

                var enrichedReservations = reservedproducts.Select(r =>
                {

                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new newCatalogInformationModel
                    {
                        IdCatalogo = r.IdCatalogo,
                        IdCategoria = r.IdCategoria,
                        intClasse = r.intClasse,
                        Classe = r.Classe,
                        Type = r.Type,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        DataDeRetornoAutomatico = r.DataDeRetornoAutomatico,
                        IdObra = r.IdObra,
                        ObraName = r.ObraName,
                        IdReservationControl = r.IdReservationControl,
                        IdReservation = r.IdReservation,
                        QuantidadeResquested = r.QuantidadeResquested,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        listProducts = r.listProducts,
                        listCA = r.listCA,
                        IdProdutoSelected = r.IdProdutoSelected,
                        DataReturn = r.DataReturn,
                        IsTransferable = _context.Produto.Where(i => i.IdCatalogo == r.IdCatalogo && i.Ativo == 1 && i.Quantidade > 0 && i.IdFerramentaria != idFerramentaria && i.IdFerramentaria != 17).ToList().Count() > 0 ? true : false,

                        // Add new properties with funcionario data
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel()
                    };
                }).ToList();



                return enrichedReservations;

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

        public async Task CancelRetirada(int idReservation, string chapa, string observacao, string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing CancelRetirada IdReservation:{idReservation}, Chapa:{chapa}, Observacao:{observacao}, TransactionId:{transactionId}", idReservation, chapa, observacao, transactionId);

                if (idReservation <= 0) throw new ProcessErrorException("IdReservation is 0");
                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("Chapa is null or whitespace", nameof(chapa));
                if (string.IsNullOrWhiteSpace(observacao)) throw new ProcessErrorException("observacao is null or whitespace", nameof(observacao));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is null or whitespace", nameof(transactionId));

                Reservations? reservation = await _context.Reservations.FindAsync(idReservation);
                if (reservation == null) throw new ProcessErrorException($"Reserva:{idReservation} não encontrado.");

                if (reservation.Status == 8) throw new ProcessErrorException($"Reserva:{idReservation} já está cancelada.");

                //using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {

                reservation.Status = 8;
                reservation.Observacao = $"Cancellado por: {chapa} - {observacao}";

                await _context.SaveChangesAsync();
                //        await transaction.CommitAsync();

                //    }
                //    catch (Exception)
                //    {
                //        await transaction.RollbackAsync();
                //        throw;
                //    }
                //}



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

        public async Task TransferRetirada(int idReservation, string observacao, int IdFerramentariaTo, string transactionId)
        {
            try
            {
                _logger.LogInformation("Processing TransferRetirada IdReservation:{id}, Observacao:{Observacao}, FerramentariaTo:{IdFerramentariaTo}, TransactionId:{transactionId}", idReservation, observacao, IdFerramentariaTo, transactionId);


                if (idReservation <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));
                if (string.IsNullOrWhiteSpace(observacao)) throw new ProcessErrorException("Observation cannot be empty", nameof(observacao));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("Observation cannot be empty", nameof(transactionId));
                if (IdFerramentariaTo <= 0) throw new ProcessErrorException("Invalid Ferramentaria ID", nameof(IdFerramentariaTo));

                Reservations? reservation = await _context.Reservations.FindAsync(idReservation);

                if (reservation == null) throw new ProcessErrorException("Reservation is null");

                using var transaction = await _context.Database.BeginTransactionAsync();
                {
                    try
                    {

                        reservation.Status = 0;
                        reservation.Observacao = observacao;
                        reservation.IdFerramentaria = IdFerramentariaTo;

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

        public async Task<Result> FinalizeProcessHandoutRetirada(List<FinalSubmissionProcess> submissions, string transactionId, int userId, int idFerramentaria)
        {
            try
            {
                using (LogContext.PushProperty("FinalHandoutReservation", submissions, destructureObjects: true))
                {
                    _logger.LogInformation("Processing FinalizeProcessHandoutRetirada with TransactionId:{TransactionId} of User:{UserId}", transactionId, userId);
                }

                if (submissions.Count == 0) throw new ProcessErrorException("submissions are empty", nameof(submissions));
                if (string.IsNullOrWhiteSpace(transactionId)) throw new ProcessErrorException("transactionId is empty", nameof(transactionId));
                if (userId == 0) throw new ProcessErrorException("userId is 0", nameof(userId));
                if (idFerramentaria == 0) throw new ProcessErrorException("IdFerramentaria is 0", nameof(idFerramentaria));

                List<string?> errors = new List<string?>();

                //await using var transaction = await _context.Database.BeginTransactionAsync();
                //{
                //    try
                //    {
                foreach (var item in submissions)
                {
                    Result result = await ProcessSubmissionItem(item, transactionId, userId, idFerramentaria);
                    if (result.IsFailure)
                    {
                        errors.Add(result.Error);
                    }
                }

                if (errors.Count > 0)
                {
                    //await transaction.RollbackAsync();
                    return Result.Failure(string.Join("<br>", errors));
                }

                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                return Result.Success();

                //    }
                //    catch (Exception)
                //    {
                //        _logger.LogError("Transaction Rollback - Insertion Fail.");
                //        await transaction.RollbackAsync();
                //        throw;
                //    }
                //}


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



        private async Task<Result> ProcessSubmissionItem(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(i => i.Id == item.IdReservation);
            if (reservation == null) return Result.Failure($"IdReservation:{item.IdReservation} not found.");

            var product = await _context.Produto.FirstOrDefaultAsync(x => x.Id == item.IdProduto);
            if (product == null) return Result.Failure($"Cannot find product with the Id:{item.IdProduto}");

            if (product.Quantidade < item.QtyRequested) return Result.Failure($"Insufficient stock for Product ID {item.IdProduto}. Available: {product.Quantidade}.");

            if (item.Classe == 3)
            {
                var historico = await _context.HistoricoAlocacao_2025.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
                if (historico != null) return Result.Failure($"IdReservation:{item.IdReservation} has already been History allocated with TransactionId:{transactionId}. possible risk of duplication.");

                await CreateAllocationHistory(item, transactionId, userId, idFerramentaria);
            }
            else
            {
                var produtoAlocado = await _context.ProdutoAlocado.FirstOrDefaultAsync(i => i.IdReservation == item.IdReservation && i.TransactionId == transactionId);
                if (produtoAlocado != null) return Result.Failure($"IdReservation:{item.IdReservation} has already been product allocated with TransactionId:{transactionId}. possible risk of duplication.");

                await CreateProdutoAlocado(item, transactionId, userId, idFerramentaria);
            }

            UpdateProductStock(product, item.QtyRequested!.Value, transactionId);
            UpdateFinalReservationStatus(reservation, transactionId);

            return Result.Success();
        }

        private async Task CreateAllocationHistory(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            var historico = new HistoricoAlocacao_2025
            {
                IdProduto = item.IdProduto,
                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                Solicitante_CodColigada = item.CodColigadaSolicitante,
                Solicitante_Chapa = item.ChapaSolicitante,
                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                Liberador_CodColigada = item.CodColigadaLiberador,
                Liberador_Chapa = item.ChapaLiberador,
                Balconista_Emprestimo_IdLogin = userId,
                Balconista_Devolucao_IdLogin = userId,
                Observacao = $"Retirada: {item.Observacao}",
                DataEmprestimo = DateTime.Now,
                DataDevolucao = item.DateReturn ?? DateTime.Now,
                IdObra = item.IdObra,
                Quantidade = item.QtyRequested,
                IdFerrOndeProdRetirado = idFerramentaria,
                IdControleCA = item.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                EmprestimoTransactionId = transactionId,
                CrachaNo = item.CrachaNo,
            };

            using (LogContext.PushProperty("HistoricoAlocacao", historico, destructureObjects: true))
            {
                _logger.LogInformation("Allocation - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            await _context.AddAsync(historico);
        }

        private async Task CreateProdutoAlocado(FinalSubmissionProcess item, string transactionId, int userId, int idFerramentaria)
        {
            string key = $"{item.IdProduto}-{item.CodColigadaSolicitante}-{item.ChapaSolicitante}-{userId}-{DateTime.Now:dd/MM/yyyy HH:mm}-{item.IdObra}-{item.QtyRequested}-{idFerramentaria}";
            string hash;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(key);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            ProdutoAlocado produtoAlocado = new ProdutoAlocado
            {
                IdProduto = item.IdProduto,
                IdObra = item.IdObra,
                IdFerrOndeProdRetirado = idFerramentaria,
                Solicitante_IdTerceiro = item.IdTerceiroSolicitante,
                Solicitante_CodColigada = item.CodColigadaSolicitante,
                Solicitante_Chapa = item.ChapaSolicitante,
                Balconista_IdLogin = userId,
                Liberador_IdTerceiro = item.IdTerceiroLiberador,
                Liberador_CodColigada = item.CodColigadaLiberador,
                Liberador_Chapa = item.ChapaLiberador,
                Observacao = $"Retirada:{item.Observacao}",
                DataPrevistaDevolucao = item.DateReturn,
                DataEmprestimo = DateTime.Now,
                Quantidade = item.QtyRequested,
                Chave = hash,
                IdControleCA = item.IdControleCA,
                IdReservation = item.IdReservation,
                TransactionId = transactionId,
                CrachaNo = item.CrachaNo,
            };

            using (LogContext.PushProperty("ProdutoAlocado", produtoAlocado, destructureObjects: true))
            {
                _logger.LogInformation("ProdutoAlocado - Transaction:{TransactionId} - ReservationId:{IdReservation}", transactionId, item.IdReservation);
            }

            await _context.AddAsync(produtoAlocado);

        }

        private void UpdateProductStock(Produto product, int finalQuantity, string transactionId)
        {
            int qtyBefore = product.Quantidade!.Value;
            int qtyAfter = product.Quantidade.Value - finalQuantity;

            _logger.LogInformation("StockUpdate - Transaction:{TransactionId} | ProductId:{ProductId} | Before:{QtyFrom} - After:{QtyAfter}", transactionId, product.Id, qtyBefore, qtyAfter);

            product.Quantidade = qtyAfter;
            _context.Update(product);
        }

        private void UpdateFinalReservationStatus(Reservations reservation, string transactionId)
        {
            _logger.LogInformation("ReservationUpdate - Transaction:{TransactionId} | ReservationId:{IdReservation} | From:{OldStatus} To:{NewStatus}", transactionId, reservation.Id, reservation.Status, 3);
            reservation.Status = 3;
            _context.Update(reservation);
        }



    }

    public class ConsultReservationRetiradaServiceDemo : IConsultReservationRetirada
    {
        private readonly DemoDB _context;
        private readonly ILogger<ConsultReservationRetiradaServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public ConsultReservationRetiradaServiceDemo(ILogger<ConsultReservationRetiradaServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ConsultationReserveModel>?> GetReservationDetailsByEmployee(GestorRRFilterModel? filter)
        {
            try
            {
                using var _ = LogContext.PushProperty("ReservationRetiradaFilter", filter, destructureObjects: true);
                _logger.LogInformation("Processing GetReservationDetailsByEmployee");

                if (filter == null) throw new ProcessErrorException("filter is null");

                List<ConsultationReserveModel>? listReservation = await (from reserve in _context.Reservations
                                                                         join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                                         join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                         join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                                         join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
                                                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                                         join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                                         where member.Chapa == filter.Chapa
                                                                          && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
                                                                          && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
                                                                          && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
                                                                          && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
                                                                          && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
                                                                         select new ConsultationReserveModel
                                                                         {
                                                                             Classe = categoria.ClassType,
                                                                             Codigo = catalogo.Codigo,
                                                                             itemNome = catalogo.Nome,
                                                                             MemberCodPessoa = member.CodPessoa,
                                                                             LeaderCodPessoa = leader.CodPessoa,
                                                                             Quantidade = reserve.Quantidade,
                                                                             Ferramentaria = ferramentaria.Nome,
                                                                             OrderNo = reservationControl.Id,
                                                                             ReservationType = reservationControl.TypeString,
                                                                             StatusString = reserve.StatusString,
                                                                         }).ToListAsync() ?? new List<ConsultationReserveModel>();

                if (listReservation == null || listReservation.Count == 0) throw new InvalidOperationException($"Nenhuma reserva/retirada encontrada para o funcionário:{filter.Chapa}");

                Dictionary<int, employeeNewInformationModel> funcionarioDict = await _context.Funcionario.Where(f => f.CodPessoa != null)
                                                                                 .GroupBy(f => f.CodPessoa)
                                                                                 .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                                 .ToDictionaryAsync(
                                                                                     f => f.CodPessoa!.Value,
                                                                                     f => new employeeNewInformationModel
                                                                                     {
                                                                                         Chapa = f.Chapa,
                                                                                         Nome = f.Nome,
                                                                                         CodSituacao = f.CodSituacao,
                                                                                         CodColigada = f.CodColigada,
                                                                                         Funcao = f.Funcao,
                                                                                         Secao = f.Secao,
                                                                                         CodPessoa = f.CodPessoa,
                                                                                     }
                                                                                 );

                var enrichedReservations = listReservation.Select(r =>
                {
                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new ConsultationReserveModel
                    {
                        Classe = r.Classe,
                        Codigo = r.Codigo,
                        itemNome = r.itemNome,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        Quantidade = r.Quantidade,
                        Ferramentaria = r.Ferramentaria,
                        OrderNo = r.OrderNo,
                        ReservationType = r.ReservationType,
                        StatusString = r.StatusString,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;

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

        public async Task<ConsultationModel?> GetReservationRetiradaInformation(int orderNo)
        {
            try
            {
                if (orderNo <= 0) throw new ProcessErrorException("OrderNo is less than or equal to 0");

                _logger.LogInformation("Processing GetReservationRetiradaInformation with OrderNo:{orderNo}", orderNo);

                ConsultationModel? consultItems = await (from reserve in _context.Reservations
                                                         join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                         join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                         join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                         join catalogo in _context.Catalogo on reserve.IdCatalogo equals catalogo.Id
                                                         join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                         join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                         where reserve.IdReservationControl == orderNo
                                                         group new
                                                         {
                                                             reserve,
                                                             reservationControl,
                                                             leader,
                                                             member,
                                                             catalogo,
                                                             categoria,
                                                             ferramentaria
                                                         } by new { reserve.IdReservationControl } into grouped
                                                         select new ConsultationModel
                                                         {
                                                             ControlId = grouped.Key.IdReservationControl,
                                                             LeaderName = grouped.First().leader.Nome,
                                                             ControlType = grouped.First().reservationControl.TypeString,
                                                             ControlStatusString = grouped.First().reservationControl.StatusString,
                                                             DateRegistration = grouped.First().reservationControl.DataRegistro.HasValue == true ? grouped.First().reservationControl.DataRegistro.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                             DateExpiration = grouped.First().reservationControl.ExpirationDate.HasValue == true ? grouped.First().reservationControl.ExpirationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                             ReservationList = grouped.Select(g => new ConsultationReserveModel
                                                             {
                                                                 Classe = g.categoria.ClassType,
                                                                 Codigo = g.catalogo.Codigo,
                                                                 itemNome = g.catalogo.Nome,
                                                                 Requester = g.member.Nome,
                                                                 Ferramentaria = g.ferramentaria.Nome,
                                                                 Quantidade = g.reserve.Quantidade,
                                                                 StatusString = g.reserve.StatusString,
                                                             }).ToList()
                                                         }).FirstOrDefaultAsync();

                return consultItems;

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

        public async Task<List<CatalogDetail>?> GetGestorListInformation(GestorRRFilterModel filter)
        {
            try
            {
                using var _ = LogContext.PushProperty("ReservationRetiradaFilter", filter, destructureObjects: true);
                _logger.LogInformation("Processing GetGestorListInformation");

                if (filter == null) throw new ProcessErrorException("filter is null");

                List<CatalogDetail>? CatalogList = await (from produto in _context.Produto
                                                          join ferramentaria in _context.Ferramentaria on produto.IdFerramentaria equals ferramentaria.Id
                                                          join catalogo in _context.Catalogo on produto.IdCatalogo equals catalogo.Id
                                                          join categoria in _context.Categoria on catalogo.IdCategoria equals categoria.Id
                                                          join categoriaPai in _context.Categoria on categoria.IdCategoria equals categoriaPai.Id
                                                          where
                                                             catalogo.Ativo == 1
                                                          && produto.Ativo == 1
                                                          && produto.IdFerramentaria != 17
                                                          && produto.Quantidade > 0
                                                          && categoria.Ativo == 1
                                                          && ferramentaria.Ativo == 1
                                                          && (filter.IdCatalogo == null || categoria.Classe == filter.IdCatalogo)
                                                          && (filter.IdClasse == null || categoria.IdCategoria == filter.IdClasse)
                                                          && (filter.IdTipo == null || categoria.Id == filter.IdTipo)
                                                          && (string.IsNullOrEmpty(filter.Item) || catalogo.Nome.Contains(filter.Item))
                                                          && (filter.Codigo == null || catalogo.Codigo == filter.Codigo)
                                                          && !catalogo.Nome!.Contains("INUTILIZAR")
                                                          select new CatalogDetail
                                                          {
                                                              Id = catalogo.Id,
                                                              IdCategoria = catalogo.IdCategoria,
                                                              Codigo = catalogo.Codigo,
                                                              Classe = categoria.Nome,
                                                              Tipo = categoriaPai.Nome,
                                                              Nome = catalogo.Nome,
                                                              PorType = catalogo.PorType,
                                                              ClassType = categoria.ClassType,
                                                              Quantity = produto.Quantidade,
                                                              IdFerramentaria = ferramentaria.Id,
                                                              Ferramentaria = ferramentaria.Nome,
                                                              Ferramentarias = new List<FerramentariaStockModel>
                                                                                    {
                                                                                        new FerramentariaStockModel
                                                                                        {
                                                                                            Id = ferramentaria.Id,
                                                                                            Nome = ferramentaria.Nome,
                                                                                            Quantity = produto.Quantidade, // Raw quantity before grouping
                                                                                        }
                                                                                    }
                                                          }).ToListAsync();

                if (CatalogList == null || CatalogList.Count == 0) throw new InvalidOperationException("Nenhum resultado encontrado.");

                List<CatalogDetail>? GroupedCatalogList = CatalogList
                                                    .GroupBy(x => x.Id)
                                                    .Select(group => new CatalogDetail
                                                    {
                                                        Id = group.Key,
                                                        IdCategoria = group.First().IdCategoria,
                                                        Codigo = group.First().Codigo,
                                                        Classe = group.First().Classe,
                                                        Tipo = group.First().Tipo,
                                                        Nome = group.First().Nome,
                                                        PorType = group.First().PorType,
                                                        ClassType = group.First().ClassType,
                                                        Quantity = group.Sum(x => x.Quantity),
                                                        Ferramentarias = group
                                                                        .SelectMany(x => x.Ferramentarias)
                                                                        .GroupBy(f => f.Id)
                                                                        .Select(fGroup => new FerramentariaStockModel
                                                                        {
                                                                            Id = fGroup.Key,
                                                                            Nome = fGroup.First().Nome,
                                                                            Quantity = fGroup.Sum(f => f.Quantity),
                                                                        })
                                                                        .ToList(),
                                                    }).ToList();

                var reservedQuantities = await _context.Reservations
                                            .Where(r => r.Status != 7 && r.Status != 8 && r.Status != 3)
                                            .GroupBy(r => new { r.IdCatalogo, r.IdFerramentaria })
                                            .Select(g => new
                                            {
                                                g.Key.IdCatalogo,
                                                g.Key.IdFerramentaria,
                                                TotalReserved = g.Sum(x => x.Quantidade)
                                            })
                                            .ToDictionaryAsync(x => new { x.IdCatalogo, x.IdFerramentaria }, x => x.TotalReserved);

                var finalCatalogList = GroupedCatalogList.Select(catalog =>
                {
                    // Update quantities for each location
                    var updatedFerramentarias = catalog.Ferramentarias!.Select(ferramentaria =>
                    {
                        var key = new
                        {
                            IdCatalogo = catalog.Id,
                            IdFerramentaria = ferramentaria.Id  // Changed from 'Id' to 'IdFerramentaria'
                        };
                        var reservedQty = reservedQuantities.TryGetValue(key, out var reserved) ? reserved : 0;

                        return new FerramentariaStockModel
                        {
                            Id = ferramentaria.Id,
                            Nome = ferramentaria.Nome,
                            Quantity = ferramentaria.Quantity,
                            ReservedQuantity = reservedQty,
                            AvailableQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
                            ferramentariaAllocatedQuantity = ferramentaria.Quantity - (reservedQty ?? 0),
                        };
                    }).ToList();

                    return new CatalogDetail
                    {
                        Id = catalog.Id,
                        IdCategoria = catalog.IdCategoria,
                        Codigo = catalog.Codigo,
                        Classe = catalog.Classe,
                        Tipo = catalog.Tipo,
                        Nome = catalog.Nome,
                        PorType = catalog.PorType,
                        ClassType = catalog.ClassType,
                        Quantity = catalog.Quantity,
                        OverallQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
                        Ferramentaria = string.Join(", ",
                                                            updatedFerramentarias
                                                                .Where(f => f.AvailableQuantity > 0)
                                                                .Select(f => f.Nome)
                                                                .Distinct()),
                        Ferramentarias = updatedFerramentarias,
                        ReservedQuantity = updatedFerramentarias.Sum(f => f.ReservedQuantity),
                        allocatedQuantity = updatedFerramentarias.Sum(f => f.AvailableQuantity),
                    };
                }).ToList();

                //if (filter.IsChecked == true) return finalCatalogList.Where(i => i.ReservedQuantity > 0).ToList();

                //if (filter.IsChecked == true) return finalCatalogList.Where(i => i.Ferramentarias!.Any(e => e.ReservedQuantity > 0)).ToList();

                if (filter.IsChecked == true)
                {
                    // First filter catalogs that have any reserved ferramentarias
                    var catalogsWithReserved = finalCatalogList
                        .Where(catalog => catalog.Ferramentarias!.Any(f => f.ReservedQuantity > 0))
                        .ToList();

                    // Then filter ferramentarias within those catalogs
                    return catalogsWithReserved.Select(catalog =>
                    {
                        var filteredFerramentarias = catalog.Ferramentarias!
                            .Where(ferramentaria => ferramentaria.ReservedQuantity > 0)
                            .ToList();

                        // Create a new catalog with only the filtered ferramentarias
                        return new CatalogDetail
                        {
                            // Copy all properties...
                            Id = catalog.Id,
                            IdCategoria = catalog.IdCategoria,
                            Codigo = catalog.Codigo,
                            Classe = catalog.Classe,
                            Tipo = catalog.Tipo,
                            Nome = catalog.Nome,
                            PorType = catalog.PorType,
                            ClassType = catalog.ClassType,
                            Quantity = catalog.Quantity,
                            // Recalculate quantities based on filtered ferramentarias
                            OverallQuantity = filteredFerramentarias.Sum(f => f.AvailableQuantity),
                            Ferramentaria = string.Join(", ",
                                filteredFerramentarias
                                    .Where(f => f.AvailableQuantity > 0)
                                    .Select(f => f.Nome)
                                    .Distinct()),
                            Ferramentarias = filteredFerramentarias,
                            ReservedQuantity = filteredFerramentarias.Sum(f => f.ReservedQuantity),
                            allocatedQuantity = filteredFerramentarias.Sum(f => f.AvailableQuantity),
                        };
                    }).ToList();
                }

                return finalCatalogList;

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

        public async Task<List<ConsultationReserveModel>?> GetReservationForCatalog(int idCatalogo)
        {
            try
            {
                _logger.LogInformation("Processing GetReservationForCatalog with idCatalogo:{IdCatalogo}", idCatalogo);

                if (idCatalogo <= 0) throw new ProcessErrorException("idcatalogo is less than or equal to 0");

                List<ConsultationReserveModel>? reservationResult = await (from reserve in _context.Reservations
                                                                           join reservationControl in _context.ReservationControl on reserve.IdReservationControl equals reservationControl.Id
                                                                           join leader in _context.LeaderData on reservationControl.IdLeaderData equals leader.Id
                                                                           join member in _context.LeaderMemberRel on reserve.IdLeaderMemberRel equals member.Id
                                                                           join ferramentaria in _context.Ferramentaria on reserve.IdFerramentaria equals ferramentaria.Id
                                                                           where reserve.IdCatalogo == idCatalogo
                                                                           select new ConsultationReserveModel
                                                                           {
                                                                               IdReservation = reserve.Id,
                                                                               Ferramentaria = ferramentaria.Nome,
                                                                               MemberCodPessoa = member.CodPessoa,
                                                                               LeaderCodPessoa = leader.CodPessoa,
                                                                               Quantidade = reserve.Quantidade,
                                                                               OrderNo = reservationControl.Id,
                                                                               ReservationType = reservationControl.TypeString,
                                                                               StatusString = reserve.StatusString,
                                                                               DateReservation = reserve.DataRegistro,
                                                                               DateReservationString = reserve.DataRegistro.HasValue == true ? reserve.DataRegistro.Value.ToShortDateString() : string.Empty,
                                                                           }).ToListAsync();

                if (reservationResult.Count == 0) throw new InvalidOperationException("Nenhum resultado encontrado");

                Dictionary<int, employeeNewInformationModel> funcionarioDict = await _context.Funcionario.Where(f => f.CodPessoa != null)
                                                                           .GroupBy(f => f.CodPessoa)
                                                                           .Select(g => g.OrderByDescending(f => f.DataMudanca).First())
                                                                           .ToDictionaryAsync(
                                                                               f => f.CodPessoa!.Value,
                                                                               f => new employeeNewInformationModel
                                                                               {
                                                                                   Chapa = f.Chapa,
                                                                                   Nome = f.Nome,
                                                                                   CodSituacao = f.CodSituacao,
                                                                                   CodColigada = f.CodColigada,
                                                                                   Funcao = f.Funcao,
                                                                                   Secao = f.Secao,
                                                                                   CodPessoa = f.CodPessoa,
                                                                               }
                                                                           );

                var enrichedReservations = reservationResult.Select(r =>
                {
                    funcionarioDict.TryGetValue(r.MemberCodPessoa ?? -1, out var memberInfo);
                    funcionarioDict.TryGetValue(r.LeaderCodPessoa ?? -1, out var leaderInfo);

                    return new ConsultationReserveModel
                    {
                        IdReservation = r.IdReservation,
                        Ferramentaria = r.Ferramentaria,
                        MemberCodPessoa = r.MemberCodPessoa,
                        LeaderCodPessoa = r.LeaderCodPessoa,
                        Quantidade = r.Quantidade,
                        OrderNo = r.OrderNo,
                        ReservationType = r.ReservationType,
                        StatusString = r.StatusString,
                        DateReservation = r.DateReservation,
                        DateReservationString = r.DateReservationString,
                        MemberInfo = memberInfo ?? new employeeNewInformationModel(),
                        LeaderInfo = leaderInfo ?? new employeeNewInformationModel(),
                    };
                }).ToList();

                return enrichedReservations;


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

    public class HistoryAlocadoServiceDemo : IHistoryAlocadoService
    {
        private readonly DemoDB _context;
        private readonly ILogger<HistoryAlocadoServiceDemo> _logger;
        protected IHttpContextAccessor _httpContextAccessor;

        public HistoryAlocadoServiceDemo(ILogger<HistoryAlocadoServiceDemo> logger, IHttpContextAccessor httpContextAccessor, DemoDB context)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<HistoryAlocadoReportModel>> GetEmployeeItemHistory(string chapa, int codColigada, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory Chapa:{chapa}, Year:{year}", chapa, year);

                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("chapa is empty.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                string tableName = $"HistoricoAlocacao_{year}";

                var dbSetProperties = _context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                if (tableProperty == null) throw new ProcessErrorException($"Table:{tableName} not found on the database.");

                var table = tableProperty.GetValue(_context, null);

                //_context.Database.SetCommandTimeout(300);

                List<HistoryAlocadoReportModel> historyResult = await (from history in (IQueryable<HistoricoAlocacao>)table
                                                                       join product in _context.Produto on history.IdProduto equals product.Id
                                                                       join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                       join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                       join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                                                                       join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                                                                       from destination in ferrDevGroup.DefaultIfEmpty()
                                                                       join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                                                                       from controle in controlGroup.DefaultIfEmpty()
                                                                       where history.Solicitante_Chapa == chapa
                                                                              && history.Solicitante_CodColigada == codColigada
                                                                              && history.DataDevolucao >= startDate
                                                                              && history.DataDevolucao <= endDate
                                                                       select new HistoryAlocadoReportModel
                                                                       {
                                                                           IdCatalogo = product.IdCatalogo,
                                                                           Code = catalog.Codigo,
                                                                           Description = catalog.Nome,
                                                                           IdProduto = product.Id,
                                                                           BorrowedDate = history.DataEmprestimo,
                                                                           BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           Qty = history.Quantidade.ToString(),
                                                                           LocationOrigin = origin.Nome,
                                                                           LocationReturn = destination != null ? destination.Nome : string.Empty,
                                                                           ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                           IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                                                                           IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                           //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                           IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                                                                           IdTransactionEmprestimo = year >= 2025 ? history.EmprestimoTransactionId : string.Empty,
                                                                           ItemClass = category.ClassType,
                                                                           CrachaNo = year >= 2025 ? history.CrachaNo : string.Empty,
                                                                       }).AsNoTracking().ToListAsync();


                //var historyResult = (from history in (IQueryable<HistoricoAlocacao>)table
                //                                                        join product in _context.Produto on history.IdProduto equals product.Id
                //                                                        join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                //                                                        join category in _context.Categoria on catalog.IdCategoria equals category.Id
                //                                                        join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                //                                                        join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                //                                                        from destination in ferrDevGroup.DefaultIfEmpty()
                //                                                        join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                //                                                        from controle in controlGroup.DefaultIfEmpty()
                //                                                        where history.Solicitante_Chapa == chapa
                //                                                               && history.Solicitante_CodColigada == codColigada
                //                                                               && history.DataDevolucao >= startDate
                //                                                               && history.DataDevolucao <= endDate
                //                                                        select new HistoryAlocadoReportModel
                //                                                        {
                //                                                            IdCatalogo = product.IdCatalogo,
                //                                                            Code = catalog.Codigo,
                //                                                            Description = catalog.Nome,
                //                                                            IdProduto = product.Id,
                //                                                            BorrowedDate = history.DataEmprestimo,
                //                                                            BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy") : string.Empty,
                //                                                            ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy") : string.Empty,
                //                                                            Qty = history.Quantidade.ToString(),
                //                                                            LocationOrigin = origin.Nome,
                //                                                            LocationReturn = destination != null ? destination.Nome : string.Empty,
                //                                                            ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                //                                                            IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                //                                                            IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                //                                                            //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                //                                                            IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                //                                                        });

                //if (historyResult == null || historyResult.Count == 0) throw new ProcessErrorException($"No Result Found for Employee:{chapa}.");
                if (historyResult == null || historyResult.Count == 0) return new List<HistoryAlocadoReportModel>();

                var userIds = historyResult
                               .Select(x => new
                               {
                                   x.IdBalconistaBorrow,
                                   x.IdBalconistaReturn
                               })
                               .Distinct()
                               .ToList();

                // Step 2: Extract and prepare user IDs for lookup
                var borrowUserIds = userIds.Select(x => x.IdBalconistaBorrow).Distinct().ToList();
                var returnUserIds = userIds.Select(x => x.IdBalconistaReturn).Where(id => id.HasValue).Distinct().ToList();
                var allUserIds = borrowUserIds.Union(returnUserIds).Distinct().ToList();

                // Step 3: Get user data from second context
                var funcionarioDict = await _context.VW_Usuario_New.Where(u => allUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                historyResult = historyResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdBalconistaReturn = x.IdBalconistaReturn,
                                    BalconistaReturn = x.IdBalconistaReturn.HasValue && funcionarioDict.ContainsKey(x.IdBalconistaReturn.Value) ? funcionarioDict[x.IdBalconistaReturn.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    IdTransactionEmprestimo = x.IdTransactionEmprestimo != null ? x.IdTransactionEmprestimo : string.Empty,
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo,
                                }).ToList();

                return historyResult;

                //var result =  (from completeHistory in historyResult
                //                                          join userBorrow in _contextBS.VW_Usuario on completeHistory.IdBalconistaBorrow equals userBorrow.Id
                //                                          join userReturn in _contextBS.VW_Usuario on completeHistory.IdBalconistaReturn equals userReturn.Id
                //                                          select new HistoryAlocadoReportModel
                //                                          {
                //                                              IdCatalogo = completeHistory.IdCatalogo,
                //                                              Code = completeHistory.Code,
                //                                              Description = completeHistory.Description,
                //                                              IdProduto = completeHistory.IdProduto,
                //                                              BorrowedDate = completeHistory.BorrowedDate,
                //                                              BorrowedDateString = completeHistory.BorrowedDateString,
                //                                              ReturnedDateString = completeHistory.ReturnedDateString,
                //                                              Qty = completeHistory.Qty,
                //                                              LocationOrigin = completeHistory.LocationOrigin,
                //                                              LocationReturn = completeHistory.LocationReturn,
                //                                              ControlCA = completeHistory.ControlCA,
                //                                              IdBalconistaBorrow = completeHistory.IdBalconistaBorrow,
                //                                              BalconistaBorrow = userBorrow.Nome,
                //                                              IdBalconistaReturn = completeHistory.IdBalconistaReturn,
                //                                              BalconistaReturn = userReturn.Nome,
                //                                              IdTransaction = completeHistory.IdTransaction,
                //                                          }).AsQueryable();

                //List<HistoryAlocadoReportModel> resulttest = new List<HistoryAlocadoReportModel>();

                //resulttest.AddRange(result);

                //List<CatalogGroupModel> FinalResult = historyResult.GroupBy(i => i.IdCatalogo)
                //                                      .Select(catalog => new CatalogGroupModel
                //                                      {
                //                                          IdCatalogo = catalog.Key,
                //                                          Description = catalog.FirstOrDefault()!.Description,
                //                                          Code = catalog.FirstOrDefault()!.Code,
                //                                          ItemAllocation = catalog.OrderByDescending(x => x.BorrowedDate).ToList()
                //                                      }).ToList();



                //return FinalResult;
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

        public async Task<List<HistoryAlocadoReportModel>> GetEmployeeItemAllocation(string chapa, int codColigada, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory Chapa:{chapa}, Year:{year}", chapa, year);

                if (string.IsNullOrWhiteSpace(chapa)) throw new ProcessErrorException("chapa is empty.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                List<HistoryAlocadoReportModel> allocationResult = await (from allocation in _context.ProdutoAlocado
                                                                          join product in _context.Produto on allocation.IdProduto equals product.Id
                                                                          join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                          join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                          join origin in _context.Ferramentaria on allocation.IdFerrOndeProdRetirado equals origin.Id
                                                                          join controle in _context.ControleCA on allocation.IdControleCA equals controle.Id into controlGroup
                                                                          from controle in controlGroup.DefaultIfEmpty()
                                                                          where allocation.Solicitante_Chapa == chapa
                                                                                 && allocation.Solicitante_CodColigada == codColigada
                                                                                 && allocation.DataEmprestimo >= startDate
                                                                                 && allocation.DataEmprestimo <= endDate
                                                                          select new HistoryAlocadoReportModel
                                                                          {
                                                                              IdProdutoAlocado = allocation.Id,
                                                                              IdCatalogo = product.IdCatalogo,
                                                                              Code = catalog.Codigo,
                                                                              Description = catalog.Nome,
                                                                              IdProduto = product.Id,
                                                                              BorrowedDate = allocation.DataEmprestimo,
                                                                              BorrowedDateString = allocation.DataEmprestimo.HasValue ? allocation.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                              ReturnedDateString = string.Empty,
                                                                              Qty = allocation.Quantidade.ToString(),
                                                                              LocationOrigin = origin.Nome,
                                                                              ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                              IdBalconistaBorrow = allocation.Balconista_IdLogin,
                                                                              //IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                              //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                              IdTransaction = allocation.TransactionId ?? string.Empty,
                                                                              ItemClass = category.ClassType,
                                                                              CrachaNo = allocation.CrachaNo,
                                                                          }).AsNoTracking().ToListAsync();

                if (allocationResult == null || allocationResult.Count == 0) return new List<HistoryAlocadoReportModel>();


                var borrowUserIds = allocationResult.Select(x => x.IdBalconistaBorrow).Distinct().ToList();

                var funcionarioDict = await _context.VW_Usuario_New.Where(u => borrowUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                allocationResult = allocationResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdProdutoAlocado = x.IdProdutoAlocado,
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    //ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    LostItems = SearchProdutoExtraviadoQuantity(x.IdProdutoAlocado),
                                    BalconistaReturn = GetBalconista(x.IdProdutoAlocado),
                                    LostDateString = LostDate(x.IdProdutoAlocado),
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo
                                }).ToList();

                return allocationResult;

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

        public async Task<List<HistoryAlocadoReportModel>> GetTerceiroItemHistory(int IdTerceiro, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetTerceiroItemHistory IdTerceiro:{IdTerceiro}, Year:{year}", IdTerceiro, year);

                if (IdTerceiro <= 0) throw new ProcessErrorException("IdTerceiro is 0.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                string tableName = $"HistoricoAlocacao_{year}";

                var dbSetProperties = _context.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                var tableProperty = dbSetProperties.FirstOrDefault(p => p.Name == tableName);

                if (tableProperty == null) throw new ProcessErrorException($"Table:{tableName} not found on the database.");

                var table = tableProperty.GetValue(_context, null);

                List<HistoryAlocadoReportModel> historyResult = await (from history in (IQueryable<HistoricoAlocacao>)table
                                                                       join product in _context.Produto on history.IdProduto equals product.Id
                                                                       join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                       join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                       join origin in _context.Ferramentaria on history.IdFerrOndeProdRetirado equals origin.Id
                                                                       join destination in _context.Ferramentaria on history.IdFerrOndeProdDevolvido equals destination.Id into ferrDevGroup
                                                                       from destination in ferrDevGroup.DefaultIfEmpty()
                                                                       join controle in _context.ControleCA on history.IdControleCA equals controle.Id into controlGroup
                                                                       from controle in controlGroup.DefaultIfEmpty()
                                                                       where history.Solicitante_IdTerceiro == IdTerceiro
                                                                              && history.Solicitante_CodColigada == 0
                                                                              && history.DataDevolucao >= startDate
                                                                              && history.DataDevolucao <= endDate
                                                                       select new HistoryAlocadoReportModel
                                                                       {
                                                                           IdCatalogo = product.IdCatalogo,
                                                                           Code = catalog.Codigo,
                                                                           Description = catalog.Nome,
                                                                           IdProduto = product.Id,
                                                                           BorrowedDate = history.DataEmprestimo,
                                                                           BorrowedDateString = history.DataEmprestimo.HasValue ? history.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           ReturnedDateString = history.DataDevolucao.HasValue ? history.DataDevolucao.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                           Qty = history.Quantidade.ToString(),
                                                                           LocationOrigin = origin.Nome,
                                                                           LocationReturn = destination != null ? destination.Nome : string.Empty,
                                                                           ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                           IdBalconistaBorrow = history.Balconista_Emprestimo_IdLogin,
                                                                           IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                           //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                           IdTransaction = year >= 2025 ? history.TransactionId : string.Empty,
                                                                           IdTransactionEmprestimo = year >= 2025 ? history.EmprestimoTransactionId : string.Empty,
                                                                           ItemClass = category.ClassType,
                                                                           CrachaNo = year >= 2025 ? history.CrachaNo : string.Empty,
                                                                       }).AsNoTracking().ToListAsync();


                //if (historyResult == null || historyResult.Count == 0) throw new ProcessErrorException($"No Result Found for Employee:{chapa}.");
                if (historyResult == null || historyResult.Count == 0) return new List<HistoryAlocadoReportModel>();

                var userIds = historyResult
                               .Select(x => new
                               {
                                   x.IdBalconistaBorrow,
                                   x.IdBalconistaReturn
                               })
                               .Distinct()
                               .ToList();

                // Step 2: Extract and prepare user IDs for lookup
                var borrowUserIds = userIds.Select(x => x.IdBalconistaBorrow).Distinct().ToList();
                var returnUserIds = userIds.Select(x => x.IdBalconistaReturn).Where(id => id.HasValue).Distinct().ToList();
                var allUserIds = borrowUserIds.Union(returnUserIds).Distinct().ToList();

                // Step 3: Get user data from second context
                var funcionarioDict = await _context.VW_Usuario_New.Where(u => allUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                historyResult = historyResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdBalconistaReturn = x.IdBalconistaReturn,
                                    BalconistaReturn = x.IdBalconistaReturn.HasValue && funcionarioDict.ContainsKey(x.IdBalconistaReturn.Value) ? funcionarioDict[x.IdBalconistaReturn.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    IdTransactionEmprestimo = x.IdTransactionEmprestimo != null ? x.IdTransactionEmprestimo : string.Empty,
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo,
                                }).ToList();

                return historyResult;

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

        public async Task<List<HistoryAlocadoReportModel>> GetTerceiroItemAllocation(int IdTerceiro, int year)
        {
            try
            {
                _logger.LogInformation("Processing GetEmployeeItemHistory IdTerceiro:{IdTerceiro}, Year:{year}", IdTerceiro, year);

                if (IdTerceiro <= 0) throw new ProcessErrorException("IdTerceiro is 0.");
                if (year <= 0) throw new ProcessErrorException("year is 0.");

                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31);

                List<HistoryAlocadoReportModel> allocationResult = await (from allocation in _context.ProdutoAlocado
                                                                          join product in _context.Produto on allocation.IdProduto equals product.Id
                                                                          join catalog in _context.Catalogo on product.IdCatalogo equals catalog.Id
                                                                          join category in _context.Categoria on catalog.IdCategoria equals category.Id
                                                                          join origin in _context.Ferramentaria on allocation.IdFerrOndeProdRetirado equals origin.Id
                                                                          join controle in _context.ControleCA on allocation.IdControleCA equals controle.Id into controlGroup
                                                                          from controle in controlGroup.DefaultIfEmpty()
                                                                          where allocation.Solicitante_IdTerceiro == IdTerceiro
                                                                                 && allocation.Solicitante_CodColigada == 0
                                                                                 && allocation.DataEmprestimo >= startDate
                                                                                 && allocation.DataEmprestimo <= endDate
                                                                          select new HistoryAlocadoReportModel
                                                                          {
                                                                              IdProdutoAlocado = allocation.Id,
                                                                              IdCatalogo = product.IdCatalogo,
                                                                              Code = catalog.Codigo,
                                                                              Description = catalog.Nome,
                                                                              IdProduto = product.Id,
                                                                              BorrowedDate = allocation.DataEmprestimo,
                                                                              BorrowedDateString = allocation.DataEmprestimo.HasValue ? allocation.DataEmprestimo.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                                                                              ReturnedDateString = string.Empty,
                                                                              Qty = allocation.Quantidade.ToString(),
                                                                              LocationOrigin = origin.Nome,
                                                                              ControlCA = controle != null ? controle.NumeroCA : string.Empty,
                                                                              IdBalconistaBorrow = allocation.Balconista_IdLogin,
                                                                              //IdBalconistaReturn = history.Balconista_Devolucao_IdLogin,
                                                                              //IdTransaction = history.TransactionId != null ? history.TransactionId : string.Empty,
                                                                              IdTransaction = allocation.TransactionId ?? string.Empty,
                                                                              ItemClass = category.ClassType,
                                                                              CrachaNo = allocation.CrachaNo,
                                                                          }).AsNoTracking().ToListAsync();

                if (allocationResult == null || allocationResult.Count == 0) return new List<HistoryAlocadoReportModel>();


                var borrowUserIds = allocationResult.Select(x => x.IdBalconistaBorrow).Distinct().ToList();

                var funcionarioDict = await _context.VW_Usuario_New.Where(u => borrowUserIds.Contains(u.Id.Value)).ToDictionaryAsync(u => u.Id.Value, u => u.Nome);

                allocationResult = allocationResult
                                .Select(x => new HistoryAlocadoReportModel
                                {
                                    IdProdutoAlocado = x.IdProdutoAlocado,
                                    IdCatalogo = x.IdCatalogo,
                                    Code = x.Code,
                                    Description = x.Description,
                                    IdProduto = x.IdProduto,
                                    BorrowedDate = x.BorrowedDate,
                                    BorrowedDateString = x.BorrowedDateString,
                                    //ReturnedDateString = x.ReturnedDateString,
                                    Qty = x.Qty,
                                    LocationOrigin = x.LocationOrigin,
                                    LocationReturn = x.LocationReturn,
                                    ControlCA = x.ControlCA,
                                    IdBalconistaBorrow = x.IdBalconistaBorrow,
                                    BalconistaBorrow = funcionarioDict.ContainsKey(x.IdBalconistaBorrow.Value) ? funcionarioDict[x.IdBalconistaBorrow.Value] : string.Empty,
                                    IdTransaction = x.IdTransaction != null ? x.IdTransaction : string.Empty,
                                    LostItems = SearchProdutoExtraviadoQuantity(x.IdProdutoAlocado),
                                    BalconistaReturn = GetBalconista(x.IdProdutoAlocado),
                                    LostDateString = LostDate(x.IdProdutoAlocado),
                                    ItemClass = x.ItemClass,
                                    CrachaNo = x.CrachaNo
                                }).ToList();

                return allocationResult;

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


        private int? SearchProdutoExtraviadoQuantity(int? IdProdutoAlocado)
        {

            List<int?> result = (
                                    from produtoExtraviado in _context.ProdutoExtraviado
                                    join produtoAlocado in _context.ProdutoAlocado on produtoExtraviado.IdProdutoAlocado equals produtoAlocado.Id
                                    join produto in _context.Produto on produtoAlocado.IdProduto equals produto.Id
                                    where produtoExtraviado.IdProdutoAlocado == IdProdutoAlocado && produtoExtraviado.Ativo == 1
                                    select produtoExtraviado.Quantidade
                                  ).ToList();

            int? totalExtraviado = result.Sum();

            return totalExtraviado;
        }

        private string? GetBalconista(int? IdProdutoAlocado)
        {
            int? IdBalconista = _context.ProdutoExtraviado.Where(x => x.IdProdutoAlocado == IdProdutoAlocado && x.Ativo == 1).Select(i => i.IdUsuario).FirstOrDefault();

            if (IdBalconista == null) return string.Empty;

            return _context.VW_Usuario_New.Where(i => i.Id == IdBalconista).Select(x => x.Nome).FirstOrDefault();

        }

        private string? LostDate(int? IdProdutoAlocado)
        {

            return _context.ProdutoExtraviado.Where(x => x.IdProdutoAlocado == IdProdutoAlocado && x.Ativo == 1)
                                 .Select(i => i.DataRegistro.Value.ToString("dd/MM/yyyy HH:mm"))
                                 .FirstOrDefault();

        }



    }



}
