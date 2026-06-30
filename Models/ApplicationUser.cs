using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NikeShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? Avatar { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}