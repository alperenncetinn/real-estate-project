using System; // Console.WriteLine için gerekli
using Microsoft.AspNetCore.Mvc;
using RealEstate.Web.Models;
using RealEstate.Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

        // --- 1. INDEX (FİLTRELEME) ---
        [HttpGet]
        public async Task<IActionResult> Index(string? type, string? sort, string? city, int? minPrice, int? maxPrice, string? roomCount)
        {
            var values = await _apiService.GetAllListingsAsync(type);
            if (values == null) values = new List<ListingViewModel>();

            if (!string.IsNullOrEmpty(city))
            {
                var searchCity = city.Trim().ToLower();
                values = values.Where(x => x.City != null && x.City.ToLower().Contains(searchCity)).ToList();
            }

            if (minPrice.HasValue) values = values.Where(x => x.Price >= minPrice.Value).ToList();
            if (maxPrice.HasValue) values = values.Where(x => x.Price <= maxPrice.Value).ToList();

            if (!string.IsNullOrEmpty(roomCount))
            {
                values = values.Where(x => x.RoomCount != null && x.RoomCount.Trim() == roomCount.Trim()).ToList();
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "price_asc": values = values.OrderBy(x => x.Price).ToList(); break;
                    case "price_desc": values = values.OrderByDescending(x => x.Price).ToList(); break;
                    case "date_desc": values = values.OrderByDescending(x => x.CreatedDate).ToList(); break;
                    case "date_asc": values = values.OrderBy(x => x.CreatedDate).ToList(); break;
                    default: values = values.OrderByDescending(x => x.CreatedDate).ToList(); break;
                }
            }

            ViewBag.CurrentType = type;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentRoomCount = roomCount;

            return View(values);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Create", "Listing") });
            return View();
        }

        // --- DEDEKTİF MODU EKLENMİŞ CREATE METODU ---
        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var currentUser = _authService.GetCurrentUser();
            model.UserId = currentUser?.Id ?? 0;

            // --- DEDEKTİF MODU BAŞLANGIÇ ---
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[WEB DEBUG] Formdan Gelen Başlık: {model.Title}");
            Console.WriteLine($"[WEB DEBUG] Formdan Gelen Oda Sayısı: '{model.RoomCount}'"); 
            Console.WriteLine($"[WEB DEBUG] Formdan Gelen Metrekare: {model.SquareMeters}");
            Console.WriteLine("--------------------------------------------------");
            // --- DEDEKTİF MODU BİTİŞ ---
            
            var (success, errorMsg) = await _apiService.CreateListingAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "İlan başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(errorMsg)) ModelState.AddModelError("", $"API Hatası: {errorMsg}");
            else ModelState.AddModelError("", "Bilinmeyen bir hata oluştu.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var value = await _apiService.GetListingByIdAsync(id);
            if (value == null) return NotFound();

            var model = new CreateListingViewModel
            {
                Title = value.Title ?? "",
                City = value.City ?? "",
                Price = value.Price,
                Description = value.Description,
                Type = value.Type,
                RoomCount = value.RoomCount?.Trim(), 
                SquareMeters = value.SquareMeters,
                UserId = value.UserId ?? 0,
                ImageUrl = value.ImageUrl
            };

            ViewBag.Id = id;
            ViewBag.CurrentImage = value.ImageUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateListingViewModel model)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");
            
            bool result = await _apiService.UpdateListingAsync(id, model);
            
            if (result) return RedirectToAction("Index");

            ModelState.AddModelError("", "Güncelleme başarısız.");
            ViewBag.Id = id;
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");
            var (success, errorMessage) = await _apiService.DeleteListingAsync(id);
            if (success) TempData["SuccessMessage"] = "Silindi!";
            else TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _apiService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return View(listing);
        }

        [HttpGet]
        public async Task<IActionResult> MyListings()
        {
            if (!_authService.IsAuthenticated()) return RedirectToAction("Login", "Auth");
            var listings = await _apiService.GetMyListingsAsync();
            return View(listings);
        }
    }
}