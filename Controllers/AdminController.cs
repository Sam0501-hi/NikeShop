using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NikeShop.Data;
using NikeShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using NikeShop.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

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

        #region Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var viewModel = new AdminDashboardViewModel
            {
                TotalOrdersToday = await _context.Orders.CountAsync(o => o.OrderDate.Date == today),
                TotalRevenueThisMonth = await _context.Orders
                    .Where(o => o.OrderDate >= startOfMonth && o.Status == "Confirmed")
                    .SumAsync(o => o.TotalPrice),
                LowStockProductsCount = await _context.ProductVariants.CountAsync(v => v.StockQuantity < 5)
            };

            return View(viewModel);
        }
        #endregion

        #region Order Management
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderViewModel
                {
                    OrderId = o.OrderId,
                    CustomerEmail = o.User != null ? o.User.Email : "N/A",
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

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng này.";
                return RedirectToAction(nameof(Orders));
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật đơn #{orderId} sang: {status}";
            }
            return RedirectToAction(nameof(OrderDetails), new { orderId = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order != null)
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đơn hàng thành công.";
            }
            return RedirectToAction(nameof(Orders));
        }
        #endregion

        #region Inventory Management
        [HttpGet]
        public async Task<IActionResult> StockManagement()
        {
            var products = await _context.Products
                .Include(p => p.Variants)
                .Select(p => new AdminProductStockViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Variants = p.Variants.Select(v => new VariantStockInfo
                    {
                        VariantId = v.ProductVariantId,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity
                    }).ToList()
                }).ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int variantId, int newQuantity)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant != null)
            {
                variant.StockQuantity = newQuantity;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật tồn kho!";
            }
            return RedirectToAction(nameof(StockManagement));
        }
        #endregion

        #region Category Management
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "Tên danh mục không được để trống";
                return RedirectToAction(nameof(Categories));
            }

            var exists = await _context.Categories.AnyAsync(c => c.CategoryName == categoryName.Trim());
            if (exists)
            {
                TempData["Error"] = "Danh mục này đã tồn tại!";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Add(new Category { CategoryName = categoryName.Trim() });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category != null && !category.Products.Any())
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa danh mục thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể xóa: Danh mục không tồn tại hoặc đang chứa sản phẩm!";
            }
            return RedirectToAction(nameof(Categories));
        }
        #endregion

        #region Product Management
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // Chỉ giữ DUY NHẤT 1 action POST CreateProduct (bản có xử lý ảnh)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.Image = "/images/products/" + fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm mới thành công!";
                return RedirectToAction(nameof(Products));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sản phẩm!";
            }
            return RedirectToAction(nameof(Products));
        }
        #endregion

        #region Edit Product
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.Image = "/images/products/" + fileName;
                }
                else
                {
                    var existing = await _context.Products.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == id);
                    product.Image = existing?.Image;
                }

                _context.Update(product);

                if (product.Variants != null)
                {
                    foreach (var v in product.Variants)
                    {
                        v.ProductId = product.ProductId;
                        if (v.ProductVariantId > 0) _context.ProductVariants.Update(v);
                        else _context.ProductVariants.Add(v);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Products));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
        #endregion
    }
}