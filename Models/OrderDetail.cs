
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikeShop.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        //=====================
        // Order
        //=====================

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        //=====================
        // Product
        //=====================

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        //=====================
        // Detail
        //=====================

        [Required]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price;
    }
}