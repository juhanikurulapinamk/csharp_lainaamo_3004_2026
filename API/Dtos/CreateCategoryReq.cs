
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    // Tämä tulee sisään rajapintaan
    public class CreateCategoryReq
    {
        [Required(ErrorMessage = "Nimi on pakollinen")]
        [MinLength(2, ErrorMessage = "Nimen pitää olla vähintään 2 merkkiä")]
        [MaxLength(50, ErrorMessage = "Nimi saa olla enintään 50 merkkiä")]
        public required string Name { get; set; }
    }
}