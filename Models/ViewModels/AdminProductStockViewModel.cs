namespace NikeShop.Models.ViewModels
{
    public class AdminProductStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<VariantStockInfo> Variants { get; set; } = new();
    }

    public class VariantStockInfo
    {
        public int VariantId { get; set; }
        public string Size { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}