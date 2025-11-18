using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using FerramentariaTest.Models;

namespace FerramentariaTest.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<fnRetornaColaboradorCracha?> GetEmployeeCardInfo(string Icard);
        Task<EmployeeInformationBS> GetEmployeeInformationBS(string matricula);
        Task<List<EmployeeInformationBS>?> SearchEmployees(string givenInformation);
        Task<List<EmployeeInformationBS>?> SearchThirdParty(string givenInformation);
        Task<string?> GetEmployeeImage(int codpessoa);
        Task<EmployeeInformationBS> GetSelectedEmployee(int codpessoa);
        Task<EmployeeInformationBS> GetSelectedThirdParty(int idTerceiro);
        Task<TermsControlModel?> CheckTermsControl(string chapa);
        Task<Result> AddToTermsControl(TermsControl termsInformation);

        Task<TermsControlResultModel> GetTermsConrolResult(int CodPessoa);

        Task UploadTermsPDF(int idTerms, byte[] imgByte);

    }
}
