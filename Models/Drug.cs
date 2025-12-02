using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Drug
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [StringLength(100)]
        public string Category { get; set; } = "";

        [StringLength(200)]
        public string Manufacturer { get; set; } = "";

        public bool PrescriptionRequired { get; set; }
    }
}
