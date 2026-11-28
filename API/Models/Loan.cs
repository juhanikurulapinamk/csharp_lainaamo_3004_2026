
namespace API.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int ItemCopyId { get; set; }
        public ItemCopy? ItemCopy { get; set; }

        public int AppUserId { get; set; }
        public AppUser? Borrower { get; set; }

        public DateTime BorrowedAt { get; set; }
        public DateTime DueAt { get; set; }

        // null tarkoittaa, että laina on yhä voimassa
        public DateTime? ReturnedAt { get; set; }
    }
}