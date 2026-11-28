
namespace API.Interfaces
{
    public interface IFineCalculator
    {
        int CalculateCents(DateTime dueAt, DateTime returnedAt);
    }
}