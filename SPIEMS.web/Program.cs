using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.BLL.Services;
using SPIEMS.DAL.Entities;

var builder = WebApplication.CreateBuilder(args);

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

// =======================
// Database (EF Core)
// =======================
builder.Services.AddDbContext<SPIEMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// BLL Services
// =======================
builder.Services.AddScoped<ExpiryPredictionService>();
builder.Services.AddScoped<BatchService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<ChartService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<ReturnToSupplierService>();

// =======================
// Authentication
// =======================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// =======================
// Authorization
// =======================
builder.Services.AddAuthorization();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =======================
// Build App
// =======================
var app = builder.Build();

// =======================
// ✅ Seed Admin + Staff (does NOT overwrite existing)
// =======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SPIEMSDbContext>();

    if (!db.Users.Any(u => u.Role == "Admin" && u.Username == "AiraMaryam"))
    {
        db.Users.Add(new User
        {
            Username = "AiraMaryam",
            Password = "aira1234",
            Role = "Admin"
        });
    }

    if (!db.Users.Any(u => u.Role == "Staff" && u.Username == "aira"))
    {
        db.Users.Add(new User
        {
            Username = "aira",
            Password = "aira1234",
            Role = "Staff"
        });
    }

    db.SaveChanges();
}

// =======================
// Middleware Pipeline
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// =======================
// Default Route
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
