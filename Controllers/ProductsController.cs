using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikeShop.Data;

namespace NikeShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị danh sách sản phẩm (có hỗ trợ lọc theo danh mục)
        public async Task<IActionResult> Index(int? categoryId)
        {
            // Lấy danh sách danh mục để truyền ra View (hiển thị menu lọc)
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return View(await products.ToListAsync());
        }

        // GET: Xem chi tiết một đôi giày
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants) // Kéo theo size và số lượng tồn kho
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}