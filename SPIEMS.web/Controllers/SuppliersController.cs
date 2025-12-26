using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.BLL.Services;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SuppliersController : Controller
{
    private readonly SupplierService _svc;
    private readonly SPIEMSDbContext _db;

    public SuppliersController(SupplierService svc, SPIEMSDbContext db)
    {
        _svc = svc;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _svc.GetAllAsync(includeInactive: true);
        return View(list);
    }

    // ✅ NOW THIS WORKS because Details view exists (added below)
    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return NotFound();

        var batches = await _db.MedicineBatches.AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.SupplierId == id) // ✅ FIXED
            .OrderByDescending(b => b.PurchaseDate)
            .ToListAsync();

        ViewBag.Batches = batches;
        return View(supplier);
    }

    [HttpGet]
    public IActionResult Create() => View(new Supplier());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid)
            return View(supplier);

        await _svc.AddAsync(supplier);
        TempData["Success"] = "Supplier added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _svc.GetByIdAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Supplier supplier)
    {
        if (!ModelState.IsValid)
            return View(supplier);

        await _svc.UpdateAsync(supplier);
        TempData["Success"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _svc.DeactivateAsync(id);
        TempData["Success"] = "Supplier deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        await _svc.ActivateAsync(id);
        TempData["Success"] = "Supplier activated.";
        return RedirectToAction(nameof(Index));
    }
}