using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesSuite.Web.DTOs;

namespace SalesSuite.Web.Controllers;

/// <summary>
/// Controlador de autenticación: Login, Registro y Logout.
/// Usa ASP.NET Core Identity con cookies (sin JWT).
/// El HomeController y este controlador son accesibles sin autenticación.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser>  _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        UserManager<IdentityUser>  userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager  = userManager;
        _signInManager = signInManager;
    }

    // ── GET /Account/Login ────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si ya está autenticado, redirige al inicio.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    // ── POST /Account/Login ───────────────────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Redirige a la URL de retorno si es local (evita open redirect).
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intenta en 15 minutos.");
        }
        else
        {
            ModelState.AddModelError(string.Empty,
                "Correo o contraseña incorrectos. Verifica tus datos e intenta de nuevo.");
        }

        return View(model);
    }

    // ── GET /Account/Register ─────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    // ── POST /Account/Register ────────────────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user   = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Inicia sesión automáticamente tras el registro.
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Exito"] = "Cuenta creada correctamente. ¡Bienvenido a SalesSuite!";
            return RedirectToAction("Index", "Home");
        }

        // Mapea los errores de Identity al modelo de validación.
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ── POST /Account/Logout ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Account/AccessDenied ─────────────────────────────────────────────
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
