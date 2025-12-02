using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        [Required]
        public int? DrugId { get; set; }
        public Drug? Drug { get; set; }

        public int? PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }

        [Required]
        public string DoctorName { get; set; } = "";

        public DateTime DateIssued { get; set; }

        [Range(1, 365)]
        public int ValidDays { get; set; } = 30;

        // Нові поля для системи завантаження рецептів клієнтами
        [StringLength(500)]
        public string? PhotoPath { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RejectionReason { get; set; }
    }
}
