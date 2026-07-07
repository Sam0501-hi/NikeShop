using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikeShop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public string? Image { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string Brand { get; set; } = "Nike";

        [StringLength(20)]
        public string Gender { get; set; } = "Unisex";

        public bool IsFeatured { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        //========================
        // Foreign Key
        //========================

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        //========================
        // Navigation
        //========================

        public ICollection<ProductVariant> Variants { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }

        // New: support multiple images per product
        public ICollection<ProductImage> Images { get; set; }

        public Product()
        {
            Variants = new List<ProductVariant>();
            OrderDetails = new List<OrderDetail>();
            Images = new List<ProductImage>();
        }
    }
}