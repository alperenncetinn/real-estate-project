using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Models;
using RealEstate.Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RealEstate.Web.Controllers
{
    public class ListingController : Controller
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        public ListingController(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        // --- 1. LİSTELEME (INDEX) ---
        [HttpGet]
        public async Task<IActionResult> Index(string? type) // <-- DEĞİŞİKLİK: Parametre eklendi
        {
            // Gelen type bilgisini (Kiralık/Satılık) servise gönderiyoruz
            var values = await _apiService.GetAllListingsAsync(type);

            // Eğer filtre varsa sayfada göstermek için ViewBag'e atabiliriz (Opsiyonel)
            ViewBag.CurrentType = type;

            return View(values);
        }

        // --- 2. EKLEME SAYFASI (CREATE GET) ---
        [HttpGet]
        public IActionResult Create()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Create", "Listing") });
            }
            return View();
        }

        // --- 3. EKLEME İŞLEMİ (CREATE POST) ---
        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool result = await _apiService.CreateListingAsync(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Ilan basariyla olusturuldu!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Ilan olusturulurken bir hata olustu. Lutfen tekrar deneyin.");
            return View(model);
        }

        // --- 4. DÜZENLEME SAYFASI (EDIT GET) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Edit", "Listing", new { id }) });
            }

            var value = await _apiService.GetListingByIdAsync(id);

            // Gelen veriyi forma dolduruyoruz
            var model = new CreateListingViewModel
            {
                Title = value.Title,
                City = value.City,
                Price = value.Price,
                Description = value.Description,

                // --- EKLENEN KISIMLAR ---
                Type = value.Type,             // <-- ÖNEMLİ: Radyo butonu doğru seçilsin
                RoomCount = value.RoomCount,   // <-- Diğer eksik bilgiler
                SquareMeters = value.SquareMeters
                // -------------------------
            };

            ViewBag.Id = id;
            ViewBag.CurrentImage = value.ImageUrl;

            return View(model);
        }

        // --- 5. DÜZENLEME KAYDETME (EDIT POST) ---
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                // Hata durumunda resmi tekrar göstermek için
                ViewBag.Id = id;
                return View(model);
            }

            bool result = await _apiService.UpdateListingAsync(id, model);

            if (result)
            {
                TempData["SuccessMessage"] = "Ilan basariyla guncellendi!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Ilan guncellenirken bir hata olustu. Bu ilani duzenleme yetkiniz olmayabilir.");
            ViewBag.Id = id;
            return View(model);
        }

        // --- 6. SİLME İŞLEMİ (DELETE) ---
        // Not: Index sayfasında <a> etiketi kullandığımız için HttpPost'u kaldırdık veya HttpGet yaptık.
        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var (success, errorMessage) = await _apiService.DeleteListingAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Ilan basariyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage ?? "Ilan silinirken bir hata olustu.";
            }

            return RedirectToAction("Index");
        }

        // --- 7. DETAY SAYFASI ---
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _apiService.GetListingByIdAsync(id);
            if (listing == null || listing.Id == 0)
            {
                return NotFound();
            }
            return View(listing);
        }

        // --- 8. ILANLARIM SAYFASI ---
        [HttpGet]
        public async Task<IActionResult> MyListings()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("MyListings", "Listing") });
            }

            var listings = await _apiService.GetMyListingsAsync();
            return View(listings);
        }
    }
}