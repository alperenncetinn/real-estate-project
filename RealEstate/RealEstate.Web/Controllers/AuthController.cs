using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Models;
using RealEstate.Web.Services;

namespace RealEstate.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage, data) = await _authService.LoginAsync(model.Email, model.Password);

            if (success && data != null)
            {
                _authService.SaveUserSession(data, model.RememberMe);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "Giriş başarısız.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage, data) = await _authService.RegisterAsync(model);

            if (success && data != null)
            {
                _authService.SaveUserSession(data, false);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "Kayıt başarısız.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authService.ClearUserSession();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var profile = await _authService.GetProfileAsync();
            if (profile == null)
            {
                return RedirectToAction("Login");
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _authService.UpdateProfileAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "Profil güncellenemedi.");
            return View(model);
        }
    }
}
