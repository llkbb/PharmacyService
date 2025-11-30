using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Drug
    {
        public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(100)] public string? Category { get; set; } // OTC, Rx, тощо
        [StringLength(200)] public string? Manufacturer { get; set; }
        [Required] public bool PrescriptionRequired { get; set; }

        public int ReorderLevel { get; set; } = 10; // мінімальний запас
    }
}
