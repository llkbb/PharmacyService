using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.ViewModels
{
    public class CustomerPurchaseViewModel
    {
        [Required]
        public int DrugId { get; set; }

        public string DrugName { get; set; } = "";

        [Required]
        public int PharmacyId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Кількість має бути від 1 до 100")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Виберіть спосіб оплати")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Online
    }
}
