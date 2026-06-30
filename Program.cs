using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NikeShop.Data;
using NikeShop.Models;
//using NikeShop.Services;

var builder = WebApplication.CreateBuilder(args);

#region Database

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

#endregion

#region Identity

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Không yêu cầu xác thực Email
    options.SignIn.RequireConfirmedAccount = false;

    // Cấu hình Password
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

#endregion

#region MVC

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

#endregion

#region Session

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

#endregion

#region Dependency Injection

//builder.Services.AddScoped<CartService>();

//builder.Services.AddScoped<OrderService>();

#endregion

var app = builder.Build();

#region Seed Data

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Bỏ comment dòng này để hàm tạo dữ liệu hoạt động
    SeedData.Initialize(services);

    // Dòng này cứ để comment, khi nào làm phân quyền Admin thì mở sau
    // await IdentitySeed.SeedAdminAsync(services); 
}

#endregion



#region Middleware

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

#endregion

#region Route

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

#endregion

app.Run();