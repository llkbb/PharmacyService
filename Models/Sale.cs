using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = default!;

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();

        // Додано для оплати
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Cancelled
        
        public DateTime? PaidAt { get; set; }
        
        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, Online
        
        [StringLength(100)]
        public string? TransactionId { get; set; }

        public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
    }

    public class SaleLine
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }
        public Sale Sale { get; set; } = default!;

        [Required]
        public int DrugId { get; set; }
        public Drug Drug { get; set; } = default!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
