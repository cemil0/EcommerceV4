using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICartService _cartService;
    private const string SessionCartKey = "CartSessionId";

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ICartService cartService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cartService = cartService;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("", "Şifreler eşleşmiyor!");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Create Customer record
            var customer = new Customer
            {
                ApplicationUserId = user.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                CustomerType = model.CustomerType == "B2B" ? CustomerType.B2B : CustomerType.B2C,
                IsActive = true,
                IsEmailVerified = false
            };

            // TODO: Save customer to database via service

            await _signInManager.SignInAsync(user, isPersistent: false);

            // Merge guest cart if exists (BR-006)
            var sessionId = HttpContext.Session.GetString(SessionCartKey);
            if (!string.IsNullOrEmpty(sessionId) && customer.CustomerId > 0)
            {
                try
                {
                    await _cartService.MergeCartsAsync(customer.CustomerId, sessionId);
                    HttpContext.Session.Remove(SessionCartKey);
                }
                catch
                {
                    // Log error but don't fail registration
                }
            }

            TempData["Success"] = "Kayıt başarılı! Hoş geldiniz!";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Merge guest cart (BR-006)
            var sessionId = HttpContext.Session.GetString(SessionCartKey);
            if (!string.IsNullOrEmpty(sessionId) && user != null)
            {
                // TODO: Get customer ID from user
                // await _cartService.MergeCartsAsync(customerId, sessionId);
                HttpContext.Session.Remove(SessionCartKey);
            }

            TempData["Success"] = "Giriş başarılı!";
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
        }
        else
        {
            ModelState.AddModelError("", "Email veya şifre hatalı!");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Çıkış yapıldı!";
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
