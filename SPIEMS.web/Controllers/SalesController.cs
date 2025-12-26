using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;

namespace SPIEMS.web.Controllers;

public class SalesController : Controller
{
    private readonly SPIEMSDbContext _db;

    public SalesController(SPIEMSDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Checkouts));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet]
    public async Task<IActionResult> Checkouts()
    {
        var checkouts = await _db.Sales
            .Include(s => s.User)
            .Include(s => s.Lines)
                .ThenInclude(l => l.MedicineBatch)
                    .ThenInclude(b => b.Medicine)
            .Where(s => s.Status == "Checkout")
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return View(checkouts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkSoldOut(int id)
    {
        var sale = await _db.Sales.FirstOrDefaultAsync(s => s.Id == id);
        if (sale == null) return NotFound();

        var staffName =
            HttpContext.Session.GetString("Username") ??
            HttpContext.Session.GetString("UserName") ??
            HttpContext.Session.GetString("LoggedInUser") ??
            User?.Identity?.Name ??
            "Staff";

        sale.Status = "SoldOut";
        sale.ProcessedBy = staffName;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"CONFIRMED by {staffName} | Saved ProcessedBy={sale.ProcessedBy}";
        return RedirectToAction(nameof(Checkouts));
    }
}
