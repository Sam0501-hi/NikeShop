using Microsoft.EntityFrameworkCore;
using NikeShop.Models;

namespace NikeShop.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Nếu đã có danh mục rồi thì không tạo thêm nữa
                if (context.Categories.Any())
                {
                    return;
                }

                var catRunning = new Category { CategoryName = "Giày chạy bộ (Running)" };
                var catLifestyle = new Category { CategoryName = "Thời trang (Lifestyle)" };

                context.Categories.AddRange(catRunning, catLifestyle);
                context.SaveChanges();

                var shoe1 = new Product
                {
                    ProductName = "Nike Air Force 1 '07",
                    Price = 2900000,
                    Description = "Sự rực rỡ tiếp tục sống trong Nike Air Force 1 '07, đôi giày bóng rổ nguyên bản mang phong cách hoàn toàn mới.",
                    Image = "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b7d9211c-26e7-431a-ac24-b0540fb3c00f/air-force-1-07-shoes-WrLlWX.png",
                    Color = "Trắng",
                    CategoryId = catLifestyle.CategoryId
                };

                var shoe2 = new Product
                {
                    ProductName = "Nike Pegasus 40",
                    Price = 3500000,
                    Description = "Một đôi giày chạy bộ có độ nảy hoàn hảo cho mọi cung đường.",
                    Image = "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e13f41fc-a621-4d3f-9173-0f73bcf2fb69/pegasus-40-road-running-shoes-HTmJdT.png",
                    Color = "Đen",
                    CategoryId = catRunning.CategoryId
                };

                context.Products.AddRange(shoe1, shoe2);
                context.SaveChanges();

                var variant1 = new ProductVariant { ProductId = shoe1.ProductId, Size = "40", StockQuantity = 10 };
                var variant2 = new ProductVariant { ProductId = shoe1.ProductId, Size = "41", StockQuantity = 15 };
                var variant3 = new ProductVariant { ProductId = shoe2.ProductId, Size = "42", StockQuantity = 5 };

                context.ProductVariants.AddRange(variant1, variant2, variant3);
                context.SaveChanges();
            }
        }
    }
}