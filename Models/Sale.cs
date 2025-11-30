using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [Required] public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = default!;

        public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
        public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
    }

    public class SaleLine
    {
        public int Id { get; set; }
        [Required] public int SaleId { get; set; }
        public Sale Sale { get; set; } = default!;
        [Required] public int DrugId { get; set; }
        public Drug Drug { get; set; } = default!;
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        [Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }
    }
}
