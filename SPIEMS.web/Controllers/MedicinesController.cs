using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.Web.Controllers;

[Authorize]
public class MedicinesController : Controller
{
    private readonly SPIEMSDbContext _db;

    public MedicinesController(SPIEMSDbContext db)
    {
        _db = db;
    }

    // GET: /Medicines/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            var meds = await _db.Medicines
                .Include(m => m.Category)
                .OrderBy(m => m.GenericName)
                .ToListAsync();

            return View(meds);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Error loading medicines: {ex.Message}";
            return View(new List<Medicine>());
        }
    }

    // GET: /Medicines/Add
    [HttpGet]
    public async Task<IActionResult> Add()
    {
        try
        {
            var categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(new Medicine { ReorderLevel = 10 });
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Error: {ex.Message}";
            return View();
        }
    }

    // POST: /Medicines/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Medicine med)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _db.Medicines.Add(med);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Medicine added successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }

        // Reload dropdown
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(med);
    }
}