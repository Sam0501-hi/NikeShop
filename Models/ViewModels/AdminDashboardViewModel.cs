namespace NikeShop.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalOrdersToday { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public int LowStockProductsCount { get; set; }
    }
}