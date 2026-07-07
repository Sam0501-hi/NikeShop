using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikeShop.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = string.Empty;

        // Màu mà ảnh này thuộc về (VD: "Đen", "Trắng"). Null/rỗng = ảnh chung, không theo màu cụ thể.
        [StringLength(50)]
        public string? Color { get; set; }

        public virtual Product? Product { get; set; }
    }
}