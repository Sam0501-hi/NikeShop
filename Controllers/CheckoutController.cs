using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NikeShop.Data;
using NikeShop.Helpers;
using NikeShop.Models;

namespace NikeShop.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào đây
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CART_KEY = "MyCart";

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Hiển thị form điền địa chỉ giao hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(cart);
        }

        // POST: Xử lý lưu đơn hàng vào Database
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string shippingAddress, string phoneNumber)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>(CART_KEY);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            // Lấy thông tin user đang đăng nhập
            var user = await _userManager.GetUserAsync(User);

            // 1. Lưu thông tin chung vào bảng Orders
            var order = new Order
            {
                UserId = user!.Id, // Dấu ! để báo C# biết user không null
                OrderDate = DateTime.Now,
                ShippingAddress = shippingAddress,
                PhoneNumber = phoneNumber,
                TotalPrice = cart.Sum(c => c.Total),
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Phải lưu trước để SQL cấp cho OrderId

            // 2. Lưu từng món giày vào bảng OrderDetails
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // 3. Xóa giỏ hàng trong Session sau khi mua thành công
            HttpContext.Session.Remove(CART_KEY);

            return RedirectToAction("Success");
        }

        // GET: Trang báo thành công
        public IActionResult Success()
        {
            return View();
        }
    }
}