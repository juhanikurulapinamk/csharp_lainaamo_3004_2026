
using API.Interfaces;

namespace API.Services
{
    // Henkilökunnalta ei peritä maksuja
    public class StaffFineCalculator : IFineCalculator
    {
        public int CalculateCents(DateTime dueAt, DateTime returnedAt) => 0;
    }
}