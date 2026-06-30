namespace NikeShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; // Thêm dòng này
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}