using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.ViewModels
{
    public class SaleCreateSimpleVm
    {
        [Required] public int PharmacyId { get; set; }
        [Required] public int DrugId { get; set; }
        [Range(1, int.MaxValue)] public int Qty { get; set; } = 1;
        [Range(0.01, double.MaxValue)] public decimal Price { get; set; }
    }
}
