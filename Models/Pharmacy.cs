using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(300)] public string? Address { get; set; }
        [Phone] public string? Phone { get; set; }

        public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
