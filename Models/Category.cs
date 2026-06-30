using System.ComponentModel.DataAnnotations;

namespace NikeShop.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Image { get; set; }

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}