
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    [Index(nameof(TagText), IsUnique = true)]
    public class Tag
    {
        public int Id { get; set; }
        public required string TagText { get; set; }

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}