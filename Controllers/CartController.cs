using Microsoft.AspNetCore.Mvc;
using NikeShop.Data;
using NikeShop.Models;
using NikeShop.Helpers;

namespace NikeShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Key để lưu giỏ hàng trong Session
        private const string CART_KEY = "MyCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ lấy giỏ hàng từ Session
        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetJson<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        }

        // POST: Thêm sản phẩm vào giỏ
        // POST: Thêm sản phẩm vào giỏ
        [HttpPost]
        public IActionResult AddToCart(int productId, string size, int quantity, string color)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = GetCartItems();

            // Kiểm tra sản phẩm cùng Size VÀ cùng Màu đã có trong giỏ chưa
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size && c.Color == color);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Image = product.Image,
                    Price = product.Price,
                    Size = size,
                    Color = color, // Gán màu sắc vào đây
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetJson(CART_KEY, cart);
            return RedirectToAction("Index");
        }
        // GET: Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        // GET: Xóa 1 món khỏi giỏ
        public IActionResult Remove(int id, string size)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == id && c.Size == size);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetJson(CART_KEY, cart);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCartItems(); // Hàm bạn đã có sẵn để lấy list giỏ hàng
            int count = cart.Sum(c => c.Quantity);
            return Json(new { count = count });
        }
    }
}