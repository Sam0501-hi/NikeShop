using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikeShop.Data; // Thay đổi theo namespace chứa ApplicationDbContext của bạn
using NikeShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NikeShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products?categoryId=1
        public async Task<IActionResult> Index(int? categoryId)
        {
            // 1. Lấy danh sách tất cả danh mục để hiển thị bên Sidebar (Cột trái)
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // 2. Lưu lại ID danh mục hiện tại để highlight bên View
            ViewBag.CurrentCategoryId = categoryId;

            // 3. Khởi tạo câu truy vấn lấy sản phẩm
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // 4. Nếu người dùng có bấm vào 1 danh mục cụ thể (categoryId có giá trị)
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // 5. Trả danh sách sản phẩm đã lọc về View
            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra nếu không có id truyền vào
            if (id == null)
            {
                return NotFound();
            }

            // Truy vấn lấy sản phẩm theo ID, kết hợp (Include) thêm thông tin của Category để hiển thị tên danh mục
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            // Kiểm tra nếu không tìm thấy sản phẩm trong Database
            if (product == null)
            {
                return NotFound();
            }

            // Trả dữ liệu sản phẩm về cho View hiển thị
            return View(product);
        }
    }
}