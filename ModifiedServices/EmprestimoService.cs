using FerramentariaTest.Models;

namespace FerramentariaTest.ModifiedServices
{
    public interface IEmprestimoService
    {
        List<EmprestimoViewModel> GetEmprestimoList();
        void AddEmprestimo(EmprestimoViewModel item);
        bool Exists(int? idProduto);
        //void ClearEmprestimos();
        //void ResetState();
    }

    public class EmprestimoService : IEmprestimoService
    {
        private readonly List<EmprestimoViewModel> _emprestimoList = new List<EmprestimoViewModel>();
        private readonly object _lock = new object(); // Synchronization object

        public List<EmprestimoViewModel> GetEmprestimoList()
        {
            lock (_lock)
            {
                return _emprestimoList.ToList(); // Return a copy to prevent external modifications
            }
        }

        public void AddEmprestimo(EmprestimoViewModel item)
        {
            lock (_lock)
            {
                _emprestimoList.Add(item);
            }
        }

        public bool Exists(int? idProduto)
        {
            lock (_lock)
            {
                return _emprestimoList.Any(item => item.IdProduto == idProduto);
            }
        }
    }

}
