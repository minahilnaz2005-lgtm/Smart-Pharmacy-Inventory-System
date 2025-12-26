using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly SPIEMSDbContext _db;
        public StaffController(SPIEMSDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var staff = await _db.Users
                .Where(u => u.Role == "Staff")
                .OrderByDescending(u => u.Id)
                .Select(u => new { u.Id, u.Username, u.Role })
                .ToListAsync();

            return View(staff);
        }

        // ✅ FIX: /Staff/Create must exist (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ FIX: /Staff/Create must exist (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password)
        {
            username = (username ?? "").Trim();
            password = password ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and Password are required.";
                return View();
            }

            var exists = await _db.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }

            var staff = new User
            {
                Username = username,
                Password = password,
                Role = "Staff"
            };

            _db.Users.Add(staff);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "Staff");
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
