using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PharmacyChain.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
