using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.BLL.Services;
using SPIEMS.DAL;

namespace SPIEMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ReturnsController : Controller
{
    private readonly ReturnToSupplierService _returnSvc;
    private readonly SPIEMSDbContext _db;

    public ReturnsController(ReturnToSupplierService returnSvc, SPIEMSDbContext db)
    {
        _returnSvc = returnSvc;
        _db = db;
    }

    // List returns log
    public async Task<IActionResult> Index()
    {
        var list = await _returnSvc.GetAllReturnsAsync();
        return View(list);
    }

    // Show near-expiry batches to return
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Suppliers = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Batches = await _returnSvc.GetNearExpiryBatchesAsync(30);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int supplierId, int batchId, int quantity, string? reason)
    {
        try
        {
            await _returnSvc.ReturnAsync(supplierId, batchId, quantity, reason);
            TempData["Success"] = "Return processed and stock updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        ViewBag.Suppliers = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Batches = await _returnSvc.GetNearExpiryBatchesAsync(30);
        return View();
    }
}