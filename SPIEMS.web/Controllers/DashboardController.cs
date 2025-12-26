using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.Web.Models;

namespace SPIEMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly SPIEMSDbContext _db;
        public DashboardController(SPIEMSDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var role = User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                ?? "Customer";

            var today = DateTime.Today;

            // --------------------
            // CARDS (DashboardVm)
            // --------------------

            // Near Expiry = batches expiring in next 30 days
            var expiryLimit = today.AddDays(30);
            var nearExpiry = await _db.MedicineBatches
                .Where(b => b.ExpiryDate != null && b.ExpiryDate >= today && b.ExpiryDate <= expiryLimit)
                .CountAsync();

            // Low Stock = batches with qty <= 10
            var lowStockCount = await _db.MedicineBatches
                .Where(b => b.Quantity <= 10)
                .CountAsync();

            var totalMedicines = await _db.Medicines.CountAsync();
            var totalBatches = await _db.MedicineBatches.CountAsync();

            var vm = new DashboardVm
            {
                NearExpiry = nearExpiry,
                LowStock = lowStockCount,
                TotalMedicines = totalMedicines,
                TotalBatches = totalBatches
            };

            // Keep old ViewBags too (your old dashboard may still use them)
            ViewBag.TotalMedicines = totalMedicines;
            ViewBag.TotalBatches = totalBatches;
            ViewBag.ExpiringSoonCount = nearExpiry;

            // --------------------
            // LOW STOCK TABLE (top 5)
            // --------------------
            var lowStock = await _db.MedicineBatches
                .Where(b => b.Quantity <= 10)
                .Include(b => b.Medicine)
                .OrderBy(b => b.Quantity)
                .Take(5)
                .Select(b => new
                {
                    Medicine = b.Medicine.BrandName ?? b.Medicine.GenericName,
                    BatchNo = b.BatchNo,
                    Qty = b.Quantity
                })
                .ToListAsync();

            ViewBag.LowStock = lowStock;

            // --------------------
            // CHART: Expiry trend (next 6 months)
            // --------------------
            var months = Enumerable.Range(0, 6)
                .Select(i => new DateTime(today.Year, today.Month, 1).AddMonths(i))
                .ToList();

            var expiryTrend = new List<int>();
            var expiryLabels = new List<string>();

            foreach (var m in months)
            {
                var start = m;
                var end = m.AddMonths(1);

                var count = await _db.MedicineBatches
                    .Where(b => b.ExpiryDate != null && b.ExpiryDate >= start && b.ExpiryDate < end)
                    .CountAsync();

                expiryTrend.Add(count);
                expiryLabels.Add(m.ToString("MMM yyyy"));
            }

            ViewBag.ExpiryLabels = expiryLabels;
            ViewBag.ExpiryTrend = expiryTrend;

            // --------------------
            // CHART: Stock by Medicine (top 8)
            // --------------------
            var stockAgg = await _db.MedicineBatches
                .Include(b => b.Medicine)
                .GroupBy(b => b.Medicine.BrandName ?? b.Medicine.GenericName)
                .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Qty)
                .Take(8)
                .ToListAsync();

            ViewBag.StockLabels = stockAgg.Select(x => x.Name).ToList();
            ViewBag.StockValues = stockAgg.Select(x => x.Qty).ToList();

            // --------------------
            // ADMIN ONLY
            // --------------------
            if (role == "Admin")
            {
                // Profit trend (last 6 days)
                var lastDays = Enumerable.Range(0, 6)
                    .Select(i => today.AddDays(-5 + i))
                    .ToList();

                var profitLabels = new List<string>();
                var profitValues = new List<decimal>();

                foreach (var d in lastDays)
                {
                    var start = d;
                    var end = d.AddDays(1);

                    var sum = await _db.Sales
                        .Where(s => s.SaleDate >= start && s.SaleDate < end)
                        .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

                    profitLabels.Add(d.ToString("dd MMM"));
                    profitValues.Add(sum);
                }

                ViewBag.ProfitLabels = profitLabels;
                ViewBag.ProfitValues = profitValues;

                // ✅ FIX: include Role so your view won't crash
                var staffUsers = await _db.Users
                    .Where(u => u.Role == "Staff")
                    .OrderByDescending(u => u.Id)
                    .Select(u => new { u.Id, u.Username, u.Role })
                    .ToListAsync();

                ViewBag.StaffUsers = staffUsers;

                // Optional: expired batches count for "Critical Alerts" if your view shows it
                ViewBag.ExpiredBatches = await _db.MedicineBatches
                    .Where(b => b.ExpiryDate != null && b.ExpiryDate < today)
                    .CountAsync();
            }

            ViewBag.Role = role;

            // ✅ FIX: return model so dashboard doesn't NullReference
            return View(vm);
        }
    }
}
