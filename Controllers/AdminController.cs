using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikeShop.Data;
using NikeShop.Models;
using NikeShop.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NikeShop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Products()
        {
            var products = _context.Products.Include(p => p.Category).Include(p => p.Images).ToList();
            return View(products);
        }

        public IActionResult CreateProduct()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, List<IFormFile> images, List<ProductImageGroupInput> imageGroups, string[] colors, string[] sizes, string colorsCsv, string sizesCsv)
        {
            ViewData["Categories"] = _context.Categories.ToList();

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            // Validate CategoryId exists and is selected
            if (product.CategoryId <= 0 || !_context.Categories.Any(c => c.CategoryId == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category.");
                return View(product);
            }

            try
            {
                // First save product to get ProductId
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var uploadDir = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // Handle uploaded images without a specific color (ảnh chung)
                if (images != null && images.Any())
                {
                    foreach (var file in images)
                    {
                        if (file.Length <= 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        var fileName = Guid.NewGuid().ToString() + ext;
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var imgPath = "/images/products/" + fileName;
                        _context.ProductImages.Add(new ProductImage { ProductId = product.ProductId, ImagePath = imgPath });
                    }
                }

                // Handle images grouped by color (ảnh riêng cho từng màu)
                if (imageGroups != null && imageGroups.Any())
                {
                    foreach (var group in imageGroups)
                    {
                        if (group.Files == null || !group.Files.Any() || string.IsNullOrWhiteSpace(group.Color)) continue;

                        foreach (var file in group.Files)
                        {
                            if (file.Length <= 0) continue;
                            var ext = Path.GetExtension(file.FileName);
                            var fileName = Guid.NewGuid().ToString() + ext;
                            var filePath = Path.Combine(uploadDir, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            var imgPath = "/images/products/" + fileName;
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImagePath = imgPath,
                                Color = group.Color.Trim()
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Normalize colors/sizes from CSV if provided
                if ((colors == null || !colors.Any()) && !string.IsNullOrWhiteSpace(colorsCsv))
                {
                    colors = colorsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
                if ((sizes == null || !sizes.Any()) && !string.IsNullOrWhiteSpace(sizesCsv))
                {
                    sizes = sizesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                // Handle colors and sizes -> create variants
                if (colors != null && colors.Any() && sizes != null && sizes.Any())
                {
                    foreach (var color in colors.Where(c => !string.IsNullOrWhiteSpace(c)))
                    {
                        foreach (var size in sizes.Where(s => !string.IsNullOrWhiteSpace(s)))
                        {
                            _context.ProductVariants.Add(new ProductVariant
                            {
                                ProductId = product.ProductId,
                                Color = color.Trim(),
                                Size = size.Trim(),
                                StockQuantity = 0
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                // Log exception (could add ILogger) and show friendly message
                ModelState.AddModelError(string.Empty, "Error saving product to database: " + ex.GetBaseException().Message);
                return View(product);
            }
        }

        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Include(p => p.Images).Include(p => p.Variants).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();
            ViewData["Categories"] = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product, List<IFormFile> images, List<ProductImageGroupInput> imageGroups, string[] colors, string[] sizes, string colorsCsv, string sizesCsv)
        {
            ViewData["Categories"] = _context.Categories.ToList();
            if (!ModelState.IsValid) return View(product);

            // Validate CategoryId
            if (product.CategoryId <= 0 || !_context.Categories.Any(c => c.CategoryId == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Please select a valid category.");
                ViewData["Categories"] = _context.Categories.ToList();
                return View(product);
            }

            var existing = _context.Products.Include(p => p.Images).Include(p => p.Variants).FirstOrDefault(p => p.ProductId == product.ProductId);
            if (existing == null) return NotFound();

            // Update fields
            existing.ProductName = product.ProductName;
            existing.Price = product.Price;
            existing.Description = product.Description;
            existing.Color = product.Color;
            existing.Image = product.Image;
            existing.CategoryId = product.CategoryId;

            try
            {
                _context.Products.Update(existing);
                await _context.SaveChangesAsync();

                // Add new images if provided (ảnh chung, không theo màu)
                var uploadDir = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                if (images != null && images.Any())
                {
                    foreach (var file in images)
                    {
                        if (file.Length <= 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        var fileName = Guid.NewGuid().ToString() + ext;
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var imgPath = "/images/products/" + fileName;
                        _context.ProductImages.Add(new ProductImage { ProductId = existing.ProductId, ImagePath = imgPath });
                    }
                    await _context.SaveChangesAsync();
                }

                // Add new images grouped by color
                if (imageGroups != null && imageGroups.Any())
                {
                    foreach (var group in imageGroups)
                    {
                        if (group.Files == null || !group.Files.Any() || string.IsNullOrWhiteSpace(group.Color)) continue;

                        foreach (var file in group.Files)
                        {
                            if (file.Length <= 0) continue;
                            var ext = Path.GetExtension(file.FileName);
                            var fileName = Guid.NewGuid().ToString() + ext;
                            var filePath = Path.Combine(uploadDir, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            var imgPath = "/images/products/" + fileName;
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = existing.ProductId,
                                ImagePath = imgPath,
                                Color = group.Color.Trim()
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Normalize colors/sizes from CSV if provided
                if ((colors == null || !colors.Any()) && !string.IsNullOrWhiteSpace(colorsCsv))
                {
                    colors = colorsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
                if ((sizes == null || !sizes.Any()) && !string.IsNullOrWhiteSpace(sizesCsv))
                {
                    sizes = sizesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                // If colors/sizes provided, optionally update variants (simple approach: add missing combos)
                if (colors != null && colors.Any() && sizes != null && sizes.Any())
                {
                    foreach (var color in colors.Where(c => !string.IsNullOrWhiteSpace(c)))
                    {
                        foreach (var size in sizes.Where(s => !string.IsNullOrWhiteSpace(s)))
                        {
                            var trimmedSize = size.Trim();
                            var exists = _context.ProductVariants.Any(v => v.ProductId == existing.ProductId && v.Color == color.Trim() && v.Size == trimmedSize);
                            if (!exists)
                            {
                                _context.ProductVariants.Add(new ProductVariant
                                {
                                    ProductId = existing.ProductId,
                                    Color = color.Trim(),
                                    Size = trimmedSize,
                                    StockQuantity = 0
                                });
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Error updating product: " + ex.GetBaseException().Message);
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductImage(int imageId, int productId)
        {
            var img = _context.ProductImages.FirstOrDefault(i => i.ProductImageId == imageId);
            if (img != null)
            {
                var physical = Path.Combine(_env.WebRootPath, img.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
                _context.ProductImages.Remove(img);
                _context.SaveChanges();
            }

            return RedirectToAction("EditProduct", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _context.Products.Include(p => p.Images).Include(p => p.Variants).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            try
            {
                // Remove related variants
                if (product.Variants != null && product.Variants.Any())
                {
                    _context.ProductVariants.RemoveRange(product.Variants);
                }

                // Remove related images and files
                if (product.Images != null && product.Images.Any())
                {
                    foreach (var img in product.Images)
                    {
                        var physical = Path.Combine(_env.WebRootPath, img.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(physical))
                        {
                            System.IO.File.Delete(physical);
                        }
                    }
                    _context.ProductImages.RemoveRange(product.Images);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Error deleting product: " + ex.GetBaseException().Message);
                return RedirectToAction("Products");
            }
        }

        public IActionResult Dashboard()
        {
            var dashboard = new AdminDashboardViewModel
            {
                TotalOrdersToday = _context.Orders.Count(o => o.OrderDate.Date == DateTime.Today),
                TotalRevenueThisMonth = _context.Orders.Where(o => o.OrderDate.Month == DateTime.Now.Month).Sum(o => o.TotalPrice),
                LowStockProductsCount = _context.ProductVariants.Count(v => v.StockQuantity < 5),
                RecentOrders = _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new AdminOrderViewModel
                    {
                        OrderId = o.OrderId,
                        CustomerName = o.User != null ? o.User.FullName : string.Empty,
                        CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                        OrderDate = o.OrderDate,
                        TotalPrice = o.TotalPrice
                    }).ToList()
            };

            return View(dashboard);
        }

        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderViewModel
                {
                    OrderId = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName : string.Empty,
                    CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                })
                .ToList();

            return View(orders);
        }

        public IActionResult StockManagement()
        {
            var stockList = _context.Products
                .Include(p => p.Variants)
                .Select(p => new AdminProductStockViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Variants = p.Variants.Select(v => new VariantStockInfo
                    {
                        VariantId = v.ProductVariantId,
                        Color = v.Color,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity
                    }).ToList()
                })
                .ToList();

            return View(stockList);
        }

        [HttpPost]
        public IActionResult UpdateStock(int variantId, int newQuantity)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.ProductVariantId == variantId);
            if (variant != null)
            {
                variant.StockQuantity = newQuantity;
                _context.SaveChanges();
            }

            return RedirectToAction("StockManagement");
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Categories.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Thêm danh mục thành công.";
            return RedirectToAction("Categories");
        }

        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(int id, Category model)
        {
            if (id != model.CategoryId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Categories.Update(model);
            _context.SaveChanges();
            TempData["Success"] = "Cập nhật danh mục thành công.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa danh mục.";
            }

            return RedirectToAction("Categories");
        }
    }
}