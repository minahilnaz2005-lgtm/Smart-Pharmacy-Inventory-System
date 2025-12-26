using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;
using SPIEMS.Web.Models;
using System.Security.Claims;

namespace SPIEMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly SPIEMSDbContext _db;
    public AccountController(SPIEMSDbContext db) => _db = db;

    // ================= LOGIN =================
    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == vm.Username && u.Password == vm.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid credentials");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        // Route by role
        if (user.Role == "Customer")
            return RedirectToAction("Index", "Shop");

        return RedirectToAction("Index", "Dashboard");
    }

    // ================= SIGNUP (CUSTOMER ONLY) =================
    [HttpGet]
    public IActionResult Signup() => View(new SignupVm());

    [HttpPost]
    public async Task<IActionResult> Signup(SignupVm vm)
    {
        if (vm.Password != vm.ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords don't match");
            return View(vm);
        }

        if (await _db.Users.AnyAsync(u => u.Username == vm.Username))
        {
            ModelState.AddModelError("", "Username already exists");
            return View(vm);
        }

        // 🔒 FORCE CUSTOMER ROLE
        var user = new User
        {
            Username = vm.Username,
            Password = vm.Password,
            Role = "Customer"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // ================= LOGOUT =================
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult Denied() => Content("Access Denied");
}
