
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    // Yhdestä lainasta voi syntyä enintään yksi maksu
    [Index(nameof(LoanId), IsUnique = true)]
    public class Fine
    {
        public int Id { get; set; }

        // Maksun maksaja eli lainaaja löytyy lainan kautta: Loan.AppUserId
        public int LoanId { get; set; }
        public Loan? Loan { get; set; }

        // Summa sentteinä
        public int AmountCents { get; set; }

        public DateTime CreatedAt { get; set; }

        // null tarkoittaa, että maksu on maksamatta
        public DateTime? PaidAt { get; set; }
    }
}