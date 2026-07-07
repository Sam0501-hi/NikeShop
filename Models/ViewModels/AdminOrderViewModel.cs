using System;

namespace NikeShop.Models.ViewModels
{
    public class AdminOrderViewModel
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
