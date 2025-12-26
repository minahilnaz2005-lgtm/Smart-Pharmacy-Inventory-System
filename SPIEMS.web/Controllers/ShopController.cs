using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.BLL.Services;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;
using SPIEMS.Web.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SPIEMS.Web.Controllers;

[Authorize(Roles = "Customer")]
public class ShopController : Controller
{
    private readonly SPIEMSDbContext _db;
    private readonly SalesService _salesService;

    public ShopController(SPIEMSDbContext db, SalesService salesService)
    {
        _db = db;
        _salesService = salesService;
    }

    public async Task<IActionResult> Index()
    {
        // List all medicines, showing stock status
        var medicines = await _db.Medicines
            .Include(m => m.Batches)
            // .Where(m => m.Batches.Any(b => b.Quantity > 0)) // Removed to show everything
            .OrderBy(m => m.GenericName)
            .ToListAsync();

        return View(medicines);
    }

    public async Task<IActionResult> AddToCart(int id, int quantity = 1)
    {
        var med = await _db.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (med == null) return NotFound();

        // Get current price (from first available batch)
        var firstBatch = med.Batches.Where(b => b.Quantity > 0).OrderBy(b => b.ExpiryDate).FirstOrDefault();
        if (firstBatch == null) {
             TempData["Error"] = "Out of stock";
             return RedirectToAction(nameof(Index));
        }

        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.MedicineId == id);
        
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemVm
            {
                MedicineId = med.Id,
                MedicineName = med.GenericName + " (" + med.BrandName + ")",
                Quantity = quantity,
                EstimatedPrice = firstBatch.SalePrice
            });
        }

        SaveCart(cart);
        TempData["Success"] = "Added to cart!";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Cart()
    {
        return View(GetCart());
    }

    public IActionResult RemoveFromCart(int id)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.MedicineId == id);
        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }
        return View("Cart", cart); // Stay on Cart page
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["Error"] = "Cart is empty";
            return RedirectToAction(nameof(Index));
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
        {
             return RedirectToAction("Login", "Account");
        }

        try 
        {
            // Convert CartItemVm to (int, int) tuple
            var items = cart.Select(c => (c.MedicineId, c.Quantity)).ToList();
            await _salesService.ProcessOrderAsync(items, userId);

            // Clear Cart
            HttpContext.Session.Remove("Cart");
            return View("OrderConfirmation");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Cart));
        }
    }

    private List<CartItemVm> GetCart()
    {
        var session = HttpContext.Session.GetString("Cart");
        return session == null ? new List<CartItemVm>() : JsonSerializer.Deserialize<List<CartItemVm>>(session);
    }

    private void SaveCart(List<CartItemVm> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }
}
