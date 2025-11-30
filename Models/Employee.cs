using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
        [Required] public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; } = default!;
        [StringLength(50)] public string Role { get; set; } = "Pharmacist"; // Pharmacist/Cashier/Manager
    }
}
