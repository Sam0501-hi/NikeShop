using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace NikeShop.Models.ViewModels
{
    // Một nhóm ảnh gắn với 1 màu cụ thể, dùng khi Admin thêm/sửa sản phẩm
    public class ProductImageGroupInput
    {
        public string Color { get; set; } = string.Empty;
        public List<IFormFile>? Files { get; set; }
    }
}