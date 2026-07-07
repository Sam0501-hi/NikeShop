using System.Collections.Generic;

namespace NikeShop.Models.ViewModels
{
    public class AdminProductStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public IEnumerable<VariantStockInfo> Variants { get; set; } = new List<VariantStockInfo>();
    }

    public class VariantStockInfo
    {
        public int VariantId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}