namespace PharmacyChain.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public List<TopDrugViewModel> TopDrugs { get; set; } = new();
        public int LowStockCount { get; set; }
    }
}
