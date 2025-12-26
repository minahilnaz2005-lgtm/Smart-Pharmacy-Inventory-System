using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;

namespace SPIEMS.web.Controllers;

public class SalesReportsController : Controller
{
    private readonly SPIEMSDbContext _db;

    public SalesReportsController(SPIEMSDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var sales = await _db.Sales
            .Include(s => s.User)
            .Include(s => s.Lines)
                .ThenInclude(l => l.MedicineBatch)
                    .ThenInclude(b => b.Medicine)
            .Where(s => s.Status.StartsWith("SoldOut"))
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        // Put something in ViewBag even if session keys differ
        ViewBag.CurrentUserName =
            HttpContext.Session.GetString("Username") ??
            HttpContext.Session.GetString("UserName") ??
            HttpContext.Session.GetString("LoggedInUser") ??
            User?.Identity?.Name ??
            "Staff";

        return View(sales);
    }
}
