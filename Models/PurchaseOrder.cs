using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required] public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = default!;
        [Required] public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = default!;
        [StringLength(50)] public string Status { get; set; } = "Draft"; // Draft/Sent/Received/Cancelled

        public List<PurchaseOrderLine> Lines { get; set; } = new();
    }

    public class PurchaseOrderLine
    {
        public int Id { get; set; }
        [Required] public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; } = default!;
        [Required] public int DrugId { get; set; }
        public Drug Drug { get; set; } = default!;
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        [Range(0, double.MaxValue)] public decimal UnitCost { get; set; }
    }
}
