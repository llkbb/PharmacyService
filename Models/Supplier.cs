using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(300)] public string? Address { get; set; }
        [Phone] public string? Phone { get; set; }
        [EmailAddress] public string? Email { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
