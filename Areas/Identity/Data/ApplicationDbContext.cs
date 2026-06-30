using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NikeShop.Models;

namespace NikeShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        //===========================
        // DbSet
        //===========================

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //-------------------------------------
            // Decimal
            //-------------------------------------

            builder.Entity<Product>()
                   .Property(x => x.Price)
                   .HasPrecision(18, 2);

            builder.Entity<Order>()
                   .Property(x => x.TotalPrice)
                   .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                   .Property(x => x.Price)
                   .HasPrecision(18, 2);

            //-------------------------------------
            // Category
            //-------------------------------------

            builder.Entity<Category>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //-------------------------------------
            // Product
            //-------------------------------------

            builder.Entity<Product>()
                .HasMany(x => x.Variants)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId);

            //-------------------------------------
            // Product
            //-------------------------------------

            builder.Entity<Product>()
                .HasMany(x => x.OrderDetails)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId);

            //-------------------------------------
            // Order
            //-------------------------------------

            builder.Entity<Order>()
                .HasMany(x => x.OrderDetails)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId);

            //-------------------------------------
            // User
            //-------------------------------------

            builder.Entity<ApplicationUser>()
                .HasMany(x => x.Orders)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);
        }
    }
}