using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Address { get; set; }

        public string Phone { get; set; } = "";

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
    }
}
