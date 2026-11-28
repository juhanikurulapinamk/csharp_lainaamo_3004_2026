
using API.Models;

namespace API.Interfaces
{
    public interface ILoanService
    {
        Task<Loan> Borrow(int itemCopyId, int userId);
        Task<IEnumerable<Loan>> GetMyLoans(int userId);

        
        // ILoanService.cs, tämä on uusi
        Task<Loan> Return(int loanId, int userId, string role);
    }
}