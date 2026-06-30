using System.ComponentModel.DataAnnotations;

namespace NikeShop.Models
{
    public class ProductVariant
    {
        [Key]
        public int ProductVariantId { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        public bool IsAvailable => StockQuantity > 0;
    }
}