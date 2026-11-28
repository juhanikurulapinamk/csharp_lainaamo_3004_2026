
namespace API.Dtos
{
    public class LoanDto
    {
        public int Id { get; set; }
        public int ItemCopyId { get; set; }
        public int AppUserId { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}