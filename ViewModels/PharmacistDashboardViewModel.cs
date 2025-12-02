namespace PharmacyChain.ViewModels
{
    public class PharmacistDashboardViewModel
    {
        public int TodaySalesCount { get; set; }
        public decimal TodaySalesTotal { get; set; }
        public int LowStockCount { get; set; }
        public int PharmaciesCount { get; set; }
        public int PendingPrescriptionsCount { get; set; }
    }
}
