using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.ViewModels
{
    public class SaleCreateVm
    {
        [Required] public int PharmacyId { get; set; }
        public int? CustomerId { get; set; }
        public List<SaleLineVm> Lines { get; set; } = new() { new SaleLineVm { Quantity = 1 } };
    }

    public class SaleLineVm
    {
        [Required] public int DrugId { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
    }
}
