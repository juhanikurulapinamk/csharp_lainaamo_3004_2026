
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class BorrowReq
    {
        // [Required] ei toimi int-tyypille, koska sillä on aina arvo (oletuksena 0).
        // Range varmistaa, että id on annettu.
        [Range(1, int.MaxValue)]
        public int ItemCopyId { get; set; }
    }
}