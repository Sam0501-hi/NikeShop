using System.ComponentModel.DataAnnotations;

namespace NikeShop.Models
{
    public class ProductVariant
    {
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public virtual Product Product { get; set; }
    }
}