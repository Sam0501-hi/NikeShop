using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NikeShop.Data;
using NikeShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore; // Rất quan trọng cho .Include()
using NikeShop.Models;
using System.Linq; // Cần thiết để hỗ trợ LINQ
namespace NikeShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            // Lấy danh sách đơn hàng kèm email người mua
            var orders = await _context.Orders
     .OrderByDescending(o => o.OrderDate)
     .Select(o => new AdminOrderViewModel
     {
         OrderId = o.OrderId,
         CustomerEmail = o.User != null ? o.User.Email : "N/A", // Truy cập trực tiếp qua o.User
         OrderDate = o.OrderDate,
         TotalPrice = o.TotalPrice,
         Status = o.Status
     })
     .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
