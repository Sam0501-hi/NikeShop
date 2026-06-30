using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikeShop.Data;
using NikeShop.Models;
namespace NikeShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sản phẩm kèm theo danh mục
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }
    }
}