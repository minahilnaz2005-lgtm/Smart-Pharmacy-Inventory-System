using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.BLL.Services;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class BatchesController : Controller
{
    private readonly SPIEMSDbContext _db;
    private readonly BatchService _batchSvc;

    public BatchesController(SPIEMSDbContext db, BatchService batchSvc)
    {
        _db = db;
        _batchSvc = batchSvc;
    }

    // GET: /Batches
    public async Task<IActionResult> Index()
    {
        var batches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.Supplier) // ✅ show supplier in list
            .OrderByDescending(b => b.PurchaseDate)
            .ToListAsync();

        return View(batches);
    }

    // GET: /Batches/Add
    [HttpGet]
    public async Task<IActionResult> Add()
    {
        ViewBag.Medicines = await _db.Medicines
            .OrderBy(m => m.GenericName)
            .ToListAsync();

        ViewBag.Suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(new MedicineBatch { PurchaseDate = DateTime.Today });
    }

    // POST: /Batches/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(MedicineBatch batch)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _batchSvc.AddBatchAsync(batch);
                TempData["Success"] = "Batch added successfully (auto-expiry applied if blank).";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        // reload dropdowns if any error
        ViewBag.Medicines = await _db.Medicines
            .OrderBy(m => m.GenericName)
            .ToListAsync();

        ViewBag.Suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(batch);
    }
}