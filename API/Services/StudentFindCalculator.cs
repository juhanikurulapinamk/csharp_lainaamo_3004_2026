
using API.Interfaces;

namespace API.Services
{
    public class StudentFineCalculator : IFineCalculator
    {
        private const int GraceDays = 3;
        private const int CentsPerDay = 50;
        private const int MaxCents = 2000;

        public int CalculateCents(DateTime dueAt, DateTime returnedAt)
        {
            // Palautettu ajoissa
            if (returnedAt <= dueAt)
            {
                return 0;
            }

            // Math.Ceiling: alkava vuorokausi lasketaan täytenä
            var daysLate = (int)Math.Ceiling((returnedAt - dueAt).TotalDays);

            // Armonaika
            if (daysLate <= GraceDays)
            {
                return 0;
            }

            var billableDays = daysLate - GraceDays;
            var fine = billableDays * CentsPerDay;

            // Yläraja
            return Math.Min(fine, MaxCents);
        }
    }
}