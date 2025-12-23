using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Services;

namespace RealEstate.Web.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly FavoriteService _favoriteService;
        private readonly AuthService _authService;

        public FavoriteController(FavoriteService favoriteService, AuthService authService)
        {
            _favoriteService = favoriteService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            var listings = await _favoriteService.GetMyFavoriteListingsAsync();
            return View(listings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int listingId, string? returnUrl)
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Auth", new { returnUrl });

            var ok = await _favoriteService.AddToFavoritesAsync(listingId);

            if (!ok)
            {
                TempData["Error"] = "Favoriye ekleme başarısız (API 401/403 dönüyor olabilir).";
                return Redirect(!string.IsNullOrWhiteSpace(returnUrl)
                    ? returnUrl
                    : Url.Action("Index", "Listing")!);
            }

            return !string.IsNullOrWhiteSpace(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }
    }
}
