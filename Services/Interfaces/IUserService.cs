using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IUserService
    {
        Task<Usuario> ValidateUser(VW_Usuario_NewViewModel user);
    }
}
