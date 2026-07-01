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
                // Kiểm tra nếu đã có dữ liệu sản phẩm rồi thì không thêm nữa
                if (context.Products.Any())
                {
                    return;
                }

                // Thêm danh mục (Categories)
                var shoes = new Category { CategoryName = "Giày Thể Thao" };
                var accessories = new Category { CategoryName = "Phụ Kiện" };
                context.Categories.AddRange(shoes, accessories);

                // Thêm sản phẩm mẫu
                // Sửa đoạn này trong SeedData.cs
                context.Products.AddRange(
                    new Product
                    {
                        ProductName = "Nike Air Force 1", // Đổi Name thành ProductName
                        Description = "Giày huyền thoại của Nike",
                        Price = 2500000,
                        Category = shoes
                    },
                    new Product
                    {
                        ProductName = "Nike Air Max 90", // Đổi Name thành ProductName
                        Description = "Êm ái và thời trang",
                        Price = 3200000,
                        Category = shoes
                    }
                );
                context.SaveChanges();
            }
        }
    }
}