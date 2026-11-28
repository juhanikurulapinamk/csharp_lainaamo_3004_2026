
using API.Interfaces;

namespace API.Services
{
    public interface IFineCalculatorFactory
    {
        IFineCalculator ForRole(string role);
    }

    public class FineCalculatorFactory : IFineCalculatorFactory
    {
        public IFineCalculator ForRole(string role)
        {
            return role == "staff"
                ? new StaffFineCalculator()
                : new StudentFineCalculator();
        }
    }
}