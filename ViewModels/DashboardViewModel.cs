namespace PharmacyChain.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPharmacies { get; set; }
        public int TotalDrugs { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public decimal SalesToday { get; set; }
        public int LowStockCount { get; set; }
        public List<TopDrugViewModel> TopDrugs { get; set; } = new();
        public List<RecentSaleViewModel> RecentSales { get; set; } = new();
    }

    public class TopDrugViewModel
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
    }

    public class RecentSaleViewModel
    {
        public int Id { get; set; }
        public string PharmacyName { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
    }
}