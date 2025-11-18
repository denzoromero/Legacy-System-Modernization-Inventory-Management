using FerramentariaTest.Entities;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<FuncionarioModel>> getLeaderList(string? givenInfo,int currentPage);
        Task<FuncionarioModel> GetEmployeeForLeader(string chapa);
        Task InsertNewLeader(LeaderData leader);
        Task DeactivateLeader(int id);
        Task ReactivateLeader(int id);
    }
}
