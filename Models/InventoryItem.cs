using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required]
        public int DrugId { get; set; }
        public Drug Drug { get; set; } = default!;

        [Required]
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = default!;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
